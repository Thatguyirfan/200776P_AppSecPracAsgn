using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200776P_PracAssignment
{
    public partial class Login : System.Web.UI.Page
    {
        string MyDBConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        string fName;
        string lName;
        string globalEmail;
        string password;
        string DOB;
        string CCInfo;
        byte[] imageBytes;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;
        int emailVerified;
        int globalAttemptsLeft;


        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void loginBtn_Click(object sender, EventArgs e)
        {
            if (validateCaptcha())
            {
                // Retrieve form values
                string email = Request.Form["email"];
                string password = Request.Form["password"];
            
                // Check if user exists in database
                if ((!String.IsNullOrEmpty(email)) && (!String.IsNullOrEmpty(password) && checkEmailDB(email)))
                {
                    int attemptsLeft = retrieveLoginAttempt(email);
                    // Check if account is locked
                    if (attemptsLeft > 0)
                    {
                        // Check password
                        SHA512Managed hashing = new SHA512Managed();
                        string dbHash = getDBHash(email);
                        string dbSalt = getDBSalt(email);

                        try
                        {
                            if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                            {
                                string pwdWithSalt = password + dbSalt;
                                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                string userHash = Convert.ToBase64String(hashWithSalt);

                                // Create session if login credentials are valid
                                if (userHash.Equals(dbHash))
                                {
                                    globalEmail = email;
                                    Session["LoggedIn"] = email;

                                    string guid = Guid.NewGuid().ToString();
                                    Session["AuthToken"] = guid;

                                    Response.Cookies.Add(new HttpCookie("AuthToken", guid));

                                    // Reset login attempts
                                    if (attemptsLeft < 3)
                                    {
                                        resetLoginAttempts(email);
                                    }

                                    Response.Redirect("Home.aspx", false);
                                    return;
                                }

                                else
                                {
                                    // Reduce number of login attempts since password was guessed wrongly
                                    attemptsLeft = failLoginAttempt(email);
                                    if (attemptsLeft > 0)
                                    {
                                        loginMsg.Text = "Incorrect username or password";
                                    }
                                    else
                                    {
                                        loginMsg.Text = "Account has been locked out after multiple attempts.\nPlease check your email";
                                    }
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }
                    }
                    else
                    {
                        loginMsg.Text = "Account has been locked out after multiple attempts.\nPlease check your email";
                    }
                }
                else
                {
                    loginMsg.Text = "Incorrect username or password";
                }
            }
            else
            {
                loginMsg.Text = "Incorrect username or password";
            }
        }

        // Function to add LoggedIn details in audit table
        //protected void auditLogin()
        //{

        //}

        //protected void retrieveDetails()
        //{
        //    SqlConnection connection = new SqlConnection(MyDBConnectionString);
        //    string sql = "SELECT * FROM Account WHERE Email=@Email";
        //    SqlCommand command = new SqlCommand(sql, connection);
        //    command.Parameters.AddWithValue("@Email", globalEmail);

        //    try
        //    {
        //        connection.Open();

        //        using (SqlDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                // Retrieve image data
        //                fName = reader["FName"].ToString();
        //                lName = reader["LName"].ToString();
        //                globalEmail = reader["Email"].ToString();
        //                finalHash = reader["PasswordHash"].ToString();
        //                DOB = reader["DOB"].ToString();
        //                imageBytes = (byte[])reader["Photo"];
        //                CCInfo = reader["CCInfo"].ToString();
        //                salt = reader["PasswordSalt"].ToString();
        //                IV = (byte[])reader["IV"];
        //                Key = (byte[])reader["Key"];
        //                emailVerified = Convert.ToInt32(reader["EmailVerified"]);
        //                globalAttemptsLeft = Convert.ToInt32(reader["AttemptsLeft"]);
        //            }
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }

        //    finally { connection.Close(); }

        //    return;
        //}

        // Function to reset number of login attempts on successful login
        protected void resetLoginAttempts(string email)
        {
            // Update database with new value for AttemptsLeft
            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sqlUpdate = "UPDATE Account SET AttemptsLeft=@AttemptsLeft WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sqlUpdate, connection);
            command.Parameters.AddWithValue("@AttemptsLeft", 3);
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

        // Function to retrieve number of login attempts
        protected int retrieveLoginAttempt(string email)
        {
            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sql = "SELECT AttemptsLeft FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            // Initialise number of attempts left
            int attemptsLeft;

            try
            {
                connection.Open();

                attemptsLeft = (int)command.ExecuteScalar();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return attemptsLeft;
        }

        // Function to reduce number of login attempts
        // and return number of attempts left
        protected int failLoginAttempt(string email)
        {
            int attemptsLeft = retrieveLoginAttempt(email);
            if (attemptsLeft > 0)
            {
                attemptsLeft = attemptsLeft - 1;
            }
            else
            {
                return 0;
            }

            // Update database with new value for AttemptsLeft
            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sqlUpdate = "UPDATE Account SET AttemptsLeft=@AttemptsLeft WHERE Email=@Email";
            SqlCommand newCommand = new SqlCommand(sqlUpdate, connection);
            newCommand.Parameters.AddWithValue("@AttemptsLeft", attemptsLeft);
            newCommand.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                newCommand.ExecuteNonQuery();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return attemptsLeft;
        }

        // Function to check if email already exists in database
        protected bool checkEmailDB(string email)
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

        protected string getDBHash(string email)
        {
            string h = null;

            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sql = "SELECT PasswordHash FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null && reader["PasswordHash"] != DBNull.Value)
                        {
                            h = reader["PasswordHash"].ToString();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }
            
            return h;
        }

        protected string getDBSalt(string email)
        {
            string s = null;

            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sql = "SELECT PasswordSalt FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordSalt"] != null && reader["PasswordSalt"] != DBNull.Value)
                        {
                            s = reader["PasswordSalt"].ToString();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return s;
        }

        public bool validateCaptcha()
        {
            bool result = true;

            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(
                "https://www.google.com/recaptcha/api/siteverify?secret=6Ldga1UeAAAAAPN0rZv1SBZXMsCqbzUShnMQPBz2 &response=" + captchaResponse
                );

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);
                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
    }

    public class MyObject
    {
        public string success { get; set; }
        public List<string> ErrorMessage { get; set; }
    }
}