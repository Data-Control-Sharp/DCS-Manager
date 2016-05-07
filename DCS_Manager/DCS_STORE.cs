using System;
using System.Data;
using System.Data.SqlClient;
using DCS_Converter;

namespace DCS_Manager
{
    /// <summary>
    /// This class performs all operations related to storing information in the DB.
    /// </summary>
    public class DCS_STORE
    {
        public string connectionString { get; }
        public string file { get; set; }
        public dynamic content { get; set; }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="connectionString">The DB connection string.</param>
        public DCS_STORE(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Parses a CSV, XML, or JSON file using DC# Converter.
        /// </summary>
        /// <param name="file">The file path.</param>
        /// <returns>The parsed data.</returns>
        public dynamic parseFile(string file)
        {
            this.content = DCS_Converter.DCS_ALL.parseFile(file);
            this.file = file;
            if (this.content != null)
            {
                return content;
            }
            else
            {
                this.content = null;
                this.file = null;
                Console.WriteLine("Parsing file unsuccessful.");
                return null;
            }
        }

        /// <summary>
        /// Stores the data into the DB based on the local content.
        /// </summary>
        /// <returns>True if the data was stored in the db, false otherwise.</returns>
        public bool storeData()
        {
            if (this.content != null)
            {
                //Parse local content into a merged datatable.
                DataTable dt = DCS_ALL.objToDataTable(this.content);

                //Create Table SQL with CreateTABLE
                string[] splitFile = file.Split('.');
                splitFile = splitFile[splitFile.Length-2].Split('\\');
                string tableName = splitFile[splitFile.Length - 1].ToUpper();
                string createTable = CreateTABLE(tableName, dt);
                //Console.WriteLine("Table Query: " + createTable);
                //Console.WriteLine("File name: " + tableName);

                //Execute command
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //Create table schema
                    using (SqlCommand command = new SqlCommand(createTable, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    //Copy data
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
                    {
                        foreach (DataColumn col in dt.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        }
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(dt);
                    }
                    return true;
                }

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stores the data into the DB based on the argument.
        /// </summary>
        /// <param name="content">The content to be stored in the DB.</param>
        /// <returns>True if the data was stored in the db, false otherwise.</returns>
        public bool storeData(dynamic content)
        {
            if (content != null)
            {
                //Parse content into a merged datatable.
                DataTable dt = DCS_ALL.objToDataTable(content);

                //Create Table SQL with CreateTABLE
                string[] splitFile = file.Split('.');
                splitFile = splitFile[splitFile.Length-2].Split('\\');
                string tableName = splitFile[splitFile.Length - 1].ToUpper();
                string createTable = CreateTABLE(tableName, dt);
                //Console.WriteLine("Table Query: " + createTable);
                //Console.WriteLine("File name: " + tableName);

                //Execute command
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //Create table schema
                    using (SqlCommand command = new SqlCommand(createTable, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    //Copy data
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
                    {
                        foreach (DataColumn col in dt.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        }
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(dt);
                    }
                    return true;
                }

            }
            else
            {
                return false;
            }
        }

        private static string CreateTABLE(string tableName, DataTable table)
        {
            string sqlsc;
            sqlsc = "CREATE TABLE " + tableName + "(";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " tinyint ";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
            return sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
        }
    }
}
