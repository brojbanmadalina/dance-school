var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin();

var api = builder.AddProject<Projects.DanceSchool>("api")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

builder.Build().Run();