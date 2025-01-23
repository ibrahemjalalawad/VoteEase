using OSVSDataLayer;
using System;
using System.Data.SqlClient;

namespace OSVSBussinesLayer
{
    public class clsAuth
    {
        public int AuthID { get; set; }
        public int VoterID { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }

        public clsAuth()
        {
            this.AuthID = -1;
            this.VoterID = -1;
            this.HashedPassword = "";
            this.Salt = "";
        }

        public clsAuth(int authID, int voterID, string hashedPassword, string salt)
        {
            this.AuthID = authID;
            this.VoterID = voterID;
            this.HashedPassword = hashedPassword;
            this.Salt = salt;
        }

        // Method to create a new Auth entry
        public bool AddNewAuth(int voterID, string password)
        {
            // Generate a new Salt and hash the password
            this.Salt = PasswordUtils.GenerateSalt();
            this.HashedPassword = PasswordUtils.HashPassword(password, this.Salt);

            // Call data layer to save Auth details
            this.AuthID = clsAuthData.AddNewAuth(voterID, this.HashedPassword, this.Salt);
            return this.AuthID != -1; // Return true if AuthID is valid (indicating successful insertion)
        }

        // Method to find authentication details by VoterID
        public static clsAuth FindByVoterID(int voterID)
        {
            if (clsAuthData.GetAuthByVoterID(voterID, out string hashedPassword, out string salt))
            {
                return new clsAuth(-1, voterID, hashedPassword, salt); // Assuming AuthID isn't retrieved
            }
            else
            {
                return null; // Voter not found
            }
        }

        // Method to verify voter's password
        public static bool VerifyVoter(int voterID, string password)
        {
            return clsAuthData.ValidateVoter(voterID, password);
        }

        // Update an existing Auth (if the password is changed)
        public bool UpdateAuth(int voterID, string newPassword)
        {
            // Generate a new Salt and hash the new password
            this.Salt = PasswordUtils.GenerateSalt();
            this.HashedPassword = PasswordUtils.HashPassword(newPassword, this.Salt);

            // Call data layer to update existing Auth record
            return clsAuthData.UpdateAuth(this.AuthID, this.HashedPassword, this.Salt);
        }

		public static bool UpdatePassword(int voterID, string hashedPassword, string salt)
		{
			bool isUpdated = false; // This will indicate whether the update was successful

			using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			{
				string query = @"UPDATE Auth 
                         SET HashedPassword = @HashedPassword, Salt = @Salt 
                         WHERE VoterID = @VoterID";

				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@VoterID", voterID);
					command.Parameters.AddWithValue("@HashedPassword", hashedPassword);
					command.Parameters.AddWithValue("@Salt", salt);

					try
					{
						connection.Open();
						int rowsAffected = command.ExecuteNonQuery();
						isUpdated = rowsAffected > 0; // Set true if at least one row was updated
					}
					catch (Exception ex)
					{
						// Handle exceptions (log error)
						// For example: Console.WriteLine($"Error updating password: {ex.Message}");
					}
				}
			}

			return isUpdated;
		}

	}
}
