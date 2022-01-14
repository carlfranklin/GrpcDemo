global using Grpc.Core;
global using GrpcShared;
global using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

builder.Services.AddSingleton<PersonsManager>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseGrpcWeb();
app.UseCors();

app.MapGrpcService<PeopleService>().EnableGrpcWeb().RequireCors("AllowAll");

app.Run();
