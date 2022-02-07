using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200776P_PracAssignment
{
    public partial class Verification : System.Web.UI.Page
    {
        string MyDBConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        string email;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Replace string below with path to .env file
            Env.Load("C:/Users/mdirf/Desktop/School/Y2S2/App. Security/Assignment/200776P_PracAssignment/.env");
            //Env.Load();
            //Env.TraversePath().Load();

            // Check if session is valid
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    // Retrieve email from session variable
                    email = Session["LoggedIn"].ToString();

                    // User has to change password every 6 months
                    // Maximum password age
                    if (passwordAge(email).AddMonths(6) < DateTime.Now)
                    {
                        Response.Redirect("ChangePassword.aspx", false);
                    }

                    return;
                }
                else
                {
                    Response.Redirect("Login.aspx", false);
                    return;
                }
            }
            else
            {
                Response.Redirect("Login.aspx", false);
                return;
            }
        }

        protected void verifyBtn_Click(object sender, EventArgs e)
        {
            if (checkEmailDB())
            {
                string userCode = Request.Form["verificationCode"];
                string dbCode = retrieveVerificationCode();
                if (userCode.Equals(dbCode))
                {
                    // Update EmailVerified in database
                    SqlConnection connection = new SqlConnection(MyDBConnectionString);
                    string sqlUpdate = "UPDATE Account SET EmailVerified=@EmailVerified WHERE Email=@Email";
                    SqlCommand command = new SqlCommand(sqlUpdate, connection);
                    command.Parameters.AddWithValue("@EmailVerified", 1);
                    command.Parameters.AddWithValue("@Email", email);

                    try
                    {
                        connection.Open();

                        command.ExecuteNonQuery();
                    }

                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }

                    finally { connection.Close(); }

                    // Delete verification code
                    deleteVerificationRecord();

                    Response.Redirect("Home.aspx", false);
                    return;
                }
            }
        }

        // Function to delete verification code in VerificationTable
        protected void deleteVerificationRecord()
        {
            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sqlUpdate = "DELETE FROM VerificationTable WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sqlUpdate, connection);
            command.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                command.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return;
        }

        // Function to retreieve verification code from database
        protected string retrieveVerificationCode()
        {
            string code = null;

            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sql = "SELECT Code FROM VerificationTable WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Code"] != null && reader["Code"] != DBNull.Value)
                        {
                            code = reader["Code"].ToString();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return code;
        }

        // Function to check if email already exists in database
        protected bool checkEmailDB()
        {
            bool result = false;

            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sql = "SELECT COUNT(*) FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0)
                {
                    result = true;
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return result;
        }

        // Function to retrieve time that password was last changed
        protected DateTime passwordAge(string email)
        {
            DateTime pwdAge = DateTime.MinValue;

            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            // SQL statement to select last 2 unique password hashes
            string sql = "SELECT LastPwdUpdate FROM Account WHERE Email = @Email;";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["LastPwdUpdate"] != null && reader["LastPwdUpdate"] != DBNull.Value)
                        {
                            string dbPwdAge = reader["LastPwdUpdate"].ToString();

                            pwdAge = DateTime.Parse(dbPwdAge);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return pwdAge;
        }
    }
}