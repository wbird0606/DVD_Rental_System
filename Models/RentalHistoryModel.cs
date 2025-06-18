using System;
using System.Collections.Generic;

public class RentalHistoryModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public List<RentalRecord> Rentals { get; set; } = new List<RentalRecord>();
}

public class RentalRecord
{
    public string FilmTitle { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal Amount { get; set; }
    public int StoreId { get; set; }
}
