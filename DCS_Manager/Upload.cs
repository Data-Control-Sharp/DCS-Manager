using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using DCS_Converter;
using System.Net;


namespace DCS_Manager
{
    /// <summary>
    /// This class currently downloads a file from a given web address and saves it locally.
    /// </summary>
    public partial class Upload : Form
    {
        public List<string> columns { get; set; }
        public Form1 myForm { get; set; }

        public Upload(Form1 myForm)
        {
            this.myForm = myForm;
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Creates webclient to get the download.
            using (var client = new WebClient())
            {
                string fileLoc = textBox1.Text;
                string fileName = fileLoc.Split('/').Last();
                try
                {
                    client.DownloadFile(fileLoc, fileName);

                    //Parsed data
                    DCS_STORE myStore = new DCS_STORE(GlobalConnectionString.ConnectionString);
                    if (myStore.parseFile(fileName) != null)
                    {
                        //Delete file
                        File.Delete(fileName);

                        if (myStore.storeData())
                        {
                            columns = new List<string>();
                            myForm.setTableSelector();
                            myForm.getColumns();
                            MessageBox.Show("Data successfully uploaded!");
                        }
                        else
                        {
                            MessageBox.Show("Data was not uploaded successfully.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Data was not parsed successfully.");
                    }
                }
                catch (Exception k)
                {
                    MessageBox.Show("Unable to download file at given URL. More Info: "+k);
                }
            }
            this.Close();
        }
    }
}
