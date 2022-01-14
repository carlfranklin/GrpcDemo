using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GrpcDemo;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
	People.PeopleClient _peopleClient;
    Person ReceivedPerson;

    private ObservableCollection<Person> people { get; set; } = new ObservableCollection<Person>();
    public ObservableCollection<Person> People { 
        get { return people; }
        set {
            people = value;
            OnPropertyChanged("People");
        }
    }

    public MainPage(People.PeopleClient peopleClient)
	{
		_peopleClient = peopleClient;
        BindingContext = this;
		InitializeComponent();
	}

    private async void GRPCStreamButtonClicked(object sender, EventArgs e)
    {
        // set up
        var token = new System.Threading.CancellationToken();
        double elapsed20 = 0;
        People.Clear();
        CounterLabel.Text = "Loading...";
        SemanticScreenReader.Announce(CounterLabel.Text);
        await Task.Delay(1);

        // start time
        var startTime = DateTime.Now;

        // the client-side for gRPC streams is a bit different.
        // First we return an AsyncServerStreamingCall<Person>
        using var call = _peopleClient.GetAllStream(
            new GetAllPeopleStreamRequest() { Person = ReceivedPerson}
        );

        // Now we can iterate through the response stream
        while (await call.ResponseStream.MoveNext(token))
        {
            // add this person to our list (this blows up in iOS)
            People.Add(call.ResponseStream.Current);

            // have we reached 20 yet?
            if (People.Count == 20)
            {
                // yes! That's enough to fill up the <select>
                elapsed20 = DateTime.Now.Subtract(startTime).TotalMilliseconds;
                CounterLabel.Text = $"Loading ({elapsed20} ms)...{People.Count}";
                // refresh the page
                SemanticScreenReader.Announce(CounterLabel.Text);
                await Task.Delay(1);
            }

            // Is the count evenly divisible by 100?
            else if (People.Count % 100 == 0)
            {
                // yes! refresh the UI.
                CounterLabel.Text = $"Loading ({elapsed20} ms)...{People.Count}";
                SemanticScreenReader.Announce(CounterLabel.Text);
                await Task.Delay(1);
            }
        }

        // show elapsed time.
        var elapsed = DateTime.Now.Subtract(startTime);
        CounterLabel.Text = $"{People.Count} records returned via gRPC Stream in {elapsed.TotalMilliseconds} ms. "
            + $" Initial 20 in {elapsed20} ms.";

    }

    private async void OnCounterClicked(object sender, EventArgs e)
	{
		var obj = new object();
		var rnd = new Random(obj.GetHashCode());
		int RandomId = rnd.Next(1, 5000);
		var request = new GetPersonByIdRequest { Id = RandomId };

		try
        {
			ReceivedPerson = await _peopleClient.GetPersonByIdAsync(request);

			if (ReceivedPerson != null)
			{
				CounterLabel.Text = $"{ReceivedPerson.Id} {ReceivedPerson.FirstName} {ReceivedPerson.LastName}";
				SemanticScreenReader.Announce(CounterLabel.Text);
			}
		}
		catch (Exception ex)
        {
			var msg = ex.Message;
        }
	}

}

