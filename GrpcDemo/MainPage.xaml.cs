using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GrpcDemo;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    People.PeopleClient _peopleClient;
    Person ReceivedPerson;
    int FirstBatch = 40;

    public ObservableCollection<Person> People { get; set; } = new();

    public MainPage(People.PeopleClient peopleClient)
    {
        _peopleClient = peopleClient;
        BindingContext = this;
        InitializeComponent();
    }

    private async void GRPCStreamButtonClicked(object sender, EventArgs e)
    {
        // set up
        var token = new CancellationToken();
        double elapsedFirstBatch = 0;
        People.Clear();
        CounterLabel.Text = "Loading...";
        SemanticScreenReader.Announce(CounterLabel.Text);
        await Task.Delay(1);

        // start time
        var startTime = DateTime.Now;

        // First we return an AsyncServerStreamingCall<Person>
        using var call = _peopleClient.GetAllStream(
            new GetAllPeopleStreamRequest { Person = ReceivedPerson }
        );

        // Use for temporary storage
        var buffer = new List<Person>();

        // flushBuffer is called whenever either FirstBatch number of records has been received
        // or the records received is divisible by 100. We have to do stuff in here
        Task flushBuffer()
            => MainThread.InvokeOnMainThreadAsync(async () =>
            {
#if __IOS__
                // This code works around a bug in the MAUI iOS CollectionView control,
                // which has been submitted as a PR by Jonathan Dick.

                // If we either have FirstBatch records or ALL the records...
                if (buffer.Count == FirstBatch || buffer.Count == 5000)
                {
                    // Completely replace the People collection (rude to the user, I know).
                    People = new ObservableCollection<Person>(buffer);
                    OnPropertyChanged(nameof(People));
                }
                // Update the UI 
                CounterLabel.Text = $"Loading {buffer.Count}";
                // calculate FirstBatch receive time based on buffer, not People
                if (buffer.Count == FirstBatch)
                    elapsedFirstBatch = DateTime.Now.Subtract(startTime).TotalMilliseconds;
#else
                // NOT iOS. This is what we normally do.

                // Copy the contents of buffer to People.
                foreach (var p in buffer)
                    People.Add(p);
                
                // Clear the buffer
                buffer.Clear();
                
                // Update the UI
                OnPropertyChanged(nameof(People));
                CounterLabel.Text = $"Loading {People.Count}";
                // Calculate FirstBatch receive time based on People, not buffer
                if (People.Count == FirstBatch)
                    elapsedFirstBatch = DateTime.Now.Subtract(startTime).TotalMilliseconds;
#endif

                //Refresh the label.
                SemanticScreenReader.Announce(CounterLabel.Text);
                await Task.Delay(1);
            });

        // Now we can iterate through the response stream
        while (await call.ResponseStream.MoveNext(token))
        {
            // add this person to temporary storage
            buffer.Add(call.ResponseStream.Current);

            var peopleCount = People.Count + buffer.Count;

            // Do we need to do something here?
            if (peopleCount == FirstBatch || peopleCount % 100 == 0)
            {
                // refresh and update
                await flushBuffer();
            }
        }

        // show elapsed time.
        var elapsed = DateTime.Now.Subtract(startTime);
        CounterLabel.Text = $"{People.Count} records returned via gRPC Stream in {elapsed.TotalMilliseconds} ms. "
            + $" Initial {FirstBatch} in {elapsedFirstBatch} ms.";
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

