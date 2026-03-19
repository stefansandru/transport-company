using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using model;

namespace persistance;

public class DestinationRepository : AbstractRepository<int, Destination>, IDestinationRepository
{
    public DestinationRepository(DatabaseConnection jdbc) : base(jdbc)
    {
    }

    public override Destination? FindById(int id)
    {
        const string query = "SELECT * FROM Destination WHERE id = @id";
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
                        return new Destination(id, name);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding Destination with ID {Id}", id);
        }

        return null;
    }

    public IEnumerable<Destination> FindByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name must not be null or empty", nameof(name));

        const string query = "SELECT * FROM Destination WHERE name = @name";
        var destinations = new List<Destination>();

        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@name", name);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(reader.GetOrdinal("id"));
                        var destinationName = reader.GetString(reader.GetOrdinal("name"));
                        destinations.Add(new Destination(id, destinationName));
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding Destination with name {Name}", name);
        }

        return destinations;
    }

    public override IEnumerable<Destination> FindAll()
    {
        var destinations = new List<Destination>();
        const string query = "SELECT * FROM Destination";

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
                        destinations.Add(new Destination(id, name));
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while finding all Destinations");
        }

        return destinations;
    }

    public override Destination? Save(Destination destination)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        const string insertQuery = "INSERT INTO Destination (name) VALUES (@name)";
        try
        {
            using (var connection = jdbc.GetConnection())
            {
                connection.Open();
                
                using (var command = new SqliteCommand(insertQuery, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@name", destination.Name);
                    command.ExecuteNonQuery();
                }

                using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", (SqliteConnection)connection))
                {
                    var id = Convert.ToInt32(idCommand.ExecuteScalar());
                    destination.Id = id;
                    return destination;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while saving Destination: {Destination}", destination);
        }

        return null;
    }

    public override Destination? Delete(int id)
    {
        var destinationToDelete = FindById(id);
        if (destinationToDelete == null)
        {
            return null;
        }

        const string query = "DELETE FROM Destination WHERE id = @id";
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
                    return destinationToDelete;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while deleting Destination with ID {Id}", id);
        }

        return null;
    }

    public override Destination? Update(Destination destination)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        const string query = "UPDATE Destination SET name = @name WHERE id = @id";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@name", destination.Name);
                command.Parameters.AddWithValue("@id", destination.Id);
                connection.Open();
                var affectedRows = command.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    return destination;
                }
            }
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Database error while updating Destination: {Destination}", destination);
        }

        return null;
    }
}