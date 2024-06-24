using KhumaloCraft.Pages.Admin.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace KhumaloCraft.Pages
{
    [BindProperties(SupportsGet = true)]
    public class MyworkPageModel : PageModel
    {
        public string? Search { get; set; }
        public string PriceRange { get; set; } = "any";
       
        public string Category { get; set; } = "any";

        public List<ProductInfo> listProducts = new List<ProductInfo>();


        public int page = 1; // the current html page
        public int totalPages = 0;
        private readonly int pageSize = 5; // books per page
        public void OnGet()
        {
            page = 1;
            string requestPage = Request.Query["page"];
            if (requestPage != null)
            {
                try
                {
                    page = int.Parse(requestPage);
                }
                catch (Exception ex)
                {
                    page = 1;
                }
            }

            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlCount = "SELECT COUNT(*) FROM products";
                    sqlCount += " WHERE (title LIKE @search )";

                    if (PriceRange.Equals("0_500"))
                    {
                        sqlCount += " AND price <= 500";
                    }
                    
                    else if (PriceRange.Equals("above500"))
                    {
                        sqlCount += " AND price >= 500";
                    }


                    


                    if (!Category.Equals("any"))
                    {
                        sqlCount += " AND category=@category";
                    }

                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + Search + "%");
                        command.Parameters.AddWithValue("@category", Category);

                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }



                    string sql = "SELECT * FROM products";
                    sql += " WHERE (title LIKE @search )";

                    if (PriceRange.Equals("0_500"))
                    {
                        sql += " AND price <= 500";
                    }
                    else if (PriceRange.Equals("500_1000"))
                    {
                        sql += " AND price >= 500 AND price <= 1000";
                    }
                   


                    


                    if (!Category.Equals("any"))
                    {
                        sql += " AND category=@category";
                    }

                    sql += " ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";


                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + Search + "%");
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

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

                                listProducts.Add(productInfo);
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

