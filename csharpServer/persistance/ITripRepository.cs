using System;
using System.Collections.Generic;
using model;

namespace persistance;

public interface ITripRepository : IRepository<int, Trip>
{
    IEnumerable<Trip> FindAllByName(string name);
    
    Trip FindByDestinationAndDateAndTime(string destination, string dateString, string timeString);


}