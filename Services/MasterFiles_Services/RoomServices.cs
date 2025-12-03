using Microsoft.Data.SqlClient;
using System.Data;
using OIT_Reservation.Models;

namespace OIT_Reservation.Services
{
    public class RoomService
    {
        private readonly string _connectionString;

        public RoomService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Save or Update Room
        public string Save(Room room, bool isNew)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_reservation_room_save", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    cmd.Parameters.AddWithValue("@RoomTypeCode", room.RoomTypeCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoomCode", room.RoomCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", room.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoomSize", room.RoomSize);
                    cmd.Parameters.AddWithValue("@RoomStatus", room.RoomStatus ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remark", room.Remarks ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsRoom", room.IsRoom);
                    cmd.Parameters.AddWithValue("@IsBanquet", room.IsBanquet);
                    cmd.Parameters.AddWithValue("@IsActive", true);
                    cmd.Parameters.AddWithValue("@IsNew", isNew);


                    // Output parameter
                    var outputRoomCode = new SqlParameter("@RoomCodeRet", SqlDbType.VarChar, 20)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputRoomCode);

                    // Execute
                    cmd.ExecuteNonQuery();

                    // Return the generated/updated RoomCode
                    return outputRoomCode.Value?.ToString() ?? "";
                }
            }
        }

        public List<Room> GetAll()
        {
            var rooms = new List<Room>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Reservation_Room", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rooms.Add(new Room
                            {
                                RoomID = (int)reader.GetInt64(reader.GetOrdinal("RoomID")),
                                RoomTypeCode = reader["RoomTypeCode"]?.ToString(),
                                RoomCode = reader["RoomCode"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                RoomSize = reader["RoomSize"] as int? ?? 0,
                                RoomStatus = reader["RoomStatus"]?.ToString(),
                                Remarks = reader["Remarks"]?.ToString(),
                                IsRoom = reader["IsRoom"] as bool? ?? false,
                                IsBanquet = reader["IsBanquet"] as bool? ?? false
                            });
                        }
                    }
                }
            }

            return rooms;
        }
        
        public string GetNextRoomCode(bool isRoom, bool isBanquet)
        {
            string nextCode = string.Empty;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_reservation_room_getnext", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IsRoom", isRoom);
                    cmd.Parameters.AddWithValue("@IsBanquet", isBanquet);

                    // Output parameter
                    SqlParameter outputParam = new SqlParameter("@NextRoomCode", SqlDbType.VarChar, 20)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    cmd.ExecuteNonQuery();

                    nextCode = outputParam.Value?.ToString();
                }
            }

            return nextCode;
        }
        public bool Delete(string roomCode)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand(
                "DELETE FROM Reservation_Room WHERE RoomCode = @RoomCode",
                conn
            );

            cmd.Parameters.AddWithValue("@RoomCode", roomCode);
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}
