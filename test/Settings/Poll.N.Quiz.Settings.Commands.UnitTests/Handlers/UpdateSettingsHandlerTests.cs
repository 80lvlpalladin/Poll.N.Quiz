using ErrorOr;
using MassTransit;
using Moq;
using Poll.N.Quiz.Settings.API.Commands.Handlers;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventQueue;
using Poll.N.Quiz.Settings.EventStore.WriteOnly;

namespace Poll.N.Quiz.Settings.Commands.UnitTests.Handlers;

public class UpdateSettingsHandlerTests
{
    [Test]
    public async Task HandleAsync_ShouldPublishSettingsUpdateEventToPublishEndpoint_AndSaveItToEventStore()
    {
        // Arrange
        var eventStoreMock = new Mock<IWriteOnlySettingsEventStore>(MockBehavior.Strict);
        var topicProducerMock = new Mock<ITopicProducer<SettingsEvent>>(MockBehavior.Strict);
        var settingsEventQueueProducer = new SettingsEventQueueProducer(topicProducerMock.Object);

        var expectedSettingsUpdateEvent = TestSettingsEventFactory
            .CreateSettingsEvents().First(se => se.EventType is SettingsEventType.UpdateEvent);

        eventStoreMock
            .Setup(x => x.SaveAsync(
                It.Is<SettingsEvent>(sce => sce.Equals(expectedSettingsUpdateEvent)), CancellationToken.None))
            .ReturnsAsync(true);
        topicProducerMock
            .Setup(x => x.Produce(
                It.Is<SettingsEvent>(sce => sce.Equals(expectedSettingsUpdateEvent)), CancellationToken.None))
            .Returns(Task.CompletedTask);

        var command = CreateCommandFrom(expectedSettingsUpdateEvent);
        var handler = new UpdateSettingsHandler(eventStoreMock.Object, settingsEventQueueProducer);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.That(result.IsError).IsFalse();
        eventStoreMock.Verify(x => x.SaveAsync(expectedSettingsUpdateEvent, CancellationToken.None), Times.Once);
        topicProducerMock.Verify(x => x.Produce(expectedSettingsUpdateEvent, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task HandleAsync_ShouldReturnFailureError_WhenEventStoreFailsToSaveEvent()
    {
        // Arrange
        var eventStoreMock = new Mock<IWriteOnlySettingsEventStore>(MockBehavior.Strict);
        var topicProducerMock = new Mock<ITopicProducer<SettingsEvent>>(MockBehavior.Strict);
        var settingsEventQueueProducer = new SettingsEventQueueProducer(topicProducerMock.Object);

        var expectedSettingsUpdateEvent = TestSettingsEventFactory
            .CreateSettingsEvents().First(se => se.EventType is SettingsEventType.UpdateEvent);

        eventStoreMock
            .Setup(x => x.SaveAsync(
                It.Is<SettingsEvent>(sce => sce.Equals(expectedSettingsUpdateEvent)), CancellationToken.None))
            .ReturnsAsync(false);

        var command = CreateCommandFrom(expectedSettingsUpdateEvent);
        var handler = new UpdateSettingsHandler(eventStoreMock.Object, settingsEventQueueProducer);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.That(result.IsError).IsTrue();
        await Assert.That(result.FirstError.Type).IsEqualTo(ErrorType.Failure);
        eventStoreMock.Verify(x => x.SaveAsync(expectedSettingsUpdateEvent, CancellationToken.None), Times.Once);
        topicProducerMock.Verify(x => x.Produce(expectedSettingsUpdateEvent, CancellationToken.None), Times.Never);
    }

    [Test]
    [MethodDataSource(
        typeof(UpdateSettingsHandlerTestDataSource),
        nameof(UpdateSettingsHandlerTestDataSource.InvalidSettingsUpdateEventsTestData))]
    public async Task HandleAsync_ShouldReturnValidationError_WhenCommandValidationFails
        (SettingsEvent invalidSettingsEvent)
    {
        // Arrange
        var eventStoreMock = new Mock<IWriteOnlySettingsEventStore>(MockBehavior.Loose);
        var topicProducerMock = new Mock<ITopicProducer<SettingsEvent>>(MockBehavior.Strict);
        var settingsEventQueueProducer = new SettingsEventQueueProducer(topicProducerMock.Object);
        var handler = new UpdateSettingsHandler(eventStoreMock.Object, settingsEventQueueProducer);
        var command = CreateCommandFrom(invalidSettingsEvent);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.That(result.IsError).IsTrue();
        await Assert.That(result.FirstError.Type).IsEqualTo(ErrorType.Validation);
        eventStoreMock.Verify(x =>
            x.SaveAsync(invalidSettingsEvent, CancellationToken.None), Times.Never);
        topicProducerMock.Verify(x =>
            x.Produce(invalidSettingsEvent, CancellationToken.None), Times.Never);
    }

    private static UpdateSettingsCommand CreateCommandFrom(SettingsEvent @event) =>
        new(@event.TimeStamp,
            @event.Version,
            @event.Metadata.ServiceName,
            @event.Metadata.EnvironmentName,
            @event.JsonData);
}

public static class UpdateSettingsHandlerTestDataSource
{
    public static IEnumerable<Func<SettingsEvent>> InvalidSettingsUpdateEventsTestData()
    {
        yield return () => new SettingsEvent(
            SettingsEventType.UpdateEvent,
            new SettingsMetadata("settings1", "environment1"),
            0,
            0,
            "dsfdsfdsfdsfds");

        yield return () => new SettingsEvent(
            SettingsEventType.UpdateEvent,
            new SettingsMetadata("settings1", string.Empty),
            0,
            0,
            "{\"key\": \"value\"}");

        yield return () => new SettingsEvent(
            SettingsEventType.UpdateEvent,
            new SettingsMetadata(string.Empty, "environment1"),
            0,
            0,
            "{\"key\": \"value\"}");

        yield return () => new SettingsEvent(
            SettingsEventType.UpdateEvent,
            new SettingsMetadata("settings1", "environment1"),
            0,
            0,
            string.Empty);
    }
}

