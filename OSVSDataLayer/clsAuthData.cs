using System;
using System.Data;
using System.Data.SqlClient;

namespace OSVSDataLayer
{
    public class clsAuthData
    {
        // Method to add a new voter with hashed password and salt
        public static int AddNewAuth(int voterID, string hashedPassword, string salt)
        {
            int authID = -1; // This will hold the newly created AuthID

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"INSERT INTO Auth (VoterID, HashedPassword, Salt) 
                                 VALUES (@VoterID, @HashedPassword, @Salt); 
                                 SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VoterID", voterID);
                    command.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                    command.Parameters.AddWithValue("@Salt", salt);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        {
                            authID = insertedID; // Get the new AuthID
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (log error)
                    }
                }
            }

            return authID;
        }

        // Method to retrieve authentication details by VoterID
        public static bool GetAuthByVoterID(int voterID, out string hashedPassword, out string salt)
        {
            bool isFound = false;
            hashedPassword = null;
            salt = null;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = "SELECT HashedPassword, Salt FROM Auth WHERE VoterID = @VoterID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VoterID", voterID);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            isFound = true;
                            hashedPassword = reader["HashedPassword"].ToString();
                            salt = reader["Salt"].ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (log error)
                    }
                }
            }

            return isFound;
        }

        // Method to validate a voter's password
        public static bool ValidateVoter(int voterID, string password)
        {
            if (GetAuthByVoterID(voterID, out string storedHashedPassword, out string storedSalt))
            {
                // Verify the password
                return PasswordUtils.VerifyPassword(password, storedSalt, storedHashedPassword);
            }
            return false; // Voter not found
        }
        // New Method to update authentication details for a voter
        public static bool UpdateAuth(int voterID, string newHashedPassword, string newSalt)
        {
            bool isUpdated = false;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"UPDATE Auth 
                                 SET HashedPassword = @NewHashedPassword, Salt = @NewSalt 
                                 WHERE VoterID = @VoterID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VoterID", voterID);
                    command.Parameters.AddWithValue("@NewHashedPassword", newHashedPassword);
                    command.Parameters.AddWithValue("@NewSalt", newSalt);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            isUpdated = true; // Update was successful
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (log error)
                    }
                }
            }

            return isUpdated;
        }
    }
}
