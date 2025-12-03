using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class ServiceTypeService
    {
        private readonly string _conn;

        public ServiceTypeService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public string GetNextServiceCode()
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetNextServiceCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["serviceCode"].ToString();
            }

            return null;
        }

        public List<ServiceType> GetAll()
        {
            var list = new List<ServiceType>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_reservation_servicetype_getall", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new ServiceType
                {
                    ServiceTypeID = Convert.ToInt32(reader["ServiceTypeID"]),
                    ServiceCode = reader["ServiceCode"].ToString(),
                    ServiceName = reader["ServiceName"].ToString(),
                    Remarks = reader["Remarks"] != DBNull.Value ? reader["Remarks"].ToString() : null,

                    Quantity = Convert.ToDecimal(reader["Quantity"]),
                    ServiceAmount = Convert.ToDecimal(reader["ServiceAmount"]),

                    IsRoom = Convert.ToBoolean(reader["IsRoom"]),
                    IsBanquet = Convert.ToBoolean(reader["IsBanquet"])
                });
            }
            return list;
        }

        public bool Create(ServiceType serviceType)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_reservation_servicetype_save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ServiceTypeID", 0);

                // ServiceCode is output
                var serviceCodeParam = new SqlParameter("@ServiceCode", SqlDbType.VarChar, 300)
                {
                    Direction = ParameterDirection.Output,
                    Value = string.Empty
                };
                cmd.Parameters.Add(serviceCodeParam);

                // Required parameters matching SP signature
                cmd.Parameters.AddWithValue("@ServiceName", serviceType.ServiceName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Remarks", serviceType.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", serviceType.Quantity);
                cmd.Parameters.AddWithValue("@ServiceAmount", serviceType.ServiceAmount);
                cmd.Parameters.AddWithValue("@IsRoom", serviceType.IsRoom);
                cmd.Parameters.AddWithValue("@IsBanquet", serviceType.IsBanquet);

                // Output parameter for return value
                var serviceTypeCodeRetParam = new SqlParameter("@ServiceTypeCodeRet", SqlDbType.VarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(serviceTypeCodeRetParam);

                conn.Open();

                int rowsAffected = 0;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader["RowsAffected"] != DBNull.Value)
                        rowsAffected = Convert.ToInt32(reader["RowsAffected"]);
                }

                // Get the generated service code
                serviceType.ServiceCode = serviceTypeCodeRetParam.Value?.ToString() ?? serviceTypeCodeRetParam.Value?.ToString() ?? string.Empty;

                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException($"Database error: {ex.Message}");
            }
        }

        public bool Update(ServiceType serviceType)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_reservation_servicetype_save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ServiceTypeID", serviceType.ServiceTypeID);

                // Output/input ServiceCode
                var serviceTypeCodeRetParam = new SqlParameter("@ServiceCode", SqlDbType.VarChar, 300)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = serviceType.ServiceCode ?? string.Empty
                };
                cmd.Parameters.Add(serviceTypeCodeRetParam);

                // Required parameters matching SP signature
                cmd.Parameters.AddWithValue("@ServiceName", serviceType.ServiceName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Remarks", serviceType.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", serviceType.Quantity);
                cmd.Parameters.AddWithValue("@ServiceAmount", serviceType.ServiceAmount);
                cmd.Parameters.AddWithValue("@IsRoom", serviceType.IsRoom);
                cmd.Parameters.AddWithValue("@IsBanquet", serviceType.IsBanquet);

                // Output returned code
                var serviceCodeRetParam = new SqlParameter("@ServiceTypeCodeRet", SqlDbType.VarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(serviceCodeRetParam);

                conn.Open();

                int rowsAffected = 0;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader["RowsAffected"] != DBNull.Value)
                        rowsAffected = Convert.ToInt32(reader["RowsAffected"]);
                }

                // Update the object with current code
                serviceType.ServiceCode = serviceCodeRetParam.Value?.ToString() ?? string.Empty;

                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException($"Database error: {ex.Message}");
            }
        }
        public bool Delete(long ServiceTypeID)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = new SqlCommand(
            "DELETE FROM Reservation_ServiceType WHERE ServiceTypeID = @ServiceTypeID",
            conn
                );

            cmd.Parameters.AddWithValue("@ServiceTypeID", ServiceTypeID);
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;



        }

    }
}
