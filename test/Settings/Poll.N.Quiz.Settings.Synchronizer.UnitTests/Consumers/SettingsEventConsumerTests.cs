using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using MassTransit;
using Moq;
using Poll.N.Quiz.Settings.Messaging.Contracts;
using Poll.N.Quiz.Settings.Messaging.Contracts.Internal;
using Poll.N.Quiz.Settings.Projection.ReadOnly;
using Poll.N.Quiz.Settings.Projection.WriteOnly;
using Poll.N.Quiz.Settings.Synchronizer.Consumers;

namespace Poll.N.Quiz.Settings.Synchronizer.UnitTests.Consumers;

public class SettingsEventConsumerTests
{

    [Test]
    public async Task Consume_WhenConsumingCreateEvent_WhenExistingProjectionIsNotNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var writeOnlyProjection = new Mock<IWriteOnlySettingsProjection>();
        var readOnlyProjection = new Mock<IReadOnlySettingsProjection>();
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        var settingsCreateEvent = TestSettingsEventFactory.CreateSettingsCreateEvent();
        context.Setup(c => c.Message).Returns(settingsCreateEvent);
        readOnlyProjection.Setup(r =>
                r.GetAsync(settingsCreateEvent.ServiceName, settingsCreateEvent.EnvironmentName))
            .ReturnsAsync((settingsCreateEvent.JsonData, settingsCreateEvent.TimeStamp));
        var consumer = new SettingsEventConsumer(writeOnlyProjection.Object, readOnlyProjection.Object);

        // Act
        var act = async () => await consumer.Consume(context.Object);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Test]
    public async Task Consume_WhenConsumingUpdateEvent_WhenExistingProjectionIsOlderThanEvent_UpdatesProjection()
    {
        // Arrange
        var writeOnlyProjection = new Mock<IWriteOnlySettingsProjection>();
        var readOnlyProjection = new Mock<IReadOnlySettingsProjection>();
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        var service1Events = TestSettingsEventFactory
            .CreateSettingsEvents().Where(se => se.ServiceName == "service1").ToArray();
        var expectedCreateEvent = service1Events[0];
        var expectedUpdateEvent = service1Events[1];
        context.Setup(c => c.Message).Returns(expectedUpdateEvent);
        readOnlyProjection.Setup(r =>
                r.GetAsync(expectedUpdateEvent.ServiceName, expectedUpdateEvent.EnvironmentName))
            .ReturnsAsync((expectedCreateEvent.JsonData, expectedCreateEvent.TimeStamp));
        var consumer = new SettingsEventConsumer(writeOnlyProjection.Object, readOnlyProjection.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        var jsonPatch = JsonSerializer.Deserialize<JsonPatch>(expectedUpdateEvent.JsonData)!;
        var jsonNode = JsonNode.Parse(expectedCreateEvent.JsonData)!;
        var patchResult = jsonPatch.Apply(jsonNode);
        var expectedNewProjection = patchResult.Result!.ToJsonString();
        writeOnlyProjection.Verify(w => w.SaveProjectionAsync(
            It.Is<uint>(value => value == expectedUpdateEvent.TimeStamp),
            It.Is<string>(value => value == expectedUpdateEvent.ServiceName),
            It.Is<string>(value => value == expectedUpdateEvent.EnvironmentName),
            It.Is<string>(value => value == expectedNewProjection),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Consume_WhenConsumingUpdateEvent_WhenExistingProjectionIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var writeOnlyProjection = new Mock<IWriteOnlySettingsProjection>();
        var readOnlyProjection = new Mock<IReadOnlySettingsProjection>();
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        var settingsUpdateEvent = TestSettingsEventFactory.CreateSettingsUpdateEvent();
        context.Setup(c => c.Message).Returns(settingsUpdateEvent);
        readOnlyProjection.Setup(r =>
                r.GetAsync(settingsUpdateEvent.ServiceName, settingsUpdateEvent.EnvironmentName))
            .ReturnsAsync(((string, uint)?)null);
        var consumer = new SettingsEventConsumer(writeOnlyProjection.Object, readOnlyProjection.Object);

        // Act
        var act = async () => await consumer.Consume(context.Object);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Test]
    public async Task Consume_WhenConsumingUpdateEvent_WhenExistingProjectionIsNewerThanEvent_ThrowsInvalidOperationException()
    {
        // Arrange
        var writeOnlyProjection = new Mock<IWriteOnlySettingsProjection>();
        var readOnlyProjection = new Mock<IReadOnlySettingsProjection>();
        var context = new Mock<ConsumeContext<SettingsEvent>>();
        var settingsUpdateEvent = new SettingsEvent
            (SettingsEventType.UpdateEvent,1, "service", "env", string.Empty);
        context.Setup(c => c.Message).Returns(settingsUpdateEvent);
        readOnlyProjection.Setup(r =>
                r.GetAsync(settingsUpdateEvent.ServiceName, settingsUpdateEvent.EnvironmentName))
            .ReturnsAsync(((string jsonData, uint lastUpdatedTimestamp)?)(string.Empty, 2));
        var consumer = new SettingsEventConsumer(writeOnlyProjection.Object, readOnlyProjection.Object);

        // Act
        var act = async () => await consumer.Consume(context.Object);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
