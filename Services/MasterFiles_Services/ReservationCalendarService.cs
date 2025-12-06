using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

public interface IReservationCalendarService
{
    Task<List<ReservationCalendarDto>> GetReservationCalendarDataAsync(DateTime start, DateTime end, int calendarType, int? statusId = null);
}

public class ReservationCalendarService : IReservationCalendarService
{
    private readonly string _connectionString;

    public ReservationCalendarService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<ReservationCalendarDto>> GetReservationCalendarDataAsync(DateTime start, DateTime end, int calendarType, int? statusId = null)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@StartDate", start);
        parameters.Add("@EndDate", end);
        parameters.Add("@CalendarType", calendarType);
        parameters.Add("@StatusId", statusId);

        var result = await connection.QueryAsync<ReservationCalendarDto>(
            "sp_GetReservationCalendarData",
            parameters,
            commandType: CommandType.StoredProcedure);

        return result.ToList();
    }
}
