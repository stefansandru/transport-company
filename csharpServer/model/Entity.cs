namespace model;

/// <summary>
/// Generic base class for all domain entities exposing the primary key.
/// </summary>
public abstract class Entity<ID>
{
    public ID Id { get;  set; }

    public Entity() {}
    
    public Entity(ID id)
    {
        this.Id = id;
    }
}