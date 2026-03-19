namespace model;

using System;

/// <summary>
/// Join entity that links a <see cref="Trip"/>, <see cref="Client"/> and seat number.
/// </summary>
public class ReservedSeat : Entity<int>
{
    public Trip Trip { get; set; }
    public Employee Employee { get; set; }
    public int? SeatNumber { get; set; }
    public Client Client { get; set; }
    public ReservedSeat() { }

    public ReservedSeat(int id, 
        Trip trip, 
        Employee employee, 
        int seatNumber, 
        Client client) : base(id)
    {
        Trip = trip;
        Employee = employee;
        SeatNumber = seatNumber;
        Client = client;
    }
}