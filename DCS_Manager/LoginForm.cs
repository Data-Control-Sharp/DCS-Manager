using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;


namespace DCS_Manager
{
    public partial class LoginForm : Form
    {

        public string username { get; set; }
        
        public LoginForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Login using a specified username and password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void login(string username, string password)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Server"] = "tcp:cshw2.database.windows.net";
            builder["Database"] = "CSHW";
            builder["User ID"] = username + "@cshw2";
            builder["Password"] = password;
            builder["TrustServerCertificate"] = false;
            builder["Connection Timeout"] = 30;
            GlobalConnectionString.ConnectionString = builder.ConnectionString;

            SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder();
            masterBuilder["Server"] = "tcp:cshw2.database.windows.net";
            masterBuilder["Database"] = "master";
            masterBuilder["User ID"] = username + "@cshw2";
            masterBuilder["Password"] = password;
            masterBuilder["TrustServerCertificate"] = false;
            masterBuilder["Connection Timeout"] = 30;
            GlobalConnectionString.MasterConnectionString = masterBuilder.ConnectionString;


        }

        /// <summary>
        /// Test the connection and ensure the application can connection to the database.
        /// Used exclusively for forms.
        /// </summary>
        public bool testFormConnection()
        {
            //Test connection
            try
            {
                using (SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.ConnectionString))
                {
                    thisConnection.Open();
                }
                this.Hide();
                return true;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Test the connection and ensure the application can connection to the database.
        /// </summary>
        public static bool testConnection()
        {
            //Test connection
            try
            {
                using (SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.ConnectionString))
                {
                    thisConnection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Action that occurs when login button is hit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //Assign connection string
            this.username = textBox1.Text;
            //Call login function
            login(username, textBox2.Text);
            //Test connection
            if (!testFormConnection())
            {
                MessageBox.Show("Error: Invalid login information. Please try again.");
            }

        }

        /// <summary>
        /// Actions that occur when the form is loading.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginForm_Load(object sender, EventArgs e)
        {
            //Help Provider
            helpProvider1.SetHelpString(textBox1, "Enter your DC# DB Username");
            helpProvider1.SetHelpString(textBox2, "Enter your DC# DB Password");
            helpProvider1.SetHelpString(button1, "Sign in with your given username and password.");

            this.AcceptButton = button1;
            textBox2.UseSystemPasswordChar = true;
        }

        /// <summary>
        /// Function that is called when the forgot password link is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void passwordLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Contact your administrator to reset your password.");
        }

        /// <summary>
        /// Function that is called when the create an account link is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        
        private void accountLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
        }
        
        
    }
}
