using System;
using System.Data;
using OSVSDataLayer;

namespace OSVSBusinessLayer
{
    public class clsVotingTokens
    {
        // Method to Generate a New Token for a Voter
        public static int GenerateNewToken(int voterID)
        {
            // Ensure voterID is valid (could add additional checks if needed)
            if (voterID <= 0)
                throw new ArgumentException("Invalid Voter ID");

            // Generate a random token (you could add a more sophisticated generation mechanism)
            string token = Guid.NewGuid().ToString();
            DateTime generatedAt = DateTime.Now;
            DateTime tokenExpiry = generatedAt.AddHours(2); // Example: token is valid for 2 hours

            // Call Data Layer to Generate Token
            return clsVotingTokensData.GenerateToken(voterID, token, generatedAt, tokenExpiry);
        }

        // Method to Mark a Token as Used
        public static bool MarkTokenAsUsed(int tokenID)
        {
            // Ensure tokenID is valid
            if (tokenID <= 0)
                throw new ArgumentException("Invalid Token ID");

            // Call Data Layer to mark the token as used
            return clsVotingTokensData.UseToken(tokenID);
        }

        // Method to Validate a Token
        public static bool ValidateToken(string token)
        {
            // Ensure token is provided
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty");

            // Call Data Layer to check if the token is valid (not used and not expired)
            return clsVotingTokensData.IsTokenValid(token);
        }

        // Method to Get a Token's Details by TokenID
        public static DataTable GetTokenDetails(int tokenID)
        {
            // Ensure tokenID is valid
            if (tokenID <= 0)
                throw new ArgumentException("Invalid Token ID");

            // Call Data Layer to retrieve token details
            return clsVotingTokensData.GetTokenByID(tokenID);
        }

        // Method to Delete a Token
        public static bool DeleteToken(int tokenID)
        {
            // Ensure tokenID is valid
            if (tokenID <= 0)
                throw new ArgumentException("Invalid Token ID");

            // Call Data Layer to delete the token
            return clsVotingTokensData.DeleteToken(tokenID);
        }

        // Method to Get All Tokens
        public static DataTable GetAllTokens()
        {
            // Call Data Layer to retrieve all tokens
            return clsVotingTokensData.GetAllTokens();
        }
    }
}
