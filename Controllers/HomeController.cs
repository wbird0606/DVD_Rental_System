using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DVD_Rental.Helpers;
using DVD_Rental.Models;

namespace DVD_Rental.Controllers
{
    public class HomeController : Controller
    {
        //public ActionResult Index()
        //{
        //    return View();
        //}
        public ActionResult Index()
        {
            var model = new DashboardViewModel();

            using (var db = new SqlHelper())
            {
                // 取得影片總數
                model.TotalFilms = db.ExecuteScalar<Int32>("SELECT COUNT(*) FROM film",null);

                // 取得顧客總數
                model.TotalCustomers = db.ExecuteScalar<Int32>("SELECT COUNT(*) FROM customer", null);

                // 取得租賃中數量
                model.TotalRentals = db.ExecuteScalar<Int32>("SELECT COUNT(*) FROM rental WHERE return_date IS NULL", null);

                // 取得分店數量
                model.TotalStores = db.ExecuteScalar<Int32>("SELECT COUNT(*) FROM store",null);

                // 取得各分店可租影片數 (示範複雜一點的查詢)
                string sql = @"
                SELECT s.store_id, COUNT(i.inventory_id) AS available_inventory
                FROM store s
                LEFT JOIN inventory i ON s.store_id = i.store_id
                LEFT JOIN rental r ON i.inventory_id = r.inventory_id AND r.return_date IS NULL
                WHERE r.rental_id IS NULL
                GROUP BY s.store_id";

                model.StoreInventories = db.ExecuteQuery(sql, null, reader => new StoreInventory
                {
                    StoreId = reader.GetInt32(0),
                    //StoreName = reader.GetString(1),
                    AvailableInventory = reader.GetInt32(1)
                });
            }

            return View(model);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}