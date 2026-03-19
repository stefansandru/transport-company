package ro.transportcompany;

public class Office extends Entity<Integer> {
    private String name;

    public Office() {
        super();
    }

    public Office(Integer id, String name) {
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
        return "Id=" + getId() + " " + name;
    }
}