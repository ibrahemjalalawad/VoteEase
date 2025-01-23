using System;
using System.Data;
using System.Data.SqlClient;

namespace OSVSDataLayer
{
    public class clsAdminData
    {
        // Method to Get Admin Info by AdminID
        public static bool GetAdminInfoByID(int AdminID, ref string UserName, ref string Password, ref string Type, ref int PersonID)
        {
            bool isFound = false;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "SELECT * FROM Admin WHERE AdminID = @AdminID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AdminID", AdminID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // The record was found
                    isFound = true;
                    UserName = (string)reader["UserName"];
                    Password = (string)reader["Password"];
                    Type = (string)reader["Type"];
                    PersonID = (int)reader["PersonID"];
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

        // Method to Add a New Admin
        public static int AddNewAdmin(string UserName, string Password, string Type, int PersonID)
        {
            int AdminID = -1;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"INSERT INTO Admin (UserName, Password, Type, PersonID)
                             VALUES (@UserName, @Password, @Type, @PersonID);
                             SELECT SCOPE_IDENTITY();";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@UserName", UserName);
            command.Parameters.AddWithValue("@Password", Password);
            command.Parameters.AddWithValue("@Type", Type);
            command.Parameters.AddWithValue("@PersonID", PersonID);

            try
            {
                connection.Open();
                object result = command.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int insertedID))
                {
                    AdminID = insertedID;
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
            }
            finally
            {
                connection.Close();
            }

            return AdminID;
        }

        // Method to Update an Admin
        public static bool UpdateAdmin(int AdminID, string UserName, string Password, string Type, int PersonID)
        {
            bool isUpdated = false;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"UPDATE Admin
                             SET UserName = @UserName, Password = @Password, Type = @Type, PersonID = @PersonID
                             WHERE AdminID = @AdminID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@AdminID", AdminID);
            command.Parameters.AddWithValue("@UserName", UserName);
            command.Parameters.AddWithValue("@Password", Password);
            command.Parameters.AddWithValue("@Type", Type);
            command.Parameters.AddWithValue("@PersonID", PersonID);

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

        // Method to Delete an Admin
        public static bool DeleteAdmin(int AdminID)
        {
            bool isDeleted = false;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "DELETE FROM Admin WHERE AdminID = @AdminID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AdminID", AdminID);

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

        // Method to Get All Admins (returns DataTable)
        public static DataTable GetAllAdmins()
        {
            DataTable dt = new DataTable();
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "SELECT * FROM Admin";

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
    }
}
