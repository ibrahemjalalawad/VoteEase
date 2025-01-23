using System;

public static class clsAuditLog
{
  

    public static void LogLogin(int personID, string description)
    {
        var logID = Guid.NewGuid(); // Generate unique LogID
        var logDate = DateTime.Now; // Capture the current timestamp
        var actionType = "Login"; // Specify the action type

        clsAuditLogsData.AddAuditLog(logID, personID, logDate, actionType, description);
    }
}
