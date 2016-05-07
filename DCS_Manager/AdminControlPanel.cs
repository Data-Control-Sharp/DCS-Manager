using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCS_Manager
{
    public partial class AdminControlPanel : Form
    {
        public AdminControlPanel()
        {
            InitializeComponent();
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            EmailSettings myES = new EmailSettings();
            myES.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CreateAccount myCA = new CreateAccount();
            myCA.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteUser myDU = new DeleteUser();
            myDU.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ChangePassword myCP = new ChangePassword();
            myCP.ShowDialog();
        }
    }
}
