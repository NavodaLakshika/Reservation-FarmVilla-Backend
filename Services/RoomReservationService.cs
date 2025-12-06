using Microsoft.Data.SqlClient;
using System.Data;

public interface IRoomReservationService
{
    Task<string> SaveOrUpdateReservationAsync(ReservationDto dto);

    
    Task<IReadOnlyList<ReservationDto>> GetAllAsync(int? top = null);

    Task<IEnumerable<ReservationDto>> GetByStatusAsync(int statusId, string? fromDate = null, string? toDate = null);
    Task<string?> GetInvoiceNumberAsync(string reservationNo);

    Task<List<string>> GetReceiptNumbersAsync(string reservationNo);


    Task<IEnumerable<InvoiceDto>> GetFinalizedInvoicesAsync(string? fromDate = null, string? toDate = null);



}

public class RoomReservationService : IRoomReservationService
{
    private readonly IConfiguration _config;

    private static object ToDbDate(DateTime value) =>
    value == DateTime.MinValue ? DBNull.Value : value;

    private static object ToDbDate(DateTime? value) =>
        value == null || value == DateTime.MinValue ? DBNull.Value : value;

    public RoomReservationService(IConfiguration config) => _config = config;

    public async Task<IReadOnlyList<ReservationDto>> GetAllAsync(int? top = null)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // Build TOP (@top) only when requested
        var topSql = top.HasValue ? "TOP (@top) " : string.Empty;

        var sql = $@"
                    SELECT {topSql}
                        h.ReservationNo,
                        h.ReservationDate,
                        h.ReservationType,
                        h.CustomerCode,
                        h.Mobile,
                        h.Telephone,
                        h.email,
                        h.TravelAgentCode,
                        h.Checkindatetime,
                        h.checkoutdatetime,
                        h.noofVehicles,
                        h.noofadults,
                        h.noofKids,
                        h.eventtype,
                        h.setupstyle,
                        h.SubTotal,
                        h.DiscountPer,
                        h.Discount,
                        h.GrossAmount,
                        h.PaidAmount,
                        h.DueAmount,
                        h.Remark,
                        h.RefundAmount,
                        h.refundnote,
                        h.ReferenceReservationNo,
                        h.BookingResourceId,
                        h.BookingReference,
                        h.ReservationStatus,
                        h.crUser,

                        -- Room details (LEFT JOIN; may be null)
                        rd.ReservationRoomDetailsID,
                        rd.RoomCode,
                        rd.PackageCode,
                        rd.noofdays,
                        rd.Price    AS rdPrice,
                        rd.Amount   AS rdAmount,
                        rd.IsDelete,
                        rd.ModifiedDate,
                        rd.checkindate,
                        rd.checkoutdate,

                        -- Service details
                        sd.ServiceCode       AS ServiceTypeCode,
                        sd.ServiceDate,
                        sd.ServiceQty        AS ServiceQuantity,
                        sd.Amount            AS ServiceAmount,
                        sd.TotalAmount       AS ServiceTotalAmount,
                        sd.serviceremark,

                        -- Payment details
                        pd.PaymentID         AS PaymentId,
                        pd.Amount            AS PayAmount,
                        pd.RefNo,
                        pd.RefDate,
                        pd.receiptNo         AS ReceiptNo

                    FROM dbo.Reservation_Hed h
                    LEFT JOIN dbo.Reservation_RoomDetails_Det rd
                        ON rd.ReservationNo = h.ReservationNo
                    AND ISNULL(rd.IsDelete,0) = 0
                    LEFT JOIN dbo.Reservation_Service_Det sd
                        ON sd.ReservationNo = h.ReservationNo
                    AND ISNULL(sd.IsDelete,0) = 0
                    LEFT JOIN dbo.Reservation_Payment_Det pd
                        ON pd.ReservationNo = h.ReservationNo
                    ORDER BY h.ReservationDate DESC, h.ReservationNo;";

        using var cmd = new SqlCommand(sql, conn);
        if (top.HasValue) cmd.Parameters.Add("@top", SqlDbType.Int).Value = top.Value;

        var map = new Dictionary<string, ReservationDto>(StringComparer.OrdinalIgnoreCase);

        await conn.OpenAsync();
        using var rdr = await cmd.ExecuteReaderAsync();

        while (await rdr.ReadAsync())
        {
            // Hed (master)
            var resNo = rdr["ReservationNo"] as string ?? "";

            if (!map.TryGetValue(resNo, out var dto))
            {
                dto = new ReservationDto
                {
                    ReservationNo = resNo,
                    ReservationDate = rdr.GetDateTime(rdr.GetOrdinal("ReservationDate")),
                    ReservationType = rdr.GetInt32(rdr.GetOrdinal("ReservationType")),
                    CustomerCode = rdr["CustomerCode"] as string ?? "",
                    Mobile = rdr["Mobile"] as string,
                    Telephone = rdr["Telephone"] as string,
                    Email = rdr["email"] as string,
                    TravelAgentCode = rdr["TravelAgentCode"] as string,
                    CheckinDateTime = rdr.GetDateTime(rdr.GetOrdinal("Checkindatetime")),
                    CheckoutDateTime = rdr.GetDateTime(rdr.GetOrdinal("checkoutdatetime")),
                    NoOfVehicles = rdr.GetInt32(rdr.GetOrdinal("noofVehicles")),
                    NoOfAdults = rdr.GetInt32(rdr.GetOrdinal("noofadults")),
                    NoOfKids = rdr.GetInt32(rdr.GetOrdinal("noofKids")),
                    EventType = rdr["eventtype"] as string,
                    SetupStyle = rdr["setupstyle"] as string,
                    SubTotal = rdr.GetDecimal(rdr.GetOrdinal("SubTotal")),
                    DiscountPer = rdr.GetDecimal(rdr.GetOrdinal("DiscountPer")),
                    Discount = rdr.GetDecimal(rdr.GetOrdinal("Discount")),
                    GrossAmount = rdr.GetDecimal(rdr.GetOrdinal("GrossAmount")),
                    PaidAmount = rdr.GetDecimal(rdr.GetOrdinal("PaidAmount")),
                    DueAmount = rdr.GetDecimal(rdr.GetOrdinal("DueAmount")),
                    ReservationNote = rdr["Remark"] as string,
                    RefundAmount = rdr.GetDecimal(rdr.GetOrdinal("RefundAmount")),
                    RefundNote = rdr["refundnote"] as string,
                    ReferenceNo = rdr["ReferenceReservationNo"] as string,
                    BookingResourceId = rdr["BookingResourceId"] is DBNull ? 0 : rdr.GetInt32(rdr.GetOrdinal("BookingResourceId")),
                    BookingReferenceNo = rdr["BookingReference"] as string,
                    ReservationStatus = rdr["ReservationStatus"] as string,
                    User = rdr["crUser"] as string,
                    RoomDetails = new List<RoomDetailDto>(),
                    ServiceDetails = new List<ServiceDetailDto>(),
                    RoomPayDetails = new List<RoomPaymentDetailDto>()
                };

                map.Add(resNo, dto);
            }

            // RoomDetails (if present)
            if (!(rdr["RoomCode"] is DBNull) && !(rdr["PackageCode"] is DBNull))
            {
                dto.RoomDetails!.Add(new RoomDetailDto
                {
                    ReservationRoomDetailsID = rdr["ReservationRoomDetailsID"] is DBNull ? 0 : Convert.ToInt32(rdr["ReservationRoomDetailsID"]),
                    ReservationNo = resNo,
                    RoomCode = rdr["RoomCode"] as string,
                    PackageCode = rdr["PackageCode"] as string,
                    NoOfDays = rdr["noofdays"] is DBNull ? 0 : Convert.ToInt32(rdr["noofdays"]),
                    Price = rdr["rdPrice"] is DBNull ? 0m : (decimal)rdr["rdPrice"],
                    Amount = rdr["rdAmount"] is DBNull ? 0m : (decimal)rdr["rdAmount"],
                    //IsDelete = rdr["IsDelete"] is DBNull ? false : Convert.ToBoolean(rdr["IsDelete"]),
                    ModifiedDate = rdr["ModifiedDate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["ModifiedDate"],
                    CheckinDate = rdr["checkindate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["checkindate"],
                    CheckoutDate = rdr["checkoutdate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["checkoutdate"]
                });
            }

            // ServiceDetails (if present)
            if (!(rdr["ServiceTypeCode"] is DBNull))
            {
                dto.ServiceDetails!.Add(new ServiceDetailDto
                {
                    ServiceTypeCode = rdr["ServiceTypeCode"] as string,
                    ServiceDate = rdr["ServiceDate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["ServiceDate"],
                    ServiceQuantity = rdr["ServiceQuantity"] is DBNull ? 0 : Convert.ToInt32(rdr["ServiceQuantity"]),
                    ServiceAmount = rdr["ServiceAmount"] is DBNull ? 0m : (decimal)rdr["ServiceAmount"],
                    ServiceTotalAmount = rdr["ServiceTotalAmount"] is DBNull ? 0m : (decimal)rdr["ServiceTotalAmount"],
                    ServiceRemark = rdr["serviceremark"] as string
                });
            }

            // PaymentDetails (if present)
            if (!(rdr["PaymentId"] is DBNull) || !(rdr["ReceiptNo"] is DBNull))
            {
                dto.RoomPayDetails!.Add(new RoomPaymentDetailDto
                {
                    PaymentId = rdr["PaymentId"] is DBNull ? 0 : Convert.ToInt32(rdr["PaymentId"]),
                    Amount = rdr["PayAmount"] is DBNull ? 0m : (decimal)rdr["PayAmount"],
                    RefNo = rdr["RefNo"] as string,
                    RefDate = rdr["RefDate"] is DBNull ? (DateTime?)null : (DateTime)rdr["RefDate"],
                    ReceiptNo = rdr["ReceiptNo"] as string
                });
            }
        }

        return map.Values
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationNo)
                .ToList();
    }

    public async Task<string> SaveOrUpdateReservationAsync(ReservationDto dto)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("sp_reservation_save", conn) { CommandType = CommandType.StoredProcedure };

        try
        {
            AddVarchar(cmd, "@ReservationNo", 300, (dto.ReservationNo ?? "").Trim());
            AddDateTime(cmd, "@ReservationDate", dto.ReservationDate);
            AddInt(cmd, "@ReservationType", dto.ReservationType);
            AddVarchar(cmd, "@CustomerCode", 300, dto.CustomerCode);

            // ✅ FIX: Properly handle StatusId - this was missing
            AddInt(cmd, "@statusid", dto.StatusId > 0 ? dto.StatusId : 1);

            AddVarcharNullable(cmd, "@Mobile", 300, dto.Mobile);
            AddVarcharNullable(cmd, "@Telephone", 300, dto.Telephone);
            AddVarcharNullable(cmd, "@email", 300, dto.Email);
            AddVarcharNullable(cmd, "@TravelAgentCode", 300, dto.TravelAgentCode);

            AddDateTime(cmd, "@Checkindatetime", dto.CheckinDateTime);
            AddDateTime(cmd, "@checkoutdatetime", dto.CheckoutDateTime);

            AddInt(cmd, "@noofVehicles", dto.NoOfVehicles);
            AddInt(cmd, "@noofadults", dto.NoOfAdults);
            AddInt(cmd, "@noofKids", dto.NoOfKids);

            AddVarcharNullable(cmd, "@EventType", 100, dto.EventType);
            AddVarcharNullable(cmd, "@SetupStyle", 100, dto.SetupStyle);

            AddMoney(cmd, "@SubTotal", dto.SubTotal);
            AddMoney(cmd, "@DiscountPer", dto.DiscountPer);
            AddMoney(cmd, "@Discount", dto.Discount);
            AddMoney(cmd, "@GrossAmount", dto.GrossAmount);
            AddMoney(cmd, "@PaidAmount", dto.PaidAmount);
            AddMoney(cmd, "@DueAmount", dto.DueAmount);

            AddVarcharNullable(cmd, "@ReservationNote", 300, dto.ReservationNote);
            AddMoney(cmd, "@RefundAmount", dto.RefundAmount);
            AddVarchar(cmd, "@RefundNote", 100, string.IsNullOrWhiteSpace(dto.RefundNote) ? "" : dto.RefundNote);

            AddVarcharNullable(cmd, "@ReferenceNo", 50, dto.ReferenceNo);

            AddInt(cmd, "@BookingResourceId", dto.BookingResourceId);
            AddVarcharNullable(cmd, "@BookingReferenceNo", 50, dto.BookingReferenceNo);

            // ✅ FIX: Ensure ReservationStatus is properly handled
            AddVarchar(cmd, "@ReservationStatus", 100,
                string.IsNullOrWhiteSpace(dto.ReservationStatus)
                    ? "Booked" // Default value if null/empty
                    : dto.ReservationStatus);

            AddVarcharNullable(cmd, "@User", 50, dto.User);

            // ---- TVPs (schemas EXACTLY how SP uses them) ----
            var dtRoom = CreateRoomDetailsTable(dto.RoomDetails);
            var pRoom = cmd.Parameters.Add("@dt_RoomDetails", SqlDbType.Structured);
            pRoom.TypeName = "dt_RoomDetails";
            pRoom.Value = dtRoom;

            var dtSvc = CreateServiceDetailsTable(dto.ServiceDetails);
            var pSvc = cmd.Parameters.Add("@dt_ServiceDetails", SqlDbType.Structured);
            pSvc.TypeName = "dt_ServiceDetails";
            pSvc.Value = dtSvc;

            var dtPay = CreateRoomPayTable(dto.RoomPayDetails);
            var pPay = cmd.Parameters.Add("@dt_RoomPayDetails", SqlDbType.Structured);
            pPay.TypeName = "dt_RoomPayDetails";
            pPay.Value = dtPay;

            // ---- Output Parameters ----
            var outResNo = new SqlParameter("@ReservationNoRet", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outResNo);

            var outInvoiceNo = new SqlParameter("@InvoiceNoRet", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outInvoiceNo);




            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();


            var reservationNo = outResNo.Value?.ToString() ?? "";
            var invoiceNo = outInvoiceNo.Value?.ToString() ?? "";

            if (!string.IsNullOrEmpty(invoiceNo))
            {
                Console.WriteLine($"Invoice generated: {invoiceNo}");
            }

            return reservationNo;
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
            Console.WriteLine($"SQL Error Number: {ex.Number}");
            Console.WriteLine($"SQL Procedure: {ex.Procedure}");
            throw new Exception($"Database error occurred while saving reservation: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw new Exception($"An error occurred while saving reservation: {ex.Message}");
        }
    }



    // Helper method to get the invoice number
    private async Task<string> GetInvoiceNumberAsync(SqlConnection conn, string reservationNo)
    {
        if (string.IsNullOrEmpty(reservationNo)) return string.Empty;

        using var cmd = new SqlCommand("SELECT InvoiceNo FROM Reservation_Hed WHERE ReservationNo = @ReservationNo", conn);
        cmd.Parameters.AddWithValue("@ReservationNo", reservationNo);

        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString() ?? string.Empty;
    }
    // ---------- TVP builders that match the SP ----------

    // @dt_RoomDetails used by MERGE:
    // INSERT (..., [checkindate], [checkoutdate]) VALUES (..., MySource.[checkin], MySource.[checkout])
    // dt_RoomDetails: exactly the columns the SP uses in MERGE (order matters)
    private static DataTable CreateRoomDetailsTable(List<RoomDetailDto>? list)
    {
        var t = new DataTable();
        t.Columns.Add("RoomCode", typeof(string));
        t.Columns.Add("PackageCode", typeof(string));
        t.Columns.Add("noofdays", typeof(int));
        t.Columns.Add("Price", typeof(decimal));
        t.Columns.Add("Amount", typeof(decimal));
        t.Columns.Add("checkin", typeof(DateTime)); // SP expects 'checkin'
        t.Columns.Add("checkout", typeof(DateTime)); // SP expects 'checkout'

        foreach (var x in list ?? Enumerable.Empty<RoomDetailDto>())
        {
            t.Rows.Add(
                x.RoomCode ?? (object)DBNull.Value,
                x.PackageCode ?? (object)DBNull.Value,
                x.NoOfDays,
                x.Price,
                x.Amount,
                ToDbDate(x.CheckinDate), // This maps to 'checkin'
                ToDbDate(x.CheckoutDate) // This maps to 'checkout'
            );
        }
        return t;
    }

    // dt_ServiceDetails: SP uses these names (no extra "Service" column)
    private static DataTable CreateServiceDetailsTable(List<ServiceDetailDto>? list)
    {
        var t = new DataTable();
        t.Columns.Add("ServiceTypeCode", typeof(string));   // 1
        t.Columns.Add("ServiceQuantity", typeof(int));      // 2
        t.Columns.Add("ServiceAmount", typeof(decimal));  // 3
        t.Columns.Add("ServiceTotalAmount", typeof(decimal));  // 4
        t.Columns.Add("ServiceDate", typeof(DateTime)); // 5
        t.Columns.Add("ServiceRemark", typeof(string));   // 6

        foreach (var x in list ?? Enumerable.Empty<ServiceDetailDto>())
        {
            t.Rows.Add(
                x.ServiceTypeCode ?? (object)DBNull.Value,
                x.ServiceQuantity,
                x.ServiceAmount,
                x.ServiceTotalAmount,
                ToDbDate(x.ServiceDate),
                x.ServiceRemark ?? (object)DBNull.Value
            );
        }
        return t;
    }

    // dt_RoomPayDetails: SP reads these (no PaymentType column)
    private static DataTable CreateRoomPayTable(List<RoomPaymentDetailDto>? list)
    {
        var t = new DataTable();
        t.Columns.Add("ReceiptNo", typeof(string));    // 1
        t.Columns.Add("PaymentID", typeof(int));       // 2  use long if BIGINT in SQL
        t.Columns.Add("Amount", typeof(decimal));   // 3
        t.Columns.Add("RefNo", typeof(string));    // 4
        t.Columns.Add("RefDate", typeof(DateTime));  // 5

        foreach (var x in list ?? Enumerable.Empty<RoomPaymentDetailDto>())
        {
            t.Rows.Add(
                x.ReceiptNo ?? (object)DBNull.Value,
                x.PaymentId,
                x.Amount,
                x.RefNo ?? (object)DBNull.Value,
                ToDbDate(x.RefDate)
            );
        }
        return t;
    }


    // ---------- Parameter helpers (typed; sizes & scales set) ----------

    private static void AddVarchar(SqlCommand cmd, string name, int size, string value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.VarChar, size);
        p.Value = value;
    }

    private static void AddVarcharNullable(SqlCommand cmd, string name, int size, string? value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.VarChar, size);
        p.Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value!.Trim();
    }

    private static void AddInt(SqlCommand cmd, string name, int value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.Int);
        p.Value = value;
    }

    private static void AddDateTime(SqlCommand cmd, string name, DateTime value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.DateTime);
        p.Value = value;
    }

    private static void AddMoney(SqlCommand cmd, string name, decimal value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.Decimal);
        p.Precision = 18;
        p.Scale = 2;
        p.Value = value;
    }


    public async Task<List<string>> GetReceiptNumbersAsync(string reservationNo)
    {
        var receiptNumbers = new List<string>();

        try
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync(); // ✅ Changed from connection to conn

            var query = @"
            SELECT DISTINCT ReceiptNo 
            FROM Reservation_Payment_Hed 
            WHERE ReservationNo = @ReservationNo 
                AND IsUpdate = 1 
            ORDER BY ReceiptNo";

            using (var command = new SqlCommand(query, conn)) // ✅ Changed from connection to conn
            {
                command.Parameters.AddWithValue("@ReservationNo", reservationNo);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        receiptNumbers.Add(reader["ReceiptNo"].ToString());
                    }
                }
            }

            // ✅ If _logger doesn't exist in your class, remove these lines or add the logger
            // _logger.LogInformation("Found {Count} receipts for reservation {ReservationNo}", 
            //     receiptNumbers.Count, reservationNo);

            return receiptNumbers;
        }
        catch (Exception ex)
        {
            // ✅ If _logger doesn't exist, use Console.WriteLine or remove
            // _logger.LogError(ex, "Error retrieving receipt numbers for reservation {ReservationNo}", reservationNo);
            Console.WriteLine($"Error retrieving receipt numbers: {ex.Message}");
            return new List<string>(); // Return empty list on error
        }
    }


    public async Task<string?> GetInvoiceNumberAsync(string reservationNo)
    {
        if (string.IsNullOrEmpty(reservationNo))
        {
            Console.WriteLine("⚠️ GetInvoiceNumberAsync: ReservationNo is null or empty");
            return null;
        }

        try
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT InvoiceNo FROM Reservation_Hed WHERE ReservationNo = @ReservationNo", conn);

            cmd.Parameters.AddWithValue("@ReservationNo", reservationNo);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            var invoiceNo = result == DBNull.Value || result == null ? null : result.ToString();

            Console.WriteLine($"✅ GetInvoiceNumberAsync: ReservationNo={reservationNo}, InvoiceNo={invoiceNo ?? "NULL"}");

            return invoiceNo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting invoice number for {reservationNo}: {ex.Message}");
            return null; // Return null instead of throwing to prevent save failure
        }
    }



    //this one working but amounts are wrong
    public async Task<IEnumerable<ReservationDto>> GetByStatusAsync(int statusId, string? fromDate = null, string? toDate = null)
{
    using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

    var sql = @"
        SELECT 
            h.ReservationNo, 
            h.ReservationDate, 
            h.ReservationType, 
            h.CustomerCode,
            h.Mobile, 
            h.Telephone, 
            h.email, 
            h.TravelAgentCode,
            h.Checkindatetime, 
            h.checkoutdatetime, 
            h.SubTotal, 
            h.DiscountPer,
            h.Discount, 
            h.GrossAmount, 
            h.PaidAmount, 
            h.DueAmount, 
            h.RefundAmount,
            h.Remark, 
            h.ReservationStatus, 
            h.BookingReference, 
            h.InvoiceNo, 
            h.invoicedate,
            h.crUser,
            h.edUser,
            h.statusid,
            h.noofVehicles,
            h.noofadults,
            h.noofKids,
            h.ReferenceReservationNo,
            h.BookingResourceId,
            h.refundnote,
            
            -- Customer Details
            c.CustomerTypeCode,
            c.Title,
            c.Name,
            c.NIC_PassportNo,
            c.NationalityCode,
            c.CountryCode,
            c.Address,
            c.CreditLimit,
            
            -- Room Details
            rd.RoomCode,
            rd.PackageCode,
            rd.noofdays,
            rd.Price as RoomPrice,
            rd.Amount as RoomAmount,
            rd.checkindate,
            rd.checkoutdate,
            
            -- Service Details
            sd.ServiceCode as ServiceTypeCode,
            sd.ServiceDate,
            sd.ServiceQty as ServiceQuantity,
            sd.Amount as ServiceAmount,
            sd.TotalAmount as ServiceTotalAmount,
            sd.serviceremark as ServiceRemark,
            
            -- Payment Details
            pd.PaymentID as PaymentId,
            pd.Amount as PayAmount,
            pd.RefNo,
            pd.RefDate,
            pd.receiptNo as ReceiptNo
            
        FROM Reservation_Hed h
        LEFT JOIN Reservation_Customer c ON h.CustomerCode = c.CustomerCode
        LEFT JOIN Reservation_RoomDetails_Det rd ON h.ReservationNo = rd.ReservationNo
        LEFT JOIN Reservation_Service_Det sd ON h.ReservationNo = sd.ReservationNo
        LEFT JOIN Reservation_Payment_Det pd ON h.ReservationNo = pd.ReservationNo
        WHERE h.statusid = @statusId";

    if (!string.IsNullOrEmpty(fromDate))
        sql += " AND CONVERT(date, h.ReservationDate) >= @fromDate";
    if (!string.IsNullOrEmpty(toDate))
        sql += " AND CONVERT(date, h.ReservationDate) <= @toDate";

    sql += " ORDER BY h.ReservationDate DESC, h.ReservationNo";

    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@statusId", statusId);

    if (!string.IsNullOrEmpty(fromDate))
        cmd.Parameters.AddWithValue("@fromDate", DateTime.Parse(fromDate));
    if (!string.IsNullOrEmpty(toDate))
        cmd.Parameters.AddWithValue("@toDate", DateTime.Parse(toDate));

    var reservations = new Dictionary<string, ReservationDto>();

    try
    {
        await conn.OpenAsync();
        using var rdr = await cmd.ExecuteReaderAsync();

        while (await rdr.ReadAsync())
        {
            var reservationNo = rdr["ReservationNo"] as string ?? "";

            if (!reservations.ContainsKey(reservationNo))
            {
                reservations[reservationNo] = new ReservationDto
                {
                    ReservationNo = reservationNo,
                    ReservationDate = rdr["ReservationDate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["ReservationDate"],
                    ReservationType = rdr["ReservationType"] is DBNull ? 0 : Convert.ToInt32(rdr["ReservationType"]),
                    CustomerCode = rdr["CustomerCode"] as string ?? "",
                    Mobile = rdr["Mobile"] as string,
                    Telephone = rdr["Telephone"] as string,
                    Email = rdr["email"] as string,
                    TravelAgentCode = rdr["TravelAgentCode"] as string,
                    CheckinDateTime = rdr["Checkindatetime"] is DBNull ? DateTime.MinValue : (DateTime)rdr["Checkindatetime"],
                    CheckoutDateTime = rdr["checkoutdatetime"] is DBNull ? DateTime.MinValue : (DateTime)rdr["checkoutdatetime"],
                    NoOfVehicles = rdr["noofVehicles"] is DBNull ? 0 : Convert.ToInt32(rdr["noofVehicles"]),
                    NoOfAdults = rdr["noofadults"] is DBNull ? 0 : Convert.ToInt32(rdr["noofadults"]),
                    NoOfKids = rdr["noofKids"] is DBNull ? 0 : Convert.ToInt32(rdr["noofKids"]),
                    SubTotal = rdr["SubTotal"] as decimal? ?? 0m,
                    DiscountPer = rdr["DiscountPer"] as decimal? ?? 0m,
                    Discount = rdr["Discount"] as decimal? ?? 0m,
                    GrossAmount = rdr["GrossAmount"] as decimal? ?? 0m,
                    PaidAmount = rdr["PaidAmount"] as decimal? ?? 0m,
                    DueAmount = rdr["DueAmount"] as decimal? ?? 0m,
                    RefundAmount = rdr["RefundAmount"] as decimal? ?? 0m,
                    ReservationNote = rdr["Remark"] as string,
                    RefundNote = rdr["refundnote"] as string,
                    ReferenceNo = rdr["ReferenceReservationNo"] as string,
                    BookingResourceId = rdr["BookingResourceId"] is DBNull ? 0 : Convert.ToInt32(rdr["BookingResourceId"]),
                    BookingReferenceNo = rdr["BookingReference"] as string,
                    ReservationStatus = rdr["ReservationStatus"]?.ToString(),
                    User = rdr["crUser"] as string,
                    Customer = new Customer
                    {
                        CustomerTypeCode = rdr["CustomerTypeCode"] as string ?? "",
                        CustomerCode = rdr["CustomerCode"] as string ?? "",
                        Title = rdr["Title"] as string ?? "",
                        Name = rdr["Name"] as string ?? "",
                        NIC_PassportNo = rdr["NIC_PassportNo"] as string ?? "",
                        NationalityCode = rdr["NationalityCode"] as string ?? "",
                        CountryCode = rdr["CountryCode"] as string ?? "",
                        Address = rdr["Address"] as string ?? "",
                        CreditLimit = rdr["CreditLimit"] is DBNull ? 0m : Convert.ToDecimal(rdr["CreditLimit"]),
                        Mobile = rdr["Mobile"] as string ?? "",
                        Telephone = rdr["Telephone"] as string ?? "",
                        Email = rdr["email"] as string ?? "",
                        TravelAgentCode = rdr["TravelAgentCode"] as string ?? "",
                        IsActive = true,
                        IsNew = false,
                        Whatsapp = "",
                        Remark = ""
                    },
                    RoomDetails = new List<RoomDetailDto>(),
                    ServiceDetails = new List<ServiceDetailDto>(),
                    RoomPayDetails = new List<RoomPaymentDetailDto>()
                };
            }

            // Add room details
            var roomCode = rdr["RoomCode"] as string;
            if (!string.IsNullOrEmpty(roomCode))
            {
                var existingRoom = reservations[reservationNo].RoomDetails.FirstOrDefault(r => r.RoomCode == roomCode);
                if (existingRoom == null)
                {
                    reservations[reservationNo].RoomDetails.Add(new RoomDetailDto
                    {
                        RoomCode = roomCode,
                        PackageCode = rdr["PackageCode"] as string,
                        NoOfDays = rdr["noofdays"] is DBNull ? 0 : Convert.ToInt32(rdr["noofdays"]),
                        Price = rdr["RoomPrice"] is DBNull ? 0m : Convert.ToDecimal(rdr["RoomPrice"]),
                        Amount = rdr["RoomAmount"] is DBNull ? 0m : Convert.ToDecimal(rdr["RoomAmount"]),
                        CheckinDate = rdr["checkindate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["checkindate"],
                        CheckoutDate = rdr["checkoutdate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["checkoutdate"]
                    });
                }
            }

            // Add service details
            var serviceCode = rdr["ServiceTypeCode"] as string;
            if (!string.IsNullOrEmpty(serviceCode))
            {
                var existingService = reservations[reservationNo].ServiceDetails.FirstOrDefault(s => s.ServiceTypeCode == serviceCode);
                if (existingService == null)
                {
                    reservations[reservationNo].ServiceDetails.Add(new ServiceDetailDto
                    {
                        ServiceTypeCode = serviceCode,
                        ServiceDate = rdr["ServiceDate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["ServiceDate"],
                        ServiceQuantity = rdr["ServiceQuantity"] is DBNull ? 0 : Convert.ToInt32(rdr["ServiceQuantity"]),
                        ServiceAmount = rdr["ServiceAmount"] is DBNull ? 0m : Convert.ToDecimal(rdr["ServiceAmount"]),
                        ServiceTotalAmount = rdr["ServiceTotalAmount"] is DBNull ? 0m : Convert.ToDecimal(rdr["ServiceTotalAmount"]),
                        ServiceRemark = rdr["ServiceRemark"] as string
                    });
                }
            }

            // ✅ Add payment details even if PaymentId = 0
            var paymentAmount = rdr["PayAmount"] is DBNull ? 0m : Convert.ToDecimal(rdr["PayAmount"]);
            if (paymentAmount > 0)
            {
                reservations[reservationNo].RoomPayDetails.Add(new RoomPaymentDetailDto
                {
                    PaymentId = rdr["PaymentId"] is DBNull ? 0 : Convert.ToInt32(rdr["PaymentId"]),
                    Amount = paymentAmount,
                    RefNo = rdr["RefNo"] as string,
                    RefDate = rdr["RefDate"] is DBNull ? (DateTime?)null : (DateTime)rdr["RefDate"],
                    ReceiptNo = rdr["ReceiptNo"] as string
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching reservations: {ex.Message}");
        throw;
    }

    return reservations.Values.ToList();
}


    public async Task<IEnumerable<InvoiceDto>> GetFinalizedInvoicesAsync(string? fromDate = null, string? toDate = null)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // This query is optimized to get proper invoice data without duplication
        var sql = @"
        SELECT 
            h.InvoiceNo,
            h.invoicedate AS InvoiceDate,
            h.ReservationNo,
            h.CustomerCode,
            c.Name AS CustomerName,
            h.GrossAmount AS TotalAmount,
            h.PaidAmount,
            h.DueAmount,
            h.ReservationStatus AS Status,
            h.crUser AS CreatedBy
        FROM Reservation_Hed h
        LEFT JOIN Reservation_Customer c ON h.CustomerCode = c.CustomerCode
        WHERE h.statusid = 6 
          AND h.InvoiceNo IS NOT NULL 
          AND h.InvoiceNo != ''";

        if (!string.IsNullOrEmpty(fromDate))
            sql += " AND CONVERT(date, h.invoicedate) >= @fromDate";
        if (!string.IsNullOrEmpty(toDate))
            sql += " AND CONVERT(date, h.invoicedate) <= @toDate";

        sql += " ORDER BY h.invoicedate DESC, h.InvoiceNo";

        using var cmd = new SqlCommand(sql, conn);

        if (!string.IsNullOrEmpty(fromDate))
            cmd.Parameters.AddWithValue("@fromDate", DateTime.Parse(fromDate));
        if (!string.IsNullOrEmpty(toDate))
            cmd.Parameters.AddWithValue("@toDate", DateTime.Parse(toDate));

        var invoices = new List<InvoiceDto>();

        try
        {
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                var invoice = new InvoiceDto
                {
                    InvoiceNo = rdr["InvoiceNo"] as string ?? "",
                    InvoiceDate = rdr["InvoiceDate"] is DBNull ? DateTime.MinValue : (DateTime)rdr["InvoiceDate"],
                    ReservationNo = rdr["ReservationNo"] as string ?? "",
                    CustomerCode = rdr["CustomerCode"] as string ?? "",
                    CustomerName = rdr["CustomerName"] as string ?? "N/A",
                    TotalAmount = rdr["TotalAmount"] is DBNull ? 0m : Convert.ToDecimal(rdr["TotalAmount"]),
                    PaidAmount = rdr["PaidAmount"] is DBNull ? 0m : Convert.ToDecimal(rdr["PaidAmount"]),
                    DueAmount = rdr["DueAmount"] is DBNull ? 0m : Convert.ToDecimal(rdr["DueAmount"]),
                    Status = rdr["Status"] as string ?? "Finalized",
                    CreatedBy = rdr["CreatedBy"] as string ?? "System"
                };

                invoices.Add(invoice);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching invoices: {ex.Message}");
            throw;
        }

        return invoices;
    }
}