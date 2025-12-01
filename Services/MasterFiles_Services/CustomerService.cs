using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OIT_Reservation.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace OIT_Reservation.Services
{
    public class CustomerService
    {
        private readonly string _conn;

        public CustomerService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public string GetNextCustomerCode()
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetNextCustomerCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["CustomerCode"].ToString();
            }
            return null;
        }




        public string CustomerSave(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            using (SqlConnection con = new SqlConnection(_conn))
            using (SqlCommand cmd = new SqlCommand("sp_Save_Customer", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CustomerTypeCode", (object)customer.CustomerTypeCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CustomerCode", (object)customer.CustomerCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", (object)customer.Title ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", (object)customer.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NIC_PassportNo", (object)customer.NIC_PassportNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NationalityCode", (object)customer.NationalityCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CountryCode", (object)customer.CountryCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Mobile", (object)customer.Mobile ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telephone", (object)customer.Telephone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@email", (object)customer.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TravelAgentCode", (object)customer.TravelAgentCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreditLimit", customer.CreditLimit != null ? customer.CreditLimit : 0);
                cmd.Parameters.AddWithValue("@IsActive", customer.IsActive);
                cmd.Parameters.AddWithValue("@IsNew", string.IsNullOrWhiteSpace(customer.CustomerCode) ? 1 : 0);
                cmd.Parameters.AddWithValue("@Address", (object)customer.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Whatsapp", (object)customer.Whatsapp ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Remark", (object)customer.Remark ?? DBNull.Value);

                SqlParameter outParam = new SqlParameter("@CustomerCodeRet", SqlDbType.VarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                con.Open();
                cmd.ExecuteNonQuery();

                return outParam.Value?.ToString() ?? string.Empty;
            }
        }


        public List<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();

            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("SELECT * FROM Reservation_Customer", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                customers.Add(new Customer
                {
                    CustomerCode = reader["CustomerCode"].ToString(),
                    CustomerTypeCode = reader["CustomerTypeCode"].ToString(),
                    Name = reader["Name"].ToString(),
                    Title = reader["Title"].ToString(),
                    NIC_PassportNo = reader["NIC_PassportNo"].ToString(),
                    NationalityCode = reader["NationalityCode"].ToString(),
                    CountryCode = reader["CountryCode"].ToString(),
                    Mobile = reader["Mobile"].ToString(),
                    Telephone = reader["Telephone"].ToString(),
                    Email = reader["Email"].ToString(),
                    TravelAgentCode = reader["TravelAgentCode"].ToString(),
                    CreditLimit = reader["CreditLimit"] != DBNull.Value ? Convert.ToDecimal(reader["CreditLimit"]) : 0,
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                    Address = reader["Address"].ToString(),
                    Whatsapp = reader["Whatsapp"].ToString(),
                    Remark = reader["Remark"].ToString(),
                });
#pragma warning restore CS8601 // Possible null reference assignment.
            }

            return customers;
        }


        public Customer GetCustomerByCode(string code)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("sp_GetCustomerByCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CustomerCode", code);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Customer
                {
                    CustomerCode = reader["CustomerCode"]?.ToString(),
                    Name = reader["Name"]?.ToString(),
                    Mobile = reader["Mobile"]?.ToString(),
                    Email = reader["email"]?.ToString(),
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    // Add other properties as needed
                };
            }

            return null;
        }
        // public Customer UpdateCustomer(Customer customer)
        // {
        //     try
        //     {
        //         using var conn = new SqlConnection(_conn);
        //         using var cmd = new SqlCommand("sp_Update_Customer", conn)
        //         {
        //             CommandType = CommandType.StoredProcedure
        //         };

        //         // ✅ Add the CustomerCode parameter for WHERE clause
        //         cmd.Parameters.AddWithValue("@CustomerCode", customer.CustomerCode ?? (object)DBNull.Value);

        //         cmd.Parameters.AddWithValue("@CustomerTypeCode", customer.CustomerTypeCode ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@Title", customer.Title ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@Name", customer.Name ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@NIC_PassportNo", customer.NIC_PassportNo ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@NationalityCode", customer.NationalityCode ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@CountryCode", customer.CountryCode ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@Mobile", customer.Mobile ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@Telephone", customer.Telephone ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@email", customer.Email ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@TravelAgentCode", customer.TravelAgentCode ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@CreditLimit", customer.CreditLimit);
        //         cmd.Parameters.AddWithValue("@IsActive", customer.IsActive);
        //         cmd.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@Whatsapp", customer.Whatsapp ?? (object)DBNull.Value);
        //         cmd.Parameters.AddWithValue("@Remark", customer.Remark ?? (object)DBNull.Value);

        //         conn.Open();
        //         cmd.ExecuteNonQuery();

        //         return customer;
        //     }
        //     catch (SqlException ex)
        //     {
        //         throw new ApplicationException($"SQL Error: {ex.Message}");
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new ApplicationException($"Error updating customer: {ex.Message}");
        //     }
        // }
        
        public string UpdateCustomerByCode(string customerCode, Customer customer)
        {
            using (var conn = new SqlConnection(_conn))
            using (var cmd = new SqlCommand("sp_Save_Customer", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CustomerCode", customer.CustomerCode ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue("@CustomerTypeCode", customer.CustomerTypeCode ?? "");
                cmd.Parameters.AddWithValue("@Title", customer.Title ?? "");
                cmd.Parameters.AddWithValue("@Name", customer.Name ?? "");
                cmd.Parameters.AddWithValue("@NIC_PassportNo", customer.NIC_PassportNo ?? "");
                cmd.Parameters.AddWithValue("@NationalityCode", customer.NationalityCode ?? "");
                cmd.Parameters.AddWithValue("@CountryCode", customer.CountryCode ?? "");
                cmd.Parameters.AddWithValue("@Mobile", customer.Mobile ?? "");
                cmd.Parameters.AddWithValue("@Telephone", customer.Telephone ?? "");
                cmd.Parameters.AddWithValue("@email", customer.Email ?? "");
                cmd.Parameters.AddWithValue("@TravelAgentCode", customer.TravelAgentCode ?? "");
                cmd.Parameters.AddWithValue("@CreditLimit", customer.CreditLimit);
                cmd.Parameters.AddWithValue("@IsActive", customer.IsActive);
                cmd.Parameters.AddWithValue("@IsNew", 0); // Important: UPDATE
                cmd.Parameters.AddWithValue("@Address", customer.Address ?? "");
                cmd.Parameters.AddWithValue("@Whatsapp", customer.Whatsapp ?? "");
                cmd.Parameters.AddWithValue("@Remark", customer.Remark ?? "");

                var outParam = new SqlParameter("@CustomerCodeRet", SqlDbType.VarChar, 20)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                return outParam.Value?.ToString() ?? "";
            }
        }


    }
}
