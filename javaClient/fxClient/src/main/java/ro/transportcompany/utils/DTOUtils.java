package ro.transportcompany.utils;

import ro.transportcompany.Destination;
import ro.transportcompany.Trip;
import ro.transportcompany.proto.TripDTO;

import java.time.LocalDate;
import java.time.LocalTime;

public class DTOUtils {

    public static Trip convertTripDTOToTrip(TripDTO tripDTO) {
        Destination destination = new Destination(null, tripDTO.getDestination());
        LocalDate date = LocalDate.parse(tripDTO.getDate());
        LocalTime time = LocalTime.parse(tripDTO.getTime());
        return new Trip(
                tripDTO.getId(),
                destination,
                date,
                time,
                tripDTO.getAvailableSeats()
        );
    }

    public static TripDTO convertTripToTripDTO(Trip trip) {
        return TripDTO.newBuilder()
                .setId(trip.getId())
                .setDestination(trip.getDestination().getName())
                .setDate(trip.getDepartureDate().toString())
                .setTime(trip.getDepartureTime().toString())
                .setAvailableSeats(trip.getAvailableSeats())
                .build();
    }

}