namespace RewardsAndRecognitionSystem.Common
{
    public static class GeneralMessages
    {
        //Analytics controller
        public static string Analytics_Error = "No active quarter is set in the system.";
        public static string Quarter_Not_Selected = "No quarter selected.";
        public static string No_Nominations_This_Quarter = "No Graphs to Display";
        public static string No_Data_For_Quarter = "No data available for this quarter.";
        public static string Unauthorized_User = "Unauthorized user.";
        public static string No_Nominations_Found = "No nominations found.";
        // There are currently no nominations for this quarter.
        public static string NotAvailable_Error = "N/A";
        public static string No_Valid_Quarter = "❌ No valid quarter found.";
        public static string No_Active_Quarter = "❌ No active quarter is set.";
        public static string Nomation_Approved = "🎉 Your Nomination is Approved!";
        public static string Selected_Award = "🎖️ You Have Been Selected for an Award!";
        public static string Nomination_Reverted = "Nomination Reverted";
        public static string No_Team_Assigned_TeamLead = "No team assigned to you as Team Lead.";
        //user controller
        public static string DuplicateCategory = "Category Already Exists";
        public static string DuplicateNomination = "Please select another category — this person is already nominated in the chosen category for this quarter.";
        public static string ApprovalError = "Invalid approval action.";
        public static string InvalidQuarterError = "Invalid YearQuarter data";
        public static string InvalidExcelData = "Invalid Excel Data";
        public static string ExcelDuplicateData = "Some nominations in Excel already Exists";
        public static string SoftDeleteUserError = "Unable to soft delete the user.";
        public static string DuplicateYearQuarterError = "Year + Quarter combination already exists. Please select another.";
        public static string ExistsQuarterError = "A quarter already exists within this date range.";
        public static string QuarterRangeError = "A quarter already exists within this date range."
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

    public static class ToastMessages_User
    {
        public const string CreateUser = "Successfully Created User";
        public const string UpdateUser = "Successfully Updates User";
        public const string DeleteUser = "Successfully Deleted User";       
    }

    public static class ToastMessages_Team
    {
        public const string CreateTeam = "Successfully Created Team";
        public const string UpdateTeam = "Successfully Updated Team";
        public const string DeleteTeam = "Successfully Deleted Team";

    }

    public static class ToastMessages_Category
    {
        public const string CreateCategory = "Successfully Created Category";
        public const string UpdateCategory = "Successfully Updated Category";
        public const string DeleteCategory = "Successfully Deleted Category";

    }

    public static class ToastMessages_YearQuarter
    {
        public const string CreateYearQuarter = "Successfully Created YearQuarter";
        public const string UpdateYearQuarter = "Successfully Updated YearQuarter";
        public const string DeleteYearQuarter = "Successfully Deleted YearQuartery";
    }

    public static class ToastMessages_Nomination
    {
        public const string CreateNomination = "Successfully Created Nomination";
        public const string UpdateNomination = "Successfully Updated Nomination";
        public const string DeleteNomination = "Successfully Deleted Nomination";
        public const string ApproveNomination = "Successfully Approved Nomination";
        public const string RejectNomination = "Successfully Rejected Nomination";
        public const string RevertNomination = "Successfully Reverted Nomination";
    }

    public static class Excel_Messages
    {
        public const string ValidExcel = "Please upload a valid Excel file.";
        public const string SaveExcelData = "Nominations saved successfully.";
    }
}
