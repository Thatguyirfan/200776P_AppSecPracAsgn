using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200776P_PracAssignment
{
    public partial class Home : System.Web.UI.Page
    {
        string MyDBConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        byte[] Key;
        byte[] IV;
        byte[] CCInfo;
        string email;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if session is valid
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    // Retrieve email from session variable
                    email = Session["LoggedIn"].ToString();
                    retrieveDetails(email);
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

        protected void signOutBtn_Click(object sender, EventArgs e)
        {
            // Cear session
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            Response.Redirect("Login.aspx", false);

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Request.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Request.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }
            if (Request.Cookies["AuthToken"] != null)
            {
                Request.Cookies["AuthToken"].Value = string.Empty;
                Request.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }
        }

        protected void retrieveDetails(string email)
        {
            SqlConnection connection = new SqlConnection(MyDBConnectionString);
            string sql = "SELECT * FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            try
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Retrieve image data
                        byte[] imageData = (byte[]) reader["Photo"];
                        photo.ImageUrl = "data:Image/png;base64," + HttpUtility.HtmlEncode(Convert.ToBase64String(imageData));
                        fullname.Text = HttpUtility.HtmlEncode(reader["FName"].ToString() + " " + reader["LName"].ToString());
                        fName_display.Text = HttpUtility.HtmlEncode(reader["FName"].ToString());
                        lName_display.Text = HttpUtility.HtmlEncode(reader["LName"].ToString());
                        email_display.Text = HttpUtility.HtmlEncode(reader["Email"].ToString());
                        email_title.Text = HttpUtility.HtmlEncode(reader["Email"].ToString());
                        DateTime DOB = DateTime.Parse(reader["DOB"].ToString());
                        DOB_display.Text = HttpUtility.HtmlEncode(DOB.ToShortDateString());
                        CCInfo = Convert.FromBase64String(reader["CCInfo"].ToString());
                        IV = Convert.FromBase64String(reader["IV"].ToString());
                        Key = Convert.FromBase64String(reader["Key"].ToString());
                    }
                    CCInfo_display.Text = HttpUtility.HtmlEncode(decryptData(CCInfo));
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            finally { connection.Close(); }

            return;
        }

        protected string decryptData(byte[] cipherText)
        {
            string plainText = null;

            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform decryptTransform = cipher.CreateDecryptor();

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptTransform, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return plainText;
        }

        protected void changePwdBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("ChangePassword.aspx", false);
            return;
        }
    }
}