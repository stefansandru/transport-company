namespace model;

using System;

public class Trip : Entity<int>
{
    public Destination Destination { get; set; }
    public DateOnly DepartureDate { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public int? AvailableSeats { get; set; }

    public Trip() { }

    public Trip(int id, Destination destination, DateOnly departureDate, TimeOnly departureTime, int? availableSeats) : base(id)
    {
        Destination = destination;
        DepartureDate = departureDate;
        DepartureTime = departureTime;
        AvailableSeats = availableSeats;
    }
    
    public override string ToString()
    {
        return $"{Destination} | {DepartureDate} | {DepartureTime} | {AvailableSeats} Available Seats";
    }
}