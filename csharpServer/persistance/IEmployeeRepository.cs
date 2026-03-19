using System;
using model;

namespace persistance;

public interface IEmployeeRepository : IRepository<int, Employee>
{
    /// <summary>
    /// Finds an employee by their username and password.
    /// </summary>
    /// <param name="username">The username of the employee to be returned.</param>
    /// <returns>The employee with the given username, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="username"/>  is null.</exception>
    Employee? FindByUsername(string username);
}