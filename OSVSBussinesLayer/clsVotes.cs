using System;
using System.Data;
using OSVSDataLayer;

namespace OSVSBusinessLayer
{
    public class clsVotes
    {
        // Method to Add a Vote for a Candidate
        //public static int AddVote(int candidateID, string token)
        //{
        //    // Ensure candidateID and token are valid
        //    if (candidateID <= 0)
        //        throw new ArgumentException("Invalid Candidate ID");
        //    if (string.IsNullOrEmpty(token))
        //        throw new ArgumentException("Token cannot be null or empty");

        //    // Call the Data Layer to add the vote
        //    return clsVotesData.AddVote(candidateID, token);
        //}

        // Method to Get Votes for a Specific Candidate
        public static DataTable GetVotesByCandidate(int candidateID)
        {
            // Ensure candidateID is valid
            if (candidateID <= 0)
                throw new ArgumentException("Invalid Candidate ID");

            // Call the Data Layer to retrieve votes for the candidate
            return clsVotesData.GetVotesByCandidateID(candidateID);
        }

        // Method to Delete a Vote (although votes shouldn't typically be deleted)
        public static bool DeleteVote(int voteID)
        {
            // Ensure voteID is valid
            if (voteID <= 0)
                throw new ArgumentException("Invalid Vote ID");

            // Call Data Layer to delete the vote
            return clsVotesData.DeleteVote(voteID);
        }

        // Method to Validate and Cast a Vote
        //public static bool CastVote(int candidateID, string token)
        //{
        //    // Validate the token using the VotingTokens class
        //    if (!clsVotingTokens.ValidateToken(token))
        //    {
        //        throw new InvalidOperationException("Invalid or expired token.");
        //    }

        //    // Add the vote
        //    int voteID = AddVote(candidateID, token);
        //    if (voteID > 0)
        //    {
        //        // Mark the token as used
        //        clsVotingTokens.MarkTokenAsUsed(int.Parse(token)); // Assuming token is the TokenID; adjust as needed
        //        return true; // Vote successfully cast
        //    }

        //    return false; // Vote casting failed
        //}
    }
}
