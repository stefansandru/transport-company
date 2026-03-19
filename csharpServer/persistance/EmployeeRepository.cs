using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using model;

namespace persistance;

public class EmployeeRepository : AbstractRepository<int, Employee>, IEmployeeRepository
{
    private readonly IOfficeRepository officeRepository;

    public EmployeeRepository(IOfficeRepository officeRepository, DatabaseConnection jdbc) : base(jdbc)
    {
        this.officeRepository = officeRepository;
    }

    public override Employee? FindById(int id)
    {
        if (id == 0)
        {
            throw new ArgumentException("ID must not be zero", nameof(id));
        }

        const string query = "SELECT * FROM Employee WHERE id = @id";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var officeId = reader.GetInt32(reader.GetOrdinal("office_id"));
                        var username = reader.GetString(reader.GetOrdinal("username"));
                        var password = reader.GetString(reader.GetOrdinal("password"));
                        var office = officeRepository.FindById(officeId);
                        if (office == null) return null;
                        return new Employee(id, username, password, office);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding Employee with ID {Id}", id);
        }

        return null;
    }

    public override IEnumerable<Employee> FindAll()
    {
        var employees = new List<Employee>();
        const string query = "SELECT * FROM Employee";

        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(reader.GetOrdinal("id"));
                        var username = reader.GetString(reader.GetOrdinal("username"));
                        var password = reader.GetString(reader.GetOrdinal("password"));
                        var officeId = reader.GetInt32(reader.GetOrdinal("office_id"));
                        var office = officeRepository.FindById(officeId);
                        if (office == null) continue;
                        employees.Add(new Employee(id, username, password, office));
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding all Employees");
        }

        return employees;
    }

    public override Employee? Save(Employee employee)
    {
        if (employee == null)
        {
            throw new ArgumentNullException(nameof(employee));
        }

        const string query = "INSERT INTO Employee (username, password, office_id) VALUES (@username, @password, @office_id)";
        try
        {
            using (var connection = jdbc.GetConnection())
            {
                connection.Open();
                
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@username", employee.Username);
                    command.Parameters.AddWithValue("@password", employee.Password);
                    command.Parameters.AddWithValue("@office_id", employee.Office.Id);
                    command.ExecuteNonQuery();
                }

                using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", (SqliteConnection)connection))
                {
                    var id = Convert.ToInt32(idCommand.ExecuteScalar());
                    employee.Id = id;
                    return employee;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while saving Employee: {Employee}", employee);
        }

        return null;
    }

    public override Employee? Delete(int id)
    {
        var employeeToDelete = FindById(id);
        if (employeeToDelete == null)
        {
            return null;
        }

        const string query = "DELETE FROM Employee WHERE id = @id";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                var affectedRows = command.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    return employeeToDelete;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while deleting Employee with ID {Id}", id);
        }

        return null;
    }

    public override Employee? Update(Employee employee)
    {
        if (employee == null)
        {
            throw new ArgumentNullException(nameof(employee));
        }

        const string query = "UPDATE Employee SET username = @username, password = @password, office_id = @office_id WHERE id = @id";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@username", employee.Username);
                command.Parameters.AddWithValue("@password", employee.Password);
                command.Parameters.AddWithValue("@office_id", employee.Office.Id);
                command.Parameters.AddWithValue("@id", employee.Id);
                connection.Open();
                var affectedRows = command.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    return employee;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while updating Employee: {Employee}", employee);
        }

        return null;
    }

    public Employee? FindByUsername(string username)
    {
        const string query = "SELECT * FROM Employee WHERE username = @username";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@username", username);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var id = reader.GetInt32(reader.GetOrdinal("id"));
                        var password = reader.GetString(reader.GetOrdinal("password"));
                        var officeId = reader.GetInt32(reader.GetOrdinal("office_id"));
                        var office = officeRepository.FindById(officeId);
                        if (office == null) return null;
                        return new Employee(id, username, password, office);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding Employee by username and password");
        }

        return null;
    }
}