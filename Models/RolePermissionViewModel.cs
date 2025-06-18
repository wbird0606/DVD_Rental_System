using System.Collections.Generic;

public class RolePermissionViewModel
{
    public int RoleId { get; set; }
    public List<PermissionModel> Permissions { get; set; }
    public HashSet<int> SelectedPermissionIds { get; set; }
}
