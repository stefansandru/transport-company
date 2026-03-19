using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using model;

namespace persistance;

public class OfficeRepository : AbstractRepository<int, Office>, IOfficeRepository
{
    public OfficeRepository(DatabaseConnection jdbc) : base(jdbc)
    {
    }

    public override Office? FindById(int id)
    {
        const string query = "SELECT * FROM Office WHERE id = @id";
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
                        var name = reader.GetString(reader.GetOrdinal("name"));
                        return new Office(id, name);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding Office with ID {Id}", id);
        }

        return null;
    }

    public override IEnumerable<Office> FindAll()
    {
        var offices = new List<Office>();
        const string query = "SELECT * FROM Office";

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
                        var name = reader.GetString(reader.GetOrdinal("name"));
                        offices.Add(new Office(id, name));
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding all Offices");
        }

        return offices;
    }

    public override Office? Save(Office office)
    {
        if (office == null)
            throw new ArgumentNullException(nameof(office));

        const string query = "INSERT INTO Office (name) VALUES (@name)";
        try
        {
            using (var connection = jdbc.GetConnection())
            {
                connection.Open();
                
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@name", office.Name);
                    command.ExecuteNonQuery();
                }

                using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", (SqliteConnection)connection))
                {
                    var id = Convert.ToInt32(idCommand.ExecuteScalar());
                    office.Id = id;
                    return office;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while saving Office: {Office}", office);
        }

        return null;
    }

    public override Office? Delete(int id)
    {
        var officeToDelete = FindById(id);
        if (officeToDelete == null)
        {
            return null;
        }

        const string query = "DELETE FROM Office WHERE id = @id";
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
                    return officeToDelete;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while deleting Office with ID {Id}", id);
        }

        return null;
    }

    public override Office? Update(Office office)
    {
        if (office == null)
            throw new ArgumentNullException(nameof(office));

        const string query = "UPDATE Office SET name = @name WHERE id = @id";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@name", office.Name);
                command.Parameters.AddWithValue("@id", office.Id);

                connection.Open();
                var affectedRows = command.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    return office;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while updating Office: {Office}", office);
        }

        return null;
    }

    public Office? FindByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name must not be null or empty", nameof(name));

        const string query = "SELECT * FROM Office WHERE name = @name";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@name", name);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var id = reader.GetInt32(reader.GetOrdinal("id"));
                        return new Office(id, name);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding Office with name {Name}", name);
        }

        return null;
    }
}