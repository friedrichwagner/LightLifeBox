//====================================================================
// Copyright (C) 2006 Bernad Pakpahan. All rights reserved.
// Email me at bern4d@gmail.com
//====================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Lumitech
{
    public class XMLData
    {
        
        private DataSet ds = new DataSet();
        private DataView dv = new DataView();
        private string TableName;
        public bool IsReadonly { get; set; }

        public XMLData(string pTablename, bool bReadOnly) 
        {
            TableName = pTablename;
            IsReadonly = bReadOnly;

            SelectAll();
        }

        public void save()
        {
            if (IsReadonly) throw new ArgumentException("Dataset marked as readonly");

            ds.WriteXml(AppDomain.CurrentDomain.BaseDirectory + "\\Data\\" + TableName + ".xml", XmlWriteMode.WriteSchema);
        }

        public void Insert(string[] arrParams)
        {

            if (IsReadonly) throw new ArgumentException("Dataset marked as readonly");

            DataRow dr = dv.Table.NewRow();
            for (int i = 0; i < arrParams.GetLength(0); i++)
                dr[i] = arrParams[i];
            
            dv.Table.Rows.Add(dr);
            save();
        }

        public void Update(string filterExpression, string[] arrParams)
        {
            if (IsReadonly) throw new ArgumentException("Dataset marked as readonly");

            /*DataRow dr = Select(filterExpression,"");
            for (int i = 0; i < arrParams.GetLength(0); i++)
                dr[i] = arrParams[i];
           
            save();*/
        }

        public void Delete(string filterExpression)
        {
            if (IsReadonly) throw new ArgumentException("Dataset marked as readonly");

            //dv.RowFilter = "categoryID='" + categoryID + "'";
            dv.RowFilter = filterExpression;
            //dv.Sort = "categoryID";
            dv.Delete(0);
            dv.RowFilter = "";
            save();
        }

        public DataView Select(string filterExpression, string sortColumn)
        {
            //dv.RowFilter = "categoryID='" + categoryID + "'";
            dv.RowFilter = filterExpression;

            if (sortColumn.Length >0)
                dv.Sort = sortColumn;

            return dv;
        }

        public DataView SelectAll()
        {
            ds.Clear();
            ds.ReadXml(AppDomain.CurrentDomain.BaseDirectory + "\\Data\\" + TableName + ".xml", XmlReadMode.ReadSchema);
            dv = ds.Tables[0].DefaultView;
            return dv;
        }
    }
}