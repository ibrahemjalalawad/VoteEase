using System;
using System.Data;
using OSVSDataLayer;

namespace OSVSBusinessLayer
{
    public class clsAdmin
    {
        // Method to Get Admin Information by AdminID
        public static bool GetAdminInfoByID(int adminID, out string userName, out string password, out string type, out int personID)
        {
            // Initialize output parameters
            userName = string.Empty;
            password = string.Empty;
            type = string.Empty;
            personID = 0;

            // Validate input
            if (adminID <= 0)
                throw new ArgumentException("Invalid Admin ID");

            // Call the Data Layer to get admin info
            return clsAdminData.GetAdminInfoByID(adminID, ref userName, ref password, ref type, ref personID);
        }

        // Method to Add a New Admin
        public static int AddNewAdmin(string userName, string password, string type, int personID)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("User Name cannot be null or empty");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty");
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty");
            if (personID <= 0)
                throw new ArgumentException("Invalid Person ID");

            // Call the Data Layer to add a new admin
            return clsAdminData.AddNewAdmin(userName, password, type, personID);
        }

        // Method to Update an Existing Admin
        public static bool UpdateAdmin(int adminID, string userName, string password, string type, int personID)
        {
            // Validate inputs
            if (adminID <= 0)
                throw new ArgumentException("Invalid Admin ID");
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("User Name cannot be null or empty");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty");
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty");
            if (personID <= 0)
                throw new ArgumentException("Invalid Person ID");

            // Call the Data Layer to update admin information
            return clsAdminData.UpdateAdmin(adminID, userName, password, type, personID);
        }

        // Method to Delete an Admin by ID
        public static bool DeleteAdmin(int adminID)
        {
            // Validate input
            if (adminID <= 0)
                throw new ArgumentException("Invalid Admin ID");

            // Call the Data Layer to delete the admin
            return clsAdminData.DeleteAdmin(adminID);
        }

        // Method to Get All Admins
        public static DataTable GetAllAdmins()
        {
            // Call the Data Layer to retrieve all admins
            return clsAdminData.GetAllAdmins();
        }
    }
}
