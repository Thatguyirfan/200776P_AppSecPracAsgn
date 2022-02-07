﻿using System;
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
            // Check if session is valid
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    // Retrieve email from session variable
                    email = Session["LoggedIn"].ToString();
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
            
            if (finalHash.Equals(dbHash))
            {
                errorMsg.Text = "New password cannot be the same as the current password";
                return;
            }
            else
            {
                // Update database with new value for Password
                SqlConnection connection = new SqlConnection(MyDBConnectionString);
                string sqlUpdate = "UPDATE Account SET PasswordHash=@PasswordHash, PasswordSalt=@PasswordSalt WHERE Email=@Email";
                SqlCommand command = new SqlCommand(sqlUpdate, connection);
                command.Parameters.AddWithValue("@PasswordHash", finalHash);
                command.Parameters.AddWithValue("@PasswordSalt", salt);
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
}