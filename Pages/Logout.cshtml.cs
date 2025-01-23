using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OSVS.Pages
{
	public class LogoutModel : PageModel
	{
		public IActionResult OnPost()
		{
			// Clear session data for logout
			HttpContext.Session.Clear();

			// Check if the request is from a `sendBeacon` call
			if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				// Respond with a JSON object
				return new JsonResult(new { success = true });
			}

			// Redirect for standard logout requests
			return RedirectToPage("/Index");
		}
	}
}
