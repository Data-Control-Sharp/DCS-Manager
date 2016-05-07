using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DCS_Converter;

namespace DCS_Manager
{
    /// <summary>
    /// This class contains all functions used to make new files.
    /// </summary>
    public class DCS_MAKE
    {
        
        public DataGridView dataGrid { get; set; }
        public string fileName { get; set; }
        //Currently not used -- only supports XML.
        public string fileType { get; set; }

        /// <summary>
        /// Class constructer.
        /// </summary>
        /// <param name="elementList">The list of elements to be output to file.</param>
        /// <param name="fileName">The output file name.</param>
        /// <param name="fileType">The output file type.</param>
        public DCS_MAKE(DataGridView dataGrid, string fileName, string fileType)
        {
            this.dataGrid = dataGrid;
            this.fileName = fileName;
            this.fileType = fileType;
        }

        /// <summary>
        /// Creates a new file based on object properties.
        /// </summary>
        public void create()
        {

            Int32 selectedCellCount = dataGrid.GetCellCount(DataGridViewElementStates.Selected);
            if (selectedCellCount > 0)
            {
                if (dataGrid.AreAllCellsSelected(true))
                {
                    MessageBox.Show("All cells are selected", "Selected Cells");
                }
                else
                {

                    //Create datatable to fill with selected cells.
                    string dtName = ((DataTable)dataGrid.DataSource).TableName;
                    DataTable dt = new DataTable(dtName);
                    DataSet ds = new DataSet();

                    //Loop through and find all selected columns.
                    for (int i = 0; i < selectedCellCount; i++)
                    {
                        if (!dt.Columns.Contains(dataGrid.SelectedCells[i].OwningColumn.Name))
                        {
                            dt.Columns.Add(dataGrid.SelectedCells[i].OwningColumn.Name, typeof(string));
                        }
                    }

                    //Stores the mappings from the old row index to the new row index.
                    Dictionary<int, int> rowMap = new Dictionary<int, int>();
                    //Loops through and adds the selected cells to the datatable.
                    for (int i = 0; i < selectedCellCount; i++)
                    {

                        //If row does not exist, add it.
                        if (!rowMap.ContainsKey(dataGrid.SelectedCells[i].RowIndex))
                        {
                            DataRow workRow = dt.NewRow();
                            workRow[dataGrid.SelectedCells[i].OwningColumn.Name] = dataGrid.SelectedCells[i].Value.ToString();
                            dt.Rows.Add(workRow);
                            rowMap.Add(dataGrid.SelectedCells[i].RowIndex, dt.Rows.Count-1);
                        }
                        //Append to existing row.
                        else
                        {
                            dt.Rows[rowMap[dataGrid.SelectedCells[i].RowIndex]][dataGrid.SelectedCells[i].OwningColumn.Name] = dataGrid.SelectedCells[i].Value.ToString();
                        }
                    }

                    //Generate file of specified type.
                    try
                    {
                        ds.Tables.Add(dt);
                        dynamic parsed = DCS_XML.parseXMLData(ds.GetXml());
                        //CSV
                        if(fileType.ToUpper() == "CSV")
                        {
                            if (DCS_CSV.outputCSV(parsed, fileName))
                            {
                                MessageBox.Show("Selected Cells successfully output to " + fileName);
                            }
                            else
                            {
                                MessageBox.Show("There was an error outputting to CSV.");
                            }
                        }
                        //XML
                        else if(fileType.ToUpper() == "XML")
                        {
                            if (DCS_XML.outputXML(parsed, fileName))
                            {
                                MessageBox.Show("Selected Cells successfully output to " + fileName);
                            }
                            else
                            {
                                MessageBox.Show("There was an error outputting to XML.");
                            }
                            
                        }
                        //JSON
                        else if(fileType.ToUpper() == "JSON")
                        {
                            if(DCS_JSON.outputJSON(parsed, fileName))
                            {
                                MessageBox.Show("Selected Cells successfully output to " + fileName);
                            }
                            else
                            {
                                MessageBox.Show("There was an error outputting to JSON.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Unsupported output file type.");
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Unable to output to XML file.\n" + ex);
                    }

                }
            }
        }

    }
}
