namespace GrpcDemo;

public partial class App : Application
{
	public App(People.PeopleClient peopleClient)
	{
		InitializeComponent();

		MainPage = new MainPage(peopleClient);
	}
}
