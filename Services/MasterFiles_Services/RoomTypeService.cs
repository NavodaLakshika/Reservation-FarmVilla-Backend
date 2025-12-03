using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class RoomTypeService
    {
        private readonly string _conn;

        public RoomTypeService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }


      public RoomType GetNextCode()
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetNextRoomTypeCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add output parameter
            var outputParam = new SqlParameter("@RoomTypeCode", SqlDbType.NVarChar, 20)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            conn.Open();
            cmd.ExecuteNonQuery(); // Use ExecuteNonQuery since we're using output, not result set

            return new RoomType
            {
                RoomTypeCode = outputParam.Value?.ToString()
            };
        }


        public List<RoomType> GetAll()
        {
            var list = new List<RoomType>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllRoomTypes", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new RoomType
                {
                    RoomTypeID = Convert.ToInt32(reader["RoomTypeID"]),
                    RoomTypeCode = reader["RoomTypeCode"].ToString(),
                    Description = reader["Description"].ToString(),
                    Remarks = reader["Remarks"].ToString(),
                });
            }

            return list;
        }

        public bool Create(RoomType roomType)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_InsertRoomType", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Output param for generated code
                var roomTypeCodeParam = new SqlParameter("@RoomTypeCode", SqlDbType.NVarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(roomTypeCodeParam);
                cmd.Parameters.AddWithValue("@Description", roomType.Description);
                cmd.Parameters.AddWithValue("@Remarks", roomType.Remarks ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();

                // Set the generated RoomTypeCode to return or log
                roomType.RoomTypeCode = roomTypeCodeParam.Value.ToString();

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000 && ex.Message.Contains("Room Type Code already exists"))
                    throw new ApplicationException("Room Type Code already exists.");

                throw new ApplicationException("Database error: " + ex.Message);
            }
        }

        public bool Update(RoomType roomType)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_UpdateRoomType", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@RoomTypeID", roomType.RoomTypeID);
            cmd.Parameters.AddWithValue("@Description", roomType.Description);
            cmd.Parameters.AddWithValue("@Remarks", roomType.Remarks ?? (object)DBNull.Value);

            var existsParam = new SqlParameter("@Exists", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(existsParam);

            conn.Open();
            cmd.ExecuteNonQuery();

            bool exists = existsParam.Value != DBNull.Value && (bool)existsParam.Value;
            return exists;
        }

        public bool Delete(string RoomTypeCode)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();

            using var cmd = new SqlCommand(
                "DELETE FROM Reservation_RoomType WHERE RoomTypeCode = @RoomTypeCode",
                conn
            );

            cmd.Parameters.AddWithValue("@RoomTypeCode",RoomTypeCode);
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}

