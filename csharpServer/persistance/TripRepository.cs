using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using model;

namespace persistance
{
    public class TripRepository : AbstractRepository<int, Trip>, ITripRepository
    {
        private readonly ILogger<TripRepository> _logger;
        public TripRepository(ILogger<TripRepository> logger, DatabaseConnection jdbc) : base(jdbc)
        {
            _logger = logger;
        }

        public override Trip? FindById(int id)
        {
            const string query = @"SELECT t.id, t.departure_date, t.departure_time, 
                t.available_seats - IFNULL((SELECT COUNT(*) FROM ReservedSeats rs WHERE rs.trip_id = t.id), 0) AS available_seats, 
                d.id as destination_id, d.name as destination_name
                FROM Trip t JOIN Destination d ON t.destination_id = d.id WHERE t.id = @id";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var trip = ReadTripWithDestination(reader);
                            return trip;
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while finding Trip with ID {Id}", id);
            }
            return null;
        }

        public override IEnumerable<Trip> FindAll()
        {
            var trips = new List<Trip>();
            const string query = @"SELECT t.id, t.departure_date, t.departure_time, 
                t.available_seats - IFNULL((SELECT COUNT(*) FROM ReservedSeats rs WHERE rs.trip_id = t.id), 0) AS available_seats, 
                d.id as destination_id, d.name as destination_name
                FROM Trip t JOIN Destination d ON t.destination_id = d.id";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var trip = ReadTripWithDestination(reader);
                            if (trip != null)
                            {
                                trips.Add(trip);
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while finding all Trips");
            }
            return trips;
        }

        public IEnumerable<Trip> FindByDestinationId(int destinationId)
        {
            var trips = new List<Trip>();
            const string query = @"SELECT t.id, t.departure_date, t.departure_time, 
                t.available_seats - IFNULL((SELECT COUNT(*) FROM ReservedSeats rs WHERE rs.trip_id = t.id), 0) AS available_seats, 
                d.id as destination_id, d.name as destination_name
                FROM Trip t JOIN Destination d ON t.destination_id = d.id WHERE t.destination_id = @destination_id";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@destination_id", destinationId);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var trip = ReadTripWithDestination(reader);
                            if (trip != null)
                            {
                                trips.Add(trip);
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while finding Trips for destination ID {DestinationId}", destinationId);
            }
            return trips;
        }

        public IEnumerable<Trip> FindByDepartureDate(DateTime departureDate)
        {
            var trips = new List<Trip>();
            const string query = @"SELECT t.id, t.departure_date, t.departure_time, 
                t.available_seats - IFNULL((SELECT COUNT(*) FROM ReservedSeats rs WHERE rs.trip_id = t.id), 0) AS available_seats, 
                d.id as destination_id, d.name as destination_name
                FROM Trip t JOIN Destination d ON t.destination_id = d.id WHERE t.departure_date = @departure_date";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@departure_date", departureDate);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var trip = ReadTripWithDestination(reader);
                            if (trip != null)
                            {
                                trips.Add(trip);
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while finding Trips for departure date {DepartureDate}", departureDate);
            }
            return trips;
        }

        public override Trip? Save(Trip trip)
        {
            const string insertQuery = "INSERT INTO Trip (destination_id, departure_date, departure_time, available_seats) VALUES (@destination_id, @departure_date, @departure_time, @available_seats)";
            try
            {
                using (var connection = jdbc.GetConnection())
                {
                    connection.Open();
                    using (var command = new SqliteCommand(insertQuery, (SqliteConnection)connection))
                    {
                        command.Parameters.AddWithValue("@destination_id", trip.Destination.Id);
                        command.Parameters.AddWithValue("@departure_date", trip.DepartureDate);
                        command.Parameters.AddWithValue("@departure_time", trip.DepartureTime);
                        command.Parameters.AddWithValue("@available_seats", trip.AvailableSeats);
                        command.ExecuteNonQuery();
                    }

                    using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", (SqliteConnection)connection))
                    {
                        var id = Convert.ToInt32(idCommand.ExecuteScalar());
                        trip.Id = id;
                        return trip;
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while saving Trip: {Trip}", trip);
            }

            return null;
        }

        public override Trip? Delete(int id)
        {
            var tripToDelete = FindById(id);
            if (tripToDelete == null)
            {
                return null;
            }

            const string query = "DELETE FROM Trip WHERE id = @id";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var affectedRows = command.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        return tripToDelete;
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while deleting Trip with ID {Id}", id);
            }

            return null;
        }

        public override Trip? Update(Trip trip)
        {
            const string query = "UPDATE Trip SET destination_id = @destination_id, departure_date = @departure_date, departure_time = @departure_time, available_seats = @available_seats WHERE id = @id";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@destination_id", trip.Destination.Id);
                    command.Parameters.AddWithValue("@departure_date", trip.DepartureDate);
                    command.Parameters.AddWithValue("@departure_time", trip.DepartureTime.ToString("HH:mm"));
                    command.Parameters.AddWithValue("@available_seats", trip.AvailableSeats);
                    command.Parameters.AddWithValue("@id", trip.Id);
                    connection.Open();
                    var affectedRows = command.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        return trip;
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while updating Trip: {Trip}", trip);
            }

            return null;
        }

        public IEnumerable<Trip> FindAllByName(string name)
        {
            var trips = new List<Trip>();
            const string query = @"SELECT t.id, t.departure_date, t.departure_time, 
                t.available_seats - IFNULL((SELECT COUNT(*) FROM ReservedSeats rs WHERE rs.trip_id = t.id), 0) AS available_seats, 
                d.id as destination_id, d.name as destination_name
                FROM Trip t JOIN Destination d ON t.destination_id = d.id WHERE d.name = @name";

            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var trip = ReadTripWithDestination(reader);
                            if (trip != null)
                            {
                                trips.Add(trip);
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while finding all Trips by name {Name}", name);
            }

            return trips;
        }

        public Trip FindByDestinationAndDateAndTime(string destination, string dateString, string timeString)
        {
            const string query = @"
        SELECT t.id, t.departure_date, t.departure_time, t.available_seats - IFNULL((SELECT COUNT(*) FROM ReservedSeats rs WHERE rs.trip_id = t.id), 0) AS available_seats, 
        d.id as destination_id, d.name as destination_name
        FROM Trip t
        JOIN Destination d ON t.destination_id = d.id
        WHERE d.name = @destination AND t.departure_date = @date AND t.departure_time = @time";

            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@destination", destination);
                    command.Parameters.AddWithValue("@date", dateString);
                    command.Parameters.AddWithValue("@time", timeString);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var trip = ReadTripWithDestination(reader);
                            if (trip != null)
                            {
                                return trip;
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while finding Trip by destination, date, and time: {Destination}, {Date}, {Time}", destination, dateString, timeString);
            }

            return null;
        }

        private Trip? ReadTripWithDestination(SqliteDataReader reader)
        {
            try
            {
                var id = reader.GetInt32(reader.GetOrdinal("id"));
                var destinationId = reader.GetInt32(reader.GetOrdinal("destination_id"));
                var destinationName = reader.GetString(reader.GetOrdinal("destination_name"));
                var departureDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("departure_date")));
                var departureTime = TimeOnly.FromTimeSpan(reader.GetDateTime(reader.GetOrdinal("departure_time")).TimeOfDay);
                var availableSeats = reader.GetInt32(reader.GetOrdinal("available_seats"));
                var destination = new Destination(destinationId, destinationName);
                return new Trip(id, destination, departureDate, departureTime, availableSeats);
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Error while extracting Trip from ResultSet");
                return null;
            }
        }
    }
}