using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.IO;


namespace DCS_Manager
{
    /// <summary>
    /// Used to give the admin an option to delete a user.
    /// </summary>
    public partial class DeleteUser : Form
    {
        public DeleteUser()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Sets enter button and hides pin field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteUser_Load(object sender, EventArgs e)
        {
            this.AcceptButton = button1;
            
            textBox4.UseSystemPasswordChar = true;
        }

        /// <summary>
        /// When clicked deletes user if exists and offers to send user e-mail letting them know.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string user;
            user = textBox1.Text;
            string pin;
            pin = textBox4.Text;
            //Checks to make sure pin exists for e-mail.
            if (File.Exists(@".\EncryptedEmailSettings.txt"))
            {   
                //Trys the delete user.
                try
                {
                    SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.MasterConnectionString);
                    SqlConnection thisConnection2 = new SqlConnection(GlobalConnectionString.ConnectionString);
                    thisConnection2.Open();
                    thisConnection.Open();
                    SqlCommand deleteLogin = new SqlCommand("DROP LOGIN \"" + user + " \";", thisConnection);
                    SqlCommand deleteUser = new SqlCommand("DROP USER \"" + user + " \";", thisConnection2);
                    deleteLogin.ExecuteNonQuery();
                    deleteUser.ExecuteNonQuery();
                    thisConnection2.Close();
                    thisConnection.Close();
                    DialogResult dialogResult1 = MessageBox.Show("User successfully deleted. Would you like to send them an e-mail letting them know?", "Send E-mail?", MessageBoxButtons.YesNo);

                    // If an e-mail wishes to be sent to a deleted user.
                    if (dialogResult1 == DialogResult.Yes)
                    {
                        try
                        {


                            System.IO.StreamReader file = new System.IO.StreamReader("EncryptedEmailSettings.txt");
                            string email = file.ReadLine();
                            string line = file.ReadLine();
                            file.Close();


                            var fromAddress = new MailAddress(email);
                            var toAddress = new MailAddress(user);
                            string fromPassword = StringCipher.Decrypt(line, pin);
                            const string subject = "DC# Account Deletion";
                            const string body = "Hello, your DCS Manager has been deleted. Please reach out to an administrator for further information.";

                            var smtp = new SmtpClient
                            {
                                Host = "smtp.gmail.com",
                                Port = 587,
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                            };
                            using (var message = new MailMessage(fromAddress, toAddress)
                            {
                                Subject = subject,
                                Body = body
                            })
                            {
                                smtp.Send(message);
                                MessageBox.Show("Email successfully sent.");
                            }
                        }
                        catch (Exception y)
                        {
                            MessageBox.Show("There was an error sending the e-mail. More Info: " + y);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No e-mail will be sent.");
                    }
                    this.Close();
                }
                catch (Exception k)
                {
                    DialogResult dialogResult = MessageBox.Show("Unable to delete user because they do not exist. More info: "+ k);
                }
            }
            else
            {
                MessageBox.Show("The config file for e-mail settings does not exist yet. Please set your e-mail settings.");
            }
        }
    }
}
