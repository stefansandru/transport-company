package ro.transportcompany;

import java.time.LocalDate;
import java.time.LocalTime;
import java.util.List;

public interface IServices {
    public Employee login(String username, String password, IObserver client) throws ServicesException;
    public void logout(Employee employee) throws ServicesException;
    public List<Trip> getAllTrips() throws ServicesException;
    public List<Seat> searchTripSeats(String destination, LocalDate date, LocalTime time) throws ServicesException;
    public void reserveSeats(String clientName, List<Integer> seatNumbers, Trip trip, Employee employee) throws ServicesException;
    public Trip getTrip(String destination, LocalDate date, LocalTime time) throws ServicesException;
    void subscribeToUpdates(int employeeId, IObserver observer);

}
