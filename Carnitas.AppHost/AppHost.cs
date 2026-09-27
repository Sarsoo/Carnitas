var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<Projects.Carnitas_Web>("web")
    .WithHttpEndpoint();

var cli = builder.AddProject<Projects.Carnitas_CLI>("cli")
    .WithReference(web)
    .WaitForStart(web);

builder.Build().Run();
