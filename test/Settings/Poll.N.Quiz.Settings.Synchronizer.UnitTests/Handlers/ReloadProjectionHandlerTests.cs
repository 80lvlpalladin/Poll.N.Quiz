using ErrorOr;
using Moq;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;
using Poll.N.Quiz.Settings.Synchronizer.Handlers;

namespace Poll.N.Quiz.Settings.Synchronizer.UnitTests.Handlers;

public class ReloadProjectionHandlerTests
{
    [Test]
    public async Task HandleAsync_ReturnsValidationError_GivenUnorderedSequenceOfEvents()
    {
        //Arrange
        var settingsMetadata = new SettingsMetadata("service1", "environment1");
        var eventsWithRuinedOrder = RuinEventsOrder(
            TestSettingsEventFactory.CreateSettingsEvents()
                .Where(se => se.Metadata == settingsMetadata)
                .ToArray());
        var readOnlySettingsEventStore = new Mock<IReadOnlySettingsEventStore>();
        readOnlySettingsEventStore.Setup(es =>
                es.GetAsync(
                    It.Is<SettingsMetadata>(m => m == settingsMetadata),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventsWithRuinedOrder);
        var writeOnlySettingsProjectionStore = new Mock<IWriteOnlySettingsProjectionStore>();
        var handler = new ReloadProjectionHandler(
            readOnlySettingsEventStore.Object,
            writeOnlySettingsProjectionStore.Object);

        //Act
        var result = await handler.Handle(
            new ReloadProjectionRequest(settingsMetadata.ServiceName, settingsMetadata.EnvironmentName),
            CancellationToken.None);

        //Assert
        await Assert.That(result.IsError).IsTrue();
        await Assert.That(result.Errors.Count).IsEqualTo(1);
        await Assert.That(result.FirstError.Type).IsEqualTo(ErrorType.Validation);
    }

    [Test]
    public async Task Handle_SavesValidSettingsProjection_GivenOrderedSequenceOfEvents()
    {
        //Arrange
        var settingsMetadata = new SettingsMetadata("service1", "environment1");
        var settingsEvents = TestSettingsEventFactory
            .CreateSettingsEvents()
            .Where(se => se.Metadata == settingsMetadata)
            .ToArray();

        var readOnlySettingsEventStore = new Mock<IReadOnlySettingsEventStore>();
        readOnlySettingsEventStore.Setup(es =>
                es.GetAsync(
                    It.Is<SettingsMetadata>(m => m == settingsMetadata),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(settingsEvents);
        var writeOnlySettingsProjectionStore = new Mock<IWriteOnlySettingsProjectionStore>();
        var handler = new ReloadProjectionHandler(
            readOnlySettingsEventStore.Object,
            writeOnlySettingsProjectionStore.Object);

        //Act
        var result = await handler.Handle(
            new ReloadProjectionRequest(settingsMetadata.ServiceName, settingsMetadata.EnvironmentName),
            CancellationToken.None);

        //Assert
        var expectedProjectionJson = TestSettingsEventFactory
            .GetExpectedResultSettings(settingsMetadata.ServiceName, settingsMetadata.EnvironmentName);
        var expectedProjectionLastUpdatedTimeStamp = settingsEvents.Last().TimeStamp;
        var expectedProjectionVersion = settingsEvents.Last().Version;
        var expectedProjection = new SettingsProjection(
            expectedProjectionJson,
            expectedProjectionLastUpdatedTimeStamp,
            expectedProjectionVersion);

        await Assert.That(result.IsError).IsFalse();
        writeOnlySettingsProjectionStore.Verify(
            x => x.SaveProjectionAsync(
                It.Is<SettingsProjection>(projection => projection == expectedProjection),
                It.Is<SettingsMetadata>(m => m == settingsMetadata),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Test]
    public async Task Handle_ReturnsNotFoundError_IfEventStoreIsEmpty()
    {
        //Arrange
        var settingsMetadata = new SettingsMetadata("ServiceName", "EnvironmentName");
        var readOnlySettingsEventStoreMock = new Mock<IReadOnlySettingsEventStore>();
        var writeOnlySettingsProjectionStoreMock = new Mock<IWriteOnlySettingsProjectionStore>();
        readOnlySettingsEventStoreMock
            .Setup(x =>
                x.GetAsync(
                    It.Is<SettingsMetadata>(metadata => metadata == settingsMetadata),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new ReloadProjectionHandler(
            readOnlySettingsEventStoreMock.Object,
            writeOnlySettingsProjectionStoreMock.Object);

        //Act
        var result = await handler.Handle(
            new ReloadProjectionRequest(settingsMetadata.ServiceName, settingsMetadata.EnvironmentName),
            CancellationToken.None);

        //Assert
        await Assert.That(result.IsError).IsTrue();
        await Assert.That(result.Errors.Count).IsEqualTo(1);
        await Assert.That(result.FirstError.Type).IsEqualTo(ErrorType.NotFound);
    }

    private SettingsEvent[] RuinEventsOrder(SettingsEvent[] events)
    {
        do
        {
            events = events.OrderBy(e => Random.Shared.Next()).ToArray();
        }
        while(events[0].EventType is SettingsEventType.CreateEvent);

        return events;
    }
}
