using OSVSDataLayer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OSVSDataLayer
{
    public class clsCandidatesData
    {
        // Method to insert a new candidate
        public static int AddNewCandidate(int personID, string bio)
        {
            int candidateID = -1; // Will hold the new CandidateID if successful
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"INSERT INTO Candidates (PersonID, Bio)
                                 VALUES (@PersonID, @Bio);
                                 SELECT SCOPE_IDENTITY();"; // Get the newly inserted CandidateID

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PersonID", personID);
                command.Parameters.AddWithValue("@Bio", bio ?? (object)DBNull.Value); // Handle nulls for Bio

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int insertedID))
                    {
                        candidateID = insertedID; // Get the new CandidateID
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception (optional: log the error)
                }
                finally
                {
                    connection.Close();
                }
            }
            return candidateID;
        }

        // Method to update an existing candidate's information
        public static bool UpdateCandidate(int candidateID, int personID, string bio)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"UPDATE Candidates 
                                 SET PersonID = @PersonID, Bio = @Bio 
                                 WHERE CandidateID = @CandidateID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CandidateID", candidateID);
                command.Parameters.AddWithValue("@PersonID", personID);
                command.Parameters.AddWithValue("@Bio", bio ?? (object)DBNull.Value); // Handle nulls for Bio

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Handle exception (optional: log the error)
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
            return (rowsAffected > 0);
        }

        // Method to get candidate information by ID
        public static DataTable GetCandidateByID(int candidateID)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"SELECT CandidateID, PersonID, Bio 
                                 FROM Candidates 
                                 WHERE CandidateID = @CandidateID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CandidateID", candidateID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        dt.Load(reader); // Load the data into the DataTable
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    // Handle exception (optional: log the error)
                }
                finally
                {
                    connection.Close();
                }
            }
            return dt;
        }

        // Method to delete a candidate by ID
        public static bool DeleteCandidate(int candidateID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"DELETE FROM Candidates 
                                 WHERE CandidateID = @CandidateID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CandidateID", candidateID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Handle exception (optional: log the error)
                }
                finally
                {
                    connection.Close();
                }
            }
            return (rowsAffected > 0);
        }

        // Method to get all candidates
        public static DataTable GetAllCandidates()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"SELECT CandidateID, PersonID, Bio 
                                 FROM Candidates 
                                 ORDER BY CandidateID";

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
                    // Handle exception (optional: log the error)
                }
                finally
                {
                    connection.Close();
                }
            }
            return dt;
        }

		public static List<Candidate> GetCandidates()
		{
			var candidates = new List<Candidate>();

			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();
				var query = @"
            SELECT c.CandidateID, c.PersonID, c.Bio, p.FirstName, p.SecondName, p.ThirdName, p.LastName, p.ImagePath, c.CandidateLink
            FROM Candidates c
            INNER JOIN People p ON c.PersonID = p.PersonID";
				var command = new SqlCommand(query, connection);
				var reader = command.ExecuteReader();

				while (reader.Read())
				{
					candidates.Add(new Candidate
					{
						Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // CandidateID
						PersonId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1), // PersonID
						Bio = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), // Bio
						FullName = $"{(reader.IsDBNull(3) ? string.Empty : reader.GetString(3))} " +
								   $"{(reader.IsDBNull(4) ? string.Empty : reader.GetString(4))} " +
								   $"{(reader.IsDBNull(5) ? string.Empty : reader.GetString(5))} " +
								   $"{(reader.IsDBNull(6) ? string.Empty : reader.GetString(6))}", // FullName
						ImagePath = reader.IsDBNull(7) ? string.Empty : reader.GetString(7), // ImagePath
						CandidateLink = reader.IsDBNull(8) ? null : reader.GetString(8) 
					});
				}
			}

			return candidates;
		}


		public static List<Candidate> GetCandidatesWithVotes()
		{
			var candidates = new List<Candidate>();

			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();
				var query = @"
            SELECT c.CandidateID, c.PersonID, c.Bio, c.IsWinner, p.FirstName, p.SecondName, p.ThirdName, p.LastName, p.ImagePath,
                   (SELECT COUNT(*) FROM Votes v WHERE v.CandidateID = c.CandidateID) AS VotesCount, c.CandidateLink
            FROM Candidates c
            INNER JOIN People p ON c.PersonID = p.PersonID
            ORDER BY VotesCount DESC";
				var command = new SqlCommand(query, connection);
				var reader = command.ExecuteReader();

				while (reader.Read())
				{
					candidates.Add(new Candidate
					{
						Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // CandidateID
						PersonId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1), // PersonID
						Bio = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), // Bio
						IsWinner = reader.IsDBNull(3) ? false : reader.GetBoolean(3), // IsWinner
						FullName = $"{(reader.IsDBNull(4) ? string.Empty : reader.GetString(4))} " +
								   $"{(reader.IsDBNull(5) ? string.Empty : reader.GetString(5))} " +
								   $"{(reader.IsDBNull(6) ? string.Empty : reader.GetString(6))} " +
								   $"{(reader.IsDBNull(7) ? string.Empty : reader.GetString(7))}", // FullName
						ImagePath = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // ImagePath
						VotesCount = reader.IsDBNull(9) ? 0 : reader.GetInt32(9), // VotesCount
						CandidateLink = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)

					});
				}
			}

			return candidates;
		}



        public static List<Candidate> GetWinningCandidates()
        {
            var candidates = new List<Candidate>();

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();
                var query = @"
            SELECT c.CandidateID, c.PersonID, c.Bio, c.IsWinner, p.FirstName, p.SecondName, p.ThirdName, p.LastName, p.ImagePath,
                   (SELECT COUNT(*) FROM Votes v WHERE v.CandidateID = c.CandidateID) AS VotesCount, c.CandidateLink
            FROM Candidates c
            INNER JOIN People p ON c.PersonID = p.PersonID
            WHERE c.IsWinner = 1
            ORDER BY VotesCount DESC";
                var command = new SqlCommand(query, connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    candidates.Add(new Candidate
                    {
                        Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // CandidateID
                        PersonId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1), // PersonID
                        Bio = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), // Bio
                        IsWinner = reader.IsDBNull(3) ? false : reader.GetBoolean(3), // IsWinner
                        FullName = $"{(reader.IsDBNull(4) ? string.Empty : reader.GetString(4))} " +
                                   $"{(reader.IsDBNull(5) ? string.Empty : reader.GetString(5))} " +
                                   $"{(reader.IsDBNull(6) ? string.Empty : reader.GetString(6))} " +
                                   $"{(reader.IsDBNull(7) ? string.Empty : reader.GetString(7))}", // FullName
                        ImagePath = reader.IsDBNull(8) ? string.Empty : reader.GetString(8), // ImagePath
                        VotesCount = reader.IsDBNull(9) ? 0 : reader.GetInt32(9), // VotesCount
                        CandidateLink = reader.IsDBNull(10) ? null : reader.GetString(10) // Fixed index for CandidateLink
                    });
                }


                return candidates;
            }
        }



		public static void ToggleWinnerStatus(int candidateId)
		{
			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();
				var query = @"
            UPDATE Candidates 
            SET IsWinner = CASE WHEN IsWinner = 1 THEN 0 ELSE 1 END
            WHERE CandidateID = @CandidateID";

				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@CandidateID", candidateId);
					command.ExecuteNonQuery();
				}
			}
		}





		public static void RemoveCandidate(int candidateId)
		{
			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();
				var query = "DELETE FROM Candidates WHERE CandidateID = @CandidateID";
				var command = new SqlCommand(query, connection);
				command.Parameters.AddWithValue("@CandidateID", candidateId);

				command.ExecuteNonQuery();
			}
		}

		public static void AddCandidate(int personId, string bio, string candidateLink)
		{
			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();
				var query = "INSERT INTO Candidates (PersonID, Bio, CandidateLink) VALUES (@PersonID, @Bio, @CandidateLink)";
				var command = new SqlCommand(query, connection);
				command.Parameters.AddWithValue("@PersonID", personId);
				if (string.IsNullOrEmpty(bio))
				{
					command.Parameters.AddWithValue("@Bio", DBNull.Value);
				}
				else
				{
					command.Parameters.AddWithValue("@Bio", bio);
				}

				if (string.IsNullOrEmpty(candidateLink))
				{
					command.Parameters.AddWithValue("@CandidateLink", DBNull.Value);
				}
				else
				{
					command.Parameters.AddWithValue("@CandidateLink", candidateLink);
				}

				command.ExecuteNonQuery();
			}
		}

		public static void ClearAllCandidates()
		{
			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				connection.Open();

				// Delete votes first to maintain referential integrity
				var deleteVotesQuery = "DELETE FROM Votes";
				using (SqlCommand deleteVotesCommand = new SqlCommand(deleteVotesQuery, connection))
				{
					deleteVotesCommand.ExecuteNonQuery();
				}

				// Then delete candidates
				var deleteCandidatesQuery = "DELETE FROM Candidates";
				using (SqlCommand deleteCandidatesCommand = new SqlCommand(deleteCandidatesQuery, connection))
				{
					deleteCandidatesCommand.ExecuteNonQuery();
				}
			}


		}

		public static bool UpdateCandidateBioInDatabase(int candidateId, string updatedBio)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					connection.Open();

					// SQL query to update the candidate's bio
					var query = @"
                UPDATE Candidates
                SET Bio = @UpdatedBio
                WHERE CandidateID = @CandidateId";

					using (SqlCommand command = new SqlCommand(query, connection))
					{
						// Add parameters to prevent SQL injection
						command.Parameters.AddWithValue("@UpdatedBio", updatedBio);
						command.Parameters.AddWithValue("@CandidateId", candidateId);

						// Execute the query
						int rowsAffected = command.ExecuteNonQuery();

						// Return true if at least one row was updated
						return rowsAffected > 0;
					}
				}
			}
			catch (Exception ex)
			{
				// Log the exception for debugging purposes
				Console.WriteLine($"Error updating candidate bio: {ex.Message}");
				return false;
			}
		}


	}
}
