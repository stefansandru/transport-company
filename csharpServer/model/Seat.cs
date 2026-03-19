namespace model;

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