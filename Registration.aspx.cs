using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
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
    public partial class Registration : System.Web.UI.Page
    {
        string MyDBConnectionString = 
            System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {
            
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

        protected void registerBtn_Click(object sender, EventArgs e)
        {
            if (validateCaptcha())
            {
                // Retrieve all form fields
                string fName = Request.Form["fName"];
                string lName = Request.Form["lName"];
                string email = Request.Form["email"];
                string password = Request.Form["password"];
                string DOB = Request.Form["DOB"];
                string CCInfo = Request.Form["CCInfo"];

                // Retrieve image file
                int imgFileLength = photo.PostedFile.ContentLength;
                byte[] imageBytes = new byte[imgFileLength];
                HttpPostedFile imageFile = photo.PostedFile;
                imageFile.InputStream.Read(imageBytes, 0, imgFileLength);

                // Initialise variable for validation
                bool validation = true;


                // Form validation
                string msg = "Field cannot be empty";
                if (String.IsNullOrEmpty(fName) || fName.Length >= 50 || Regex.IsMatch(fName, "[^A-Za-z]"))
                {
                    if (String.IsNullOrEmpty(fName))
                    {
                        fName_lbl.Text = msg;
                        
                    }
                    else if (fName.Length >= 50)
                    {
                        fName_lbl.Text = "Cannot be more than 50 letters";
                        
                    }
                    else if (Regex.IsMatch(fName, "[^A-Za-z]"))
                    {
                        fName_lbl.Text = "Can only contain letters";
                    }
                    validation = false;
                }
                else { fName_lbl.Text = ""; }

                if (String.IsNullOrEmpty(lName) || lName.Length >= 50 || Regex.IsMatch(lName, "[^A-Za-z]"))
                {
                    if (String.IsNullOrEmpty(lName))
                    {
                        lName_lbl.Text = msg;                  
                    }
                    else if (lName.Length >= 50)
                    {
                        lName_lbl.Text = "Cannot be more than 50 letters";
                    }
                    else if (Regex.IsMatch(lName, "[^A-Za-z]"))
                    {
                        lName_lbl.Text = "Can only contain letters";
                    }   
                    validation = false;
                }
                else { lName_lbl.Text = ""; }

                var emailCheck = new EmailAddressAttribute();
                if (String.IsNullOrEmpty(email) || !emailCheck.IsValid(email) || checkEmailDB(email))
                {  
                    if (String.IsNullOrEmpty(email))
                    {
                        email_lbl.Text = msg;
                    }
                    else if (!emailCheck.IsValid(email))
                    {
                        email_lbl.Text = "Invalid email address";
                    }
                    else if (checkEmailDB(email))
                    {
                        email_lbl.Text = "Email already exists";
                    }
                    validation = false;
                }
                else { email_lbl.Text = ""; }

                DateTime parsedDOB = DateTime.Parse(DOB);
                if (String.IsNullOrEmpty(DOB) || parsedDOB >= DateTime.Now)
                {
                    if (String.IsNullOrEmpty(DOB))
                    {
                        DOB_lbl.Text = msg;
                    }
                    else if (parsedDOB >= DateTime.Now)
                    {
                        DOB_lbl.Text = "Invalid date selected";
                    }
                    validation = false;
                }
                else { DOB_lbl.Text = ""; }

                if (photo.HasFile == false)
                {
                    photo_lbl.Text = msg;
                    validation = false;
                }
                else { photo_lbl.Text = ""; }

                if (String.IsNullOrEmpty(CCInfo) || CCInfo.Length != 16 || Regex.IsMatch(CCInfo, "[^0-9]"))
                {
                    if (String.IsNullOrEmpty(CCInfo))
                    {
                        CCInfo_lbl.Text = msg;
                    }
                    else if (CCInfo.Length != 16)
                    {
                        CCInfo_lbl.Text = "Invalid credit card number";
                    }
                    else if (Regex.IsMatch(CCInfo, "[^0-9]"))
                    {
                        CCInfo_lbl.Text = "Invalid credit card number";
                    }
                    validation = false;
                }
                else { CCInfo_lbl.Text = ""; }


                // Password Validation
                int scores = checkPassword(password);
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
                password_lbl.Text = "Password Strength : " + status;
                if (scores < 4)
                {
                    password_lbl.ForeColor = Color.Red;
                    validation = false;
                    return;
                }
                password_lbl.ForeColor = Color.Green;

                // Redirect to Login & store in database if validation == true
                if (validation)
                {
                    // Store input in database

                    // Generate salt
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                    byte[] saltByte = new byte[8];
                    rng.GetBytes(saltByte);
                    salt = Convert.ToBase64String(saltByte);

                    SHA512Managed hashing = new SHA512Managed();

                    string pwdWithSalt = password + salt;
                    byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(password));
                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                    finalHash = Convert.ToBase64String(hashWithSalt);

                    RijndaelManaged cipher = new RijndaelManaged();
                    cipher.GenerateKey();
                    Key = cipher.Key;
                    IV = cipher.IV;

                    createAccount(fName, lName, email, finalHash, DOB, imageBytes, CCInfo, salt, IV, Key);

                    Response.Redirect("Login.aspx", false);
                    return;
                }

            }
            else
            {
                registerMsg.Text = "Please try again";
                return;
            }
        }

        protected void createAccount(string fName, string lName, string email, string finalHash, 
            string DOB, byte[] imageFile, string CCInfo, string salt, byte[] IV, byte[] Key)
        {
            try
            {
                string sqlString = "INSERT INTO Account VALUES(" +
                    "@Id, @FName, @LName, @Email, @PasswordHash, @DOB, " +
                    "@Photo, @CCInfo, @PasswordSalt, @IV, @Key, @EmailVerified, @AttemptsLeft, @LastPwdUpdate)";

                using (SqlConnection con = new SqlConnection(MyDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sqlString))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                            cmd.Parameters.AddWithValue("@FName", fName);
                            cmd.Parameters.AddWithValue("@LName", lName);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@DOB", DOB);
                            cmd.Parameters.AddWithValue("@Photo", imageFile);
                            cmd.Parameters.AddWithValue("@CCInfo", Convert.ToBase64String(encryptData(CCInfo)));
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                            cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                            cmd.Parameters.AddWithValue("@EmailVerified", 0);
                            cmd.Parameters.AddWithValue("@AttemptsLeft", 3);
                            cmd.Parameters.AddWithValue("@LastPwdUpdate", DateTime.Now);

                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        // Function to excrpt data using symmetric key
        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();

                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return cipherText;
        }

        public bool validateCaptcha()
        {
            bool result = true;

            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(
                "https://www.google.com/recaptcha/api/siteverify?secret=6Ldga1UeAAAAAPN0rZv1SBZXMsCqbzUShnMQPBz2 &response=" + HttpUtility.UrlEncode(captchaResponse)
                );

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        MyRegisterObject jsonObject = js.Deserialize<MyRegisterObject>(jsonResponse);
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

                int count = (int) command.ExecuteScalar();
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
    }

    public class MyRegisterObject
    {
        public string success { get; set; }
        public List<string> ErrorMessage { get; set; }
    }
}