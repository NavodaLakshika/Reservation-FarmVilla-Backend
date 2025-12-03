using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class TravelAgentService
    {
        private readonly string _conn;

        public TravelAgentService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public string GetNextTravelAgentCode()
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetNextTravelAgentCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return reader["travelAgentCode"].ToString();
            }

            return null;
        }

        public List<TravelAgent> GetAll()
        {
            var list = new List<TravelAgent>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllTravelAgents", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new TravelAgent
                {
                    TravelAgentID = Convert.ToInt32(reader["TravelAgentID"]),
                    TravelAgentCode = reader["TravelAgentCode"].ToString(),
                    Description = reader["Description"].ToString(),
                });
            }

            return list;
        }

        public bool Create(TravelAgent travelAgent)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_InsertTravelAgent", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Output param for generated code
                var travelAgentCodeParam = new SqlParameter("@TravelAgentCode", SqlDbType.NVarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(travelAgentCodeParam);
                cmd.Parameters.AddWithValue("@Description", travelAgent.Description);

                conn.Open();
                cmd.ExecuteNonQuery();

                // Set the generated RoomTypeCode to return or log
                travelAgent.TravelAgentCode = travelAgentCodeParam.Value.ToString();

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000 && ex.Message.Contains("Travle Agent Code already exists"))
                    throw new ApplicationException("Travle Agent Code already exists.");

                throw new ApplicationException("Database error: " + ex.Message);
            }
        }

        public bool Update(TravelAgent travelAgent)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_UpdateTravelAgent", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TravelAgentID", travelAgent.TravelAgentID);
            cmd.Parameters.AddWithValue("@Description", travelAgent.Description);

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
        
        public bool Delete(long TravelAgentID)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = new SqlCommand(
                "DELETE FROM Reservation_TravelAgent WHERE TravelAgentID = @TravelAgentID",
                conn
                );

            cmd.Parameters.AddWithValue("@TravelAgentID", TravelAgentID);
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}