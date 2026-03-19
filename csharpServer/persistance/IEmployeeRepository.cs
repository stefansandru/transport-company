using System;
using model;

namespace persistance;

public interface IEmployeeRepository : IRepository<int, Employee>
{
    Employee? FindByUsername(string username);
}