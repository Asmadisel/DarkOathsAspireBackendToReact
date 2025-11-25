// AppHost/AppHost.cs
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var authDb = builder.AddPostgres("authdb") // БД для AuthService
    .WithPgAdmin(); // PgAdmin покажет обе БД

var apiDb = builder.AddPostgres("apidb"); 

var redis = builder.AddRedis("redis");

// Aspire теперь будет ждать полной готовности PostgreSQL
builder.AddProject<Projects.DarkOathsAspireBackendToReact_ApiService>("apiservice")
    .WithReference(apiDb)
    .WithReference(redis)
    .WaitFor(apiDb);

builder.AddProject<Projects.DarkOathsAspireBackendToReact_Web>("webfrontend")
    .WithReference(redis);

builder.AddProject<Projects.DarkOathsAspireBackendToReact_AuthService>("authservice")
    .WithReference(authDb)
    .WaitFor(authDb);

builder.Build().Run();