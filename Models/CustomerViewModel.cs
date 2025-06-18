using System;

namespace DVD_Rental.Models
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int StoreId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
