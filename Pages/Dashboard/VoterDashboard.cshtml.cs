using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OSVS.Services;
using OSVSBusinessLayer;
using OSVSDataLayer;
using OSVSDataLayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSVS.Pages.Dashboard
{


	public class VoterDashboardModel : PageModel
	{
		public string UserName { get; set; }
		public bool IsAdmin { get; private set; }
		public bool HasVoted { get; set; }
		public ElectionStatus CurrentElection { get; set; }
		public List<Candidate> Candidates { get; set; } = new List<Candidate>();
		public List<Candidate> WinningCandidates { get; set; } = new List<Candidate>();

		private readonly FaceVerificationService _faceVerificationService;

		public VoterDashboardModel(FaceVerificationService faceVerificationService)
		{
			_faceVerificationService = faceVerificationService ?? throw new ArgumentNullException(nameof(faceVerificationService));
		}

		public void OnGet()
		{
			var personId = HttpContext.Session.GetInt32("PersonID");
			var voterId = HttpContext.Session.GetInt32("VoterID");
			if (personId.HasValue)
			{
				HasVoted = OSVSDataLayer.clsVotersData.HasVoted(personId.Value);
				UserName = OSVSDataLayer.clsPersonData.GetUserName(personId.Value);  // Fetch the username using PersonID
				HasVoted = OSVSDataLayer.clsVotersData.HasVoted(voterId.Value); // Use PersonID to check voting status
				IsAdmin = OSVSDataLayer.clsPersonData.IsAdmin(personId.Value);
			}

			CurrentElection = GetElectionStatus();
			if (!CurrentElection.IsElectionActive)
			{
				WinningCandidates = clsCandidatesData.GetWinningCandidates();
			}
			Candidates = GetCandidates(); // Fetch candidates
		}

		//public async Task<IActionResult> OnPostVoteNowAsync(int candidateId, string capturedImage)
		//{
		//	var personId = HttpContext.Session.GetInt32("PersonID");
		//	var voterId = HttpContext.Session.GetInt32("VoterID");

		//	if (!personId.HasValue)
		//	{
		//		TempData["ErrorMessage"] = "You must be logged in to vote.";
		//		return RedirectToPage();
		//	}

		//	// Check if the voter has already voted
		//	bool hasVoted = OSVSDataLayer.clsVotersData.HasVoted(voterId.Value);
		//	if (hasVoted)
		//	{
		//		TempData["ErrorMessage"] = "You have already voted. You can only vote once.";
		//		return RedirectToPage();
		//	}

		//	// Validate the captured image
		//	if (string.IsNullOrEmpty(capturedImage))
		//	{
		//		TempData["ErrorMessage"] = "No image captured. Please try again.";
		//		return RedirectToPage();
		//	}

		//	// Convert Base64 string to a stream
		//	var imageData = Convert.FromBase64String(capturedImage.Replace("data:image/jpeg;base64,", ""));
		//	using (var imageStream = new MemoryStream(imageData))
		//	{
		//		// Path to the stored image for the voter
		//		string storedImagePath = clsPersonData.GetFaceImagePath(personId.Value); // Replace with the actual method to fetch the stored image path

		//		if (string.IsNullOrEmpty(storedImagePath))
		//		{
		//			TempData["ErrorMessage"] = "No stored image found for face verification. Please contact support.";
		//			return RedirectToPage();
		//		}

		//		// Perform face verification
		//		bool isVerified = await _faceVerificationService.VerifyFaceAsync(storedImagePath, imageStream);

		//		if (!isVerified)
		//		{
		//			TempData["ErrorMessage"] = "Face verification failed (Faces don't match). You are not authorized to vote.";
		//			return RedirectToPage();
		//		}
		//	}

		//	// Proceed with voting
		//	int voteId = OSVSDataLayer.clsVotesData.AddVoteWithToken(candidateId, voterId.Value);

		//	if (voteId != -1)
		//	{
		//		// Mark the voter as having voted
		//		bool markAsVotedSuccess = OSVSDataLayer.clsVotersData.MarkAsVoted(personId.Value);

		//		if (markAsVotedSuccess)
		//		{
		//			TempData["SuccessMessage"] = "Your vote has been successfully cast!";
		//		}
		//		else
		//		{
		//			TempData["ErrorMessage"] = "Your vote was cast, but we couldn't update your vote status. Please contact support.";
		//		}
		//	}
		//	else
		//	{
		//		TempData["ErrorMessage"] = "There was an error while casting your vote. Please try again.";
		//	}

		//	return RedirectToPage();
		//}

		public async Task<IActionResult> OnPostVoteNowAsync(int candidateId, string capturedImageBase64)
		{
			var personId = HttpContext.Session.GetInt32("PersonID");
			var voterId = HttpContext.Session.GetInt32("VoterID");

			if (!personId.HasValue)
			{
				TempData["ErrorMessage"] = "You must be logged in to vote.";
				return RedirectToPage();
			}

			// Check if the voter has already voted
			bool hasVoted = OSVSDataLayer.clsVotersData.HasVoted(voterId.Value);
			if (hasVoted)
			{
				TempData["ErrorMessage"] = "You have already voted. You can only vote once.";
				return RedirectToPage();
			}

			// Validate the captured image
			if (string.IsNullOrEmpty(capturedImageBase64))
			{
				TempData["ErrorMessage"] = "No image captured. Please try again.";
				return RedirectToPage();
			}

			// Fetch the stored image path for the voter
			string storedImagePath = clsPersonData.GetFaceImagePath(personId.Value);

			if (string.IsNullOrEmpty(storedImagePath) || !System.IO.File.Exists(storedImagePath))
			{
				TempData["ErrorMessage"] = "No stored image found for face verification. Please contact support.";
				return RedirectToPage();
			}

			// Perform face verification
			var (isVerified, confidence) = _faceVerificationService.VerifyFaces(storedImagePath, capturedImageBase64);
			double confidencePercentage = (1 - confidence) * 100; // Convert confidence to a user-friendly percentage

			if(confidence == 0)
			{
				TempData["ErrorMessage"] = $"Face verification failed. No face detected. Please try again.";
				return RedirectToPage();
			}
			// Add confidence threshold condition
			if (!isVerified || confidence > 0.3)
			{
				TempData["ErrorMessage"] = $"Face verification failed. Your face was only {confidencePercentage:F0}% accurate. It must be at least 70%. Please try again.";
				return RedirectToPage();
			}

			// Proceed with voting
			int voteId = OSVSDataLayer.clsVotesData.AddVoteWithToken(candidateId, voterId.Value);

			if (voteId != -1)
			{
				// Mark the voter as having voted
				bool markAsVotedSuccess = OSVSDataLayer.clsVotersData.MarkAsVoted(personId.Value);

				if (markAsVotedSuccess)
				{
					TempData["SuccessMessage"] = $"Your vote has been successfully cast! Face verification was {confidencePercentage:F0}% accurate.";
				}
				else
				{
					TempData["ErrorMessage"] = "Your vote was cast, but we couldn't update your vote status. Please contact support.";
				}
			}
			else
			{
				TempData["ErrorMessage"] = "There was an error while casting your vote. Please try again.";
			}

			return RedirectToPage();
		}




		public IActionResult OnGetVoteNow()
		{
			if (HasVoted)
			{
				return Content("You have already cast your vote in this election.", "text/plain");
			}
			var candidates = GetCandidates();
			return Partial("_VoteNow", candidates);
		}

		public IActionResult OnPostLogout()
		{
			// Clear session data for logout
			HttpContext.Session.Clear();
			return RedirectToPage("/Index");
		}

		private ElectionStatus GetElectionStatus()
		{
			var electionStatus = clsElectionStatusData.GetCurrentElectionStatus(); // Assuming this fetches the single active election
			if (electionStatus != null)
			{
				return new ElectionStatus
				{
					IsElectionActive = electionStatus.IsElectionActive,
					StartDate = electionStatus.StartDate,
					EndDate = electionStatus.EndDate
				};
			}
			return null;
		}

		private List<Candidate> GetCandidates()
		{
			return clsCandidatesData.GetCandidates();
		}


	}
}
