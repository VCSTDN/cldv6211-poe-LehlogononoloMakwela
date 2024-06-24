using KhumaloCraft.Pages.Admin.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace KhumaloCraft.Pages
{
    [BindProperties]
    public class CartModel : PageModel
    {
        [Required(ErrorMessage = "The Address is required")]
        public string Address { get; set; } = "";

        [Required]
        public string PaymentMethod { get; set; } = "";

        public List<OrderItem> listOrderItems = new List<OrderItem>();
        public decimal subtotal = 0;
        public decimal shippingFee = 5;
        public decimal total = 0;

        private Dictionary<String, int> getProductDictionary()
        {
            var productDictionary = new Dictionary<string, int>();

            // Read shopping cart items from cookie
            string cookieValue = Request.Cookies["shopping_cart"] ?? "";

            if (cookieValue.Length > 0)
            {
                string[] productIdArray = cookieValue.Split('-');

                for (int i = 0; i < productIdArray.Length; i++)
                {
                    string productId = productIdArray[i];
                    if (productDictionary.ContainsKey(productId))
                    {
                        productDictionary[productId] += 1;
                    }
                    else
                    {
                        productDictionary.Add(productId, 1);
                    }
                }
            }

            return productDictionary;
        }


        public void OnGet()
        {
            var productDictionary = getProductDictionary();


            
            string? action = Request.Query["action"];
            string? id = Request.Query["id"];

            if (action != null && id != null && productDictionary.ContainsKey(id))
            {
                if (action.Equals("add"))
                {
                    productDictionary[id] += 1;
                }
                else if (action.Equals("sub"))
                {
                    if (productDictionary[id] > 1) productDictionary[id] -= 1;
                }
                else if (action.Equals("delete"))
                {
                    productDictionary.Remove(id);
                }


               
                string newCookieValue = "";
                foreach (var keyValuePair in productDictionary)
                {
                    for (int i = 0; i < keyValuePair.Value; i++)
                    {
                        newCookieValue += "-" + keyValuePair.Key;
                    }
                }

                if (newCookieValue.Length > 0)
                    newCookieValue = newCookieValue.Substring(1);

                var cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(365);
                cookieOptions.Path = "/";

                Response.Cookies.Append("shopping_cart", newCookieValue, cookieOptions);

                // redirect to the same page:
                //   - to remove the query string from the url
                //   - to set the shopping cart size using the updated cookie
                Response.Redirect(Request.Path.ToString());
                return;


            }
            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM products WHERE id=@id";
                    foreach (var keyValuePair in productDictionary)
                    {
                        string productID = keyValuePair.Key;
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@id", productID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    OrderItem item = new OrderItem();

                                    item.productInfo.Id = reader.GetInt32(0);
                                    item.productInfo.Title = reader.GetString(1);
                                    
                                    item.productInfo.Price = reader.GetDecimal(2);
                                    item.productInfo.Category = reader.GetString(3);
                                    item.productInfo.Description = reader.GetString(4);
                                    item.productInfo.ImageFileName = reader.GetString(5);
                                    item.productInfo.CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

                                    item.numCopies = keyValuePair.Value;
                                    item.totalPrice = item.numCopies * item.productInfo.Price;

                                    listOrderItems.Add(item);


                                    subtotal += item.totalPrice;
                                    total = subtotal + shippingFee;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Address = HttpContext.Session.GetString("address") ?? "";

            TempData["Total"] = "" + total;
            TempData["ProductIdentifiers"] = "";
            TempData["DeliveryAddress"] = "";
        }


        public string errorMessage = "";
        public string successMessage = "";

        public void OnPost()
        {
            int client_id = HttpContext.Session.GetInt32("id") ?? 0;
            if (client_id < 1)
            {
                Response.Redirect("/Auth/Login");
                return;
            }

            if (!ModelState.IsValid)
            {
                errorMessage = "Data validation failed";
                return;
            }

            // Read shopping cart items from cookie
            var productDictionary = getProductDictionary();
            if (productDictionary.Count < 1)
            {
                errorMessage = "Your cart is empty";
                return;
            }

            string productIdentifiers = Request.Cookies["shopping_cart"] ?? "";
            TempData["ProductIdentifiers"] = productIdentifiers;
            TempData["DeliveryAddress"] = Address;
            if (PaymentMethod == "credit_card" || PaymentMethod == "paypal")
            {
                Response.Redirect("/Checkout");
                return;
            }

            // save the order in the database
            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // create a new order in the orders table
                    int newOrderId = 0;
                    string sqlOrder = "INSERT INTO orders (client_id, order_date, shipping_fee, " +
                        "delivery_address, payment_method, payment_status, order_status) " +
                        "OUTPUT INSERTED.id " +
                        "VALUES (@client_id, CURRENT_TIMESTAMP, @shipping_fee, " +
                        "@delivery_address, @payment_method, 'pending', 'created')";

                    using (SqlCommand command = new SqlCommand(sqlOrder, connection))
                    {
                        command.Parameters.AddWithValue("@client_id", client_id);
                        command.Parameters.AddWithValue("@shipping_fee", shippingFee);
                        command.Parameters.AddWithValue("@delivery_address", Address);
                        command.Parameters.AddWithValue("@payment_method", PaymentMethod);

                        newOrderId = (int)command.ExecuteScalar();
                    }


                    // add the ordered books to the order_items table
                    string sqlItem = "INSERT INTO order_items (order_id, book_id, quantity, unit_price) " +
                        "VALUES (@order_id, @book_id, @quantity, @unit_price)";

                    foreach (var keyValuePair in productDictionary)
                    {
                        string productID = keyValuePair.Key;
                        int quantity = keyValuePair.Value;
                        decimal unitPrice = getProductPrice(productID);

                        using (SqlCommand command = new SqlCommand(sqlItem, connection))
                        {
                            command.Parameters.AddWithValue("@order_id", newOrderId);
                            command.Parameters.AddWithValue("@book_id", productID);
                            command.Parameters.AddWithValue("@quantity", quantity);
                            command.Parameters.AddWithValue("@unit_price", unitPrice);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            //Delete the Cookie "shopping_cart" from Browser.
            Response.Cookies.Delete("shopping_cart");

            successMessage = "Order created successfully";
        }


        private decimal getProductPrice(string productID)
        {
            decimal price = 0;

            try
            {
                string connectionString = "Data Source=labVMH8OX\\SQLEXPRESS;Initial Catalog=Khumalocraft;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT price FROM products WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", productID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                price = reader.GetDecimal(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return price;
        }
    }

}
    

    public class OrderItem
    {
        public ProductInfo productInfo = new ProductInfo();
        public int numCopies = 0;
        public decimal totalPrice = 0;
    }

