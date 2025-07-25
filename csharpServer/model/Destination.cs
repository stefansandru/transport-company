namespace model;

/// <summary>
/// City or location where a <see cref="Trip"/> is headed.
/// </summary>
public class Destination : Entity<int>
{
    public string Name { get; set; }

    public Destination(): base() { }

    public Destination(int id, string name) :base(id)
    {
        Name = name;
    }
    
    public override string ToString()
    {
        return Name;
    }
}