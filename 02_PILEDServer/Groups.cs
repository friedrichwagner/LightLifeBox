using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LightLife.Data;

namespace PILEDServer
{
    //public delegate void delegteUpdateDataTable(PILEDData p);

    public partial class frmGroups : Form
    {
        DataTable dt = new DataTable("groups");

        public frmGroups()
        {
            InitializeComponent();

            CreateDataTable();
        }

        private void CreateDataTable()
        {
            dt.Columns.Add("groupid", typeof(int));
            dt.Columns.Add("CCT", typeof(int));
            dt.Columns.Add("brightness", typeof(int));
            dt.Columns.Add("x", typeof(double));
            dt.Columns.Add("y", typeof(double));
            dt.Columns.Add("mode", typeof(PILEDMode));
            dt.Columns.Add("time", typeof(DateTime));
            //dt.Columns.Add("sender", typeof(string));
            //dt.Columns.Add("receiver", typeof(string));

            dt.Rows.Add(1, 0, 0, 0, 0, PILEDMode.SET_NONE, DateTime.Now);
            dt.Rows.Add(2, 0, 0, 0, 0, PILEDMode.SET_NONE, DateTime.Now);
            dt.Rows.Add(3, 0, 0, 0, 0, PILEDMode.SET_NONE, DateTime.Now);
            dt.Rows.Add(4, 0, 0, 0, 0, PILEDMode.SET_NONE, DateTime.Now);
            dt.Rows.Add(5, 0, 0, 0, 0, PILEDMode.SET_NONE, DateTime.Now);
            dt.Rows.Add(6, 0, 0, 0, 0, PILEDMode.SET_NONE, DateTime.Now);

            dgvGroups.DataSource = dt;
        }

        public void UpdateDataTable(PILEDData p)
        {
            if (InvokeRequired) { Invoke(new Action<PILEDData>(UpdateDataTable), new[] { p }); return; }

            int row = p.groupid - 1;

            dt.Rows[row].SetField<int>(1, p.cct);
            dt.Rows[row].SetField<int>(2, p.brightness);
            dt.Rows[row].SetField<double>(3, p.xy[0]);
            dt.Rows[row].SetField<double>(4, p.xy[1]);
            dt.Rows[row].SetField<PILEDMode>(5, p.mode);
            dt.Rows[row].SetField<DateTime>(5, DateTime.Now);
        }
    }
}
