using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OSVSDataLayer;
using OSVSDataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OSVS.Pages.AdminDashboard
{
	public class AdminDashboardModel : PageModel
	{
		[BindProperty]
		[ValidateNever] // Skip validation for this property
		public string Search { get; set; }

		public List<Candidate> Candidates { get; set; } = new List<Candidate>();
		public List<Person> SearchResults { get; set; } = new List<Person>();
		public ElectionStatus CurrentElection { get; set; } = new ElectionStatus();

		public string UserName { get; set; }


		public void OnGet()
		{
			var personId = HttpContext.Session.GetInt32("PersonID");
			if (personId.HasValue)
			{
				UserName = clsPersonData.GetUserName(personId.Value);

			}
			CurrentElection = GetElectionStatus();
			// Load candidates with votes count
			Candidates = clsCandidatesData.GetCandidatesWithVotes();
		}

		public IActionResult OnPostSearch()
		{
			ModelState.Remove("Search");
			LoadUserInfo();
			CurrentElection = GetElectionStatus();
			// Search for people based on the provided search term
			if (!string.IsNullOrEmpty(Search))
			{
				SearchResults = clsPersonData.GetPeople()
					.Where(p => p.FullName.Contains(Search, StringComparison.OrdinalIgnoreCase)
							 || p.NationalNo.ToString().Contains(Search))
					.ToList();
			}

			// Reload the candidates for display in the "Existing Candidates" section
			Candidates = clsCandidatesData.GetCandidatesWithVotes();

			return Page();
		}

		public IActionResult OnPostAddCandidate(int CandidateId, string Bio, string CandidateLink)
		{
			ModelState.Remove("Search");
			ModelState.Remove("Bio");
			ModelState.Remove("CandidateLink");
			CurrentElection = GetElectionStatus();
			LoadUserInfo();


			// Ensure the Candidates list is up-to-date
			Candidates = clsCandidatesData.GetCandidatesWithVotes();

			// Validate: Add candidate only if they are not already in the list
			if (!Candidates.Any(c => c.PersonId == CandidateId))
			{
				clsCandidatesData.AddCandidate(CandidateId, Bio, CandidateLink);
			}
			else
			{
				// Add error and stop further execution
				ModelState.AddModelError("", "This person is already a candidate.");
				return Page();
			}

			// Reload the Candidates list to reflect changes
			Candidates = clsCandidatesData.GetCandidatesWithVotes();

			return Page();
		}

		public IActionResult OnPostRemoveCandidate(int CandidateId)
		{
			ModelState.Remove("Search");
			ModelState.Remove("Bio");
			ModelState.Remove("CandidateLink");
			CurrentElection = GetElectionStatus();
			LoadUserInfo();

			// Remove the candidate
			clsCandidatesData.RemoveCandidate(CandidateId);

			// Reload candidates
			Candidates = clsCandidatesData.GetCandidatesWithVotes();

			return Page();
		}

		public IActionResult OnPostToggleWinner(int CandidateId)
		{
			ModelState.Remove("Search");
			CurrentElection = GetElectionStatus();
			clsCandidatesData.ToggleWinnerStatus(CandidateId);
			LoadUserInfo();

			// Reload candidates for the UI
			Candidates = clsCandidatesData.GetCandidatesWithVotes();
			return Page();
		}

		public IActionResult OnPostAnnounceResults()
		{

			ModelState.Remove("Search");
			CurrentElection = GetElectionStatus();
			LoadUserInfo();

			// Ensure the election is still active
			if (!CurrentElection.IsElectionActive)
			{
				TempData["ErrorMessage"] = "The election is already closed. Results cannot be announced again.";
				return RedirectToPage();
			}

			// Perform the action
			clsElectionStatusData.EndElection(); // Update election status to inactive
			TempData["SuccessMessage"] = "Results have been announced. Voting is now closed.";
			return RedirectToPage();
		}

		public IActionResult OnPostClearCandidates()
		{
			LoadUserInfo();
			ModelState.Remove("Search");
			// Logic to clear all candidates and reset votes
			OSVSDataLayer.clsCandidatesData.ClearAllCandidates();
			TempData["SuccessMessage"] = "All candidates and votes have been cleared successfully.";
			return RedirectToPage();
		}

		public IActionResult OnPostUpdateElection(bool IsElectionActive, DateTime StartDate, DateTime EndDate)
		{
			// Logic to update election status and dates
			OSVSDataLayer.clsElectionStatusData.UpdateElectionStatus(IsElectionActive, StartDate, EndDate);
			LoadUserInfo();
			TempData["SuccessMessage"] = "Election details updated successfully.";
			return RedirectToPage();
		}

		public IActionResult OnPostUpdateBio(int CandidateId, string UpdatedBio)
		{
			LoadUserInfo();
			if (string.IsNullOrWhiteSpace(UpdatedBio))
			{
				ModelState.AddModelError("", "Bio cannot be empty.");
				return RedirectToPage();
			}

			// Update the candidate's bio in the database
			var success = clsCandidatesData.UpdateCandidateBioInDatabase(CandidateId, UpdatedBio);

			if (!success)
			{
				ModelState.AddModelError("", "Failed to update bio. Please try again.");
			}

			return RedirectToPage();
		}


		public IActionResult OnPostLogout()
		{
			// Clear session data for logout
			HttpContext.Session.Clear();
			return RedirectToPage("/Index");
		}

		public void LoadUserInfo()
		{
			var personId = HttpContext.Session.GetInt32("PersonID");

			if (personId.HasValue)
			{
				// Fetch the username based on session data or database
				UserName = clsPersonData.GetUserName(personId.Value);
			}
			
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



	}
}
