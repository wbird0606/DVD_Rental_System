using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using DVD_Rental.Helpers;

namespace DVD_Rental.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string hashedPassword = ToSHA1(password);

            using (var db = new SqlHelper())
            {
                var result = db.ExecuteQuery("SELECT staff_id, store_id FROM staff WHERE username = @username AND password = @password",
                    p =>
                    {
                        p.AddWithValue("username", username);
                        p.AddWithValue("password", hashedPassword);
                    },
                    reader => new
                    {
                        StaffId = reader.GetInt32(0),
                        StoreId = reader.GetInt32(1)
                    }
                );

                if (result.Count > 0)
                {
                    var user = result[0];
                    Session["StaffId"] = user.StaffId;
                    Session["StoreId"] = user.StoreId;

                    LoadRolesAndPermissions(user.StaffId);
                    Session["StaffName"] = GetStaffNameById(user.StaffId);

                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "帳號或密碼錯誤。";
            return View();
        }

        private string GetStaffNameById(int staffId)
        {
            using (var db = new SqlHelper())
            {
                return db.ExecuteScalar<string>(
                    "SELECT first_name || ' ' || last_name FROM staff WHERE staff_id = @id",
                    p => p.AddWithValue("id", staffId)
                ) ?? "Unknown";
            }
        }

        private void LoadRolesAndPermissions(int staffId)
        {
            var roles = new List<string>();
            var permissions = new List<string>();

            using (var db = new SqlHelper())
            {
                var results = db.ExecuteQuery(@"
                    SELECT r.role_name, p.permission_name
                    FROM staff_role sr
                    JOIN role r ON sr.role_id = r.role_id
                    JOIN role_permission rp ON r.role_id = rp.role_id
                    JOIN permission p ON rp.permission_id = p.permission_id
                    WHERE sr.staff_id = @staffId",
                    p => p.AddWithValue("staffId", staffId),
                    reader => new
                    {
                        Role = reader.GetString(0),
                        Permission = reader.GetString(1)
                    }
                );

                foreach (var row in results)
                {
                    if (!roles.Contains(row.Role)) roles.Add(row.Role);
                    if (!permissions.Contains(row.Permission)) permissions.Add(row.Permission);
                }
            }

            Session["Roles"] = roles;
            Session["Permissions"] = permissions;
        }

        public static string ToSHA1(string plainText)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] hashBytes = sha1.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
