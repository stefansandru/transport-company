namespace model;

/// <summary>
/// An employee that can log into the desktop application and reserve seats for clients.
/// </summary>
public class Employee : Entity<int>
{
    public string Username { get; set; }
    public string Password { get; set; }
    public Office Office { get; set; }

    public Employee() : base() { }

    public Employee(int id, string username, string password, Office office) : base(id)
    {
        Username = username;
        Password = password;
        Office = office;
    }
}