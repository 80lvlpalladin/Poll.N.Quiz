using Moq;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.Messaging.Contracts.Internal;
using Poll.N.Quiz.Settings.Projection.WriteOnly;
using Poll.N.Quiz.Settings.Synchronizer.Services;

namespace Poll.N.Quiz.Settings.Synchronizer.UnitTests.Services;

public class ProjectionUpdaterTests
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
        var projectionUpdater = new ProjectionUpdater
            (readOnlySettingsEventStoreMock.Object, writeOnlyProjectionMock.Object);
        var expectedProjectionString =
            TestSettingsEventFactory.GetExpectedResultSettings(expectedServiceName, expectedEnvironmentName);
        var expectedCreationDate =
            TestSettingsEventFactory.CreateSettingsEvents()
                .Where(se => se.ServiceName == expectedServiceName &&
                             se.EnvironmentName == expectedEnvironmentName)
                .OrderBy(se => se.TimeStamp).Last().TimeStamp;

        //Act
        await projectionUpdater.InitializeProjectionAsync(CancellationToken.None);

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
