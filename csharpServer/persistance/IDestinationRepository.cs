using System;
using System.Collections.Generic;
using model;

namespace persistance;

public interface IDestinationRepository : IRepository<int, Destination>
{
    /// <summary>
    /// Finds destinations by their name.
    /// </summary>
    /// <param name="name">The name of the destination to be returned.</param>
    /// <returns>An <see cref="IEnumerable{Destination}"/> encapsulating the destinations with the given name.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null.</exception>
    IEnumerable<Destination> FindByName(string name);
}