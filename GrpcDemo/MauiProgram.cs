global using GrpcShared;
global using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace GrpcDemo;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        string BaseAddress = "https://carlsgrpcserver.azurewebsites.net/";
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddScoped(services =>
        {
            var baseUri = new Uri(BaseAddress);
            var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(new HttpClientHandler())
            });
            return new People.PeopleClient(channel);
        });

        return builder.Build();
    }
}
