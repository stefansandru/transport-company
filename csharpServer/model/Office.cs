namespace model;

public class Office : Entity<int>
{
    public string Name { get; set; }

    public Office() { }

    public Office(int id, string name) : base(id)
    {
        Name = name;
    }

    public override string ToString()
    {
        return $"Office {{ Id = {Id}, Name = {Name} }}";
    }
}