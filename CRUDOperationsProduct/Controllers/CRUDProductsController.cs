using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUDOperationsProduct.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRUDProductsController : ControllerBase
    {
        SqlConnection con;

        public CRUDProductsController(IConfiguration c)
        {
            con = new SqlConnection();
            con.ConnectionString = c.GetConnectionString("conStr");
        }

        [HttpGet("products")]
        public ActionResult<List<Product>> GetAllProducts()
        {
            SqlDataAdapter sda = new SqlDataAdapter("select * from HarshalProduct", con);
            DataSet ds = new DataSet();
            sda.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0)
            {
                return NoContent();
            }

            else
            {
                int count = 0;
                Product p;
                List<Product> ProductList = new List<Product>();
                while (count < ds.Tables[0].Rows.Count)
                {
                    p = new Product();
                    p.ProductId = Convert.ToInt32(ds.Tables[0].Rows[count]["ProductId"]);
                    p.Name = ds.Tables[0].Rows[count]["Name"].ToString();
                    p.Description = ds.Tables[0].Rows[count]["Description"].ToString();
                    p.Price = (double)ds.Tables[0].Rows[count]["Price"];
                    p.Category = ds.Tables[0].Rows[count]["Category"].ToString();
                    ProductList.Add(p);
                    count++;
                }
                return ProductList;
            }
        }

        [HttpGet("products/{id}")]
        public ActionResult<Product> GetProductByID(int id)
        {
            SqlCommand cmd = new SqlCommand("select * from HarshalProduct where ProductId = @productID");
            cmd.Connection = con;
            cmd.Parameters.Add(new SqlParameter("productID", SqlDbType.Int));
            cmd.Parameters["productID"].Value = id;
            SqlDataReader reader;
            con.Open();
            reader = cmd.ExecuteReader();
            Product p = null;

            if (reader.HasRows == false)
            {
                return NotFound("Record not found");
            }
            else
            {
                while (reader.Read())
                {
                    p = new Product();
                    p.ProductId = Convert.ToInt32(reader["ProductId"]);
                    p.Name = reader["Name"].ToString();
                    p.Description = reader["Description"].ToString();
                    p.Price = (double)reader["Price"];
                    p.Category = reader["Category"].ToString();
                }
                con.Close();
                return p;
            }
        }

        [HttpPost("products")]
        public ActionResult<string> CreateProduct(Product p)
        {
            SqlDataReader reader;
            bool isRepeatEntry = false;
            SqlCommand repeatCmd = new SqlCommand("select * from HarshalProduct", con);
            repeatCmd.Connection.Open();
            reader = repeatCmd.ExecuteReader();
            while(reader.Read())
            {
                if (Convert.ToInt32(reader["ProductId"]) == p.ProductId)
                {
                    isRepeatEntry = true;
                    break;
                }
                   
                else
                {
                    isRepeatEntry = false;
                }
            }
            repeatCmd.Connection.Close();


            if(isRepeatEntry == false)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "insert into HarshalProduct values(@productID, @name, @description, @price, @category)";
                cmd.Connection = con;
                cmd.Parameters.Add(new SqlParameter("productID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar, 250));
                cmd.Parameters.Add(new SqlParameter("description", SqlDbType.VarChar, 250));
                cmd.Parameters.Add(new SqlParameter("price", SqlDbType.Float));
                cmd.Parameters.Add(new SqlParameter("category", SqlDbType.VarChar, 15));
                cmd.Parameters["productID"].Value = p.ProductId;
                cmd.Parameters["name"].Value = p.Name;
                cmd.Parameters["description"].Value = p.Description;
                cmd.Parameters["price"].Value = p.Price;
                cmd.Parameters["category"].Value = p.Category;
                con.Open();
                int records = cmd.ExecuteNonQuery();
                con.Close();
                return Ok(records.ToString() + " record inserted successfully !");
            }

            else
            {
                return Problem("Repeated Primary Key is not allowed");
            }
        }

        [HttpDelete("products/{id}")]
        public ActionResult<string> DeleteProduct(int id)
        {
            SqlCommand cmd = new SqlCommand("delete from HarshalProduct where ProductId = @productID");
            cmd.Connection = con;
            cmd.Parameters.Add(new SqlParameter("productID", SqlDbType.Int));
            cmd.Parameters["productID"].Value = id;
            con.Open();
            int records = cmd.ExecuteNonQuery();
            con.Close();
            if(records == 0)
            {
                return NotFound("Record not found");
            }
            else
            {
                return Ok(records.ToString() + " record deleted successfully !");
            }
        }

        [HttpPut("products/{id}")]
        public ActionResult<string> UpdateProduct(int id, Product p)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "update HarshalProduct set Name = @name, Description = @description, Price = @price, Category = @category where ProductId = @productID";
            cmd.Connection = con;
            cmd.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar, 250));
            cmd.Parameters.Add(new SqlParameter("description", SqlDbType.VarChar, 250));
            cmd.Parameters.Add(new SqlParameter("price", SqlDbType.Float));
            cmd.Parameters.Add(new SqlParameter("category", SqlDbType.VarChar, 15));
            cmd.Parameters.Add(new SqlParameter("productID", SqlDbType.Int));
            cmd.Parameters["name"].Value = p.Name;
            cmd.Parameters["description"].Value = p.Description;
            cmd.Parameters["price"].Value = p.Price;
            cmd.Parameters["category"].Value = p.Category;
            cmd.Parameters["productID"].Value = id;
            con.Open();
            int records = cmd.ExecuteNonQuery();
            con.Close();
            if(records == 0)
            {
                return NotFound("Record not found");
            }

            else
            {
                return Ok(records.ToString() + " record updated successfully !");
            }

        }
    }
}
