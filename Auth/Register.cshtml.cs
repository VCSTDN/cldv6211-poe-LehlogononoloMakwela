using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace KhumaloCraft.Pages.Auth
{
	
	[BindProperties]
	public class RegisterModel : PageModel
    {

		[Required(ErrorMessage = "The First Name is required")]
		public string Firstname { get; set; } = "";

		[Required(ErrorMessage = "The Last Name is required")]
		public string Lastname { get; set; } = "";

		[Required(ErrorMessage = "The Email is required"), EmailAddress]
		public string Email { get; set; } = "";

		public string? Phone { get; set; } = "";

		[Required(ErrorMessage = "The Address is required")]
		public string Address { get; set; } = "";

		[Required(ErrorMessage = "Password is required")]
		[StringLength(50, ErrorMessage = "Password must be between 5 and 50 characters", MinimumLength = 5)]
		public string Password { get; set; } = "";

		[Required(ErrorMessage = "Confirm Password is required")]
		[Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
		public string ConfirmPassword { get; set; } = "";


		public string errorMessage = "";
		public string successMessage = "";

		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)

		{

			base.OnPageHandlerExecuting(context);



			if (HttpContext.Session.GetString("role") != null)

			{

				context.Result = new RedirectResult("/");

			}

		}
		public void OnGet()
        {
        }

		public void OnPost()
		{
			if (!ModelState.IsValid)
			{
				errorMessage = "Data validation failed";
				return;
			}

			if (Phone == null) Phone = "";

			string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sql = "INSERT INTO users " +
					"(firstname, lastname, email, phone, address, password, role) VALUES " +
					"(@firstname, @lastname, @email, @phone, @address, @password, 'client');";

					var passwordHasher = new PasswordHasher<IdentityUser>();
					string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

					using (SqlCommand command = new SqlCommand(sql, connection))
					{

						command.Parameters.AddWithValue("@firstname", Firstname);
						command.Parameters.AddWithValue("@lastname", Lastname);
						command.Parameters.AddWithValue("@email", Email);
						command.Parameters.AddWithValue("@phone", Phone);
						command.Parameters.AddWithValue("@address", Address);
						command.Parameters.AddWithValue("@password", hashedPassword);

						command.ExecuteNonQuery();

					}
				}

			}
			catch (Exception ex)
			{
				if (ex.Message.Contains(Email))
				{
					errorMessage = "Email address already used";
				}
				else
				{
					errorMessage = ex.Message;
				}

				return;

			}

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM users WHERE email=@email";

					using (SqlCommand command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@email", Email);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								int id = reader.GetInt32(0);
								string firstname = reader.GetString(1);
								string lastname = reader.GetString(2);
								string email = reader.GetString(3);
								string phone = reader.GetString(4);
								string address = reader.GetString(5);
								//string hashedPassword = reader.GetString(6);
								string role = reader.GetString(7);
								string created_at = reader.GetDateTime(8).ToString("MM/dd/yyyy");

								HttpContext.Session.SetInt32("id", id);
								HttpContext.Session.SetString("firstname", firstname);
								HttpContext.Session.SetString("lastname", lastname);
								HttpContext.Session.SetString("email", email);
								HttpContext.Session.SetString("phone", phone);
								HttpContext.Session.SetString("address", address);
								HttpContext.Session.SetString("role", role);
								HttpContext.Session.SetString("created_at", created_at);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}


			successMessage = "Account created successfully";

			Response.Redirect("/");
		}
    }
}
