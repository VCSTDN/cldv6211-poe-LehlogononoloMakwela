using KhumaloCraft.Pages.Admin.Products;
using KhumaloCraft.Pages.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace KhumaloCraft.Pages.Admin.Orders
{
    
    public class IndexModel : PageModel
    {
        public List<OrderInfo> listOrders = new List<OrderInfo>();

        public int page = 1; // the current html page
        public int totalPages = 0;
        private readonly int pageSize = 3;



        public void OnGet()
        {
            try
            {
                string requestPage = Request.Query["page"];
                page = int.Parse(requestPage);
            }
            catch (Exception ex)
            {
                page = 1;
            }

            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlCount = "SELECT COUNT(*) FROM orders";
                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM orders ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderInfo orderInfo = new OrderInfo();
                                orderInfo.id = reader.GetInt32(0);
                                orderInfo.clientId = reader.GetInt32(1);
                                orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
                                orderInfo.shippingFee = reader.GetDecimal(3);
                                orderInfo.deliveryAddress = reader.GetString(4);
                                orderInfo.paymentMethod = reader.GetString(5);
                                orderInfo.paymentStatus = reader.GetString(6);
                                orderInfo.orderStatus = reader.GetString(7);

                                orderInfo.items = OrderInfo.getOrderItems(orderInfo.id);

                                listOrders.Add(orderInfo);
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
    
    public class OrderItemInfo
    {
        public int id;
        public int orderId;
        public int bookId;
        public int quantity;
        public decimal unitPrice;

        public ProductInfo productInfo = new ProductInfo();
    }

    public class OrderInfo
    {
        public int id;
        public int clientId;
        public string orderDate;
        public decimal shippingFee;
        public string deliveryAddress;
        public string paymentMethod;
        public string paymentStatus;
        public string orderStatus;

        public List<OrderItemInfo> items = new List<OrderItemInfo>();

        public static List<OrderItemInfo> getOrderItems(int orderId)
        {
            List<OrderItemInfo> items = new List<OrderItemInfo>();

            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT order_items.*, books.* FROM order_items, books " +
                        "WHERE order_items.order_id=@order_id AND order_items.book_id = books.id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@order_id", orderId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderItemInfo item = new OrderItemInfo();

                                item.id = reader.GetInt32(0);
                                item.orderId = reader.GetInt32(1);
                                item.bookId = reader.GetInt32(2);
                                item.quantity = reader.GetInt32(3);
                                item.unitPrice = reader.GetDecimal(4);

                                item.productInfo.Id = reader.GetInt32(5);
                                item.productInfo.Title = reader.GetString(6);
                                
                                item.productInfo.Price = reader.GetDecimal(7);
                                item.productInfo.Category = reader.GetString(8);
                                item.productInfo.Description = reader.GetString(9);
                                item.productInfo.ImageFileName = reader.GetString(10);
                                item.productInfo.CreatedAt = reader.GetDateTime(11).ToString("MM/dd/yyyy");

                                items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return items;
        }
    }


