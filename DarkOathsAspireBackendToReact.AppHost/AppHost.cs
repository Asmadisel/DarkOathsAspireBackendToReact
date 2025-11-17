// AppHost/AppHost.cs
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgresdb")
    .WithPgAdmin();

var redis = builder.AddRedis("redis");

// Aspire теперь будет ждать полной готовности PostgreSQL
builder.AddProject<Projects.DarkOathsAspireBackendToReact_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres);

builder.AddProject<Projects.DarkOathsAspireBackendToReact_Web>("webfrontend")
    .WithReference(redis);

builder.Build().Run();