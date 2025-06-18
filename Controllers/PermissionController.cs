using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Helpers;
using DVD_Rental.Models;

namespace DVD_Rental.Controllers
{
    [Filters.StaffAuthorizeAttribute(Permission = "Manage Permissions")]
    public class PermissionController : Controller
    {
        public ActionResult Index()
        {
            var sql = "SELECT permission_id, permission_name FROM permission ORDER BY permission_id";
            var permissions = new List<PermissionModel>();

            using (var db = new SqlHelper())
            {
                permissions = db.ExecuteQuery(sql, null, reader => new PermissionModel
                {
                    PermissionId = reader.GetInt32(0),
                    PermissionName = reader.GetString(1)
                });
            }

            return View(permissions);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string permissionName)
        {
            var sql = "INSERT INTO permission (permission_name) VALUES (@permissionName)";
            using (var db = new SqlHelper(true)) // 使用交易
            {
                db.ExecuteNonQuery(sql, p => p.AddWithValue("permissionName", permissionName));
                db.Commit();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var sql = "SELECT permission_id, permission_name FROM permission WHERE permission_id = @id";
            PermissionModel permission = null;

            using (var db = new SqlHelper())
            {
                permission = db.ExecuteSingle(sql, p => p.AddWithValue("id", id), reader => new PermissionModel
                {
                    PermissionId = reader.GetInt32(0),
                    PermissionName = reader.GetString(1)
                });
            }

            return View(permission);
        }

        [HttpPost]
        public ActionResult Edit(int id, string permissionName)
        {
            var sql = "UPDATE permission SET permission_name = @name WHERE permission_id = @id";
            using (var db = new SqlHelper(true)) // 使用交易
            {
                db.ExecuteNonQuery(sql, p =>
                {
                    p.AddWithValue("name", permissionName);
                    p.AddWithValue("id", id);
                });
                db.Commit();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var sql = "DELETE FROM permission WHERE permission_id = @id";
            using (var db = new SqlHelper(true)) // 使用交易
            {
                db.ExecuteNonQuery(sql, p => p.AddWithValue("id", id));
                db.Commit();
            }

            return RedirectToAction("Index");
        }
    }
}
