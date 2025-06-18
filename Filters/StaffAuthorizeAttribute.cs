using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace DVD_Rental.Filters
{
    public class StaffAuthorizeAttribute : AuthorizeAttribute
    {
        public string Permission { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            if (session["StaffId"] == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            if (!string.IsNullOrEmpty(Permission))
            {
                var permissions = session["Permissions"] as List<string>;
                if (permissions == null || !permissions.Contains(Permission))
                {
                    filterContext.Result = new HttpStatusCodeResult(403); // Forbidden
                }
            }
        }
    }
}
