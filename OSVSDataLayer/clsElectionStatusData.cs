using System;
using System.Data;
using System.Data.SqlClient;

namespace OSVSDataLayer
{
	public class ElectionStatus
	{
		public int StatusID { get; set; }
		public bool IsElectionActive { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public static class clsElectionStatusData
	{
		private static string connectionString = clsDataAccessSettings.ConnectionString;

		// Retrieve the current election status
		public static ElectionStatus GetCurrentElectionStatus()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				var query = "SELECT TOP 1 StatusID, IsElectionActive, StartDate, EndDate FROM ElectionStatus ORDER BY StatusID DESC";
				var command = new SqlCommand(query, connection);
				var reader = command.ExecuteReader();

				if (reader.Read())
				{
					return new ElectionStatus
					{
						StatusID = reader.GetInt32(0),
						IsElectionActive = reader.GetBoolean(1),
						StartDate = reader.GetDateTime(2),
						EndDate = reader.GetDateTime(3)
					};
				}
			}

			return null; // Return null if no election status is found
		}

		// Update the election status
		public static void UpdateElectionStatus(bool isElectionActive, DateTime startDate, DateTime endDate)
		{
			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();

				var updateQuery = @"
                UPDATE ElectionStatus
                SET IsElectionActive = @IsElectionActive,
                    StartDate = @StartDate,
                    EndDate = @EndDate
                WHERE StatusID = 1"; // Assuming there's only one row for the current election status.

				using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
				{
					updateCommand.Parameters.AddWithValue("@IsElectionActive", isElectionActive);
					updateCommand.Parameters.AddWithValue("@StartDate", startDate);
					updateCommand.Parameters.AddWithValue("@EndDate", endDate);

					updateCommand.ExecuteNonQuery();
				}
			}
		}

		// Insert a new election status
		public static void InsertElectionStatus(bool isActive, DateTime startDate, DateTime endDate)
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				var query = @"
                    INSERT INTO ElectionStatus (IsElectionActive, StartDate, EndDate)
                    VALUES (@IsElectionActive, @StartDate, @EndDate)";
				var command = new SqlCommand(query, connection);
				command.Parameters.AddWithValue("@IsElectionActive", isActive);
				command.Parameters.AddWithValue("@StartDate", startDate);
				command.Parameters.AddWithValue("@EndDate", endDate);
				command.ExecuteNonQuery();
			}
		}

		public static void EndElection()
		{
			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();
				var query = "UPDATE ElectionStatus SET IsElectionActive = 0 WHERE IsElectionActive = 1";
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.ExecuteNonQuery();
				}
			}
		}

	}
}
