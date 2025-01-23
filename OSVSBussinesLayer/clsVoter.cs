using OSVSDataLayer;
using System;
using System.Data;

namespace OSVSBussinesLayer
{
    public class clsVoter
    {
        // Properties
        public int VoterID { get; private set; }
        public int PersonID { get; set; }
        public bool IsVoted { get; set; }

        // Constructor for a new voter
        public clsVoter(int personID, bool isVoted = false)
        {
            this.PersonID = personID;
            this.IsVoted = isVoted;
        }

        // Private constructor for internal use when retrieving voter from DB
        private clsVoter(int voterID, int personID, bool isVoted)
        {
            this.VoterID = voterID;
            this.PersonID = personID;
            this.IsVoted = isVoted;
        }

        // Method to add a new voter
        public bool AddNewVoter()
        {
            this.VoterID = clsVotersData.AddNewVoter(this.PersonID, this.IsVoted);

            // Return true if voter was successfully added
            return this.VoterID != -1;
        }

        // Static method to find a voter by VoterID
        public static clsVoter FindByVoterID(int voterID)
        {
            int personID = -1;
            bool isVoted = false;

            bool isFound = clsVotersData.GetVoterInfoByID(voterID, ref personID, ref isVoted);
            if (isFound)
            {
                return new clsVoter(voterID, personID, isVoted);
            }
            return null; // If voter not found
        }

        // Method to update an existing voter
        public bool UpdateVoter()
        {
            return clsVotersData.UpdateVoter(this.VoterID, this.PersonID, this.IsVoted);
        }

        // Static method to delete a voter by VoterID
        public static bool DeleteVoter(int voterID)
        {
            return clsVotersData.DeleteVoter(voterID);
        }

        // Static method to get all voters (returns DataTable)
        public static DataTable GetAllVoters()
        {
            return clsVotersData.GetAllVoters();
        }

        // Method to check if a voter has voted
        public bool HasVoted()
        {
            return this.IsVoted;
        }

        // Method to mark voter as having voted
        public void MarkAsVoted()
        {
            this.IsVoted = true;
            this.UpdateVoter();
        }
    }
}
