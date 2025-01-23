using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace OSVS.Pages.ForgotPassword
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();


        public class InputModel
        {
            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; }

			[Required(ErrorMessage = "New password is required.")]
			[StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least {2} characters long.")]
			[DataType(DataType.Password)]
			[Display(Name = "New Password")]
			public string NewPassword { get; set; }

			[Required(ErrorMessage = "Confirm password is required.")]
			[DataType(DataType.Password)]
			[Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
			[Display(Name = "Confirm Password")]
			public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet()
        {
			TempData["IsEmailValid"] = false;
			return Page();
        }

		public IActionResult OnPost()
		{
			// Retrieve IsEmailValid from TempData
			bool isEmailValid = TempData["IsEmailValid"] as bool? ?? false;
			TempData.Keep("IsEmailValid"); // Keep TempData for the next request

			if (!isEmailValid)
			{
				if (string.IsNullOrWhiteSpace(Input.Email))
				{
					ModelState.AddModelError("Input.Email", "Email is required.");
					return Page();
				}

				// Check if email exists
				int personId = OSVSBussinesLayer.clsPerson.GetPersonIDByEmail(Input.Email);
				if (personId == -1)
				{
					ModelState.AddModelError("Input.Email", "Email does not exist in our records.");
					return Page();
				}

				// If email is valid, store in TempData and show password reset fields
				TempData["IsEmailValid"] = true;
				TempData["Email"] = Input.Email; // Store the email for the next request
				ModelState.Clear(); // Clear existing ModelState errors
				return Page();
			}

			// If email is valid, update password
			if (ModelState.IsValid)
			{
				// Retrieve email from TempData
				string email = TempData["Email"] as string;
				if (string.IsNullOrWhiteSpace(email))
				{
					ModelState.AddModelError("", "Unable to retrieve email. Please start over.");
					return Page();
				}

				int personId = OSVSBussinesLayer.clsPerson.GetPersonIDByEmail(email);
				var voterId = OSVSDataLayer.clsVotersData.GetVoterIDByPersonID(personId);
				var salt = OSVSDataLayer.PasswordUtils.GenerateSalt();
				var hashedPassword = OSVSDataLayer.PasswordUtils.HashPassword(Input.NewPassword, salt);
				bool isUpdated = OSVSBussinesLayer.clsAuth.UpdatePassword(voterId, hashedPassword, salt);

				if (isUpdated)
				{
					TempData["IsEmailValid"] = false;
					ViewData["Message"] = "Password successfully reset. Redirecting to login...";
					ViewData["Success"] = "True"; // For redirecting on success

				}
				else
				{
					ModelState.AddModelError("", "Failed to reset password. Please try again.");
				}
			}

			return Page();
		}


		private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
