var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgresdb")
                      .WithPgAdmin();

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.DarkOathsAspireBackendToReact_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
                        .WithReference(postgres);

builder.AddProject<Projects.DarkOathsAspireBackendToReact_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
