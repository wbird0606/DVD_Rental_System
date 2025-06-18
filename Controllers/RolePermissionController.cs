using System.Collections.Generic;
using System.Web.Mvc;
using DVD_Rental.Helpers;
using DVD_Rental.Models;

public class RolePermissionController : Controller
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

    public ActionResult Edit(int roleId)
    {
        var allPermissions = new List<PermissionModel>();
        var selectedPermissionIds = new HashSet<int>();

        using (var db = new SqlHelper())
        {
            // 取得全部權限
            string sql = "SELECT permission_id, permission_name FROM permission";
            allPermissions = db.ExecuteQuery(sql, null, reader => new PermissionModel
            {
                PermissionId = reader.GetInt32(0),
                PermissionName = reader.GetString(1)
            });

            // 取得已勾選權限ID
            sql = "SELECT permission_id FROM role_permission WHERE role_id = @roleId";
            selectedPermissionIds = new HashSet<int>(
                db.ExecuteQuery(sql, p => p.AddWithValue("roleId", roleId), reader => reader.GetInt32(0))
            );
        }

        var vm = new RolePermissionViewModel
        {
            RoleId = roleId,
            Permissions = allPermissions,
            SelectedPermissionIds = selectedPermissionIds
        };

        return View(vm);
    }

    [HttpPost]
    public ActionResult Edit(int roleId, int[] selectedPermissions)
    {
        using (var db = new SqlHelper(true))  // 帶 transaction
        {
            // 刪除舊權限
            string sql = "DELETE FROM role_permission WHERE role_id = @roleId";
            db.ExecuteNonQuery(sql, p => p.AddWithValue("roleId", roleId));

            // 新增新權限
            if (selectedPermissions != null)
            {
                sql = "INSERT INTO role_permission (role_id, permission_id) VALUES (@rid, @pid)";
                foreach (var pid in selectedPermissions)
                {
                    db.ExecuteNonQuery(sql, p =>
                    {
                        p.AddWithValue("rid", roleId);
                        p.AddWithValue("pid", pid);
                    });
                }
            }

            db.Commit();
        }

        return RedirectToAction("Index");
    }
}
