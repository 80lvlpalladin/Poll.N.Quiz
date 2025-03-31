using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using MassTransit;
using Moq;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.Domain.Internal;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;
using Poll.N.Quiz.Settings.Synchronizer.Consumers;

namespace Poll.N.Quiz.Settings.Synchronizer.UnitTests.Consumers;

public class SettingsEventQueueConsumerTests
{

    [Test]
    public async Task Consume_WhenConsumingCreateEvent_WhenExistingProjectionIsNotNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var writeOnlyProjection = new Mock<IWriteOnlySettingsProjectionStore>();
        var readOnlyProjection = new Mock<IReadOnlySettingsProjectionStore>();
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        var settingsCreateEvent = TestSettingsEventFactory.CreateSettingsCreateEvent();
        var settingsAggregate = SettingsAggregate.CreateFrom(settingsCreateEvent).Value;
        settingsAggregate.ApplyEvent(settingsCreateEvent);

        context.Setup(c => c.Message).Returns(settingsCreateEvent);
        readOnlyProjection
            .Setup(r => r.GetAsync(settingsCreateEvent.Metadata))
            .ReturnsAsync(settingsAggregate.GetCurrentProjection().Value);
        var consumer = new SettingsEventQueueConsumer
            (writeOnlyProjection.Object, readOnlyProjection.Object);

        // Act
        var act = async () => await consumer.Consume(context.Object);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Test]
    public async Task Consume_WhenConsumingUpdateEvent_WhenExistingProjectionIsOlderThanEvent_UpdatesProjection()
    {
        // Arrange
        var writeOnlyProjection = new Mock<IWriteOnlySettingsProjectionStore>();
        var readOnlyProjection = new Mock<IReadOnlySettingsProjectionStore>();
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        var service1Events = TestSettingsEventFactory
            .CreateSettingsEvents().Where(se => se.Metadata.ServiceName == "service1").ToArray();
        var expectedCreateEvent = service1Events[0];
        var expectedUpdateEvent = service1Events[1];
        var settingsAggregate = SettingsAggregate.CreateFrom(expectedCreateEvent).Value;
        settingsAggregate.ApplyEvent(expectedCreateEvent);
        context.Setup(c => c.Message).Returns(expectedUpdateEvent);
        readOnlyProjection.Setup(r =>
                r.GetAsync(expectedUpdateEvent.Metadata))
            .ReturnsAsync(settingsAggregate.GetCurrentProjection().Value);
        var consumer = new SettingsEventQueueConsumer
            (writeOnlyProjection.Object, readOnlyProjection.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        settingsAggregate.ApplyEvent(expectedUpdateEvent);
        var expectedNewProjection = settingsAggregate.GetCurrentProjection().Value;
        writeOnlyProjection.Verify(w => w.SaveProjectionAsync(
            It.Is<SettingsProjection>(projection =>
                projection.JsonData == expectedNewProjection.JsonData &&
                projection.Version == expectedNewProjection.Version &&
                projection.LastUpdatedTimestamp == expectedNewProjection.LastUpdatedTimestamp),
            It.IsAny<SettingsMetadata>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Consume_WhenConsumingUpdateEvent_WhenExistingProjectionIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var writeOnlyProjection = new Mock<IWriteOnlySettingsProjectionStore>();
        var readOnlyProjection = new Mock<IReadOnlySettingsProjectionStore>();
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        var settingsUpdateEvent = TestSettingsEventFactory.CreateSettingsUpdateEvent();
        context
            .Setup(c => c.Message)
            .Returns(settingsUpdateEvent);
        readOnlyProjection
            .Setup(r =>
                r.GetAsync(settingsUpdateEvent.Metadata))
            .ReturnsAsync((SettingsProjection?)null);
        var consumer = new SettingsEventQueueConsumer
            (writeOnlyProjection.Object, readOnlyProjection.Object);

        // Act
        var act = async () => await consumer.Consume(context.Object);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Test]
    public async Task Consume_WhenConsumingUpdateEvent_WhenExistingProjectionIsNewerThanEvent_ThrowsInvalidOperationException()
    {
        // Arrange
        var writeOnlyProjectionStoreMock = new Mock<IWriteOnlySettingsProjectionStore>();
        var readOnlyProjectionStoreMock = new Mock<IReadOnlySettingsProjectionStore>();
        var settingsUpdateEvent = new SettingsEvent(
            SettingsEventType.UpdateEvent,
            new SettingsMetadata("service", "env"),
            10,
            2,
            "not relevant");
        var existingProjection = new SettingsProjection(
            "not relevant",
            20,
            3);
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        context.Setup(c => c.Message).Returns(settingsUpdateEvent);
        readOnlyProjectionStoreMock
            .Setup(r =>
                r.GetAsync(settingsUpdateEvent.Metadata))
            .ReturnsAsync(existingProjection);
        var consumer = new SettingsEventQueueConsumer
            (writeOnlyProjectionStoreMock.Object, readOnlyProjectionStoreMock.Object);

        // Act
        var act = async () => await consumer.Consume(context.Object);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
