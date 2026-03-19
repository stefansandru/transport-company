package ro.transportcompany;

import io.grpc.ManagedChannel;
import io.grpc.stub.StreamObserver;
import ro.transportcompany.proto.*;
import ro.transportcompany.utils.DTOUtils;

import java.time.LocalDate;
import java.time.LocalTime;
import java.util.ArrayList;
import java.util.List;

/**
 * Thin wrapper around the gRPC stubs generated from <code>transport-company.proto</code>.
 * The proxy hides wire-level details from the JavaFX controllers and converts between
 * DTOs and plain domain objects.
 */
public class GrpcServicesProxy implements IServices {
    /**
     * Thin wrapper around the gRPC stubs generated from <code>transport-company.proto</code>.
     * The proxy hides wire-level details from the JavaFX controllers and converts between
     * DTOs and plain domain objects.
     */
    private final ManagedChannel channel;
    private final TransportCompanyGrpc.TransportCompanyBlockingStub grpcStub;
    private final TransportCompanyGrpc.TransportCompanyStub asyncStub;

    private StreamObserver<NotifySeatsReservedReply> notifyStream;

    public GrpcServicesProxy(TransportCompanyGrpc.TransportCompanyBlockingStub grpcStub) {
        this.grpcStub = grpcStub;
        this.channel   = (io.grpc.ManagedChannel) grpcStub.getChannel();
        this.asyncStub = TransportCompanyGrpc.newStub(channel);
    }

    /* ======================== AUTH ======================== */

    @Override
    public Employee login(String username, String password, IObserver client)
            throws ServicesException {

        LoginReply reply = grpcStub.login(LoginRequest.newBuilder()
                                                     .setUsername(username)
                                                     .setPassword(password)
                                                     .build());

        if (reply.getEmployeeId() == -1) {
            throw new ServicesException("Incorrect username");
        } else if (reply.getEmployeeId() == -2) {
            throw new ServicesException("Incorrect password");
        } else if (reply.getEmployeeId() == -3) {
            throw new ServicesException("User already logged in");
        } else if (reply.getEmployeeId() < 0) {
            throw new ServicesException("Login failed");
        }

        subscribeToUpdates(reply.getEmployeeId(), client);

        return new Employee(reply.getEmployeeId(),
                            reply.getUsername(),
                            "",
                            new Office());
    }

    @Override
    public void logout(Employee employee) throws ServicesException {
        try {
            grpcStub.logout(LogoutRequest.newBuilder()
                                         .setEmployeeId(employee.getId())
                                         .build());
        } finally {
            if (notifyStream != null) {
                notifyStream.onCompleted();
                notifyStream = null;
            }
            channel.shutdownNow();
        }
    }

    /* ======================== CRUD ======================== */

    @Override
    public List<Trip> getAllTrips() throws ServicesException {
        try {
            TripsReply response = grpcStub.getAllTrips(AllTripsRequest.newBuilder().build());
            List<Trip> trips = new ArrayList<>();
            for (TripDTO tripDTO : response.getTripsList()) {
                trips.add(DTOUtils.convertTripDTOToTrip(tripDTO));
            }
            return trips;
        } catch (Exception e) {
            throw new ServicesException("Error retrieving trips: " + e.getMessage(), e);
        }
    }

    @Override
    public Trip getTrip(String destination, LocalDate date, LocalTime time)
            throws ServicesException {
        try {
            GetTripReply response = grpcStub.getTrip(
                    GetTripRequest.newBuilder()
                                  .setDestination(destination)
                                  .setDate(date.toString())
                                  .setTime(time.toString())
                                  .build());

            if (!response.hasTrip())
                throw new ServicesException("Trip not found");

            return DTOUtils.convertTripDTOToTrip(response.getTrip());
        } catch (Exception e) {
            throw new ServicesException("Error getting trip: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Seat> searchTripSeats(String destination, LocalDate date, LocalTime time)
            throws ServicesException {
        try {
            SearchTripSeatsReply response = grpcStub.searchTripSeats(
                    SearchTripSeatsRequest.newBuilder()
                                           .setDestination(destination)
                                           .setDate(date.toString())
                                           .setTime(time.toString())
                                           .build());

            List<Seat> seats = new ArrayList<>();
            for (SeatDTO dto : response.getSeatsList()) {
                seats.add(new Seat(dto.getSeatNumber(), dto.getClientName()));
            }
            return seats;
        } catch (Exception e) {
            throw new ServicesException("Error searching trip seats: " + e.getMessage(), e);
        }
    }

    @Override
    public void reserveSeats(String clientName, List<Integer> seatNumbers,
                             Trip trip, Employee employee) throws ServicesException {
        try {
            ReserveSeatsReply reply = grpcStub.reserveSeats(
                    ReserveSeatsRequest.newBuilder()
                                       .setClientName(clientName)
                                       .addAllSeatNumbers(seatNumbers)
                                       .setTrip(DTOUtils.convertTripToTripDTO(trip))
                                       .setEmployeeId(employee.getId())
                                       .build());

            if (!reply.getSuccess())
                throw new ServicesException("Reservation failed: " + reply.getMessage());
        } catch (io.grpc.StatusRuntimeException e) {
            String detail = e.getStatus().getDescription();
            if (detail == null || detail.isEmpty()) detail = e.getMessage();
            throw new ServicesException(detail, e);
        } catch (Exception e) {
            throw new ServicesException("Error reserving seats: " + e.getMessage(), e);
        }
    }

    /* ======================== NOTIFICĂRI ======================== */

    @Override
    public void subscribeToUpdates(int employeeId, IObserver observer) {

        NotifySeatsReservedRequest request = NotifySeatsReservedRequest
                .newBuilder()
                .setEmployeeId(employeeId)
                .build();

        notifyStream = new StreamObserver<NotifySeatsReservedReply>() {
            @Override
            public void onNext(NotifySeatsReservedReply value) {
                try {
                    observer.seatsReserved();
                } catch (ServicesException e) {
                }
            }
            @Override public void onError(Throwable t)  { /* Log error if needed */ }
            @Override public void onCompleted()         { }
        };

        asyncStub.notifySeatsReserved(request, notifyStream);
    }}