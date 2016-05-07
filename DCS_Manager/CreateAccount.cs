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
    
    public partial class CreateAccount : Form
    {
        
        public CreateAccount()
        {
            InitializeComponent();
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.AcceptButton = button1;
            textBox2.UseSystemPasswordChar = true;
            textBox3.UseSystemPasswordChar = true;
            textBox4.UseSystemPasswordChar = true;
        }

        /// <summary>
        /// //The button does a number of checks to dynamically create the user and send them a confirmation e-mail.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            
           
            //Setting the textbox variables and regex to check for e-mail validity.
            string user;
            user= textBox1.Text;
            string password;
            password = textBox2.Text;
            string cpassword;
            cpassword = textBox3.Text;
            string pin;
            pin = textBox4.Text;
            Regex regex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
            Match match = regex.Match(user);
            //Checks to ensure e-mail settings are set.
            if (File.Exists(@".\EncryptedEmailSettings.txt"))
            {
                //Check to ensure the username entered is a e-mail.
                if(match.Success)
                {
                    // Ttempts to create the login after checking to ensure passwords match and all boxes contain data
                    using (SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.MasterConnectionString))
                    {
                        SqlCommand createUser = new SqlCommand("CREATE LOGIN \"" + user + "\" WITH PASSWORD = '" + password + "';", thisConnection);


                        thisConnection.Open();
                        try
                        {
                            //Validates no empty boxes
                            if (user != "" && password != "" && cpassword != "" && password == cpassword && pin!="")
                            {
                                //Creates the log in.
                                createUser.ExecuteNonQuery();

                            }
                            //Signifies one of the boxes are empty or passwords don't match.
                            else
                            {
                                MessageBox.Show("Error: Invalid user information. Please try again.");
                            }
                        }

                        //Error from SQL call to database.
                        catch (Exception r)
                        {
                            MessageBox.Show("Create user failure" + r);

                        }
                        thisConnection.Close();

                    }
                    //This is used to create the user, give them perms, and send them an e-mail.
                    using (SqlConnection thisConnection2 = new SqlConnection(GlobalConnectionString.ConnectionString))
                    {

                        SqlCommand createUser2 = new SqlCommand("CREATE USER \"" + user + "\" FOR LOGIN \"" + user + "\";", thisConnection2);
                        SqlCommand createUser3 = new SqlCommand("GRANT SELECT, INSERT, UPDATE TO \"" + user + "\";", thisConnection2);

                        thisConnection2.Open();

                        try
                        {
                            //Ensures fields are not blank and passwords match.
                            if (user != "" && password != "" && cpassword != "" && password == cpassword && pin!="")
                            {
                                createUser2.ExecuteNonQuery();
                                createUser3.ExecuteNonQuery();
                                
                            }
                            // One of the fields are blank or password doesn't match.
                            else
                            {
                                MessageBox.Show("Error: Invalid user information. Please try again.");
                            }
                            try
                            {


                                System.IO.StreamReader file = new System.IO.StreamReader("EncryptedEmailSettings.txt");
                                string email = file.ReadLine();
                                string line = file.ReadLine();
                                file.Close();


                                var fromAddress = new MailAddress(email);
                                var toAddress = new MailAddress(user);
                                string fromPassword = StringCipher.Decrypt(line, pin);
                                const string subject = "DC# Account Creation";
                                const string body = "Hello, welcome to DC# Manager. An account has been created with your e-mail address. Your account will have read only permissions. If you did not sign up for an account, please reply to this e-mail.";

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
                                    MessageBox.Show("User " + user + " was created successfully.");
                                }
                            }
                            // This is an error in sending the e-mail. Can be from incorrect pin being entered. Gives user option on whether or not they still wish to create user.
                            catch
                            {
                                DialogResult dialogResult = MessageBox.Show("Failed to send the user an account creation e-mail. Check to ensure you used the correct pin. Do you wish to still create this user?", "E-mail Error", MessageBoxButtons.YesNo);
                                if (dialogResult == DialogResult.No)
                                {
                                    SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.MasterConnectionString);
                                    SqlConnection thisConnection3 = new SqlConnection(GlobalConnectionString.ConnectionString);
                                    thisConnection3.Open();
                                    thisConnection.Open();
                                    SqlCommand deleteLogin = new SqlCommand("DROP LOGIN \"" + user + " \";", thisConnection);
                                    SqlCommand deleteUser = new SqlCommand("DROP USER \"" + user + " \";", thisConnection3);
                                    deleteLogin.ExecuteNonQuery();
                                    deleteUser.ExecuteNonQuery();
                                    thisConnection3.Close();
                                    thisConnection.Close();
                                    MessageBox.Show("User was not created due to issues sending the confirmation e-mail.");
                                }
                                else
                                {
                                    MessageBox.Show("User is created but a confirmation e-mail was not sent.");
                                }
                            }
                        }

                        //Means the user is not created.
                        catch (Exception r)
                        {
                            MessageBox.Show("Create user failure" + r);

                        }



                        thisConnection2.Close();
                        //Code for getting password, decrypting it, and then sending an e-mail to the usernames e-mail address.
                       

                    }
                    
                    
                }
                else
                {
                    MessageBox.Show("The username must be a e-mail.");
                }
            }
            else
            {
                MessageBox.Show("The config file for e-mail settings does not exist yet. Please set your e-mail settings.");
            }
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
