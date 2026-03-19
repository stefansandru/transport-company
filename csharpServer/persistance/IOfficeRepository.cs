using System;
using model;

namespace persistance;

public interface IOfficeRepository : IRepository<int, Office>
{
    Office? FindByName(string name);
}