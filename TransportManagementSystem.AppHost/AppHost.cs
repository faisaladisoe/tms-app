var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TransportManagementSystem>("transportmanagementsystem");

builder.Build().Run();
