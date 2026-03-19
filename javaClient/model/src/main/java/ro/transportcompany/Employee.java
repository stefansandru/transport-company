package ro.transportcompany;

public class Employee extends Entity<Integer> {
    private String username;
    private String password;
    private Office office;

    public Employee() {
        super();
    }

    public Employee(Integer id, String username, String password, Office office) {
        super(id);
        this.username = username;
        this.password = password;
        this.office = office;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getPassword() {
        return password;
    }

    public void setPassword(String password) {
        this.password = password;
    }

    public Office getOffice() {
        return office;
    }

    public void setOffice(Office office) {
        this.office = office;
    }

    @Override
    public String toString() {
        return "Employee{" +
                "id=" + getId() +
                ", username='" + username + '\'' +
                ", office=" + office +
                '}';
    }
}