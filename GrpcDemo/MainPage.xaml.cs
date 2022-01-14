using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GrpcDemo;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    People.PeopleClient _peopleClient;
    Person ReceivedPerson;

    public List<Person> People { get; } = new ();
    
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
            new GetAllPeopleStreamRequest { Person = ReceivedPerson}
        );

        var buffer = new List<Person>();

        Task flushBuffer()
            => MainThread.InvokeOnMainThreadAsync(async () =>
            {
                People.AddRange(buffer);
                buffer.Clear();
                OnPropertyChanged(nameof(People));

                elapsed20 = DateTime.Now.Subtract(startTime).TotalMilliseconds;
                // yes! refresh the UI.
                CounterLabel.Text = $"Loading ({elapsed20} ms)...{People.Count}";
                SemanticScreenReader.Announce(CounterLabel.Text);
                await Task.Delay(1);
            });

        // Now we can iterate through the response stream
        while (await call.ResponseStream.MoveNext(token))
        {
            // add this person to our list (this blows up in iOS)
            buffer.Add(call.ResponseStream.Current);

            var peopleCount = People.Count + buffer.Count;

            // Is the count evenly divisible by 100?
            if (peopleCount == 20 || peopleCount % 100 == 0)
            {
                await flushBuffer();
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

