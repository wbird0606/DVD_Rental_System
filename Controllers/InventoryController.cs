using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Helpers;
using DVD_Rental.Models;

namespace DVD_Rental.Controllers
{
    [Filters.StaffAuthorizeAttribute(Permission = "Manage Inventory")]
    public class InventoryController : Controller
    {
        public ActionResult Index()
        {
            var filmList = new List<SelectListItem>();

            using (var db = new SqlHelper())
            {
                var sql = "SELECT film_id, title FROM film ORDER BY title";
                filmList = db.ExecuteQuery(sql, null, reader => new SelectListItem
                {
                    Value = reader.GetInt32(0).ToString(),
                    Text = reader.GetString(1)
                });
            }

            ViewBag.FilmList = filmList;
            return View();
        }

        [HttpPost]
        public ActionResult Query(int filmId)
        {
            var model = new DVD_Rental.Models.InventoryStatusModel { FilmId = filmId };
            int storeId = Session["StoreId"] != null ? (int)Session["StoreId"] : 0;

            using (var db = new SqlHelper())
            {
                // 取得影片標題
                model.Title = db.ExecuteScalar<string>(
                    "SELECT title FROM film WHERE film_id = @filmId",
                    p => p.AddWithValue("filmId", filmId)
                );

                // 查詢可用庫存
                string sql = @"
                    SELECT i.store_id, COUNT(*) AS available
                    FROM inventory i
                    WHERE i.film_id = @filmId
                    AND NOT EXISTS (
                        SELECT 1 FROM rental r
                        WHERE r.inventory_id = i.inventory_id AND r.return_date IS NULL
                    )
                    /**store_clause**/
                    GROUP BY i.store_id";

                if (storeId != 0)
                    sql = sql.Replace("/**store_clause**/", "AND i.store_id = @storeId");
                else
                    sql = sql.Replace("/**store_clause**/", "");

                model.StoreInventories = db.ExecuteQuery(sql,
                    p =>
                    {
                        p.AddWithValue("filmId", filmId);
                        if (storeId != 0)
                            p.AddWithValue("storeId", storeId);
                    },
                    reader => new StoreInventory
                    {
                        StoreId = reader.GetInt32(0),
                        Available = reader.GetInt32(1)
                    });
            }

            return View("Result", model);
        }
    }
}
