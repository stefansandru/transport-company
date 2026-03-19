namespace model;

/// <summary>
/// Simple value object representing a seat number inside a coach (1-18).
/// </summary>
public class Seat
{
    public int SeatNumber { get; }
    public string ClientName { get; }

    public Seat(int seatNumber, string clientName)
    {
        SeatNumber = seatNumber;
        ClientName = string.IsNullOrEmpty(clientName) ? "-" : clientName;
    }
    
    public override string ToString()
    {
        return $"Seat {SeatNumber} reserved by {ClientName}";
    }
}