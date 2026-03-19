namespace model;

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