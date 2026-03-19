package ro.transportcompany;

public class Client extends Entity<Integer> {
    private String name;

    public Client() {
        super();
    }

    public Client(Integer id, String name) {
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
        return "Client{" +
                "id=" + getId() +
                ", name='" + name + '\'' +
                '}';
    }
}