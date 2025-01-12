
using ErrorOr;
using MassTransit;
using Moq;
using Poll.N.Quiz.Settings.Commands.Handlers;
using Poll.N.Quiz.Settings.EventStore.WriteOnly;
using Poll.N.Quiz.Settings.Messaging.Contracts;
using Poll.N.Quiz.Settings.Messaging.Contracts.Internal;

namespace Poll.N.Quiz.Settings.Commands.UnitTests.Handlers;

public class CreateSettingsHandlerTests
{
    [Test]
    public async Task HandleAsync_ShouldPublishSettingsCreateEventToPublishEndpoint_AndSaveItToEventStore()
    {
        //Arrange
        var eventStoreMock = new Mock<IWriteOnlySettingsEventStore>(MockBehavior.Strict);
        var publishEndpointMock = new Mock<IPublishEndpoint>(MockBehavior.Strict);


        var expectedSettingsCreateEvent = TestSettingsEventFactory
            .CreateSettingsEvents().First(se => se.EventType is SettingsEventType.CreateEvent);

        eventStoreMock
            .Setup(x => x.SaveAsync(
                It.Is<SettingsEvent>(sce => sce.Equals(expectedSettingsCreateEvent)), CancellationToken.None))
            .ReturnsAsync(true);
        publishEndpointMock
            .Setup(x => x.Publish(
                It.Is<SettingsEvent>(sce => sce.Equals(expectedSettingsCreateEvent)), CancellationToken.None))
            .Returns(Task.CompletedTask);

        var command = CreateCommandFrom(expectedSettingsCreateEvent);
        var handler = new CreateSettingsHandler(eventStoreMock.Object, publishEndpointMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        await Assert.That(result.IsError).IsFalse();
        eventStoreMock.Verify(x => x.SaveAsync(expectedSettingsCreateEvent, CancellationToken.None), Times.Once);
        publishEndpointMock.Verify(x => x.Publish(expectedSettingsCreateEvent, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task HandleAsync_ShouldReturnError_WhenEventStoreFailsToSaveEvent()
    {
        //Arrange
        var eventStoreMock = new Mock<IWriteOnlySettingsEventStore>(MockBehavior.Strict);
        var publishEndpointMock = new Mock<IPublishEndpoint>(MockBehavior.Strict);

        var expectedSettingsCreateEvent = TestSettingsEventFactory
            .CreateSettingsEvents().First(se => se.EventType is SettingsEventType.CreateEvent);

        eventStoreMock
            .Setup(x => x.SaveAsync(
                It.Is<SettingsEvent>(sce => sce.Equals(expectedSettingsCreateEvent)), CancellationToken.None))
            .ReturnsAsync(false);

        var command = CreateCommandFrom(expectedSettingsCreateEvent);
        var handler = new CreateSettingsHandler(eventStoreMock.Object, publishEndpointMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        await Assert.That(result.IsError).IsTrue();
        eventStoreMock.Verify(x => x.SaveAsync(expectedSettingsCreateEvent, CancellationToken.None), Times.Once);
        publishEndpointMock.Verify(x => x.Publish(expectedSettingsCreateEvent, CancellationToken.None), Times.Never);
    }

    [Test]
    [MethodDataSource(
        typeof(CreateSettingsHandlerTestDataSource),
        nameof(CreateSettingsHandlerTestDataSource.InvalidSettingsCreateEventsTestData))]
    public async Task HandleAsync_ShouldReturnValidationError_WhenCommandValidationFails
        (SettingsEvent invalidSettingsEvent)
    {
        //Arrange
        var eventStoreMock = new Mock<IWriteOnlySettingsEventStore>(MockBehavior.Loose);
        var publishEndpointMock = new Mock<IPublishEndpoint>(MockBehavior.Loose);
        var handler = new CreateSettingsHandler(eventStoreMock.Object, publishEndpointMock.Object);
        var command = CreateCommandFrom(invalidSettingsEvent);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        await Assert.That(result.IsError).IsTrue();
        await Assert.That(result.FirstError.Type).IsEqualTo(ErrorType.Validation);
        eventStoreMock.Verify(x =>
            x.SaveAsync(invalidSettingsEvent, CancellationToken.None), Times.Never);
        publishEndpointMock.Verify(x =>
            x.Publish(invalidSettingsEvent, CancellationToken.None), Times.Never);
    }

    private static CreateSettingsCommand CreateCommandFrom(SettingsEvent @event) =>
        new(@event.TimeStamp,
            @event.ServiceName,
            @event.EnvironmentName,
            @event.JsonData);
}


public static class CreateSettingsHandlerTestDataSource
{
    public static IEnumerable<Func<SettingsEvent>> InvalidSettingsCreateEventsTestData()
    {
        yield return () => new SettingsEvent(
            SettingsEventType.CreateEvent,
            10,
            "service1",
            "environment1",
            "dsfdsfdsfdsfds");

        yield return () => new SettingsEvent(
            SettingsEventType.CreateEvent,
            10,
            "service1",
            string.Empty,
            "{\"key\": \"value\"}");

        yield return () => new SettingsEvent(
            SettingsEventType.CreateEvent,
            10,
            string.Empty,
            "environment1",
            "{\"key\": \"value\"}");

        yield return () => new SettingsEvent(
            SettingsEventType.CreateEvent,
            10,
            "service1",
            "environment1",
            string.Empty);
    }
}
