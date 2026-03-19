package ro.transportcompany;


import java.time.LocalDate;
import java.time.LocalTime;

public class Trip extends Entity<Integer> {
    private Destination destination;
    private LocalDate departureDate;
    private LocalTime departureTime;
    private Integer availableSeats;

    public Trip() {
        super();
    }

    public Trip(
            Integer id,
            Destination destination,
            LocalDate departureDate,
            LocalTime departureTime,
            Integer availableSeats) {
        super(id);
        this.destination = destination;
        this.departureDate = departureDate;
        this.departureTime = departureTime;
        this.availableSeats = availableSeats;
    }

    public Destination getDestination() {
        return destination;
    }

    public void setDestination(Destination destination) {
        this.destination = destination;
    }

    public LocalDate getDepartureDate() {
        return departureDate;
    }

    public void setDepartureDate(LocalDate departureDate) {
        this.departureDate = departureDate;
    }

    public LocalTime getDepartureTime() {
        return departureTime;
    }

    public void setDepartureTime(LocalTime departureTime) {
        this.departureTime = departureTime;
    }

    public Integer getAvailableSeats() {
        return availableSeats;
    }

    public void setAvailableSeats(Integer availableSeats) {
        this.availableSeats = availableSeats;
    }

    @Override
    public String toString() {
        return "Trip{" +
                "destination=" + destination +
                ", departureDate=" + departureDate +
                ", departureTime=" + departureTime +
                ", availableSeats=" + availableSeats +
                '}';
    }
}