namespace model;

public abstract class Entity<ID>
{
    public ID Id { get;  set; }

    public Entity() {}
    
    public Entity(ID id)
    {
        this.Id = id;
    }
}