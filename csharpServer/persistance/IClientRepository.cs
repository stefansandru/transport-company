using System;
using model;
using model;

namespace persistance;

public interface IClientRepository : IRepository<int, Client>
{
    Client? FindByName(string username);
}