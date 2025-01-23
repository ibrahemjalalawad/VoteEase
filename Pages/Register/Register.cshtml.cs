using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OSVS.Pages.Register
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            [Display(Name = "National ID")]
            public string NationalId { get; set; }

            public bool IsNationalIdVerified { get; set; } = false;

            [RequiredIfVisible("IsNationalIdVerified")] // Custom validation attribute
            [StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least {2} characters long.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string? Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

		public IActionResult OnPost()
		{
			// Check if the National ID is verified
			bool isNationalIdVerified = TempData["IsNationalIdVerified"] as bool? ?? false;
			TempData.Keep("IsNationalIdVerified"); // Keep TempData available for the next request

			if (!isNationalIdVerified)
			{
				int personID = OSVSDataLayer.clsPersonData.GetPersonIDByNationalNo(Input.NationalId);

				if (string.IsNullOrWhiteSpace(Input.NationalId))
				{
					ModelState.AddModelError("Input.NationalId", "The National ID is required.");
					return Page();
				}

				if (personID == -1)
				{
					ModelState.AddModelError("Input.NationalId", "The National ID is invalid or does not exist.");
					ModelState.Remove("Input.Password");
					ModelState.Remove("Input.ConfirmPassword");
					return Page();
				}

				// Check if the person is already registered as a voter
				bool isAlreadyRegistered = OSVSDataLayer.clsVotersData.IsVoterRegistered(personID);
				if (isAlreadyRegistered)
				{
					TempData["ErrorMessage"] = "You are already registered as a voter. Please log in.";
					ModelState.Remove("Input.Password");
					ModelState.Remove("Input.ConfirmPassword");
					return Page(); // Return to the same page to show the error modal
				}

				// Set TempData flag to true for National ID verification
				TempData["IsNationalIdVerified"] = true;
				ModelState.Clear(); // Clear previous errors to proceed with password input
				return Page();
			}

			// Handle password registration here...
			if (!ModelState.IsValid)
			{
				return Page(); // Display any validation errors in the password fields
			}

			// Proceed with registration
			int personIDVerified = OSVSDataLayer.clsPersonData.GetPersonIDByNationalNo(Input.NationalId);
			var salt = OSVSDataLayer.PasswordUtils.GenerateSalt();
			var hashedPassword = OSVSDataLayer.PasswordUtils.HashPassword(Input.Password, salt);

			int voterID = OSVSDataLayer.clsVotersData.AddNewVoter(personIDVerified);
			OSVSDataLayer.clsAuthData.AddNewAuth(voterID, hashedPassword, salt);

			// Indicate successful registration
			TempData["ShowSuccessModal"] = true;

			return Page(); // Redirect back to the page to show the modal
		}




	}

	// Custom validation attribute to require the password fields only if the National ID is verified
	public class RequiredIfVisibleAttribute : ValidationAttribute
    {
        private readonly string _propertyName;

        public RequiredIfVisibleAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property with name {_propertyName} not found.");
            }

            var isVisible = (bool)property.GetValue(validationContext.ObjectInstance);
            if (isVisible && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
            }

            return ValidationResult.Success;
        }
    }
}
