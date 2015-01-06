using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Input;
using System.Reflection;
using FirstFloor.ModernUI.Windows;
using System.Windows;
using Lumitech;
using System.Data.SqlClient;
using System.IO;
using System.Data;


namespace Lumitech
{
    /// <summary>
    /// Class for generator of Excel file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class ExportToExcel
    {
        // Excel object references.
        private Excel.Application _excelApp = null;
        private Excel.Workbooks _books = null;
        private Excel._Workbook _book = null;
        private Excel.Sheets _sheets = null;
        private Excel._Worksheet _sheet = null;
        private Excel.Range _range = null;
        private Excel.Font _font = null;

        private Excel.Range _findrange = null;
        // Optional argument variable
        private object _optionalValue = Missing.Value;

        /// <summary>
        /// Generate report and sub functions
        /// </summary>
        public void GenerateReport<T>(string TemplatePath, Dictionary<string, string> HeaderData, T drPosData)
        {
            try
            {
                if (!File.Exists(TemplatePath)) throw new ArgumentException("Template does not exist!");
                if (HeaderData == null) throw new ArgumentNullException("No Header data");
                if (drPosData == null)  throw new ArgumentNullException("No Position data");

                Mouse.SetCursor(Cursors.Wait);
                CreateExcelRef(TemplatePath);
                FillSheet(HeaderData, drPosData);
                OpenReport();
                Mouse.SetCursor(Cursors.Arrow);
            }
            catch (Exception ex)
            {
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
            finally
            {
                ReleaseObject(_sheet);
                ReleaseObject(_sheets);
                ReleaseObject(_book);
                ReleaseObject(_books);
                ReleaseObject(_excelApp);
            }
        }

        /// <summary>
        /// Make Microsoft Excel application visible
        /// </summary>
        private void OpenReport()
        {
            _excelApp.Visible = true;
        }

        /// <summary>
        /// Populate the Excel sheet
        /// </summary>
        private void FillSheet<T>(Dictionary<string, string> HeaderData, T drPosData)
        {
            CreateHeader(HeaderData);
            WritePosData(drPosData);
        }

        /// <summary>
        /// Write data into the Excel sheet
        /// </summary>
        /// <param name="header"></param>
        private void WritePosData<T>(T drPosData)
        {
            _findrange = _sheet.Cells.Find("#startdata");
            int cntRows=0;
            int VisibleRowCnt = 0;
            bool bData = false;

            if (typeof(T) == typeof(SqlDataReader))
            {
                VisibleRowCnt = (drPosData as SqlDataReader).VisibleFieldCount;
                bData = (drPosData as SqlDataReader).Read();
            }
            else if (typeof(T) == typeof(DataTable))
            {
                VisibleRowCnt = (drPosData as DataTable).Columns.Count;
                bData = (drPosData as DataTable).Rows.Count > 0;
            }
            else throw new ArgumentException("Wrong Export data format!");

            if (_findrange != null)
            {

                while (bData)
                {
                    for (int i = 0; i < VisibleRowCnt; i++)
                    {
                        if (typeof(T) == typeof(SqlDataReader))
                            _findrange.Offset[cntRows, i].Value = (drPosData as SqlDataReader).GetValue(i);
                        else if (typeof(T) == typeof(DataTable))
                            _findrange.Offset[cntRows, i].Value = (drPosData as DataTable).Rows[cntRows][i];
                    }

                    if (cntRows % 2 == 0)
                    {
                        _findrange.Offset[cntRows, 0].EntireRow.Interior.Pattern = Excel.XlPattern.xlPatternSolid;
                        _findrange.Offset[cntRows, 0].EntireRow.Interior.Color = 13882323;
                    }
                    else
                        _findrange.Offset[cntRows, 0].EntireRow.Interior.Pattern = Excel.XlPattern.xlPatternNone;

                    cntRows++;

                    if (typeof(T) == typeof(SqlDataReader))                  
                        bData = (drPosData as SqlDataReader).Read();
                    else if (typeof(T) == typeof(DataTable))
                        bData = (drPosData as DataTable).Rows.Count > cntRows;                    
                }
            }
            AutoFitColumns("A1", cntRows+10, VisibleRowCnt + 1);
            _sheet.PageSetup.PrintArea = "A1:" + _findrange.Offset[cntRows, VisibleRowCnt-1].get_Address();
        }

        /// <summary>
        /// Method to make columns auto fit according to data
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        private void AutoFitColumns(string startRange, int rowCount, int colCount)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.Columns.AutoFit();
        }
        /// <summary>
        /// Create header from the properties
        /// </summary>
        /// <returns></returns>
        private void CreateHeader(Dictionary<string, string> HeaderData)
        {
            foreach (var pair in HeaderData)
            {
                _findrange = _sheet.Cells.Find("#"+pair.Key);

                if (_findrange != null)
                {
                    _findrange.Value = pair.Value;
                }
            }
        }

        /// <summary>
        /// Set Header style as bold
        /// </summary>
        private void SetHeaderStyle()
        {
            _font = _range.Font;
            _font.Bold = true;
        }

        /// <summary>
        /// Method to add an excel rows
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        /// <param name="values"></param>
        private void AddExcelRows (string startRange, int rowCount, int colCount, object values)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.set_Value(_optionalValue, values);
        }

        /// <summary>
        /// Create Excel application parameters instances
        /// </summary>
        private void CreateExcelRef(string TemplatePath)
        {
            _excelApp = new Excel.Application();                                            //ReadOnly=true to force SaveAs... (not to overwrite Template)
            //_books = (Excel.Workbooks)_excelApp.Workbooks.Open(TemplatePath);
            //_book = (Excel._Workbook)(_books.Add(_optionalValue));
            _book = (Excel.Workbook)_excelApp.Workbooks.Open(TemplatePath, Type.Missing, true);
            _sheets = (Excel.Sheets)_book.Worksheets;
            _sheet = (Excel._Worksheet)(_sheets.get_Item(1));
        }
        /// <summary>
        /// Release unused COM objects
        /// </summary>
        /// <param name="obj"></param>
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception)
            {
                obj = null;
                //MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
