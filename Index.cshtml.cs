using KhumaloCraft.Pages.Admin.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace KhumaloCraft.Pages
{
    public class IndexModel : PageModel
    {
       public List<ProductInfo> listNewestCrafts = new List<ProductInfo>();
        public List<ProductInfo> listTopSales = new List<ProductInfo>();

        public void OnGet()
        {
            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT TOP 4 * FROM products ORDER BY created_at DESC";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductInfo productInfo = new ProductInfo();
                                productInfo.Id = reader.GetInt32(0);
                                productInfo.Title = reader.GetString(1);

                                productInfo.Price = reader.GetDecimal(2);
                                productInfo.Category = reader.GetString(3);
                                productInfo.Description = reader.GetString(4);
                                productInfo.ImageFileName = reader.GetString(5);
                                productInfo.CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

                                listNewestCrafts.Add(productInfo);
                            }
                        }
                    }

                    

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductInfo productInfo = new ProductInfo();
                                productInfo.Id = reader.GetInt32(0);
                                productInfo.Title = reader.GetString(1);

                                productInfo.Price = reader.GetDecimal(2);
                                productInfo.Category = reader.GetString(3);
                                productInfo.Description = reader.GetString(4);
                                productInfo.ImageFileName = reader.GetString(5);
                                productInfo.CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

                                listTopSales.Add(productInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
        
