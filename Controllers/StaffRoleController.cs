using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Helpers;
using DVD_Rental.Models;

namespace DVD_Rental.Controllers
{
    public class StaffRoleController : Controller
    {
        public ActionResult Index()
        {
            var staffRoles = new List<StaffRoleModel>();
            using (var db = new SqlHelper())
            {
                string sql = @"
                    SELECT sr.staff_role_id, s.staff_id, s.first_name || ' ' || s.last_name AS staff_name,
                           r.role_id, r.role_name
                    FROM staff_role sr
                    JOIN staff s ON sr.staff_id = s.staff_id
                    JOIN role r ON sr.role_id = r.role_id
                    ORDER BY sr.staff_role_id";

                staffRoles = db.ExecuteQuery(sql, null, reader => new StaffRoleModel
                {
                    StaffRoleId = reader.GetInt32(0),
                    StaffId = reader.GetInt32(1),
                    StaffName = reader.GetString(2),
                    RoleId = reader.GetInt32(3),
                    RoleName = reader.GetString(4)
                });
            }
            return View(staffRoles);
        }

        public ActionResult Create()
        {
            ViewBag.StaffList = GetStaffList();
            ViewBag.RoleList = GetRoleList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(int staffId, int roleId)
        {
            using (var db = new SqlHelper())
            {
                string sql = "INSERT INTO staff_role (staff_id, role_id) VALUES (@staffId, @roleId)";
                db.ExecuteNonQuery(sql, p =>
                {
                    p.AddWithValue("staffId", staffId);
                    p.AddWithValue("roleId", roleId);
                });
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            using (var db = new SqlHelper())
            {
                string sql = "DELETE FROM staff_role WHERE staff_role_id = @id";
                db.ExecuteNonQuery(sql, p => p.AddWithValue("id", id));
            }
            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetStaffList()
        {
            using (var db = new SqlHelper())
            {
                string sql = "SELECT staff_id, first_name || ' ' || last_name FROM staff ORDER BY staff_id";
                return db.ExecuteQuery(sql, null, reader => new SelectListItem
                {
                    Value = reader.GetInt32(0).ToString(),
                    Text = reader.GetString(1)
                });
            }
        }

        private List<SelectListItem> GetRoleList()
        {
            using (var db = new SqlHelper())
            {
                string sql = "SELECT role_id, role_name FROM role ORDER BY role_id";
                return db.ExecuteQuery(sql, null, reader => new SelectListItem
                {
                    Value = reader.GetInt32(0).ToString(),
                    Text = reader.GetString(1)
                });
            }
        }
    }
}
