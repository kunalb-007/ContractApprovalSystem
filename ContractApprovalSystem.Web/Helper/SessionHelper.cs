namespace ContractApprovalSystem.Web.Helpers
{
    public static class SessionHelper
    {
        public static int? GetUserId(HttpContext context)
        {
            var userIdStr = context.Session.GetString("UserId");
            return int.TryParse(userIdStr, out int userId) ? userId : null;
        }
        
        public static string GetUserRole(HttpContext context)
        {
            return context.Session.GetString("UserRole");
        }
        
        public static string GetUserName(HttpContext context)
        {
            return context.Session.GetString("UserName");
        }
        
        public static bool IsAuthenticated(HttpContext context)
        {
            return GetUserId(context).HasValue;
        }
        
        public static bool IsManager(HttpContext context)
        {
            return GetUserRole(context) == "Manager";
        }
    }
}
