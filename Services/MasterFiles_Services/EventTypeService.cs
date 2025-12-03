using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class EventTypeService
    {
        private readonly string _conn;

        public EventTypeService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }
        public string GetNextEventCode()
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetNextEventTypeCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["EventCode"].ToString();
            }
            return null;
        }


        public List<EventType> GetAll()
        {
            var list = new List<EventType>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllEventTypes", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new EventType
                {
                    EventTypeID = Convert.ToInt32(reader["EventTypeID"]),
                    EventCode = reader["EventCode"].ToString(),
                    Description = reader["Description"].ToString(),
                    Remarks = reader["Remarks"].ToString(),
                });
            }
            return list;
        }

        public bool Create(EventType eventType)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_InsertEventType", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Output param for generated code
                var eventCodeParam = new SqlParameter("@EventCode", SqlDbType.NVarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(eventCodeParam);
                cmd.Parameters.AddWithValue("@Description", eventType.Description);
                cmd.Parameters.AddWithValue("@Remarks", eventType.Remarks ?? (object)DBNull.Value);
                

                conn.Open();
                cmd.ExecuteNonQuery();

                // Set the generated EventTypeCode to return or log
                eventType.EventCode = eventCodeParam.Value.ToString();

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000 && ex.Message.Contains("Event Type Code already exists"))
                    throw new ApplicationException("Event Type Code already exists.");

                throw new ApplicationException("Database error: " + ex.Message);
            }
        }

        public bool Update(EventType eventType)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_UpdateEventType", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@EventTypeID", eventType.EventTypeID);
            cmd.Parameters.AddWithValue("@Description", eventType.Description);
            cmd.Parameters.AddWithValue("@Remarks", eventType.Remarks ?? (object)DBNull.Value);

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

        internal object Save(Customer customer)
        {
            throw new NotImplementedException();
        }
        
        public bool Delete(string eventTypeId)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();

            using var cmd = new SqlCommand(
                "DELETE FROM Reservation_EventType WHERE EventTypeId = @EventTypeId",
                conn
                );
            cmd.Parameters.AddWithValue("@EventTypeId", eventTypeId);
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}