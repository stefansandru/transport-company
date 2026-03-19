package ro.transportcompany;

/**
 * Base class for all domain entities that require an ID
 * @param <ID> The type of the entity's identifier
 */
public abstract class Entity<ID> {
    private ID id;

    public Entity() {
    }

    public Entity(ID id) {
        this.id = id;
    }

    public ID getId() {
        return id;
    }

    public void setId(ID id) {
        this.id = id;
    }
}
