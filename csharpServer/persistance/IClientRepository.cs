using System;
using model;
using model;

namespace persistance;

public interface IClientRepository : IRepository<int, Client>
{
    /// <summary>
    /// Finds a client by their username.
    /// </summary>
    /// <param name="username">The username of the client to be returned.</param>
    /// <returns>The client with the given username, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="username"/> is null.</exception>
    Client? FindByName(string username);
}