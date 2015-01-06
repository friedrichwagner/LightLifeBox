using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Lumitech;
using System.ComponentModel;
using System.Data;
using MvvmFoundation.Wpf;
using Lumitech.Helpers;

namespace Lumitech
{
    public class DataItemQSHead : ObservableObject
    {
        public int HeadID;
        public int LocationID {get; set;}
        public string FAUF {get; set;}
        public string ArticleNr {get; set;}
        public string Remark;
        public DateTime? ProductionDate {get; set;}
        public DateTime TimeStamp;
        public string UserID { get; set; }
        private int _cntTotal;
        public int cntTotal
        {
            get { return _cntTotal; }
            set { _cntTotal = value; RaisePropertyChanged("cntTotal"); RaisePropertyChanged("cntNotOK"); }
        }

        public int _cntOK;
        public int cntOK
        {
            get { return _cntOK; }
            set { _cntOK = value; RaisePropertyChanged("cntOK"); RaisePropertyChanged("cntNotOK"); }
        }

        public int cntNotOK
        {
            get { return _cntTotal - _cntOK; }
            private set {}
        }

        public void setEmpty()
        {
            HeadID=0;
            LocationID = 0;
            FAUF = string.Empty;
            Remark = string.Empty;
            UserID = string.Empty;
            ArticleNr = string.Empty;
            _cntTotal = 0;
            _cntOK = 0;
            ProductionDate = DateTime.Today;
            TimeStamp = DateTime.Now;
        }
    }

    public class DataItemQSPos : ObservableObject
    {
        public int HeadID { get; set; }
        public int PosID { get; set; }

        private Single _mAPhosphor;
        private Single _mABlue;
        private Single _mARed;

        public Single mAPhosphor { get { return _mAPhosphor; } set { _mAPhosphor = value; RaisePropertyChanged("mAPhosphor"); } }
        public Single mABlue { get { return _mABlue; } set { _mABlue = value; RaisePropertyChanged("mABlue"); } }
        public Single mARed { get { return _mARed; } set { _mARed = value; RaisePropertyChanged("mARed"); } }

        private bool? _PhosphorOK;
        private bool? _BlueOK;
        private bool? _RedOK;

        public bool? PhosphorOK { get { return _PhosphorOK; } set { _PhosphorOK = value; RaisePropertyChanged("PhosphorOK"); } }
        public bool? BlueOK { get { return _BlueOK; } set { _BlueOK = value; RaisePropertyChanged("BlueOK"); } }
        public bool? RedOK { get { return _RedOK; } set { _RedOK = value; RaisePropertyChanged("RedOK"); } }

        private Single _Temperature;
        private bool? _TSensorOK;
        private bool? _EepromOK;

        public Single Temperature { get { return _Temperature; } set { _Temperature = value; RaisePropertyChanged("Temperature"); } }
        public bool? TSensorOK { get { return _TSensorOK; } set { _TSensorOK = value; RaisePropertyChanged("TSensorOK"); } }

        public bool? EepromOK { get { return _EepromOK; } set { _EepromOK = value; RaisePropertyChanged("EepromOK"); } }
        public DateTime TimeStamp { get; set; }

        private string _Remark;
        public string Remark { get { return _Remark; } set { _Remark = value; RaisePropertyChanged("Remark"); } }

        public bool AllOK 
        {
            get { return ((bool)PhosphorOK && (bool)BlueOK && (bool)RedOK && (bool)TSensorOK && (bool)EepromOK); } 
        }

        public DataItemQSPos()
        {
        }

        public void Reset(bool? val=null)
        {
            mAPhosphor = float.NaN;
            mABlue = float.NaN;
            mARed = float.NaN;

            PhosphorOK = val;
            BlueOK = val;
            RedOK = val;

            Remark = String.Empty;

            TSensorOK = val;
            EepromOK = val;
            Temperature = Single.NaN;

            //AllOK = false;
        }
    }

    class LTQSData : ObservableObject
    {
        private LTSQLCommand cmd;
        private const string sqlGetNextHeadID="select isNull(max(headid),0)+1 from QCOrderHead";
        private const string sqlInsertHead = "insert into QCOrderHead(headid, locationid, fauf, articlenr, remark, productiondate, userid)" +
                                            " values(:1, :2, :3, :4, :5, :6,:7)";
        private const string sqlGetNextPosID = "select isNull(max(PosId),0)+1 from QCOrderPos where headid=:1";
        private const string sqlInsertPos = "insert into QCOrderPos(HeadID, PosID, mAPhosphor, PhosphorOK, mABlue, BlueOK, mARed, RedOK, Temperature, TSensorOK, EEPROMOK,  Remark) " +
                                            " values (:1, :2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12)";

        //private const string sqlGetPosbyHeadId = "select #ANZAHL PosID, mAPhosphor, PhosphorOK, mABlue, BlueOK, mARed, RedOK, Temperature, TSensorOK, EEPROMOK,  Remark from QCOrderPos where headid=:1 #OrderBy";
        private const string sqlGetPosbyHeadId = "select #ANZAHL * from QCOrderPos where headid=:1 #OrderBy";
        private const string sqlGetTotalTested = "select count(*) from QCOrderPos where headid=:1";

        private const string sqlGetArticle = "select * from Article ";
        private const string sqlGetLocation = "select * from Location ";
        private const string sqlGetOrderHead = "select * from QCOrderHead ";
        private const string sqlGetOrderPos = "select * from QCOrderPos ";

        private DataSet _ds;
        public DataSet ds { get { return _ds; } }
        private bool _OnlineData;
        public bool OnlineData { get { return _OnlineData; } }
        private const string XMLDataPath = "/Data/LEDTestData";

        public LTQSData(bool bOnline)
        {
            Settings ini = Settings.GetInstance();

            _OnlineData = bOnline;

            //use dataset also with online connection for lookup databinding of location and article
            _ds = new DataSet("LEDTEST");

            if (_OnlineData)
            {

                SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();
                sqlsb.DataSource = ini.ReadString("LTQC-Database", "DataSource", "");
                sqlsb.InitialCatalog = ini.ReadString("LTQC-Database", "InitialCatalog", "");
                //sqlsb.UserID = SAPUserId;
                //sqlsb.Password = SAPUserPWD;
                sqlsb.UserID = ini.ReadString("LTQC-Database", "UserID", "");
                sqlsb.Password = ini.ReadString("LTQC-Database", "Password", "");

                cmd = new LTSQLCommand(new SqlConnection(sqlsb.ConnectionString));
                cmd.Connection.Open();

                _ds.Tables.Add("Article");
                _ds.Tables.Add("Location");
                _ds.Tables[0].Load(getReader(sqlGetArticle));
                _ds.Tables[1].Load(getReader(sqlGetLocation));
            }
            else
            {               
                _ds.ReadXmlSchema(AppDomain.CurrentDomain.BaseDirectory + XMLDataPath+ ".xsd");
                _ds.ReadXml(AppDomain.CurrentDomain.BaseDirectory + XMLDataPath + ".xml");
            }
        }

        #region ONLINE_DATA

        private int SaveHeadOnline(DataItemQSHead h, DataItemQSPos p)
        {
            int id=0;

            StringBuilder stmt = new StringBuilder(sqlGetNextHeadID);
            cmd.prep(stmt.ToString());
            cmd.Exec();

            if (cmd.dr.Read())
            {
                 id = cmd.dr.GetInt32(0);
            }

            int i=0;
            stmt = new StringBuilder(sqlInsertHead);
            cmd.prep(stmt.ToString());
            cmd.Params[i++] = id;
            cmd.Params[i++] = h.LocationID;
            cmd.Params[i++] = h.FAUF;
            cmd.Params[i++] = h.ArticleNr;
            cmd.Params[i++] = h.Remark;
            cmd.Params[i++] = h.ProductionDate.ToString();
            cmd.Params[i++] = h.UserID;
            
            cmd.Exec();

            h.HeadID = id;

            return id;
        }

        private void SavePosOnline(DataItemQSHead h, DataItemQSPos p)
        {
            StringBuilder stmt = new StringBuilder(sqlGetNextPosID);
            cmd.prep(stmt.ToString());
            cmd.Params[0] = p.HeadID;            
            cmd.Exec();

            if (cmd.dr.Read())
            {
                p.PosID = cmd.dr.GetInt32(0);
            }

            int i = 0;
            stmt = new StringBuilder(sqlInsertPos);
            cmd.prep(stmt.ToString());

            cmd.Params[i++] = p.HeadID;
            cmd.Params[i++] = p.PosID;
            cmd.Params[i++] = p.mAPhosphor;
            cmd.Params[i++] = p.PhosphorOK;
            cmd.Params[i++] = p.mABlue;
            cmd.Params[i++] = p.BlueOK;
            cmd.Params[i++] = p.mARed;
            cmd.Params[i++] = p.RedOK;
            cmd.Params[i++] = p.Temperature;
            cmd.Params[i++] = p.TSensorOK;
            cmd.Params[i++] = p.EepromOK;
            cmd.Params[i++] = p.Remark;
            cmd.Exec();
        }

        public SqlDataReader getDataAlreadyCollected(int HeadId, int cntRows)
        {
            StringBuilder stmt = new StringBuilder(sqlGetPosbyHeadId);
            if (cntRows > 0)
            {
                //Für Anzeige in Form
                stmt.Replace("#ANZAHL", "TOP " + cntRows.ToString());
                stmt.Replace("#OrderBy", " order by TimeStamp desc");
            }
            else
            {
                //Für Excel Export
                stmt.Replace("#ANZAHL", "");
                stmt.Replace("#OrderBy", " order by TimeStamp asc");
            }
            cmd.prep(stmt.ToString());
            cmd.Params[0] = HeadId;            
            cmd.Exec();
            return cmd.dr;
        }

        public int getTotal(int HeadId)
        {
            StringBuilder stmt = new StringBuilder(sqlGetTotalTested);
            cmd.prep(stmt.ToString());
            cmd.Params[0] = HeadId;            
            cmd.Exec();
            return cmd.dr.GetInt32(0);
        }

        private SqlDataReader getReader(string sql)
        {
            StringBuilder stmt = new StringBuilder(sql);
            cmd.prep(stmt.ToString());
            cmd.Exec();
            return cmd.dr;
        }

        public SqlDataReader getOrderList()
        {
            StringBuilder stmt = new StringBuilder(sqlGetOrderHead);
            cmd.prep(stmt.ToString());
            cmd.Exec();
            return cmd.dr;
        }

        public bool getOrder(ref DataItemQSHead h, int locationid, string articlenr, string batchnr, string pd)
        {
            h.HeadID = 0;
            h.LocationID = 0;
            h.FAUF = string.Empty;
            h.ArticleNr = string.Empty;
            h.Remark = string.Empty;
            h.ProductionDate = DateTime.Today;
            h.UserID = string.Empty;
            h.cntTotal = 0;
            h.cntOK = 0;

            StringBuilder stmt = new StringBuilder(sqlGetOrderHead + " where LocationID=:1 and articleNr=:2 and fauf=:3 and ProductionDate=:4");
            cmd.prep(stmt.ToString());
            cmd.Params[0] = locationid.ToString();
            cmd.Params[1] = articlenr;
            cmd.Params[2] = batchnr;
            cmd.Params[3] = pd;

            cmd.Exec();
            if (cmd.dr.Read())
            {
                h.HeadID = cmd.dr.GetInt32(0);
                h.LocationID = cmd.dr.GetInt32(1);
                h.FAUF = cmd.dr.GetString(2);
                h.ArticleNr = cmd.dr.GetString(3);
                h.Remark = cmd.dr.GetString(4);
                h.ProductionDate = cmd.dr.GetDateTime(5);
                h.UserID = cmd.dr.GetString(7);

                return true;
            }
            else
            {

                h.LocationID = locationid;
                h.FAUF = batchnr;
                h.ArticleNr = articlenr;
                h.Remark = "";
                h.ProductionDate = DateTime.Parse(pd);
                h.UserID = "";

                //InsertHead(ref h);
            }

            return false;
        }

        #endregion

        #region OFFLINE_HANDLING

        public DataTable getArticleList()
        {
            return _ds.Tables["Article"];
        }


        public DataTable getLocationList()
        {
             return _ds.Tables["Location"];
        }

        public int SaveHead(DataItemQSHead head, DataItemQSPos pos)
        {
            if (_OnlineData)
                return SaveHeadOnline(head, pos);
            else
                return SaveHeadOffline(head, pos);
        }

        private int SaveHeadOffline(DataItemQSHead head, DataItemQSPos pos)
        {
            DataTable dt = _ds.Tables["QCOrderHead"];
            head.HeadID = dt.Rows.Count + 1;
            pos.PosID = 0;
            head.cntOK = 0;
            head.cntTotal = 0;

            DataRow[] foundRows;
            foundRows = dt.Select("LocationID='" + head.LocationID.ToString() + "' AND FAUF='" + head.FAUF + "' AND ArticleNr='" + head.ArticleNr + "'", "");

            if (foundRows.Length > 0)
            {
                head.HeadID = Int32.Parse(foundRows[0]["HeadID"].ToString());
                DataTable dt1 = _ds.Tables["QCOrderPos"];
                foundRows = dt1.Select("HeadID='" + head.HeadID + "'");
                pos.PosID = foundRows.Length;
                head.cntTotal = foundRows.Length;

                dt = _ds.Tables["QCOrderPos"];
                foundRows = dt.Select("HeadID='" + head.HeadID.ToString() + "' and AllOK='true'");
                head.cntOK = foundRows.Length;
            }
            else
            {
                DataRow row = dt.NewRow();

                row["HeadID"] = head.HeadID;
                row["LocationID"] = head.LocationID;
                row["FAUF"] = head.FAUF;
                row["ArticleNr"] = head.ArticleNr;
                row["Remark"] = head.Remark;
                row["ProductionDate"] = head.ProductionDate;
                row["TimeStamp"] = DateTime.Now;
                row["UserID"] = head.UserID;
                row["enabled"] = true;

                dt.Rows.Add(row);
                WriteDataToXML();
            }
            pos.HeadID = head.HeadID;

            return head.HeadID;
        }

        public void SavePos(DataItemQSHead head, DataItemQSPos pos)
        {
            if (_OnlineData)
                SavePosOnline(head, pos);
            else
                SavePosOffline(head, pos);
        }

        private void SavePosOffline(DataItemQSHead head, DataItemQSPos pos)
        {
            DataTable dt = _ds.Tables["QCOrderPos"];
            DataRow row = dt.NewRow();

            pos.PosID++;
            head.cntTotal++;

            row["HeadID"] = pos.HeadID;
            row["PosID"] = pos.PosID;
            row["mAPhosphor"] = pos.mAPhosphor;
            row["mABlue"] = pos.mABlue;
            row["mARed"] = pos.mARed;
            row["PhosphorOK"] = pos.PhosphorOK;
            row["BlueOK"] = pos.BlueOK;
            row["RedOK"] = pos.RedOK;
            row["Temperature"] = pos.Temperature;
            row["TSensorOK"] = pos.TSensorOK;
            row["EepromData"] = "";
            row["EepromOK"] = pos.EepromOK;
            row["Remark"] = pos.Remark;
            row["TimeStamp"] = DateTime.Now;

            if (pos.AllOK)            
                head.cntOK++;
            
            row["AllOK"] = pos.AllOK;

            dt.Rows.Add(row);
            WriteDataToXML();
        }

        private void WriteDataToXML()
        {
            if (!_OnlineData)
                ds.WriteXml(AppDomain.CurrentDomain.BaseDirectory + XMLDataPath + ".xml");
        }


        //Only used for first time writing to XML Schema
        private void WriteXMLSchema(SqlConnectionStringBuilder sqlsb)
        {
            DataSet ds = new DataSet("LEDTEST");
            SqlDataAdapter da = new SqlDataAdapter(
                "select * from Article;" +
                "select * from Location;" +
                "select * from QCOrderHead;" +
                "select * from QCOrderPos;"
                , sqlsb.ConnectionString);
            da.Fill(ds);

            //for (int i = 0; i < ds.Tables.Count; i++)
            ds.WriteXmlSchema("LEDTEST.xsd");
            ds.WriteXml("LEDTestData.xml");

        }


        #endregion

        /*protected void OnPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }*/
    }
}
