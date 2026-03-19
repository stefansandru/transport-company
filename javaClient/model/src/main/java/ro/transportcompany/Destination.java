package ro.transportcompany;

public class Destination extends Entity<Integer> {
    private String name;

    public Destination() {
        super();
    }

    public Destination(Integer id, String name) {
        super(id);
        this.name = name;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    @Override
    public String toString() {
        return name;
    }
}