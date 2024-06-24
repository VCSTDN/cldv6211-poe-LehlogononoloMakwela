using KhumaloCraft.Pages.Admin.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net.Http.Headers;

namespace KhumaloCraft.Pages
{
    public class ProductDetailsModel : PageModel
    {
        public ProductInfo productInfo = new ProductInfo();
        public void OnGet(int? id)
        {
            if (id == null)
            {
                Response.Redirect("/");
                return;
            }

            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM products WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                productInfo.Id = reader.GetInt32(0);
                                productInfo.Title = reader.GetString(1);

                                productInfo.Price = reader.GetDecimal(2);
                                productInfo.Category = reader.GetString(3);
                                productInfo.Description = reader.GetString(4);
                                productInfo.ImageFileName = reader.GetString(5);
                                productInfo.CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                Response.Redirect("/");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/");
                return;
            }
        }
    }
}

