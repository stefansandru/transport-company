using System;
using System.Collections.Generic;
using model;

namespace persistance;

public interface IDestinationRepository : IRepository<int, Destination>
{
    IEnumerable<Destination> FindByName(string name);
}