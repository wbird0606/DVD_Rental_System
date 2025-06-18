using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Filters;
using DVD_Rental.Helpers;

namespace DVD_Rental.Controllers
{
    [StaffAuthorize(Permission = "Access Reports")]
    public class ReportController : Controller
    {
        public ActionResult Index()
        {
            var categoryStats = new List<Tuple<string, int>>();
            var topFilms = new List<Tuple<string, int>>();
            var storeStats = new List<Tuple<int, int>>();
            int currentStoreId = (int)Session["StoreId"];

            using (var db = new SqlHelper())
            {
                // 1. 類別別租借次數
                string categorySql = @"
                    SELECT c.name, COUNT(r.rental_id)
                    FROM rental r
                    JOIN inventory i ON r.inventory_id = i.inventory_id
                    JOIN film_category fc ON i.film_id = fc.film_id
                    JOIN category c ON fc.category_id = c.category_id
                    GROUP BY c.name ORDER BY COUNT DESC";

                categoryStats = db.ExecuteQuery(categorySql, null, reader =>
                    Tuple.Create(reader.GetString(0), reader.GetInt32(1)));

                // 2. 熱門影片前10名
                string topFilmSql = @"
                    SELECT f.title, COUNT(r.rental_id) AS rentals
                    FROM rental r
                    JOIN inventory i ON r.inventory_id = i.inventory_id
                    JOIN film f ON i.film_id = f.film_id
                    GROUP BY f.title ORDER BY rentals DESC LIMIT 10";

                topFilms = db.ExecuteQuery(topFilmSql, null, reader =>
                    Tuple.Create(reader.GetString(0), reader.GetInt32(1)));

                // 3. 各分店租借總數（分總部員工與分店員工）
                string storeSql;
                if (currentStoreId == 0)
                {
                    // 總部員工看全部分店
                    storeSql = @"
                        SELECT i.store_id, COUNT(r.rental_id)
                        FROM rental r
                        JOIN inventory i ON r.inventory_id = i.inventory_id
                        GROUP BY i.store_id ORDER BY i.store_id";

                    storeStats = db.ExecuteQuery(storeSql, null, reader =>
                        Tuple.Create(reader.GetInt32(0), reader.GetInt32(1)));
                }
                else
                {
                    // 分店員工僅看自己店
                    storeSql = @"
                        SELECT i.store_id, COUNT(r.rental_id)
                        FROM rental r
                        JOIN inventory i ON r.inventory_id = i.inventory_id
                        WHERE i.store_id = @storeId
                        GROUP BY i.store_id";

                    storeStats = db.ExecuteQuery(storeSql, p =>
                        p.AddWithValue("storeId", currentStoreId), reader =>
                        Tuple.Create(reader.GetInt32(0), reader.GetInt32(1)));
                }
            }

            ViewBag.CategoryStats = categoryStats;
            ViewBag.TopFilms = topFilms;
            ViewBag.StoreStats = storeStats;

            return View();
        }
    }
}
