namespace ContractApprovalSystem.Core.Enums
{
    public static class ContractStatus
    {
        public const string Draft = "Draft";
        public const string PendingApproval = "PendingApproval";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
    
    public static class UserRoles
    {
        public const string User = "User";
        public const string Manager = "Manager";
    }
}
