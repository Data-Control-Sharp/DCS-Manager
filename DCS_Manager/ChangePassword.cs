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
    /// The code will first attempt to send the given user a message letting them know the password is changed. If this fails in any way the password is not changed
    /// since the user cannot be notified. If it succeeds then the password will be changed.
    /// </summary>
    public partial class ChangePassword : Form
    {
        public ChangePassword()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string user = textBox1.Text;
            string pass = textBox2.Text;
            string cpass = textBox3.Text;
            string pin = textBox4.Text;
            //First checks to ensure user has same password in both boxes.
            if (File.Exists(@".\EncryptedEmailSettings.txt"))
            {
                if (pass != cpass || pass == "" || user == "" || cpass == "" || pin == "")
                {
                    MessageBox.Show("Field was left blank or Password and Confirm New did not match, please re-enter password and confirm new password.");
                }
                else
                {
                    //Tries to send the e-mail.
                    try
                    {
                        System.IO.StreamReader file = new System.IO.StreamReader("EncryptedEmailSettings.txt");
                        string email = file.ReadLine();
                        string line = file.ReadLine();
                        file.Close();


                        var fromAddress = new MailAddress(email);
                        var toAddress = new MailAddress(user);
                        string fromPassword = StringCipher.Decrypt(line, pin);
                        const string subject = "DC# Password Change";
                        const string body = "Hello, your password has been changed by an Administrator. If you were not aware of this change or have any questions please reach out to your administrator.";

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

                        }
                        //If e-mail succeeds attempts to change password.
                        try
                        {


                            SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.MasterConnectionString);
                            thisConnection.Open();
                            SqlCommand changeLoginP = new SqlCommand("ALTER LOGIN \"" + user + "\" WITH PASSWORD = \'" + pass + "\';", thisConnection);
                            changeLoginP.ExecuteNonQuery();
                            thisConnection.Close();
                            MessageBox.Show("Password Successfully Changed and user has been notified.");
                        }
                        //Password change error.
                        catch (Exception r)
                        {
                            MessageBox.Show("Error Changing Password, note that the user will still receive a message saying the password is changed." + r);
                        }

                    }
                    //E-mail send error.
                    catch (Exception P)
                    {
                        MessageBox.Show("Error sending e-mail, password not changed" + P);
                    }
                }
            }
            else
            {
                MessageBox.Show("The e-mail settings file does not exist, please use your e-mail settings and save them.");
            }
            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void ChangePassword_Load(object sender, EventArgs e)
        {
            this.AcceptButton = button1;
            textBox2.UseSystemPasswordChar = true;
            textBox3.UseSystemPasswordChar = true;
            textBox4.UseSystemPasswordChar = true;
        }
    }
}
