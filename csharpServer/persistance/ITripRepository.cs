using System;
using System.Collections.Generic;
using model;

namespace persistance;

public interface ITripRepository : IRepository<int, Trip>
{
    /// <summary>
    /// Finds all trips by the destination name.
    /// </summary>
    /// <param name="name">The name of the destination to be returned.</param>
    /// <returns>An <see cref="IEnumerable{Trip}"/> encapsulating the trips with the given destination name.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null.</exception>
    IEnumerable<Trip> FindAllByName(string name);
    
    Trip FindByDestinationAndDateAndTime(string destination, string dateString, string timeString);


}