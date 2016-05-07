using System;
using System.Collections.Generic;

namespace DCS_Manager
{
    /// <summary>
    /// This class performs all operations involving removing data from the database.
    /// </summary>
    public class DCS_REMOVE
    {

        public string connectionString { get; }
        public List<string> contentList { get; set; }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="connectionString">The DB connection string.</param>
        /// <param name="contentList">The list of data to be removed from the DB.</param>
        public DCS_REMOVE(string connectionString, List<string> contentList)
        {
            this.connectionString = connectionString;
            this.contentList = contentList;
        }

        /// <summary>
        /// This function removes content from the DB based on object properties.
        /// </summary>
        public void strip()
        {

        }
    }
}
