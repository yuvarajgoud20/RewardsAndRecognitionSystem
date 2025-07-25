namespace RewardsAndRecognitionSystem.Common
{
    public static class GeneralMessages
    {
        //Analytics controller
        public static string Analytics_Error = "No active quarter is set in the system.";
        public static string Quarter_Not_Selected = "No quarter selected.";
        public static string No_Nominations_This_Quarter = "No nominations were made in this quarter.";
        public static string No_Data_For_Quarter = "No data available for this quarter.";
        public static string Unauthorized_User = "Unauthorized user.";
        //
        public static string NotAvailable_Error = "N/A";
        public static string No_Active_Quarter = "❌ No active quarter is set.";
        public static string Nomation_Approved = "🎉 Your Nomination is Approved!";
        public static string Selected_Award = "🎖️ You Have Been Selected for an Award!";
        public static string Nomination_Reverted = "Nomination Reverted";
        //user controller
       
    }
    public static class GeneralMessages_User
    {
        public const string NoRoleError = "No role assigned to user.";
        public const string TeamChangeError = "Team cannot be changed for TeamLead or Manager roles.";
        public const string UserNotFoundError = "User not found.";
        public const string UnknownRole = "Unknown";
        public const string EmailSubjectWelcome = "🎉 Welcome to Rewards and Recognition!";
        public const string EmailFooter = "— This email was sent from the Rewards & Recognition system";
        public const string LoginInstructions = "Please log in and change your password immediately to secure your account.";
    }
}
