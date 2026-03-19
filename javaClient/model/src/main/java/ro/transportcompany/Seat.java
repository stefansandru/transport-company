package ro.transportcompany;

public class Seat {
    private final int seatNumber;
    private final String clientName;

    public Seat(int seatNumber, String clientName) {
        this.seatNumber = seatNumber;
        this.clientName = clientName;
    }

    public int getSeatNumber() {
        return seatNumber;
    }

    public String getClientName() {
        return clientName == null ? "-" : clientName;
    }
}