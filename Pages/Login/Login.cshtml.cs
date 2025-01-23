using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace OSVS.Pages.Login
{
	public class LoginModel : PageModel
	{


		[BindProperty]
		public InputModel Input { get; set; } = new InputModel();

		public class InputModel
		{
			[Required(ErrorMessage = "The National ID is required.")]
			[Display(Name = "National ID")]
			public string NationalId { get; set; }

			[Required(ErrorMessage = "The password is required.")]
			[DataType(DataType.Password)]
			[Display(Name = "Password")]
			public string Password { get; set; }

			[Display(Name = "Remember me?")]
			public bool RememberMe { get; set; }
		}

		public void OnGet()
		{
		}

		public IActionResult OnPost()
		{
			if (!ModelState.IsValid)
			{
				// Handle validation errors
				return Page();
			}

			// Login logic
			var personId = OSVSDataLayer.clsPersonData.GetPersonIDByNationalNo(Input.NationalId);
			var voterId = OSVSDataLayer.clsVotersData.GetVoterIDByPersonID(personId);
			var isValidLogin = OSVSDataLayer.clsAuthData.ValidateVoter(voterId, Input.Password);

			if (personId == -1)
			{
				ModelState.AddModelError("Input.NationalId", "The National ID is invalid or does not exist.");
				return Page();
			}
			else if (!isValidLogin)
			{
				ModelState.AddModelError("Input.Password", "The password is incorrect.");
				return Page();
			}

			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
				return Content(string.Join("<br>", errors), "text/html");
			}

			// Successful login
			HttpContext.Session.SetInt32("PersonID", personId); // Set session for person
			HttpContext.Session.SetInt32("VoterID", voterId);   // Set session for voter

			// Log the successful login to AuditLogs
			clsAuditLog.LogLogin(personId, "User successfully logged in.");

			// Show success notification
			TempData["ShowSuccessNotification"] = true;
			return Page(); // Re-render the page to show the toast and redirect
		}
	}
}
