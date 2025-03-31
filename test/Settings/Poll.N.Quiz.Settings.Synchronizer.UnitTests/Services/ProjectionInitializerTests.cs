/*
using Moq;
using Poll.N.Quiz.Settings.Domain.Internal;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.Messaging.Contracts.Internal;
using Poll.N.Quiz.Settings.Projection.WriteOnly;
using Poll.N.Quiz.Settings.Synchronizer.Services;

namespace Poll.N.Quiz.Settings.Synchronizer.UnitTests.Services;

public class ProjectionInitializerTests
{
    [Test]
    [Arguments("service1", "environment1")]
    [Arguments("service2", "environment2")]
    public async Task InitializeProjectonAsync_ConstructsProjection_FromSettingsEvents
        (string expectedServiceName, string expectedEnvironmentName)
    {
        //Arrange
        var writeOnlyProjectionMock = new Mock<IWriteOnlySettingsProjection>(MockBehavior.Loose);
        var readOnlySettingsEventStoreMock = new Mock<IReadOnlySettingsEventStore>(MockBehavior.Strict);
        readOnlySettingsEventStoreMock
            .Setup(es => es.GetAllEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestSettingsEventFactory.CreateSettingsEvents().ToArray);
        var projectionUpdater = new ProjectionInitializer
            (readOnlySettingsEventStoreMock.Object, writeOnlyProjectionMock.Object);
        var expectedProjectionString =
            TestSettingsEventFactory.GetExpectedResultSettings(expectedServiceName, expectedEnvironmentName);
        var expectedCreationDate =
            TestSettingsEventFactory.CreateSettingsEvents()
                .Where(se => se.ServiceName == expectedServiceName &&
                             se.EnvironmentName == expectedEnvironmentName)
                .OrderBy(se => se.TimeStamp).Last().TimeStamp;

        //Act
        await projectionUpdater.ExecuteAsync(CancellationToken.None);

        //Assert
        writeOnlyProjectionMock.Verify(wp => wp.SaveProjectionAsync(
                expectedCreationDate,
                expectedServiceName,
                expectedEnvironmentName,
                expectedProjectionString,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
*/
