using DCS_Manager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace DCS_Manager
{
    /// <summary>
    /// This class performs all searching operations.
    /// </summary>
    public class DCS_SEARCH
    {

        //Global Variables
        private int current = 0;
        private String SQLString = null;
        private String prevSQL = null;
        private String prevField = null;
        private List<String> box = new List<String>();

        public string connectionString { get; }
        public string query { get; set; }
        public Form1 form { get; set; }
        public List<string> results;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="form">The calling form.</param>
        /// <param name="connectionString">The DB connection string.</param>
        /// <param name="query">The query string.</param>
        public DCS_SEARCH(Form1 form, string connectionString, string query)
        {
            this.form = form;
            this.connectionString = connectionString;
            this.query = query;
        }

        /// <summary>
        /// Used in sortByDistance method.
        /// </summary>
        class distanceObject
        {
            public string value { get; set; }
            public int distance { get; set; }
        }

        /// <summary>
        /// Executes search based on user input when search button is pressed.
        /// </summary>
        public void searchButton()
        {

            //Initialization.
            string suggestion = null;
            current = 0;
            SQLString = null;
            prevSQL = "SELECT * FROM " + form.currentTable;
            prevField = form.searchBox1.Text;
            box.Clear();

            //Adds user input to a list for later use.
            box.Add(form.searchBox1.Text);

            //Search field must be filled.
            if (form.searchBox1.Text != "")
            {

                //Resets error for first field
                //form.errorProvider1.Clear();

                //Recursively finds search results via SQL
                List<String> printList = findList();
                form.maxProgressBar();

                //If not results are found.
                if (printList == null || printList.Count() == 0)
                {
                    MessageBox.Show("No Results Found. Try limiting your query fields.");

                }
                else
                {
                    //If list count is greater than 1, run suggestion method.
                    if (printList.Count() > 1)
                    {
                        suggestion = suggestionField();
                    }

                    //Makes a suggestion to limit results.
                    if (suggestion != null)
                    {
                        MessageBox.Show("If you wish to improve results, enter data in the " + suggestion + " field.");
                    }

                }
            }
            else
            {
                //form.errorProvider1.SetError(form.searchBox1, "Do not leave blank.");
            }
        }

        /// <summary>
        /// Searches for a specific query.
        /// Builds upon itself with each new query.
        /// Very similar to a rule-based system.
        /// </summary>
        /// <returns>A list of objects found given the query SQLString.</returns>
        private List<String> findList()
        {

            String temp = null;
            String temp2 = null;
            
            //SQL String Query
            
            SQLString = String.Format("SELECT * FROM " + form.currentTable + " WHERE CAST({0} AS char(20))=\'{1}\'", form.columns[0], form.searchBox1.Text); //Two params, the column and value within column.
            for (int i = 0; i < form.columns.Count; i++)
            {
                if (i > 0) {
                    
                    SQLString += String.Format(" OR CAST({0} AS char(20))=\'{1}\'", form.columns[i], form.searchBox1.Text);
                }
            }

            List<String> currentList = new List<String>();
            List<String> finalList = new List<String>();

            //Opens SQL connection to database for use.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                SqlCommand command = new SqlCommand(SQLString, connection);
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                {
                    form.fillTable(dataAdapter);
                }
                command.CommandType = CommandType.Text; //Change to char
                command.Connection = connection;
                connection.Open();
                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    //Places elements from SQL query into lists.
                    while (reader.Read())
                    {
                        temp = reader[current].ToString();
                        temp2 = reader[form.columns.Count - 1].ToString() ;
                        currentList.Add(temp);
                        finalList.Add(temp2);
                        form.incrementProgressBar();
                    }

                    //Check if there were no results.
                    if (finalList.Count() == 0)
                    {
                            String resultString = didYouMean(prevSQL, prevField);
                            if (resultString != null)
                            {
                                return findList();
                            }
                            
                        return null;
                    }

                    return finalList;
                }

                //If a mistake is made.
                catch(Exception e)
                {
                    MessageBox.Show("Find List Failure: " + e);
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }

        }

        /// <summary>
        /// Returns a suggested field for use in the DYM method.
        /// </summary>
        /// <returns>Returns a suggestion string for the user to input.</returns>
        public string suggestionField()
        {

            //Loops until empty field is found.
            //Since fields are ordered by importance, first empty field limits the search the most.
            for (int i = 0; i < box.Count; i++)
            {
                if (box[i] == "")
                {
                    return form.columns[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Makes a random suggestion based on last SQL query if no results are found.
        /// </summary>
        /// <param name="tempSQL">The SQL command last used.</param>
        /// <param name="tempField">The field last used.</param>
        /// <returns>Returns the random suggestion or null if user does not wish to continue.</returns>
        public string didYouMean(String tempSQL, String tempField)
        {

            //Initialized Variables.
            string pattern = Char.ToString(tempField[0]);
            List<String> suggestList = new List<String>();
            string temp = null;
            //string temp2 = null;

            //Opens SQL connection to the database.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(tempSQL, connection);
                command.CommandType = CommandType.Text; //Change to char
                command.Connection = connection;
                connection.Open();
                try
                {
                    //Adds previous recursion data to a couple of lists.
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 0; i < form.columns.Count; i++) { 
                            temp = reader[i].ToString();
                            suggestList.Add(temp);
                        }
                        //Progesses progress bar.
                        form.incrementProgressBar();
                    }

                    //Removes duplicate values.
                    List<String> distinctStringList = suggestList.Distinct().ToList();
                    List<String> sortedStringList = sortByDistance(distinctStringList, tempField);
                    int forCounter = 0;

                    //Asks if the user meant something else.
                    foreach (string s in sortedStringList)
                    {
                        DialogResult dialogResult = MessageBox.Show("Did you mean " + s +
                            "?", "Suggestion " + (forCounter + 1).ToString(), MessageBoxButtons.YesNoCancel);

                        //Yes selected
                        //Change text box value.

                        if (dialogResult == DialogResult.Yes)
                        {
                            form.searchBox1.Text = s;
                            return (s);
                        }
                        //No selected
                        else if (dialogResult == DialogResult.No)
                        {
                            //Continue Loop
                        }
                        //Cancel selected.
                        else
                            break;
                        forCounter++;

                        //If three suggestions have been declined.
                        if (forCounter == 3)
                        {
                            return null;
                        }

                    }
                    return null;
                }
                //Failure.
                catch(Exception e)
                {
                    MessageBox.Show("DYM Failure: " + e);
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Sorts a list of strings based on the DamerauLevenshtein algorithm.
        /// </summary>
        /// <param name="unsortedList">The unsorted list that will be sorted and returned.</param>
        /// <param name="input">The string to compare to for sorting.</param>
        /// <returns>A list sorted by DamerauLevenshtein algorithm.</returns>
        public List<String> sortByDistance(List<String> unsortedList, string input)
        {

            //Initialized Variables
            List<distanceObject> localList = new List<distanceObject>();
            List<String> finalList = new List<String>();

            //Adds unsorted list to object.
            foreach (string s in unsortedList)
            {
                localList.Add(new distanceObject { value = s, distance = DamerauLevenshtein.DamerauLevenshteinDistanceTo(s, input) });
            }

            //Sorts by distance.
            System.Collections.Generic.IEnumerable<distanceObject> query = localList.OrderBy(x => x.distance);

            //Returns object to a final string list.
            foreach (distanceObject x in query)
            {
                finalList.Add(x.value);
            }

            return finalList;
        }

    }
}