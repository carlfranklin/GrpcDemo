global using GrpcShared;
global using Grpc.Net.Client;
namespace GrpcDemo;
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		string BaseAddress = "https://localhost:7216";
	
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
			var channel = GrpcChannel.ForAddress(baseUri);
			return new People.PeopleClient(channel);
		});

		return builder.Build();
	}
}
