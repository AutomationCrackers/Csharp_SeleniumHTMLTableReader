using OpenQA.Selenium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Csharp_HTMLTableReader
{
    public class HTMLTableUtility
    {

        private static List<TableDatacollection> tableDataCollection;

        public static List<TableDatacollection> ReadTable(IWebElement table)
        {
            //Initialize the Table
            tableDataCollection = new List<TableDatacollection>();

            //Get all the Columns(th Tags) from the Table or DOM
            var columns = table.FindElements(By.TagName("th"));

            //Get all the Rows (th Tags) from the Table or DOM
            var rows = table.FindElements(By.TagName("tr"));

            //Initialize Row Index
            int rowIndex = 0;
            foreach (var row in rows)
            {
                //initialize column Index
                int columnIndex = 0;
                var columnDatas = row.FindElements(By.TagName("td"));

                //store Data only if it has value in a row
                if (columnDatas.Count != 0)
                {
                    foreach (var columnValue in columnDatas)
                    {
                        tableDataCollection.Add(new TableDatacollection
                        {
                            RowNumber = rowIndex,
                            ColumnName = columns[columnIndex].Text != "" ?
                                       columns[columnIndex].Text : columnIndex.ToString(),
                            ColumnValue = columnValue.Text,
                            ColumnSpecialValues = GetControl(columnValue)
                        });

                        //Move to Next Column
                        columnIndex++;

                    }
                    rowIndex++;
                }
            }
            return tableDataCollection;
        }

        //GetControl- Method
        private static ColumnSpecialValue GetControl(IWebElement columnValue)
        {
            ColumnSpecialValue columnSpecialValue = null;
            //Check if the control has specific tags like <input> or hyerlink<a> etc
            if(columnValue.FindElements(By.TagName("a")).Count > 0)
            {
                columnSpecialValue = new ColumnSpecialValue
                {
                    ElementCollection = columnValue.FindElements(By.TagName("a")),
                    ControlType = "hyperLink"
                };
            }
            if (columnValue.FindElements(By.TagName("input")).Count > 0)
            {
                columnSpecialValue = new ColumnSpecialValue
                {
                    ElementCollection = columnValue.FindElements(By.TagName("input")),
                    ControlType = "input"
                };
            }
            return columnSpecialValue;
        }

        //Perform Action inside the HTMLTable Cell
        public static void PerformActionOnCell(string columnIndex, string refColumnName, string refColumnValue, string controlToOperate=null)
        {
            foreach(int rowNumber in GetDynamicRowNumber(refColumnName, refColumnValue))
            {
                var cell = (from e in tableDataCollection
                            where e.ColumnName == columnIndex && e.RowNumber == rowNumber
                            select e.ColumnSpecialValues).SingleOrDefault();

                //Need to Operate on those Controls
                if(controlToOperate !=null && cell!=null)
                {
                    //since Based on the Control type, the retrieving of text changes
                    //created this kind of control
                    if(cell.ControlType=="hyperlink")
                    {
                        var returnedControl = (from c in cell.ElementCollection
                                               where c.Text == controlToOperate
                                               select c).SingleOrDefault();
                        //Todo : As of now only Click is supported
                        returnedControl?.Click();

                    }
                    else
                    {
                        cell.ElementCollection?.First().Click();
                    }
                }
            }
        }

        public static IEnumerable GetDynamicRowNumber(string columnName, string columnValue)
        {
            //Dynamic Row
            foreach(var table in tableDataCollection)
            {
                if(table.ColumnName==columnName && table.ColumnValue==columnValue)
                {
                    yield return table.RowNumber;
                }
            }
        }
    }

    public class TableDatacollection
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public ColumnSpecialValue ColumnSpecialValues { get; set; }
    }

    public class ColumnSpecialValue
    {
        public IEnumerable<IWebElement> ElementCollection { get; set; }
        public string ControlType { get; set; }
    }


}
