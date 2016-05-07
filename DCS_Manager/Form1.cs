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
using DCS_Converter;

namespace DCS_Manager
{
    /// <summary>
    /// This form is the main form. It contains the bulk of the functionality
    /// and spawns children forms.
    /// </summary>
    public partial class Form1 : Form
    {

        DCS_SEARCH mySearch;
        public List<string> columns { get; set; }
        public string currentTable { get; set; }
        private SqlConnection conn;
        private DataSet ds;
        private SqlDataAdapter da;
        private bool adminPerms = true;

        /// <summary>
        /// This constructor initializes the form.
        /// </summary>
        /// <param name="username">The username of the currently signed in user.</param>
        public Form1(string username)
        {
            InitializeComponent();
            label1.Text = username;
            columns = new List<string>();
            setTableSelector();
            currentTable = comboBox1.GetItemText(comboBox1.SelectedItem);
            //currentTable = "DEPARTMENT, DEPENDENT, DEPT_LOCATIONS, EMPLOYEE, PROJECT, WORKS_ON";
            initFillTable();

            //If user does not have create table perms.
            if (!hasTablePerms()) {
                adminPerms = false;
                dataGridView1.ReadOnly = true;
                accountLabel.Enabled = false;
                accountLabel.Hide();
                button3.Enabled = false;
                button3.Hide();
                button4.Enabled = false;
                button4.Hide();
                adminControlToolStripMenuItem.Visible = false;
                fileControlToolStripMenuItem.Visible = false;
                saveChangesToolStripMenuItem1.Visible = false;


            }

            //Assign Toolbar clicks.
            uploadFileToolStripMenuItem1.Click += new EventHandler(button3_Click);
            outputFileToolStripMenuItem1.Click += new EventHandler(button4_Click);
            saveChangesToolStripMenuItem1.Click += new EventHandler(button2_Click);



            //Create search object
            mySearch = new DCS_SEARCH(this, GlobalConnectionString.ConnectionString, "");

            //Sets progress bar max.
            string stmt = "SELECT count(*) FROM " + currentTable;
            int count = 0;

            //Obtains number of rows in SQL table.
            using (SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.ConnectionString))
            {
                using (SqlCommand cmdCount = new SqlCommand(stmt, thisConnection))
                {
                    thisConnection.Open();
                    count = (int)cmdCount.ExecuteScalar();
                    thisConnection.Close();
                }
            }
            toolStripProgressBar1.Maximum = (int)(count * columns.Count * 0.5);
        }

        /// <summary>
        /// This function sets up the table selector combo box.
        /// </summary>
        public void setTableSelector()
        {
            //Sets progress bar max.
            string stmt = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME<>'sysdiagrams'";

            //Obtains number of rows in SQL table.
            using (SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(stmt, thisConnection))
                {
                    thisConnection.Open();
                    try
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        Dictionary<string, string> comboDict = new Dictionary<string, string>();
                        int whileCounter = 1;
                        //Places elements from SQL query into lists.
                        while (reader.Read())
                        {
                            comboDict.Add(whileCounter.ToString(), reader[2].ToString());
                            whileCounter++;
                        }
                        comboBox1.DataSource = new BindingSource(comboDict, null);
                        comboBox1.DisplayMember = "Value";
                        comboBox1.ValueMember = "Key";
                    }
                    catch
                    {
                        MessageBox.Show("Error: Could not query database tables.");
                    }
                    thisConnection.Close();
                }
            }
        }

        /// <summary>
        /// This function calculates the number of columns in the current table.
        /// </summary>
        public void getColumns()
        {
            string[] currentTableArray = currentTable.Split(',');
            foreach(string table in currentTableArray)
            {
                //Sets progress bar max.
                string stmt = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + table.Trim() + "'";

                //Obtains number of rows in SQL table.
                using (SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(stmt, thisConnection))
                    {
                        thisConnection.Open();
                        try
                        {
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                //Console.WriteLine(reader[3].ToString());
                                columns.Add(table.Trim() + "." + reader[3].ToString());
                            }

                        }
                        catch
                        {
                            MessageBox.Show("Error: Could not query database table: " + table);
                        }
                        thisConnection.Close();
                    }
                }
            }
                /*
                foreach (DataGridViewTextBoxColumn column in dataGridView1.Columns)
                {
                    columns.Add(column.Name);
                    //MessageBox.Show(column.Name);
                }  
                */
        }

        /// <summary>
        /// Increments the progress bar by 1.
        /// </summary>
        public void incrementProgressBar()
        {
            if (toolStripProgressBar1.Value < toolStripProgressBar1.Maximum)
            {
                toolStripProgressBar1.Value++;
            }
        }

        /// <summary>
        /// Maxes out the progress bar.
        /// </summary>
        public void maxProgressBar()
        {
            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
        }

        /// <summary>
        /// Clears out the progress bar.
        /// </summary>
        public void clearProgressBar()
        {
            toolStripProgressBar1.Value = 0;
        }

        /// <summary>
        /// Clears fields.
        /// </summary>
        public void clear()
        {
            searchBox1.Clear();
        }

        /// <summary>
        /// Checks if the current user as create table perms.
        /// </summary>
        /// <returns>True if user has create table perms, false otherwise.</returns>
        public static bool hasTablePerms()
        {
            string stmt = "SELECT HAS_PERMS_BY_NAME(db_name(), 'DATABASE', 'CREATE TABLE')";
            //Opens SQL connection to database for use.
            using (SqlConnection connection = new SqlConnection(GlobalConnectionString.ConnectionString))
            {

                using (SqlCommand command = new SqlCommand(stmt, connection))
                {

                    command.CommandType = CommandType.Text; //Change to char
                    command.Connection = connection;
                    connection.Open();
                    object result = command.ExecuteScalar();
                    connection.Close();
                    if (Convert.ToInt32(result) == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Fills the database table with the search results.
        /// </summary>
        /// <param name="dataAdapter">The search results as per an SQL query.</param>
        public void fillTable(SqlDataAdapter dataAdapter)
        {
            //Create data table
            DataTable t = new DataTable();
            //Reset Data Grid in GUI
            dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = true;
            //Set global dataset
            ds = new DataSet();
            //Add dataTable to global dataset to be used in update function.
            ds.Tables.Add(t);
            //Fill dataTable using the dataAdapter
            dataAdapter.Fill(t);
            //Set the source of the dataGrid as the dataTable.
            dataGridView1.DataSource = t;
            conn.Close();
            
        }

        /// <summary>
        /// This function should be called when the table selection changes.
        /// This function updates the progress bar max as well.
        /// </summary>
        public void initFillTable()
        {
            string stmt = "SELECT * FROM " + currentTable;
            string stmt2 = "SELECT count(*) FROM " + currentTable;
            //string stmt = "SELECT * FROM DEPARTMENT CROSS JOIN EMPLOYEE";
            //string stmt2 = "SELECT count(*) FROM DEPARTMENT CROSS JOIN EMPLOYEE";
            //string stmt = "SELECT * FROM DEPARTMENT FULL OUTER JOIN EMPLOYEE ON DEPARTMENT.Mgr_ssn=EMPLOYEE.Ssn";
            //string stmt2 = "SELECT count(*) FROM DEPARTMENT FULL OUTER JOIN EMPLOYEE ON DEPARTMENT.Mgr_ssn=EMPLOYEE.Ssn";
            int count = 0;

            //Set the selected table label.
            label2.Text = currentTable;

            //Opens SQL connection to database for use.
            using (SqlConnection connection = new SqlConnection(GlobalConnectionString.ConnectionString))
            {

                using (SqlCommand command = new SqlCommand(stmt, connection)) {

                    // Create global connection
                    conn = new SqlConnection();
                    //Open connection
                    conn.ConnectionString = GlobalConnectionString.ConnectionString;
                    conn.Open();
                    // Set global dataAdapter
                    da = new SqlDataAdapter("SELECT * FROM " + currentTable, conn);
                    // Fill in insert, update, and delete commands
                    SqlCommandBuilder cmdBldr = new SqlCommandBuilder(da);
                    fillTable(da);
                }
                using(SqlCommand command = new SqlCommand(stmt2, connection))
                {
                    //Obtains number of rows in SQL table.
                    connection.Open();
                    count = (int)command.ExecuteScalar();
                    connection.Close();
                }
            }

            //Obtain columns
            columns.Clear();
            getColumns();

            toolStripProgressBar1.Maximum = (int)(count * columns.Count * 0.5);
        }

        /// <summary>
        /// This function displays a file save browser to the user, allowing them to select a file. 
        /// </summary>
        /// <returns>File name of the selected file.</returns>
        public string saveBrowser()
        {
            string file = null;
            DialogResult result = saveFileDialog1.ShowDialog(); //Show the dialog.
            if (result == DialogResult.OK) //Test result.
            {
                file = saveFileDialog1.FileName;

            }
            Console.WriteLine(result); //Used for debugging purposes
            return file;
        }

        /// <summary>
        /// Actions that occur after the form has loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //Help Provider
            helpProvider1.SetHelpString(label1, "The currently signed in user.");
            helpProvider1.SetHelpString(label2, "The currently selected table(s)");
            helpProvider1.SetHelpString(linkLabel1, "Sign out of the current user.");
            helpProvider1.SetHelpString(accountLabel, "View admin portal.");
            helpProvider1.SetHelpString(dataGridView1, "Table views are displayed here. You can limit " +
                "the results with search queries.");
            helpProvider1.SetHelpString(comboBox1, "Select a table to appear in the data grid.");
            helpProvider1.SetHelpString(searchBox1, "Enter a query to limit the results in the data grid.");
            helpProvider1.SetHelpString(button1, "Execute the query.");
            helpProvider1.SetHelpString(button2, "Upload any changes to the data grid to the Azure database. " +
                "This requires admin permissions.");
            helpProvider1.SetHelpString(button3, "Upload a data file to the Azure database.");
            helpProvider1.SetHelpString(button4, "Download selected data table cells to an output file.");
            helpProvider1.SetHelpString(checkBox2, "Merges the next table you select with the currently selected one.");

            saveFileDialog1.Filter = "CSV File (.csv)|*.csv|JSON File (.json)|*.json|XML File (.xml)|*.xml";
            saveFileDialog1.FileName = "";
            openFileDialog1.FileName = "";

            this.AcceptButton = button1;

            //Create context menu used for dataGridView1
            ContextMenuStrip mnu = new ContextMenuStrip();
            ToolStripMenuItem mnuOut = new ToolStripMenuItem("Output to file...");
            ToolStripMenuItem mnuDel = new ToolStripMenuItem("Delete Table...");
            //Assign event handlers
            mnuOut.Click += new EventHandler(button4_Click);
            //Add to main context menu
            mnu.Items.AddRange(new ToolStripItem[] { mnuOut });
            //If user is an admin
            if (adminPerms)
            {
                mnuDel.Click += new EventHandler(deleteTable);
                mnu.Items.AddRange(new ToolStripItem[] { mnuDel });
            }

            //Assign to datagridview
            dataGridView1.ContextMenuStrip = mnu;

        }

        /// <summary>
        /// Action that occurs when the logout link is used.
        /// Entire application is restarted.
        /// TODO: Find safer way to perform a logout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Application.Restart();
        }

        /// <summary>
        /// Event executed when clicking on the "Create Account" link.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void accountLabel_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Create Form2 and display it.
            AdminControlPanel myCP = new AdminControlPanel();
            myCP.ShowDialog();
        }

        /// <summary>
        /// Action that occurs when combo box index changes.
        /// Current table is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //Resets progress bar.
            clearProgressBar();

            //Change Table
            if (!checkBox2.Checked)
            {
                currentTable = comboBox1.GetItemText(comboBox1.SelectedItem);
            }
            //Merge tables
            else
            {
                if (!currentTable.Contains(comboBox1.GetItemText(comboBox1.SelectedItem)))
                {
                    currentTable += ", " + comboBox1.GetItemText(comboBox1.SelectedItem);
                }
            }
            initFillTable();
        }

        /// <summary>
        /// If the search button is hit, this function is called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //Resets progress bar.
            clearProgressBar();

            //Reset Data Table
            if (searchBox1.Text == "")
            {
                initFillTable();
            }
            //Perform Search
            else {
                mySearch.searchButton();
            }
        }

        /// <summary>
        /// Action that occurs when the save changes button is hit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //Ensure there is only one table selected.
            if (currentTable.Split(',').Length < 2)
            {
                try
                {
                    da.Update(ds, ds.Tables[0].TableName);
                    MessageBox.Show("Changes successfully saved to the database.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Error: You cannot save changes when tables are joined.");
            }
        }

        /// <summary>
        /// This button launches a control window allowing user to upload file from 
        /// computer or URL.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Is the file on your computer?", "Where's the file?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string file = "";
                DialogResult result = openFileDialog1.ShowDialog();
                //If a file is selected, parse it to a dynamic object to be stored in the DB.
                if (result == DialogResult.OK)
                {
                    file = openFileDialog1.FileName;
                    //Parsed data
                    DCS_STORE myStore = new DCS_STORE(GlobalConnectionString.ConnectionString);
                    if (myStore.parseFile(file) != null)
                    {
                        if (myStore.storeData())
                        {
                            columns = new List<string>();
                            setTableSelector();
                            getColumns();
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
                openFileDialog1.FileName = "";
                this.Close();
            }
            else
            {
                DialogResult dialogResult2 = MessageBox.Show("Do you have the URL of the file?", "Where's the file?", MessageBoxButtons.YesNo);
                if (dialogResult2 == DialogResult.Yes)
                {
                    Upload myUP = new Upload(this);
                    myUP.ShowDialog();
                }

                else
                {
                    MessageBox.Show("If the file is not saved locally, and you do not have the link to the file, then you will not be able to upload the file.");
                }
            }
        }

        /// <summary>
        /// Selected cells button.
        /// Sends the selected cells to an output file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //Prompt user to save file.
            string saveFile = saveBrowser();
            if(saveFile == null)
            {
                return;
            }

            //Store selected cells
            DCS_MAKE myMake = new DCS_MAKE(dataGridView1, saveFile, saveFile.Split('.').Last());
            myMake.create();
        }

        /// <summary>
        /// Drop a table from the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteTable(object sender, EventArgs e)
        {
            if (currentTable.Split(',').Length == 1)
            {
               DialogResult result = MessageBox.Show("Are you sure you want to delete the " + currentTable + " table?", "Continue?", MessageBoxButtons.YesNoCancel);
                if(result == DialogResult.Yes)
                {
                    //Sets progress bar max.
                    string stmt = "DROP TABLE " + currentTable;

                    //Obtains number of rows in SQL table.
                    using (SqlConnection thisConnection = new SqlConnection(GlobalConnectionString.ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(stmt, thisConnection))
                        {
                            thisConnection.Open();
                            cmd.ExecuteScalar();
                            thisConnection.Close();
                        }
                    }
                    //Update columns
                    columns = new List<string>();
                    setTableSelector();
                    getColumns();
                }
            }
            else
            {
                MessageBox.Show("You may not have tables merged when deleting a table.");
            }

        }

        private void uploadFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
        }

        private void outputFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void adminControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AdminControlPanel myCP = new AdminControlPanel();
            myCP.ShowDialog();
        }

        private void fileControlToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveChangesToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void signOutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
    }
}