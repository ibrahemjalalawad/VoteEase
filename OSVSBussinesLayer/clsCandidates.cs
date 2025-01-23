using System;
using System.Data;
using OSVSDataLayer;

namespace OSVSBusinessLayer
{
    public class clsCandidates
    {
		// Method to Add a New Candidate
		public static int AddNewCandidate(int personID, string bio)
        {
            // Validate inputs
            if (personID <= 0)
                throw new ArgumentException("Invalid Person ID");
            if (string.IsNullOrEmpty(bio))
                throw new ArgumentException("Bio cannot be null or empty");

            // Call the Data Layer to insert the new candidate
            return clsCandidatesData.AddNewCandidate(personID, bio);
        }

        // Method to Update Existing Candidate Information
        public static bool UpdateCandidate(int candidateID, int personID, string bio)
        {
            // Validate inputs
            if (candidateID <= 0)
                throw new ArgumentException("Invalid Candidate ID");
            if (personID <= 0)
                throw new ArgumentException("Invalid Person ID");
            if (string.IsNullOrEmpty(bio))
                throw new ArgumentException("Bio cannot be null or empty");

            // Call the Data Layer to update candidate information
            return clsCandidatesData.UpdateCandidate(candidateID, personID, bio);
        }

        // Method to Get Candidate Information by ID
        public static DataTable GetCandidateByID(int candidateID)
        {
            // Validate input
            if (candidateID <= 0)
                throw new ArgumentException("Invalid Candidate ID");

            // Call the Data Layer to retrieve candidate information
            return clsCandidatesData.GetCandidateByID(candidateID);
        }

        // Method to Delete a Candidate by ID
        public static bool DeleteCandidate(int candidateID)
        {
            // Validate input
            if (candidateID <= 0)
                throw new ArgumentException("Invalid Candidate ID");

            // Call the Data Layer to delete the candidate
            return clsCandidatesData.DeleteCandidate(candidateID);
        }

        // Method to Get All Candidates
        public static DataTable GetAllCandidates()
        {
            // Call the Data Layer to retrieve all candidates
            return clsCandidatesData.GetAllCandidates();
        }
    }
}
