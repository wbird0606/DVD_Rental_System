using DVD_Rental;
using DVD_Rental.Filters;
using DVD_Rental.Helpers;
using DVD_Rental.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

public class RentalController : Controller
{
    [StaffAuthorize]
    public ActionResult Index()
    {
        ViewBag.Customers = GetCustomerList();
        ViewBag.Stores = GetStoreList(RBACHelper.GetStoreId(this));
        return View();
    }

    [StaffAuthorize]
    [HttpPost]
    public ActionResult History(int customerId, int? storeId)
    {
        var sessionStoreId = RBACHelper.GetStoreId(this);
        bool isHeadOffice = RBACHelper.IsHeadOffice(sessionStoreId);
        int? actualStoreId = isHeadOffice ? storeId : sessionStoreId;

        var model = new RentalHistoryModel { CustomerId = customerId };

        using (var db = new SqlHelper())
        {
            // 取得客戶名稱
            model.CustomerName = db.ExecuteScalar<string>(
                "SELECT first_name || ' ' || last_name FROM customer WHERE customer_id = @id",
                p => p.AddWithValue("id", customerId)
            );

            // 查詢租借紀錄
            string sql = @"
                SELECT f.title, r.rental_date, r.return_date, COALESCE(p.amount, 0), s.store_id
                FROM rental r
                JOIN inventory i ON r.inventory_id = i.inventory_id
                JOIN film f ON i.film_id = f.film_id
                JOIN store s ON i.store_id = s.store_id
                LEFT JOIN payment p ON p.rental_id = r.rental_id
                WHERE r.customer_id = @id";

            if (actualStoreId.HasValue)
                sql += " AND s.store_id = @storeId";

            sql += " ORDER BY r.rental_date DESC";

            var rentals = db.ExecuteQuery(sql, p =>
            {
                p.AddWithValue("id", customerId);
                if (actualStoreId.HasValue)
                    p.AddWithValue("storeId", actualStoreId.Value);
            }, reader => new RentalRecord
            {
                FilmTitle = reader.GetString(0),
                RentalDate = reader.GetDateTime(1),
                ReturnDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                Amount = reader.GetDecimal(3),
                StoreId = reader.GetInt32(4)
            });

            model.Rentals.AddRange(rentals);
        }

        ViewBag.SelectedStoreId = actualStoreId;
        ViewBag.Stores = GetStoreList(sessionStoreId);
        return View("History", model);
    }

    private List<SelectListItem> GetCustomerList()
    {
        var sql = "SELECT customer_id, first_name || ' ' || last_name FROM customer ORDER BY first_name";

        using (var db = new SqlHelper())
        {
            return db.ExecuteQuery(sql, null, reader => new SelectListItem
            {
                Value = reader.GetInt32(0).ToString(),
                Text = reader.GetString(1)
            });
        }
    }

    private List<SelectListItem> GetStoreList(int? sessionStoreId)
    {
        var sql = "SELECT store_id FROM store ORDER BY store_id";
        var list = new List<SelectListItem>();

        if (RBACHelper.IsHeadOffice(sessionStoreId))
        {
            list.Add(new SelectListItem { Value = "", Text = "所有分店" });
        }

        using (var db = new SqlHelper())
        {
            var stores = db.ExecuteQuery(sql, null, reader => reader.GetInt32(0));

            foreach (var id in stores)
            {
                if (RBACHelper.IsHeadOffice(sessionStoreId) || sessionStoreId == id)
                {
                    list.Add(new SelectListItem
                    {
                        Value = id.ToString(),
                        Text = $"分店 #{id}"
                    });
                }
            }
        }

        return list;
    }
}
