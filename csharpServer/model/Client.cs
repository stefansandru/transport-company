namespace model;

/// <summary>
/// A person for whom seats are reserved. Does not authenticate – only stored for reference.
/// </summary>
public class Client : Entity<int>
{
    public string Name { get; set; }

    public Client() : base()
    {
    }

    public Client(int id, string name) : base(id)
    {
        Name = name;
    }
}