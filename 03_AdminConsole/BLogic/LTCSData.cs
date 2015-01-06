using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MvvmFoundation.Wpf;
using System.Windows.Input;
using System.ComponentModel;
using Lumitech;
using Lumitech.Helpers;

namespace Lumitech
{
    public class DataItemReadout : ObservableObject
    {
        private int _Pos;
        public int Pos { get { return _Pos; } set { _Pos = value; RaisePropertyChanged("Pos"); } }

        private string _ModuleNr;
        public string ModuleNr { get { return _ModuleNr; } set { _ModuleNr = value; RaisePropertyChanged("ModuleNr"); } }

        private string _SerialNr;
        public string SerialNr { get { return _SerialNr; } set { _SerialNr = value; RaisePropertyChanged("SerialNr"); } }

        private Single _mA;
        public Single mA { get { return _mA; } set { _mA = value; RaisePropertyChanged("mA"); } }

        private string _KZ;
        public string KZ { get { return _KZ; } 
            set { _KZ = value; RaisePropertyChanged("KZ"); } }

        private bool? _OK;
        public bool? OK { get { return _OK; } set { _OK = value; RaisePropertyChanged("OK"); } }

        private string _Comment;
        public string Comment { get { return _Comment; } set { _Comment = value; RaisePropertyChanged("Comment"); } }

        private DateTime _TestDate;
        public DateTime TestDate { get { return _TestDate; } set { _TestDate = value; RaisePropertyChanged("TestDate"); } }

        public void Empty()
        {
            Pos = -1;
            ModuleNr = String.Empty;
            SerialNr = String.Empty;
            mA = Single.NaN;
            KZ = String.Empty;
            OK = false;
            Comment = String.Empty;
            TestDate = DateTime.Now;
        }

    }

    class LTCSData
    {
          private const string sqlUpdateLTCS = " update tProductData set ProductionOrderNr='#Fauf', CustomerOrderNr='#OrderNr', labeldate=getdate()  where ModuleNr='#MYLTLFAUF' and SerialNr = '#MYLTLSN'";
        //private const string sqlCheckUpdateLTCS = " select 1 from tProductData where ModuleNr='#MYLTLFAUF' and SerialNr = '#MYLTLSN'";
        private const string sqlUpdateLTCS2 = " update tProductData set mA='#mA', OK='#ok', KZ='#KZ', Comment='#comment', testdate = getdate()  where ModuleNr='#MYLTLFAUF' and SerialNr = '#MYLTLSN'";
        //private const string sqlSampleOrderShow = " select TOP 100 mID, Name, shortdescription, longdescription, LTE, LTEF, LTL from tSampleOrder order by ID desc";
        //private const string sqlInsertSampleOrder = "insert into tSampleOrder(mID, Name, shortdescription, longdescription, LTE, LTEF, LTL, UserId, LabelNr) values(:1, :2, :3, :4, :5, :6, :7, :8, :9)";
        //private const string sqlMaxSampleOrderID = "select isNull(MAX(mid) +1,1) as nextid from tSampleOrder  " +
        //                                            " where created > Cast(YEAR(getdate()) as varchar(4)) + Cast('-01-' as varchar(4)) + '01'";                                    
        //private const string sqlSelectProductData = "select * from tProductData where  ProductionOrdernr='#Fauf' and CustomerOrderNr='#OrderNr' order by ModuleNr, SerialNr";
        private const string sqlSelectProductData = "select #ANZAHL * from tProductData where  ProductionOrdernr='#Fauf' and CustomerOrderNr='#OrderNr' #OrderBy";
        private const string sqlTotalProductData = "select isNull(count(*),0) as ANZ from tProductData where  ProductionOrdernr='#Fauf' and CustomerOrderNr='#OrderNr'";

        private const string sqlUpdateFlashCount = "update tFlashCount  Set cnt = cnt + 1 where ProductionOrdernr = '#Fauf'" +
                             " if @@ROWCOUNT = 0" +
                             " insert into TFlashCount(ProductionOrdernr, cnt) values ('#Fauf',1)";
        private const string sqlFlashedQty = "select IsNull(cnt,0) as CNT from tFlashCount where ProductionOrderNr='#Fauf'";

        private const string sqlExportData = "Select ROW_NUMBER() OVER(ORDER BY testdate asc) AS Row, ModuleNr, SerialNr, IsNull(mA,0) as mA, IsNull(OK,0) as OK, isNull(KZ,'') as KZ, "
                                           +" IsNull(testdate,'') as TestDate, isNull(Comment,'')  as Comment from tProductData where ProductionOrderNr=:1 and CustomerOrderNr=:2 order by 1";

        private LTSQLCommand cmd;
        public DataItemReadout data; 

        public LTCSData()
        {
            Settings ini = Settings.GetInstance();
            data = new DataItemReadout();
            data.Empty();

            SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();
            sqlsb.DataSource = ini.ReadString("LTCS-Database", "DataSource", "");
            sqlsb.InitialCatalog = ini.ReadString("LTCS-Database", "InitialCatalog", "");
            //sqlsb.UserID = SAPUserId;
            //sqlsb.Password = SAPUserPWD;
            sqlsb.UserID = ini.ReadString("LTCS-Database", "UserID", "");
            sqlsb.Password = ini.ReadString("LTCS-Database", "Password", "");

            cmd = new LTSQLCommand(new SqlConnection(sqlsb.ConnectionString));
            if (sqlsb.DataSource.Length>0)
                cmd.Connection.Open();
        }

        public int getFlashedQty(string fauf)
        {
            int ret =0;            
            
            StringBuilder stmt = new StringBuilder(sqlFlashedQty);
            stmt.Replace("#Fauf", fauf);
            cmd.prep(stmt.ToString());
            cmd.Exec();

            if (cmd.dr.Read())
                ret = cmd.dr.GetInt32(0);     

            return ret;
        }

        public int updFlashedQty(string fauf)
        {
            StringBuilder stmt = new StringBuilder(sqlUpdateFlashCount);
            stmt.Replace("#Fauf", fauf);
            cmd.prep(stmt.ToString());
            cmd.Exec();

            return getFlashedQty(fauf);
        }

        public SqlDataReader getDataAlreadyCollected(string fauf, string OrderNr, int cntRows)
        {
            StringBuilder stmt = new StringBuilder(sqlSelectProductData);

            if (cntRows > 0)
            {
                //Für Anzeige in Form
                stmt.Replace("#ANZAHL", "TOP " + cntRows.ToString());
                stmt.Replace("#OrderBy", " order by testDate desc");
            }
            else
            {
                //Für Excel Export
                stmt.Replace("#ANZAHL", "");
                stmt.Replace("#OrderBy", " order by testDate asc");
            }

            stmt.Replace("#Fauf", fauf);
            stmt.Replace("#OrderNr", OrderNr);
            cmd.prep(stmt.ToString());
            cmd.Exec();

            return cmd.dr;
        }

        public SqlDataReader getDatatoExport(string fauf, string OrderNr)
        {
            StringBuilder stmt = new StringBuilder(sqlExportData);

            cmd.prep(stmt.ToString());
            cmd.Params[0] = fauf;
            cmd.Params[1] = OrderNr;
            cmd.Exec();
            return cmd.dr;
        }


        public int getTotalData(string fauf, string OrderNr)
        {
            int ret = 0;

            StringBuilder stmt = new StringBuilder(sqlTotalProductData);
            stmt.Replace("#Fauf", fauf);
            stmt.Replace("#OrderNr", OrderNr);
            cmd.prep(stmt.ToString());
            cmd.Exec();

            if (cmd.dr.Read())
                ret = Int32.Parse(cmd.dr["ANZ"].ToString());    

            return ret;            
        }

        public bool updAfterTest()
        {
            StringBuilder stmt = new StringBuilder(sqlUpdateLTCS2);
            stmt.Replace("#mA", data.mA.ToString());
            stmt.Replace("#ok", (data.OK.Equals("ok") ? "1" : "0"));
            stmt.Replace("#KZ", data.KZ);
            stmt.Replace("#comment", data.Comment);
            stmt.Replace("#MYLTLFAUF", data.ModuleNr);
            stmt.Replace("#MYLTLSN", data.SerialNr);
            cmd.prep(stmt.ToString());
            return (cmd.Exec() > 0);
        }

        public bool updAfterPrint(string fauf, string kauf, DataItemReadout d)
        {
            StringBuilder stmt = new StringBuilder(sqlUpdateLTCS);
            stmt.Replace("#Fauf", fauf);
            stmt.Replace("#OrderNr", kauf);
            stmt.Replace("#MYLTLFAUF", d.ModuleNr);
            stmt.Replace("#MYLTLSN", d.SerialNr);
            cmd.prep(stmt.ToString());
            cmd.Exec();

            return (cmd.Exec() > 0);
        }
    }
}
