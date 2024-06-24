using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace KhumaloCraft.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        public List<ProductInfo> listProducts = new List<ProductInfo>();
        public string search = "";
        public int page = 1; // the current html page
        public int totalPages = 0;
        private readonly int pageSize = 5; // books per page
        public string column = "id";
        public string order = "desc";
        public void OnGet()
        {
            search = Request.Query["search"];
            if (search == null) search = "";
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

            string[] validColumns = { "id", "title", "authors", "num_pages", "price", "category", "created_at" };
            column = Request.Query["column"];
            if (column == null || !validColumns.Contains(column))
            {
                column = "id";
            }

            order = Request.Query["order"];
            if (order == null || !order.Equals("asc"))
            {
                order = "desc";
            }

            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();


                    string sqlCount = "SELECT COUNT(*) FROM products";
                    if (search.Length > 0)
                    {
                        sqlCount += " WHERE title LIKE @search ";
                    }

                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");

                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM products  ";
                    if (search.Length > 0)
                    {
                        sql += " WHERE title LIKE @search ";
                    }

                    sql += sql += " ORDER BY " + column + " " + order; //" ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");
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

    public class ProductInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageFileName { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }
}
