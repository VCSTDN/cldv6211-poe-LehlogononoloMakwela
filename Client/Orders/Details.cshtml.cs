using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace KhumaloCraft.Pages.Client.Orders
{
    public class DetailsModel : PageModel
    {
        public OrderInfo orderInfo = new OrderInfo();
        public void OnGet(int id)
        {
            int clientId = HttpContext.Session.GetInt32("id") ?? 0;

            if (id < 1)
            {
                Response.Redirect("/Client/Orders/Index");
                return;
            }


            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();


                    string sql = "SELECT * FROM orders WHERE id=@id AND client_id=@client_id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@client_id", clientId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderInfo.id = reader.GetInt32(0);
                                orderInfo.clientId = reader.GetInt32(1);
                                orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
                                orderInfo.shippingFee = reader.GetDecimal(3);
                                orderInfo.deliveryAddress = reader.GetString(4);
                                orderInfo.paymentMethod = reader.GetString(5);
                                orderInfo.paymentStatus = reader.GetString(6);
                                orderInfo.orderStatus = reader.GetString(7);

                                orderInfo.items = OrderInfo.getOrderItems(orderInfo.id);
                            }
                            else
                            {
                                Response.Redirect("/Client/Orders/Index");
                                return;
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Client/Orders/Index");
            }
        }
    }
}

        
