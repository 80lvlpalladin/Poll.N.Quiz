var builder = DistributedApplication.CreateBuilder(args);

var eventStore = builder.AddMongoDB("EventStore");
//var mongodb = mongoEventStore.AddDatabase("mongodb");
var projection = builder.AddRedis("Projection");
var messageBroker = builder.AddKafka("MessageBroker");

builder
    .AddProject<Projects.Poll_N_Quiz_Settings_API>("SettingsAPI")
    .WithReference(eventStore)
    .WithReference(projection)
    .WithReference(messageBroker);


builder.Build().Run();
