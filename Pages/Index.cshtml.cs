using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OSVSDataLayer;

namespace OSVS.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public bool IsLoggedIn { get; private set; }
		public string UserName { get; private set; }

		public void OnGet()
		{

			if (!ValidateTabIdentifier(HttpContext))
			{
				// Redirect to login if the session is invalidated
				 RedirectToPage();
			}
			// Check if user is logged in by looking for session data
			var personId = HttpContext.Session.GetInt32("PersonID");
			var voterId = HttpContext.Session.GetInt32("VoterID");

			if (personId != null && voterId != null)
			{
				IsLoggedIn = true;

				// Retrieve the user's name using `personId` or other logic
				// Replace the placeholder logic with your data-fetching mechanism
				UserName = clsPersonData.GetUserName(personId.Value); // Replace with your method to fetch user name
			}
			else
			{
				IsLoggedIn = false;
			}
		}

		public IActionResult OnPostLogout()
		{
			// Clear session data for logout
			HttpContext.Session.Clear();
			return RedirectToPage("/Index");
		}

		private  bool ValidateTabIdentifier(HttpContext context)
		{
			var tabIdentifier = context.Request.Headers["X-Tab-Identifier"].ToString();
			var sessionTabIdentifier = context.Session.GetString("TabIdentifier");

			// If the session doesn't have a tab identifier, store the current one
			if (string.IsNullOrEmpty(sessionTabIdentifier))
			{
				context.Session.SetString("TabIdentifier", tabIdentifier);
				return true;
			}

			// Validate the tab identifier only if it's present
			if (!string.IsNullOrEmpty(tabIdentifier) && sessionTabIdentifier != tabIdentifier)
			{
				context.Session.Clear();
				return false; // Session invalidated
			}

			return true; // Session is valid
		}

	}
}
