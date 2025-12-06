public class ReservationCalendarDto
{
    public int RoomID { get; set; }
    public string RoomCode { get; set; }
    public string Description { get; set; }
    public string RoomSize { get; set; }
    public bool IsRoom { get; set; }
    public bool IsBanquet { get; set; }
    public DateTime DateValue { get; set; }
    public string DisplayDate { get; set; }
    public string DayName { get; set; }
    public string ReservationNo { get; set; }
    public DateTime? ReservationDate { get; set; }
    public string ReservationStatus { get; set; }
    public int? StatusId { get; set; }
    public string CustomerCode { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public DateTime? CheckinDateTime { get; set; }
    public DateTime? CheckoutDateTime { get; set; }
    public decimal? GrossAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? DueAmount { get; set; }
    public string CustomerName { get; set; }
    public string CustomerTitle { get; set; }
    public string ColorCode { get; set; }
    public string StatusName { get; set; }
    public string ReservedRoomCode { get; set; }
    public DateTime? CheckinDate { get; set; }
    public DateTime? CheckoutDate { get; set; }
    public decimal? RoomAmount { get; set; }
    public decimal? Price { get; set; }
    public int? NoOfDays { get; set; }
}
