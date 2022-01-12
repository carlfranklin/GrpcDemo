global using Grpc.Core;
global using GrpcShared;
global using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddSingleton<PersonsManager>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapGrpcService<PeopleService>();

app.Run();
