public class PeopleService : People.PeopleBase
{
    PersonsManager personsManager;

    public PeopleService(PersonsManager _personsManager)
    {
        personsManager = _personsManager;
    }

    public override Task<PeopleReply> GetAll(GetAllPeopleRequest request,
        ServerCallContext context)
    {
        var reply = new PeopleReply();
        reply.People.AddRange(personsManager.People);
        return Task.FromResult(reply);
    }

    public override async Task GetAllStream(GetAllPeopleStreamRequest request, 
        IServerStreamWriter<Person> responseStream, ServerCallContext context)
    {
        // Use this pattern to return a stream in a gRPC service.

        var receivedPerson = request.Person;

        // retrieve the list
        var people = personsManager.People;
        
        // write each item to the responseStream, which does the rest
        foreach (var person in people)
        {
            await responseStream.WriteAsync(person);
        }
    }

    public override Task<Person> GetPersonById(GetPersonByIdRequest request,
        ServerCallContext context)
    {
        var result = (from x in personsManager.People
                      where x.Id == request.Id
                      select x).FirstOrDefault();
        return Task.FromResult(result);
    }
}
