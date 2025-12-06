using Microsoft.Data.SqlClient;

public class InvoiceDto
{
    public string InvoiceNo { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string ReservationNo { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string Status { get; set; }
    public string CreatedBy { get; set; }
}

