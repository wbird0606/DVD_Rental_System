using System.Collections.Generic;

namespace DVD_Rental.Models
{
    public class DashboardViewModel
    {
        public int TotalFilms { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalRentals { get; set; }
        public int TotalStores { get; set; }
        public List<StoreInventory> StoreInventories { get; set; } = new List<StoreInventory>();
    }
    public class InventoryStatusModel
    {
        public int FilmId { get; set; }
        public string Title { get; set; }
        public List<StoreInventory> StoreInventories { get; set; } = new List<StoreInventory>();
    }
    public class StoreInventory
    {
        public int FilmId { get; set; } 
        public int StoreId { get; set; }
        //public string StoreName { get; set; }
        public int AvailableInventory { get; set; }
        public int Available { get; internal set; }
    }
}
