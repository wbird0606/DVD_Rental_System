using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DVD_Rental
{
    public static class RBACHelper
    {
        public static bool IsHeadOffice(int? storeId)
        {
            return storeId == 0;
        }

        public static int? GetStoreId(Controller controller)
        {
            return controller.Session["StoreId"] as int?;
        }

        public static bool HasPermission(Controller controller, string permission)
        {
            var permissions = controller.Session["Permissions"] as List<string>;
            return permissions != null && permissions.Contains(permission);
        }
    }

}