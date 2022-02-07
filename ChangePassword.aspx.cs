using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200776P_PracAssignment
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        string MyDBConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        byte[] Key;
        byte[] IV;
        string email;
        static string salt;

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

                    if (passwordAge(email).AddMonths(6) < DateTime.Now)
                    {
                        alertMsg_display.Text = "You need to change your password!";
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
        protected void confirmBtn_Click(object sender, EventArgs e)
        {
            if (validateCaptcha())
            {
                // Retrieve form values
                string currPassword = Request.Form["currPassword"];
                string newPassword = Request.Form["newPassword"];

                // Check if user exists in database
                if ((!String.IsNullOrEmpty(currPassword)) && (!String.IsNullOrEmpty(newPassword) && checkEmailDB(email)))
                {
                    // Check current password
                    SHA512Managed hashing = new SHA512Managed();
                    string dbHash = getDBHash(email);
                    string dbSalt = getDBSalt(email);

                    try
                    {
                        if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                        {
                            string pwdWithSalt = currPassword + dbSalt;
                            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                            string userHash = Convert.ToBase64String(hashWithSalt);

                            // Validate new password if current password is correct
                            if (userHash.Equals(dbHash))
                            {
                                // New password Validation
                                int scores = checkPassword(newPassword);
                                string status = "";
                                switch (scores)
                                {
                                    case 1:
                                        status = "Very Weak";
                                        break;
                                    case 2:
                                        status = "Weak";
                                        break;
                                    case 3:
                                        status = "Medium";
                                        break;
                                    case 4:
                                        status = "Strong";
                                        break;
                                    case 5:
                                        status = "Very Strong";
                                        break;
                                    default:
                                        break;
                                }
                                newPassword_lbl.Text = "Password Strength : " + status;
                                if (scores < 4)
                                {
                                    newPassword_lbl.ForeColor = Color.Red;
                                    return;
                                }

                                // If password is strong enough, update database with new password
                                newPassword_lbl.ForeColor = Color.Green;
                                updatePassword(newPassword);
                                return;
                            }

                            else
                            {
                                errorMsg.Text = "Incorrect password";
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
                    errorMsg.Text = "Incorrect password or email";
                }
            }
            else
            {
                errorMsg.Text = "Please try again";
            }
        }

        // Function to update database with new password
        protected void updatePassword(string password)
        {
            // Get salt from database
            salt = getDBSalt(email);
            string dbHash = getDBHash(email);
            SHA512Managed hashing = new SHA512Managed();

            string pwdWithSalt = password + salt;
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

            string finalHash = Convert.ToBase64String(hashWithSalt);
            
            // Error if current password == new password
            if (finalHash.Equals(dbHash))
            {
                errorMsg.Text = "New password cannot be the same as the current password";
                return;
            }

            // Error if new password == 1 of 2 last passwords
            else if (passwordHistory(email).Contains(finalHash))
            {
                errorMsg.Text = "New password cannot be the same as the last 2 passwords";
                return;
            }

            // Error if time of password update is within 1 day of time that password was last changed
            else if (passwordAge(email).AddDays(1) > DateTime.Now)
            {
                errorMsg.Text = "You cannot change passwords more than once within 1 day";
                return;
            }

            else
            {
                // Update database with new value for Password
                SqlConnection connection = new SqlConnection(MyDBConnectionString);
                string sqlUpdate = "UPDATE Account SET PasswordHash=@PasswordHash, PasswordSalt=@PasswordSalt, LastPwdUpdate=@LastPwdUpdate WHERE Email=@Email";
                SqlCommand command = new SqlCommand(sqlUpdate, connection);
                command.Parameters.AddWithValue("@PasswordHash", finalHash);
                command.Parameters.AddWithValue("@PasswordSalt", salt);
                command.Parameters.AddWithValue("@LastPwdUpdate", DateTime.Now);
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

                Response.Redirect("Home.aspx", false);
                return;
            }
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

        // Function to retrieve password history
        protected List<String> passwordHistory(string email)
        {
            List<String> pwdList = new List<String>();

            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            // SQL statement to select last 2 unique password hashes
            string sql = "SELECT PasswordHash, UpdatedOn FROM AccountAudit WHERE Email = @Email ORDER BY UpdatedOn DESC;";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            var dbHash = getDBHash(email);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null && reader["PasswordHash"] != DBNull.Value)
                        {
                            var pwd = reader["PasswordHash"].ToString();

                            if (!pwd.Equals(dbHash))
                            {
                                if (!pwdList.Contains(pwd))
                                {
                                    pwdList.Add(pwd);
                                    Debug.WriteLine(pwd);

                                    if (pwdList.Count() == 2)
                                    {
                                        return pwdList;
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return pwdList;
        }

        private int checkPassword(string password)
        {
            int score = 0;

            if (password.Length < 12)
            {
                return 1;
            }
            else
            {
                score = 1;
            }

            if (Regex.IsMatch(password, "[a-z]"))
            {
                score++;
            }

            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score++;
            }

            if (Regex.IsMatch(password, "[0-9]"))
            {
                score++;
            }

            if (Regex.IsMatch(password, "[^a-zA-Z0-9\\s]"))
            {
                score++;
            }

            return score;
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
                "https://www.google.com/recaptcha/api/siteverify?secret=" + HttpUtility.UrlEncode(Environment.GetEnvironmentVariable("SECRET_KEY")) + " &response=" + HttpUtility.UrlEncode(captchaResponse)
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
}