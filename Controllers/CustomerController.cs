using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Models;
using DVD_Rental.Helpers;

namespace DVD_Rental.Controllers
{
    [Filters.StaffAuthorizeAttribute(Permission = "View Customers")]
    public class CustomerController : Controller
    {
        public ActionResult Index()
        {
            var customers = new List<CustomerViewModel>();
            int? storeId = RBACHelper.GetStoreId(this);

            string sql = @"
                SELECT c.customer_id, c.first_name || ' ' || c.last_name AS name,
                       c.email, c.active, s.store_id,
                       a.address, ci.city, co.country,
                       c.create_date
                FROM customer c
                JOIN store s ON c.store_id = s.store_id
                JOIN address a ON c.address_id = a.address_id
                JOIN city ci ON a.city_id = ci.city_id
                JOIN country co ON ci.country_id = co.country_id
                /**where_clause**/
                ORDER BY c.customer_id";

            if (!RBACHelper.IsHeadOffice(storeId))
                sql = sql.Replace("/**where_clause**/", "WHERE s.store_id = @storeId");
            else
                sql = sql.Replace("/**where_clause**/", "");

            using (var db = new SqlHelper())
            {
                customers = db.ExecuteQuery(
                    sql,
                    p =>
                    {
                        if (!RBACHelper.IsHeadOffice(storeId))
                            p.AddWithValue("storeId", storeId.Value);
                    },
                    reader => new CustomerViewModel
                    {
                        CustomerId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        IsActive = reader.GetInt32(3) == 1,
                        StoreId = reader.GetInt32(4),
                        Address = reader.GetString(5),
                        City = reader.GetString(6),
                        Country = reader.GetString(7),
                        CreateDate = reader.GetDateTime(8)
                    }
                );
            }

            return View(customers);
        }
    }
}
