using System.Collections.Concurrent;
using Grpc.Core;
using TransportCompany.GrpcServer;
using model;
using persistance;
using Microsoft.Extensions.Logging;

namespace grpcServer;

/// <summary>
/// gRPC service that glues the desktop client to the database. Handles login, trip search
/// and seat reservation logic while broadcasting updates to all connected employees.
/// </summary>
public class TransportCompanyService : TransportCompany.GrpcServer.TransportCompany.TransportCompanyBase
{
    private readonly IClientRepository _clientRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITripRepository _tripRepository;
    private readonly IReservedSeatRepository _reservedSeatRepository;
    private readonly ILogger<TransportCompanyService> _logger;

    private static readonly ConcurrentDictionary<int, bool> LoggedInEmployees = new();

    private static readonly ConcurrentDictionary<int, IServerStreamWriter<NotifySeatsReservedReply>> EmployeeStreams = new();

    /// <summary>
    /// Creates the service instance and wires in all required dependencies.
    /// </summary>
    /// <param name="clientRepository">Data-access layer for <see cref="Client"/> records.</param>
    /// <param name="employeeRepository">Data-access layer for <see cref="Employee"/> records.</param>
    /// <param name="tripRepository">Data-access layer for <see cref="Trip"/> records.</param>
    /// <param name="reservedSeatRepository">Data-access layer for <see cref="ReservedSeat"/> records.</param>
    /// <param name="logger">Structured logger injected by ASP-NET host.</param>
    public TransportCompanyService(
        IClientRepository clientRepository,
        IEmployeeRepository employeeRepository, 
        ITripRepository tripRepository,
        IReservedSeatRepository reservedSeatRepository,
        ILogger<TransportCompanyService> logger)
    {
        _clientRepository = clientRepository;
        _employeeRepository = employeeRepository;
        _tripRepository = tripRepository;
        _reservedSeatRepository = reservedSeatRepository;
        _logger = logger;
    }  

    /// <summary>
    /// Authenticates an employee using the provided credentials.
    /// </summary>
    /// <remarks>
    /// Magic return codes used by the legacy Java client:
    /// <list type="bullet">
    ///   <item><description><c>-1</c>: employee not found</description></item>
    ///   <item><description><c>-2</c>: wrong password</description></item>
    ///   <item><description><c>-3</c>: already logged in from another machine</description></item>
    /// </list>
    /// </remarks>
    /// <returns>A populated <see cref="LoginReply"/>.</returns>
    public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
    {
        var employee = _employeeRepository.FindByUsername(request.Username);
        
        if (employee == null)
        {
            _logger.LogWarning("Employee not found!");
            return Task.FromResult(new LoginReply { EmployeeId = -1, Username = "INVALID" });
        }

        if (LoggedInEmployees.ContainsKey(employee.Id))
        {
            _logger.LogWarning("Employee already logged in!");
            return Task.FromResult(new LoginReply { EmployeeId = -3, Username = "ALREADY_LOGGED" });
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, employee.Password))
        {
            _logger.LogWarning("Invalid password!");
            return Task.FromResult(new LoginReply { EmployeeId = -2, Username = "INVALID" });
        }
        
        LoggedInEmployees[employee.Id] = true;
        return Task.FromResult(new LoginReply
        {
            EmployeeId = employee.Id,
            Username = employee.Username,
        });
    }

    /// <summary>
    /// Removes the employee from the in-memory session pool and shuts down its notification stream.
    /// </summary>
    public override Task<LogoutReply> Logout(LogoutRequest request, ServerCallContext context)
    {
        LoggedInEmployees.TryRemove(request.EmployeeId, out _);
        EmployeeStreams.TryRemove(request.EmployeeId, out _);
        return Task.FromResult(new LogoutReply
        {
            Success = true,
            Message = "Logged out successfully"
        });
    }

    /// <summary>
    /// Retrieves a lightweight projection of every trip in the database.
    /// </summary>
    public override Task<TripsReply> GetAllTrips(AllTripsRequest request, ServerCallContext context)
    {
        var trips = _tripRepository.FindAll().ToList();
        var reply = new TripsReply();
        foreach (var trip in trips)
        {
            var protoTrip = new TransportCompany.GrpcServer.TripDTO
            {
                Id = trip.Id,
                Destination = trip.Destination.Name,
                Date = trip.DepartureDate.ToString("yyyy-MM-dd"),
                Time = trip.DepartureTime.ToString(@"HH\:mm"),
                AvailableSeats = trip.AvailableSeats ?? 0
            };
            reply.Trips.Add(protoTrip);
        }
        return Task.FromResult(reply);
    }

    /// <summary>
    /// Gets details for a single trip identified by destination, date and time.
    /// </summary>
    public override Task<GetTripReply> GetTrip(GetTripRequest request, ServerCallContext context)
    {
        if (!DateOnly.TryParse(request.Date, out var date))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid date format"));
        if (!TimeOnly.TryParse(request.Time, out var time))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid time format"));

        var trip = _tripRepository.FindByDestinationAndDateAndTime(request.Destination, request.Date, request.Time);
        if (trip == null)
            return Task.FromResult(new GetTripReply());

        var tripDto = new TripDTO
        {
            Id = trip.Id,
            Destination = trip.Destination.Name,
            Date = trip.DepartureDate.ToString("yyyy-MM-dd"),
            Time = trip.DepartureTime.ToString(@"HH\:mm"),
            AvailableSeats = trip.AvailableSeats ?? 0
        };

        return Task.FromResult(new GetTripReply { Trip = tripDto });
    }

    /// <summary>
    /// Returns the full seat map (18 seats) for the given trip so the UI can mark them as occupied.
    /// </summary>
    public override Task<SearchTripSeatsReply> SearchTripSeats(SearchTripSeatsRequest request, ServerCallContext context)
    {
        if (!DateOnly.TryParse(request.Date, out var date))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid date format"));
        if (!TimeOnly.TryParse(request.Time, out var time))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid time format"));

        if (_reservedSeatRepository == null)
            throw new NotSupportedException("ReservedSeatRepository not available");

        var reservedSeats = _reservedSeatRepository.FindByTripDestinationDateTime(
            request.Destination, request.Date, request.Time);

        var reply = new SearchTripSeatsReply();

        for (int seat = 1; seat <= 18; seat++)
        {
            string clientName = reservedSeats.FirstOrDefault(s => s.SeatNumber == seat)?.Client?.Name ?? "-";
            reply.Seats.Add(new SeatDTO
            {
                SeatNumber = seat,
                ClientName = clientName
            });
        }

        return Task.FromResult(reply);
    }

    /// <summary>
    /// Persists a batch of seat reservations and notifies all other online employees via server-streaming.
    /// </summary>
    public override async Task<ReserveSeatsReply> ReserveSeats(ReserveSeatsRequest request, ServerCallContext context)
    {
        var client = _clientRepository.FindByName(request.ClientName);
        if (client == null)
        {
            client = new Client { Name = request.ClientName };
            client = _clientRepository.Save(client);
        }

        var trip = _tripRepository.FindById(request.Trip.Id);
        if (trip == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Trip not found"));

        var employee = _employeeRepository.FindById(request.EmployeeId);
        if (employee == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Employee not found"));
        
        if (!_reservedSeatRepository.AreSeatsAvailable(trip.Id, request.SeatNumbers))
        {
            throw new RpcException(
                new Status(
                    StatusCode.FailedPrecondition,
                    "Some seats are already reserved."
                ),
                "Some seats are already reserved.");
        }

        foreach (var seatNumber in request.SeatNumbers)
        {
            var reservedSeat = new ReservedSeat
            {
                Trip = trip,
                Employee = employee,
                SeatNumber = seatNumber,
                Client = client!
            };
            _reservedSeatRepository.Save(reservedSeat);
        }

        trip.AvailableSeats = (trip.AvailableSeats ?? 0) - request.SeatNumbers.Count;

        foreach (var entry in EmployeeStreams)
        {
            if (entry.Key != employee.Id)
            {
                try
                {
                    await entry.Value.WriteAsync(new NotifySeatsReservedReply());
                }
                catch
                {
                    EmployeeStreams.TryRemove(entry.Key, out _);
                }
            }
        }

        var reply = new ReserveSeatsReply
        {
            Success = true,
            Message = $"Reserved {request.SeatNumbers.Count} seat(s) for {client!.Name} on trip {trip.Id}"
        };

        return reply;
    }

    /// <summary>
    /// Long-lived streaming RPC – every employee keeps this open to receive <see cref="NotifySeatsReservedReply"/> events.
    /// </summary>
    public override async Task NotifySeatsReserved(NotifySeatsReservedRequest request, IServerStreamWriter<NotifySeatsReservedReply> responseStream, ServerCallContext context)
    {
        EmployeeStreams[request.EmployeeId] = responseStream; 
        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }
        finally
        {
            EmployeeStreams.TryRemove(request.EmployeeId, out _);
        }
    }
}