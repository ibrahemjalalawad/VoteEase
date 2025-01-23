namespace OSVSDataLayer.Models
{
	public class Candidate
	{
		public int Id { get; set; }
		public int PersonId { get; set; }
		public int NationalNo { get; set; }
		public string FullName { get; set; }
		public string? Bio { get; set; }
		public string ImagePath { get; set; }
		public int VotesCount { get; set; }
		public bool IsWinner { get; set; }
		public string CandidateLink { get; set; }
	}

}
