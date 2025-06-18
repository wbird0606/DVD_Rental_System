using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Filters;
using DVD_Rental.Helpers;

namespace DVD_Rental.Controllers
{
    [StaffAuthorize(Permission = "Manage Roles")]
    public class RoleController : Controller
    {
        public ActionResult Index()
        {
            var roles = new List<RoleModel>();
            using (var db = new SqlHelper())
            {
                string sql = "SELECT role_id, role_name FROM role ORDER BY role_id";
                roles = db.ExecuteQuery(sql, null, reader => new RoleModel
                {
                    RoleId = reader.GetInt32(0),
                    RoleName = reader.GetString(1)
                });
            }
            return View(roles);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string roleName)
        {
            using (var db = new SqlHelper())
            {
                string sql = "INSERT INTO role (role_name) VALUES (@roleName)";
                db.ExecuteNonQuery(sql, p => p.AddWithValue("roleName", roleName));
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            RoleModel role = null;
            using (var db = new SqlHelper())
            {
                string sql = "SELECT role_id, role_name FROM role WHERE role_id = @id";
                role = db.ExecuteSingle(sql, p => p.AddWithValue("id", id), reader => new RoleModel
                {
                    RoleId = reader.GetInt32(0),
                    RoleName = reader.GetString(1)
                });
            }
            return View(role);
        }

        [HttpPost]
        public ActionResult Edit(int id, string roleName)
        {
            using (var db = new SqlHelper())
            {
                string sql = "UPDATE role SET role_name = @name WHERE role_id = @id";
                db.ExecuteNonQuery(sql, p =>
                {
                    p.AddWithValue("name", roleName);
                    p.AddWithValue("id", id);
                });
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            using (var db = new SqlHelper())
            {
                string sql = "DELETE FROM role WHERE role_id = @id";
                db.ExecuteNonQuery(sql, p => p.AddWithValue("id", id));
            }
            return RedirectToAction("Index");
        }
    }
}
