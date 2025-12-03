using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class setupStyleService
    {
        private readonly string _conn;

        public setupStyleService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }


        public string GetNextSetupStyleCode()
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetNextSetupStyleCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["setupStyleCode"].ToString();
            }

            return null;
        }

        public List<SetupStyle> GetAll()
        {
            var list = new List<SetupStyle>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetAllSetupStyleTypes", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new SetupStyle
                {
                    SetupStyleTypeID = Convert.ToInt32(reader["SetupStyleTypeID"]),
                    SetupStyleCode = reader["SetupStyleCode"].ToString(),
                    Description = reader["Description"].ToString(),
                    Remarks = reader["Remarks"].ToString(),
                });
            }
            return list;
        }

        public bool Create(SetupStyle setupStyle)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_InsertSetupStyleType", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Output parameter for generated code
                var setupStyleCodeParam = new SqlParameter("@SetupStyleCode", SqlDbType.NVarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(setupStyleCodeParam);

                // Required inputs
                cmd.Parameters.AddWithValue("@Description", setupStyle.Description);
                cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(setupStyle.Remarks) ? (object)DBNull.Value : setupStyle.Remarks);

                conn.Open();
                cmd.ExecuteNonQuery();

                // Set generated code back to model
                setupStyle.SetupStyleCode = setupStyleCodeParam.Value.ToString();

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000 && ex.Message.Contains("Setup Style Type Code already exists"))
                    throw new ApplicationException("Setup Style Type Code already exists.");

                throw new ApplicationException("Database error: " + ex.Message);
            }
        }

        public bool Update(SetupStyle setupStyle)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_UpdateSetupStyleType", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@SetupStyleTypeID", setupStyle.SetupStyleTypeID);
            cmd.Parameters.AddWithValue("@Description", setupStyle.Description);
            cmd.Parameters.AddWithValue("@Remarks", setupStyle.Remarks ?? (object)DBNull.Value);

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

        public bool Delete(long SetupStyleTypeID)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = new SqlCommand(
                "DELETE FROM Reservation_SetupStyleType WHERE SetupStyleTypeID = @SetupStyleTypeID",
                conn
                );

            cmd.Parameters.AddWithValue("@SetupStyleTypeID", SetupStyleTypeID);
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}