using OSVSDataLayer;
using System;
using System.Data.SqlClient;

public static class clsAuditLogsData
{
  
    public static void AddAuditLog(Guid logID, int personID, DateTime logDate, string actionType, string description)
    {
        using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
        {
            var query = @"
                INSERT INTO AuditLogs (LogID, PersonID, LogDate, ActionType, Description)
                VALUES (@LogID, @PersonID, @LogDate, @ActionType, @Description)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@LogID", logID);
                command.Parameters.AddWithValue("@PersonID", personID);
                command.Parameters.AddWithValue("@LogDate", logDate);
                command.Parameters.AddWithValue("@ActionType", actionType);
                command.Parameters.AddWithValue("@Description", description);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
