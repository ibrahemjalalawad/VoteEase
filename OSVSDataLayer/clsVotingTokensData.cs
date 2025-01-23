using System;
using System.Data;
using System.Data.SqlClient;

namespace OSVSDataLayer
{
    public class clsVotingTokensData
    {
        // Method to generate a new voting token
        public static int GenerateToken(int voterID, string token, DateTime generatedAt, DateTime tokenExpiry)
        {
            int tokenID = -1; // Will hold the new TokenID if successful
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"INSERT INTO VotingTokens (VoterID, Token, IsUsed, GeneratedAt, TokenExpiry)
                                 VALUES (@VoterID, @Token, @IsUsed, @GeneratedAt, @TokenExpiry);
                                 SELECT SCOPE_IDENTITY();"; // Get the newly inserted TokenID

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@VoterID", voterID);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@IsUsed", false); // Initially, the token is not used
                command.Parameters.AddWithValue("@GeneratedAt", generatedAt);
                command.Parameters.AddWithValue("@TokenExpiry", tokenExpiry);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int insertedID))
                    {
                        tokenID = insertedID; // Get the new TokenID
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
            return tokenID;
        }

        // Method to mark a token as used
        public static bool UseToken(int tokenID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"UPDATE VotingTokens 
                                 SET IsUsed = @IsUsed 
                                 WHERE TokenID = @TokenID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TokenID", tokenID);
                command.Parameters.AddWithValue("@IsUsed", true); // Mark as used

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

        // Method to check if a token is valid
        public static bool IsTokenValid(string token)
        {
            bool isValid = false;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"SELECT COUNT(*) 
                                 FROM VotingTokens 
                                 WHERE Token = @Token AND IsUsed = @IsUsed AND TokenExpiry > @CurrentDate";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@IsUsed", false); // Check if the token is not used
                command.Parameters.AddWithValue("@CurrentDate", DateTime.Now); // Check if not expired

                try
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    isValid = (count > 0); // Token is valid if count > 0
                }
                catch (Exception ex)
                {
                    // Handle exception (optional: log the error)
                    isValid = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            return isValid;
        }

        // Method to get a token by ID
        public static DataTable GetTokenByID(int tokenID)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"SELECT TokenID, VoterID, Token, IsUsed, GeneratedAt, TokenExpiry 
                                 FROM VotingTokens 
                                 WHERE TokenID = @TokenID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TokenID", tokenID);

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

        // Method to delete a token by ID
        public static bool DeleteToken(int tokenID)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"DELETE FROM VotingTokens 
                                 WHERE TokenID = @TokenID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TokenID", tokenID);

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

        // Method to get all tokens
        public static DataTable GetAllTokens()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"SELECT TokenID, VoterID, Token, IsUsed, GeneratedAt, TokenExpiry 
                                 FROM VotingTokens 
                                 ORDER BY TokenID";

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
    }
}
