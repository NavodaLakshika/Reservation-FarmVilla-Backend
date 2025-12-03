using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class PackageInfoService
    {
        private readonly string _conn;

        public PackageInfoService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }


        public string GetNextPackageCode()
        {
        using var conn = new SqlConnection(_conn);
        using var cmd = new SqlCommand("sp_GetNextPackageCode", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader["packageCode"].ToString();
        }

        return null;
        }
    

        public List<PackageInfo> GetAll()
        {
            var list = new List<PackageInfo>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_reservation_package_getall", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new PackageInfo
                {
                    PackageID = Convert.ToInt32(reader["PackageID"]),
                    PackageCode = reader["PackageCode"].ToString(),
                    PackageName = reader["PackageName"].ToString(),
                    PackageDuration = Convert.ToDecimal(reader["PackageDuration"]),
                    Remarks = reader["Remarks"] != DBNull.Value ? reader["Remarks"].ToString() : null,

                    RoomPrice = Convert.ToDecimal(reader["RoomPrice"]),
                    RoomCost = Convert.ToDecimal(reader["RoomCost"]),
                    RoomAmount = Convert.ToDecimal(reader["RoomAmount"]),
                    FoodAmount = Convert.ToDecimal(reader["FoodAmount"]),
                    BeverageAmount = Convert.ToDecimal(reader["BeverageAmount"]),

                    IsRoom = Convert.ToBoolean(reader["IsRoom"]),
                    IsBanquet = Convert.ToBoolean(reader["IsBanquet"]),
                    IsVilla = Convert.ToBoolean(reader["IsVilla"])
                });
            }
            return list;
        }

        public bool Create(PackageInfo packageInfo)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_reservation_package_save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // PackageID = 0 for insert (REQUIRED by SP)
                cmd.Parameters.AddWithValue("@PackageID", 0);

                // PackageCode is output
                var packageCodeParam = new SqlParameter("@PackageCode", SqlDbType.VarChar, 300)
                {
                    Direction = ParameterDirection.Output,
                    Value = string.Empty
                };
                cmd.Parameters.Add(packageCodeParam);

                // Required parameters matching SP signature
                cmd.Parameters.AddWithValue("@PackageName", packageInfo.PackageName ?? string.Empty);
                cmd.Parameters.AddWithValue("@PackageDuration", packageInfo.PackageDuration);
                cmd.Parameters.AddWithValue("@Remark", packageInfo.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RoomPrice", packageInfo.RoomPrice);
                cmd.Parameters.AddWithValue("@RoomCost", packageInfo.RoomCost);
                cmd.Parameters.AddWithValue("@RoomAmount", packageInfo.RoomAmount);
                cmd.Parameters.AddWithValue("@FoodAmount", packageInfo.FoodAmount);
                cmd.Parameters.AddWithValue("@BeverageAmount", packageInfo.BeverageAmount);
                cmd.Parameters.AddWithValue("@IsRoom", packageInfo.IsRoom);
                cmd.Parameters.AddWithValue("@IsBanquet", packageInfo.IsBanquet);
                cmd.Parameters.AddWithValue("@IsVilla", packageInfo.IsVilla);

                // Output parameter for return value
                var packageCodeRetParam = new SqlParameter("@PackageCodeRet", SqlDbType.VarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(packageCodeRetParam);

                conn.Open();

                int rowsAffected = 0;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader["RowsAffected"] != DBNull.Value)
                        rowsAffected = Convert.ToInt32(reader["RowsAffected"]);
                }

                // Get the generated package code
                packageInfo.PackageCode = packageCodeRetParam.Value?.ToString() ?? packageCodeParam.Value?.ToString() ?? string.Empty;

                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException($"Database error: {ex.Message}");
            }
        }

        public bool Update(PackageInfo packageInfo)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                using var cmd = new SqlCommand("sp_reservation_package_save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Send PackageID for update (REQUIRED by SP)
                cmd.Parameters.AddWithValue("@PackageID", packageInfo.PackageID);

                // Output/input PackageCode
                var packageCodeParam = new SqlParameter("@PackageCode", SqlDbType.VarChar, 300)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = packageInfo.PackageCode ?? string.Empty
                };
                cmd.Parameters.Add(packageCodeParam);

                // Required parameters matching SP signature
                cmd.Parameters.AddWithValue("@PackageName", packageInfo.PackageName ?? string.Empty);
                cmd.Parameters.AddWithValue("@PackageDuration", packageInfo.PackageDuration);
                cmd.Parameters.AddWithValue("@Remark", packageInfo.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RoomPrice", packageInfo.RoomPrice);
                cmd.Parameters.AddWithValue("@RoomCost", packageInfo.RoomCost);
                cmd.Parameters.AddWithValue("@RoomAmount", packageInfo.RoomAmount);
                cmd.Parameters.AddWithValue("@FoodAmount", packageInfo.FoodAmount);
                cmd.Parameters.AddWithValue("@BeverageAmount", packageInfo.BeverageAmount);
                cmd.Parameters.AddWithValue("@IsRoom", packageInfo.IsRoom);
                cmd.Parameters.AddWithValue("@IsBanquet", packageInfo.IsBanquet);
                cmd.Parameters.AddWithValue("@IsVilla", packageInfo.IsVilla);

                // Output returned code
                var packageCodeRetParam = new SqlParameter("@PackageCodeRet", SqlDbType.VarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(packageCodeRetParam);

                conn.Open();

                int rowsAffected = 0;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader["RowsAffected"] != DBNull.Value)
                        rowsAffected = Convert.ToInt32(reader["RowsAffected"]);
                }

                // Update the object with current code
                packageInfo.PackageCode = packageCodeRetParam.Value?.ToString() ?? string.Empty;

                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException($"Database error: {ex.Message}");
            }
        }

        public bool Delete(long PackageID)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = new SqlCommand(
            "DELETE FROM Reservation_Package WHERE PackageID = @PackageID",
            conn
                );

            cmd.Parameters.AddWithValue("@PackageID", PackageID);
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;



        }
    }
}
