using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using model;

namespace persistance
{
    public class ReservedSeatRepository : AbstractRepository<int, ReservedSeat>, IReservedSeatRepository
    {
        private readonly ITripRepository tripRepository;
        private readonly IEmployeeRepository employeeRepository;
        private readonly IClientRepository clientRepository;
        private readonly ILogger<ReservedSeatRepository> _logger;

        public ReservedSeatRepository(ITripRepository tripRepository, IEmployeeRepository employeeRepository, IClientRepository clientRepository, ILogger<ReservedSeatRepository> logger, DatabaseConnection jdbc) : base(jdbc)
        {
            this.tripRepository = tripRepository;
            this.employeeRepository = employeeRepository;
            this.clientRepository = clientRepository;
            _logger = logger;
        }
        
        public override ReservedSeat? FindById(int id)
        {
            throw new NotSupportedException("FindById is not supported for ReservedSeatRepository.");
        }

        public override IEnumerable<ReservedSeat> FindAll()
        {
            throw new NotSupportedException("FindAll is not supported for ReservedSeatRepository.");
        }
        
        public override ReservedSeat? Save(ReservedSeat reservedSeat)
        {
            const string query = "INSERT INTO ReservedSeats (trip_id, employee_id, seat_number, client_id) VALUES (@trip_id, @employee_id, @seat_number, @client_id)";
            try
            {
                using (var connection = jdbc.GetConnection())
                {
                    connection.Open();
                    using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                    {
                        command.Parameters.AddWithValue("@trip_id", reservedSeat.Trip.Id);
                        command.Parameters.AddWithValue("@employee_id", reservedSeat.Employee.Id);
                        command.Parameters.AddWithValue("@seat_number", reservedSeat.SeatNumber);
                        command.Parameters.AddWithValue("@client_id", reservedSeat.Client.Id);
                        command.ExecuteNonQuery();
                    }
                    using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", (SqliteConnection)connection))
                    {
                        var id = Convert.ToInt32(idCommand.ExecuteScalar());
                        reservedSeat.Id = id;
                        return reservedSeat;
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while saving ReservedSeat: {ReservedSeat}", reservedSeat);
            }
            return null;
        }

        public override ReservedSeat? Delete(int id)
        {
            const string query = @"DELETE FROM ReservedSeats WHERE id = @id RETURNING id, seat_number, 
               trip_id, departure_date, departure_time, available_seats, 
               destination_id, destination_name,
               employee_id, employee_username, employee_password,
               client_id, client_name";
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
                            return ReadReservedSeatWithAll(reader);
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while deleting ReservedSeat with ID {Id}", id);
            }
            return null;
        }

        public override ReservedSeat? Update(ReservedSeat reservedSeat)
        {
            const string query = "UPDATE ReservedSeats SET trip_id = @trip_id, employee_id = @employee_id, seat_number = @seat_number, client_id = @client_id WHERE id = @id";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@trip_id", reservedSeat.Trip.Id);
                    command.Parameters.AddWithValue("@employee_id", reservedSeat.Employee.Id);
                    command.Parameters.AddWithValue("@seat_number", reservedSeat.SeatNumber);
                    command.Parameters.AddWithValue("@client_id", reservedSeat.Client.Id);
                    command.Parameters.AddWithValue("@id", reservedSeat.Id);
                    connection.Open();
                    var affectedRows = command.ExecuteNonQuery();
                    if (affectedRows > 0)
                    {
                        return reservedSeat;
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while updating ReservedSeat: {ReservedSeat}", reservedSeat);
            }
            return null;
        }

        public List<ReservedSeat> FindByTripDestinationDateTime(string destination, string date, string time)
        {
            var reservedSeats = new List<ReservedSeat>();
            const string query = @"
        SELECT rs.id, rs.seat_number, 
               t.id as trip_id, t.departure_date, t.departure_time, t.available_seats, 
               d.id as destination_id, d.name as destination_name,
               e.id as employee_id, e.username as employee_name, e.username as employee_username, e.password as employee_password,
               c.id as client_id, c.name as client_name
        FROM ReservedSeats rs
        JOIN Trip t ON rs.trip_id = t.id
        JOIN Destination d ON t.destination_id = d.id
        LEFT JOIN Employee e ON rs.employee_id = e.id
        LEFT JOIN Client c ON rs.client_id = c.id
        WHERE d.name = @destination AND t.departure_date = @date AND t.departure_time = @time";
            try
            {
                using (var connection = jdbc.GetConnection())
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    command.Parameters.AddWithValue("@destination", destination);
                    command.Parameters.AddWithValue("@date", date);
                    command.Parameters.AddWithValue("@time", time);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var reservedSeat = ReadReservedSeatWithAll(reader);
                            if (reservedSeat != null)
                            {
                                reservedSeats.Add(reservedSeat);
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Database error while finding ReservedSeat by trip destination, date and time: {Destination}: {Date}, {Time}", destination, date, time);
            }
            return reservedSeats;
        }

        public bool AreSeatsAvailable(int tripId, IEnumerable<int> seatNumbers)
        {
            const string query = "SELECT seat_number FROM ReservedSeats WHERE trip_id = @trip_id AND seat_number IN ({0})";
            var seatNumberList = seatNumbers.ToList();
            if (!seatNumberList.Any()) return true;
            var inClause = string.Join(",", seatNumberList.Select((s, i) => "@seat" + i));
            var finalQuery = string.Format(query, inClause);
            using (var connection = jdbc.GetConnection())
            using (var command = new SqliteCommand(finalQuery, (SqliteConnection)connection))
            {
                command.Parameters.AddWithValue("@trip_id", tripId);
                for (int i = 0; i < seatNumberList.Count; i++)
                {
                    command.Parameters.AddWithValue("@seat" + i, seatNumberList[i]);
                }
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    return !reader.Read();
                }
            }
        }

        private ReservedSeat? ReadReservedSeatWithAll(SqliteDataReader reader)
        {
            try
            {
                var id = reader.GetInt32(reader.GetOrdinal("id"));
                var seatNumber = reader.GetInt32(reader.GetOrdinal("seat_number"));
                var tripId = reader.GetInt32(reader.GetOrdinal("trip_id"));
                var departureDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("departure_date")));
                var departureTime = TimeOnly.FromTimeSpan(reader.GetDateTime(reader.GetOrdinal("departure_time")).TimeOfDay);
                var availableSeats = reader.GetInt32(reader.GetOrdinal("available_seats"));
                var destinationId = reader.GetInt32(reader.GetOrdinal("destination_id"));
                var destinationName = reader.GetString(reader.GetOrdinal("destination_name"));
                var destination = new Destination(destinationId, destinationName);
                var trip = new Trip(tripId, destination, departureDate, departureTime, availableSeats);
                Employee? employee = null;
                if (!reader.IsDBNull(reader.GetOrdinal("employee_id")))
                {
                    var employeeId = reader.GetInt32(reader.GetOrdinal("employee_id"));
                    var employeeUsername = reader.IsDBNull(reader.GetOrdinal("employee_username")) ? null : reader.GetString(reader.GetOrdinal("employee_username"));
                    var employeePassword = reader.IsDBNull(reader.GetOrdinal("employee_password")) ? null : reader.GetString(reader.GetOrdinal("employee_password"));
                    if (employeeUsername != null && employeePassword != null)
                        employee = new Employee(employeeId, employeeUsername, employeePassword, null);
                }
                Client? client = null;
                if (!reader.IsDBNull(reader.GetOrdinal("client_id")))
                {
                    var clientId = reader.GetInt32(reader.GetOrdinal("client_id"));
                    var clientName = reader.IsDBNull(reader.GetOrdinal("client_name")) ? null : reader.GetString(reader.GetOrdinal("client_name"));
                    if (clientName != null)
                        client = new Client(clientId, clientName);
                }
                return new ReservedSeat
                {
                    Id = id,
                    Trip = trip,
                    Employee = employee,
                    Client = client,
                    SeatNumber = seatNumber
                };
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Error while extracting ReservedSeat from ResultSet");
                return null;
            }
        }
    }
}