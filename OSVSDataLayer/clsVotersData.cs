using System;
using System.Data;
using System.Data.SqlClient;

namespace OSVSDataLayer
{
    public class clsVotersData
    {
        // Method to Get Voter Info by VoterID
        public static bool GetVoterInfoByID(int VoterID, ref int PersonID, ref bool IsVoted)
        {
            bool isFound = false;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "SELECT * FROM Voters WHERE VoterID = @VoterID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VoterID", VoterID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // The record was found
                    isFound = true;
                    PersonID = (int)reader["PersonID"];
                    IsVoted = (bool)reader["IsVoted"];
                }
                else
                {
                    // No record found
                    isFound = false;
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                isFound = false;
            }
            finally
            {
                connection.Close();
            }

            return isFound;
        }

		public static int GetVoterIDByPersonID(int personID)
		{
			int voterID = -1; // Default for not found
			string query = "SELECT VoterID FROM Voters WHERE PersonID = @PersonID";

			SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
			SqlCommand command = new SqlCommand(query, connection);
			command.Parameters.AddWithValue("@PersonID", personID);

			try
			{
				connection.Open();
				var result = command.ExecuteScalar();

				if (result != null && int.TryParse(result.ToString(), out int id))
				{
					voterID = id;
				}
			}
			catch (Exception ex)
			{
				// Handle exception
			}
			finally
			{
				connection.Close();
			}

			return voterID;
		}

		public static bool IsVoterRegistered(int personID)
		{
			bool isRegistered = false; // Default for not registered
			string query = "SELECT COUNT(*) FROM Voters WHERE PersonID = @PersonID";

			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			using (SqlCommand command = new SqlCommand(query, connection))
			{
				command.Parameters.AddWithValue("@PersonID", personID);

				try
				{
					connection.Open();
					int count = (int)command.ExecuteScalar();
					isRegistered = count > 0; // Returns true if a record exists
				}
				catch (Exception ex)
				{
					// Handle exception (optional: log the error)
				}
			}

			return isRegistered;
		}





		// Method to Add a New Voter
		public static int AddNewVoter(int PersonID, bool IsVoted = false)
        {
            int VoterID = -1;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"INSERT INTO Voters (PersonID, IsVoted)
                             VALUES (@PersonID, @IsVoted);
                             SELECT SCOPE_IDENTITY();";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PersonID", PersonID);
            command.Parameters.AddWithValue("@IsVoted", IsVoted);

            try
            {
                connection.Open();
                object result = command.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int insertedID))
                {
                    VoterID = insertedID;
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            finally
            {
                connection.Close();
            }

            return VoterID;
        }

        // Method to Update a Voter
        public static bool UpdateVoter(int VoterID, int PersonID, bool IsVoted)
        {
            bool isUpdated = false;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"UPDATE Voters
                             SET PersonID = @PersonID, IsVoted = @IsVoted
                             WHERE VoterID = @VoterID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@VoterID", VoterID);
            command.Parameters.AddWithValue("@PersonID", PersonID);
            command.Parameters.AddWithValue("@IsVoted", IsVoted);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                isUpdated = rowsAffected > 0;
            }
            catch (Exception ex)
            {
                isUpdated = false;
            }
            finally
            {
                connection.Close();
            }

            return isUpdated;
        }

        // Method to Delete a Voter
        public static bool DeleteVoter(int VoterID)
        {
            bool isDeleted = false;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "DELETE FROM Voters WHERE VoterID = @VoterID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VoterID", VoterID);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                isDeleted = rowsAffected > 0;
            }
            catch (Exception ex)
            {
                isDeleted = false;
            }
            finally
            {
                connection.Close();
            }

            return isDeleted;
        }

        // Method to Get All Voters (returns DataTable)
        public static DataTable GetAllVoters()
        {
            DataTable dt = new DataTable();
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "SELECT * FROM Voters";

            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    dt.Load(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // Log or handle the exception
            }
            finally
            {
                connection.Close();
            }

            return dt;
        }

		public static bool HasVoted(int voterId)
		{
			bool hasVoted = false;
			string query = "SELECT IsVoted FROM Voters WHERE VoterID = @VoterID";

			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				SqlCommand command = new SqlCommand(query, connection);
				command.Parameters.AddWithValue("@VoterID", voterId);

				try
				{
					connection.Open();
					object result = command.ExecuteScalar();
					if (result != null && result != DBNull.Value)
					{
						hasVoted = Convert.ToBoolean(result);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error checking voting status: " + ex.Message);
				}
			}

			return hasVoted;
		}

		public static bool MarkAsVoted(int personId)
		{
			SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
			string query = "UPDATE Voters SET IsVoted = 1 WHERE PersonID = @PersonID";

			SqlCommand command = new SqlCommand(query, connection);
			command.Parameters.AddWithValue("@PersonID", personId);

			try
			{
				connection.Open();
				int rowsAffected = command.ExecuteNonQuery();
				return rowsAffected > 0; // True if update succeeded
			}
			catch (Exception ex)
			{
				// Handle exception or log
			}
			finally
			{
				connection.Close();
			}

			return false; // Return false if the update failed
		}


	}
}
