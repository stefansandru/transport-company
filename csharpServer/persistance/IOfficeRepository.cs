using System;
using model;

namespace persistance;

public interface IOfficeRepository : IRepository<int, Office>
{
    /// <summary>
    /// Finds an office by its name.
    /// </summary>
    /// <param name="name">The name of the office to be returned.</param>
    /// <returns>The office with the given name, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null.</exception>
    Office? FindByName(string name);
}