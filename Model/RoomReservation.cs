public class ReservationDto {
    public string? ReservationNo { get; set; }
    public DateTime ReservationDate { get; set; }
    public int ReservationType { get; set; }
    public string CustomerCode { get; set; } = "";
    public string? Mobile { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? TravelAgentCode { get; set; }
    public DateTime CheckinDateTime { get; set; }
    public DateTime CheckoutDateTime { get; set; }
    public int NoOfVehicles { get; set; }
    public int NoOfAdults { get; set; }
    public int NoOfKids { get; set; }
    public string? EventType { get; set; }
    public string? SetupStyle { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountPer { get; set; }
    public decimal Discount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string? ReservationNote { get; set; }
    public decimal RefundAmount { get; set; }
    public string? RefundNote { get; set; }
    public string? ReferenceNo { get; set; }
    public int BookingResourceId { get; set; }
    public string? BookingReferenceNo { get; set; }
    public string? ReservationStatus { get; set; }
    public string? User { get; set; }
    public string? CrUser { get; set; }
    public string? EdUser { get; set; }
    public List<RoomDetailDto>? RoomDetails { get; set; }
    public List<ServiceDetailDto>? ServiceDetails { get; set; }
    public List<RoomPaymentDetailDto>? RoomPayDetails { get; set; }
    public Customer Customer { get; set; } = new Customer();

    public int StatusId { get; set; } = 1;

}
public class ReceiptInfo
{
    public string ReceiptNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaymentId { get; set; }
}

public class RoomDetailDto {
    public int ReservationRoomDetailsID { get; set; }
    public string? ReservationNo { get; set; }
    public string? RoomCode { get; set; }
    public string? PackageCode { get; set; }
    public int NoOfDays { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public bool IsDelete { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime CheckinDate { get; set; }
    public DateTime CheckoutDate { get; set; }
}

public class ServiceDetailDto {
    public string? ServiceTypeCode { get; set; }
    public DateTime ServiceDate { get; set; }
    public int ServiceQuantity { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal ServiceTotalAmount { get; set; }
    public string? ServiceRemark { get; set; }
}

public class RoomPaymentDetailDto {
    public int PaymentId { get; set; }
    //public string PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string? RefNo { get; set; }
    public DateTime? RefDate { get; set; }
    public string? ReceiptNo { get; set; }
    
    public int PaymentTypeId { get; set; } // Add this to map payment types properly
}




