using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace DCS_Manager
{
    public partial class EmailSettings : Form
    {
        public EmailSettings()
        {
            InitializeComponent();
        }
        /// <summary>
        /// When clicked, the button will generate a text file containing the encrypted text file. The pin box is used as the encryption key. Same encryption key must be used when decrypting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            
            string email;
            email = textBox1.Text;
            string password;
            password = textBox2.Text;
            string pin;
            pin = textBox4.Text;
            Regex regex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
            Match match = regex.Match(email);
            //Checks for valid e-mail address
            if (match.Success)
            {
                //Checks for blank boxes
                if (email != "" && password != "" && pin != "")
                {
                    string encryptedstring = StringCipher.Encrypt(password, pin);
                    string[] lines = { email, encryptedstring };
                    System.IO.File.WriteAllLines(@".\EncryptedEmailSettings.txt", lines);
                    MessageBox.Show("The information has been saved.");
                }
                else
                {
                    MessageBox.Show("One of the boxes was left blank.");
                }
            }
            else
            {
                MessageBox.Show("This is not a valid e-mail.");
            }


            this.Close();
        }

        private void EmailSettings_Load(object sender, EventArgs e)
        {
            this.AcceptButton = button1;
            textBox2.UseSystemPasswordChar = true;
            textBox4.UseSystemPasswordChar = true;
        }
    }
}
