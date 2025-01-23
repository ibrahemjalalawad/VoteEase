using System;
using System.Data;
using System.Data.SqlClient;

namespace OSVSDataLayer
{
    public class clsVotesData
    {
		public static int AddVoteWithToken(int CandidateID, int VoterID)
		{
			int VoteID = -1;

			string connectionString = clsDataAccessSettings.ConnectionString;

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				string query = @"
            BEGIN TRANSACTION;

            -- Generate a unique token for the voter
            DECLARE @Token NVARCHAR(50) = NEWID();

            INSERT INTO VotingTokens (VoterID, Token, IsUsed, GeneratedAt, TokenExpiry)
            VALUES (@VoterID, @Token, 1, GETDATE(), DATEADD(DAY, 1, GETDATE()));

            -- Insert the vote
            INSERT INTO Votes (CandidateID, Token, VoteTimestamp)
            VALUES (@CandidateID, @Token, @VoteTimestamp);

            SELECT SCOPE_IDENTITY();

            COMMIT TRANSACTION;";

				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@CandidateID", CandidateID);
					command.Parameters.AddWithValue("@VoterID", VoterID);
					command.Parameters.AddWithValue("@VoteTimestamp", DateTime.Now);

					try
					{
						connection.Open();
						object result = command.ExecuteScalar();

						if (result != null && int.TryParse(result.ToString(), out int insertedID))
						{
							VoteID = insertedID; // Return the new VoteID
						}
					}
					catch (Exception ex)
					{
						// Log or handle the exception
					}
				}
			}

			return VoteID; // Return the VoteID (or -1 if failed)
		}



		public static DataTable GetVotesByCandidateID(int CandidateID)
        {
            DataTable dt = new DataTable();
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            string query = "SELECT * FROM Votes WHERE CandidateID = @CandidateID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CandidateID", CandidateID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    dt.Load(reader); // Load the results into the DataTable
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // Handle the exception (optional logging can be added)
            }
            finally
            {
                connection.Close();
            }

            return dt; // Return the DataTable containing the votes for the candidate
        }






        //You can't Delete a vote but this is just in case
        public static bool DeleteVote(int VoteID)
        {
            int rowsAffected = 0;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            string query = @"DELETE FROM Votes WHERE VoteID = @VoteID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@VoteID", VoteID);

            try
            {
                connection.Open();
                rowsAffected = command.ExecuteNonQuery(); // Execute the delete operation
            }
            catch (Exception ex)
            {
                // Handle the exception (optional logging can be added)
            }
            finally
            {
                connection.Close();
            }

            return (rowsAffected > 0); // Return true if the vote was deleted successfully
        }
    }
}
