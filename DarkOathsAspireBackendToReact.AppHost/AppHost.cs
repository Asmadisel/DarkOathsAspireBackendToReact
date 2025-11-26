// AppHost/AppHost.cs
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var authDb = builder.AddPostgres("authdb") // БД для AuthService
    .WithPgAdmin(); // PgAdmin покажет обе БД

var apiDb = builder.AddPostgres("apidb"); 

var redis = builder.AddRedis("redis");

// Aspire теперь будет ждать полной готовности PostgreSQL
var apiService = builder.AddProject<Projects.DarkOathsAspireBackendToReact_ApiService>("apiservice")
    .WithReference(apiDb)
    .WithReference(redis)
    .WaitFor(apiDb);

builder.AddProject<Projects.DarkOathsAspireBackendToReact_Web>("webfrontend")
    .WithReference(redis);

var authService = builder.AddProject<Projects.DarkOathsAspireBackendToReact_AuthService>("authservice")
    .WithReference(authDb)
    .WaitFor(authDb);

var vkBotMigrations = builder.AddProject<Projects.VkDarkOathsBot>("vkbot-migrations")
    .WithReference(authDb)
    .WithArgs("--migrate")
    .WaitFor(authDb)
    .WaitFor(authService);


var vkDarkOathsBot = builder.AddProject<Projects.VkDarkOathsBot>("vkdarkoathsbot")
    .WithReference(authDb);

//var myReactApp = builder.AddNpmApp("my-react-app", "../MyReactApp", "serve")
//    .WithHttpEndpoint(port: 3000, env: "PORT");

builder.Build().Run();