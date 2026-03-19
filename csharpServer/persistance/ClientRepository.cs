using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using model;

namespace persistance;

public class ClientRepository : AbstractRepository<int, Client>, IClientRepository
{
    private readonly ILogger<ClientRepository> _logger;

    public ClientRepository(DatabaseConnection jdbc, ILogger<ClientRepository> logger) : base(jdbc)
    {
        _logger = logger;
    }

    public override Client? FindById(int id)
    {
        const string query = "SELECT * FROM Client WHERE id = @id";
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
                        return new Client(id, name);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Database error while finding Client with ID {Id}", id);
        }

        return null;
    }

    public Client? FindByUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username must not be null or empty", nameof(username));

        const string query = "SELECT * FROM Client WHERE name = @username";
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
                        return new Client(id, username);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Database error while finding Client with username {Username}", username);
        }

        return null;
    }

    public Client FindByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name must not be null or empty", nameof(name));

        const string query = "SELECT * FROM Client WHERE name = @name";
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
                        return new Client(id, name);
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Database error while finding Client with name {Name}", name);
        }

        return null;
    }

    public override IEnumerable<Client> FindAll()
    {
        var clients = new List<Client>();
        const string query = "SELECT * FROM Client";

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
                        clients.Add(new Client(id, name));
                    }
                }
            }
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Database error while finding all Clients");
        }

        return clients;
    }

    public override Client? Save(Client client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        const string query = "INSERT INTO Client (name) VALUES (@name) RETURNING Id;";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@name", client.Name);
                connection.Open();
                var id = Convert.ToInt32(command.ExecuteScalar());
                return new Client(id, client.Name);
            }
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Database error while saving Client: {Client}", client);
        }

        return null;
    }

    public override Client? Delete(int id)
    {
        var clientToDelete = FindById(id);
        if (clientToDelete == null)
        {
            return null;
        }

        const string query = "DELETE FROM Client WHERE id = @id";
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
                    return clientToDelete;
                }
            }
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Database error while deleting Client with ID {Id}", id);
        }

        return null;
    }

    public override Client? Update(Client client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        const string query = "UPDATE Client SET name = @name WHERE id = @id";
        try
        {
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@name", client.Name);
                command.Parameters.AddWithValue("@id", client.Id);
                connection.Open();
                var affectedRows = command.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    return client;
                }
            }
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Database error while updating Client: {Client}", client);
        }

        return null;
    }
}