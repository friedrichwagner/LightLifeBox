using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.ComponentModel;
//using System.Runtime.CompilerServices;
using MvvmFoundation.Wpf;
using System.Windows.Input;
using Lumitech.Helpers;

namespace Lumitech
{
    public enum SAPArticleType { SAP_NONE, SAP_LTF, SAP_LTS, SAP_LTE, SAP_LTEF, SAP_LTL, SAP_LTK, SAP_LTX, SAP_LABEL}
    public delegate void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e);

    public class SAPArticle : ObservableObject
    {
        private string _Nr;
        public string Nr { get {return _Nr;}
            set { _Nr = value; RaisePropertyChanged("Nr"); RaisePropertyChanged("Text"); }
        }

        private SAPArticleType _type;
        public SAPArticleType type
        {
            get { return _type; }
            set { 
                _type = value;

                switch (_type)
                {
                    case (SAPArticleType.SAP_NONE): _TypeDescription = "unknown"; break;
                    case (SAPArticleType.SAP_LTF): _TypeDescription = "Fertigprodukt"; break;
                    case (SAPArticleType.SAP_LTS): _TypeDescription = "Fertigprodukt"; break;
                    case (SAPArticleType.SAP_LTE): _TypeDescription = "Elektronik"; break;
                    case (SAPArticleType.SAP_LTEF): _TypeDescription = "Firmware"; break;
                    case (SAPArticleType.SAP_LTL): _TypeDescription = "LED Modul"; break;
                    case (SAPArticleType.SAP_LTK): _TypeDescription = "LED Modul"; break;
                    case (SAPArticleType.SAP_LTX): _TypeDescription = "Zubehör"; break;
                    case (SAPArticleType.SAP_LABEL): _TypeDescription = "Label"; break;
                    default: _TypeDescription = "unknown"; break;
                }

                RaisePropertyChanged("type"); 
                RaisePropertyChanged("TypeDescription");                
            }
        }


        private string _TypeDescription;
        public string TypeDescription
        {
            get { return _TypeDescription; }
            set { _TypeDescription = value; RaisePropertyChanged("TypeDescription"); }
        }

        private string _Description;
        public string Description { get { return _Description; } set { _Description = value; RaisePropertyChanged("Description"); RaisePropertyChanged("Text"); } }
        //public string Description { get { return _Description; }}

        public string Text { get { return _Nr + "\r\n" + _Description; } }

        public string UProp1 {get; set;}
        public string UProp2 { get; set; }
        public Single Qty { get; set; }

        public SAPArticle(SAPArticleType t, string pNr, string pDesc, string p1, string p2, float pQty)
        {
            type = t;  Nr = pNr; Description = pDesc; UProp1 = p1; UProp2 = p2; Qty = pQty;
        }

        public void setEmpty()
        {
            type = SAPArticleType.SAP_NONE; Nr = ""; Description = ""; UProp1 = ""; UProp2 = ""; Qty = 0.0f;
        }
    }

    public class SAPData : ObservableObject
    {
        //Produktionsauftrag
        private const string sqlFauf = "select T1.Docnum, T1.ItemCode, T1.PlannedQty, T2.Itemcode,  T2.ItmsGrpCod from OWOR T1, OITM T2 where DocNum='#Fauf' " +
                                        " and  T1.ItemCode = T2.ItemCode and T2.ItmsGrpCod in (104,109)";

        //Stückliste zu einem FAUF auslesen, notwendig am Sheet "labels" zum Drucken, da auch Musteraufträge gedruckt werden sollen
        private const string sqlFaufStueli = //"select T0.DocNum, T0.ItemCode, T2.ItemName, T2.U_PROP1, T2.U_PROP2, T0.PlannedQty as Qty" +
                                             "select T0.DocNum, T0.ItemCode, T2.ItemName, IsNull(null,0) as U_PROP1, IsNull(null,0) as U_PROP2, T0.PlannedQty as Qty" +
                                             " from OWOR T0, OITM T2 where T0.ItemCode=T2.ItemCode and T0.DocNum = '#Fauf' " +
                                             " Union " +
                                             //" select T0.DocNum, T1.ItemCode, T2.ItemName, T2.U_PROP1, T2.U_PROP2, T1.BaseQty " +
                                             " select T0.DocNum, T1.ItemCode, T2.ItemName, IsNull(null,0) as U_PROP1, IsNull(null,0) as U_PROP2, T1.BaseQty " +
                                             " from OWOR T0, WOR1 T1, OITM T2 " +
                                             " where T0.Docentry = T1.Docentry And T1.ItemCode = T2.ItemCode " +
                                             " and T0.DocNum = '#Fauf' " +
                                             " and (T1.ItemCode like 'LTEF-%' or T1.ItemCode like 'LTE-%' or T1.ItemCode like 'LTL-%' or T1.ItemCode like 'LTK-%' or  (T1.ItemCode between '2000' and '3000' )) " +
                                             " order by 2 desc";
        //Artikelstamm
        //private const string sqlArtStamm = "select Itemcode, Itemname, ItmsGrpCod, IsNull(U_Prop1,0) as U_PROP1, IsNull(U_Prop2,0) as U_PROP2 from OITM where Itemcode='#ArtNr'";
        private const string sqlArtStamm = "select Itemcode, Itemname, ItmsGrpCod, IsNull(null,0) as U_PROP1, IsNull(null,0) as U_PROP2 from OITM where Itemcode='#ArtNr'";

        private const string sqlStueli = //"select  T0.Code, T1.ItemName, T1.U_PROP1, T1.U_PROP2, T0.Quantity  " +
                                        "select  T0.Code as Itemcode, T1.ItemName, IsNull(null,0) as U_PROP1, IsNull(null,0) as U_PROP2, T0.Quantity as Qty " +
                                        " from ITT1 T0, OITM T1  " +
                                        " where T0.Father = '#ArtNr' " +
                                        " and T0.Code = T1.ItemCode  " +
                                        //" and (T0.Code like 'LTEF-%' or T0.Code like 'LTE-%' or T1.ItemCode like 'LTL-%' or T1.ItemCode like 'LTK-%' or  (T0.Code between '2000' and '3000' ))  " +
                                        " and (T0.Code like 'LTEF-%' or T0.Code like 'LTE-%' or T1.ItemCode like 'LTL-%' or T1.ItemCode like 'LTK-%') " +
                                        " order by 1 desc";

        //Für Label Print only
        private const string sqlLabels3 = //"select  T0.Code, T1.ItemName, T1.U_PROP1, T1.U_PROP2, T0.Quantity from ITT1 T0, OITM T1 " +
                                        "select  T0.Code, T1.ItemName, IsNull(null,0) as U_PROP1, IsNull(null,0) as U_PROP2, T0.Quantity from ITT1 T0, OITM T1 " +
                                        " where T0.Father = '#ArtNr' and T0.Code = T1.ItemCode and (T0.Code between '2000' and '3000' ) " +
                                        " order by 1";

        private string sFAUF;
        public string FAUF { get { return sFAUF; }
            set { sFAUF = value; RaisePropertyChanged("FAUF"); }
        }

        private SAPArticle sapFert;
        public SAPArticle Fert { get { return sapFert; } }

        private SAPArticle sapLTE;
        public SAPArticle LTE { get { return sapLTE; } }

        private SAPArticle sapLTL;
        public SAPArticle LTL { get { return sapLTL; } }

        private SAPArticle sapLTEF;
        public SAPArticle LTEF { get { return sapLTEF; } }

        private Dictionary<string, SAPArticle> sapLabels = new Dictionary<string, SAPArticle>();
        public Dictionary<string, SAPArticle> Labels { get { return sapLabels; } }

        private const string SAPUserId="Leser";
        private const string SAPUserPWD="Lumitech1";

        private bool bIsFauf;
        public bool isFauf { get { return bIsFauf; } }
        private Single siFaufQty;
        public Single FaufQty { get { return siFaufQty; } }

        private BackgroundWorker bw1 = new BackgroundWorker();
        private List<object> bw1_arguments = new List<object>();

        private LTSQLCommand cmd;     

        public SAPData()
        {
            Settings ini = Settings.GetInstance();

            SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();
            sqlsb.DataSource = ini.ReadString("SAP-Database", "DataSource", "");
            sqlsb.InitialCatalog = ini.ReadString("SAP-Database", "InitialCatalog", "");
            //sqlsb.UserID = SAPUserId;
            //sqlsb.Password = SAPUserPWD;
            sqlsb.UserID = ini.ReadString("SAP-Database", "UserID", "");
            sqlsb.Password = ini.ReadString("SAP-Database", "Password", "");

            cmd = new LTSQLCommand(new SqlConnection(sqlsb.ConnectionString));
            //ResetData();

            bIsFauf = false;
            sFAUF = String.Empty;
            siFaufQty = 0.0f;
            sapFert = new SAPArticle(SAPArticleType.SAP_NONE, "", "", "", "", 0.0f);
            sapLTE = new SAPArticle(SAPArticleType.SAP_NONE, "", "", "", "", 0.0f);
            sapLTL = new SAPArticle(SAPArticleType.SAP_NONE, "", "", "", "", 0.0f);
            sapLTEF = new SAPArticle(SAPArticleType.SAP_NONE, "", "", "", "", 0.0f);
            sapLabels.Clear();

            bw1.DoWork += (obj, li) => bw_DoWork(this, bw1_arguments);
        }

        private void ResetData()
        {
            bIsFauf = false;
            sFAUF = String.Empty;
            siFaufQty = 0.0f;
            sapFert.setEmpty();
            sapLTE.setEmpty();
            sapLTL.setEmpty();
            sapLTEF.setEmpty();
            sapLabels.Clear();
        }

        public bool getDataAsync(string FAUF_or_ArtNr, RunWorkerCompletedEventHandler del)
        {
            if (bw1.IsBusy == true) return false;

            //funktioniert nur, wenn der gleich Delegate übergeben wird
            bw1.RunWorkerCompleted -= del;

            bw1_arguments.Clear();
            bw1_arguments.Add(FAUF_or_ArtNr);          
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(del);

            bw1.RunWorkerAsync();

            return true;
        }

        private void bw_DoWork(object sender, List<object> obj)
        {
            string fauf = obj[0] as String;
            getData(fauf);
            if (!bIsFauf) throw new ArgumentException("No Fauf!");
        }

        private void setSAPArticle(SAPArticle s, SAPArticleType t)
        {
            s.type = t;
            s.Nr = cmd.dr["Itemcode"].ToString();
            s.Description = cmd.dr["Itemname"].ToString();
            s.UProp1 = cmd.dr["U_PROP1"].ToString();
            s.UProp2 = cmd.dr["U_PROP2"].ToString();
            s.Qty = 0.0f;
        }

        public bool getData(string FAUF_or_Artnr)
        {
            try
            {
                int tmp;
                string artnr = FAUF_or_Artnr;
                StringBuilder stmt;

                if (FAUF_or_Artnr.Length == 0) return false;

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                    cmd.Connection.Open();

                bool b = Int32.TryParse(FAUF_or_Artnr, out tmp);

                ResetData();

                //1. If we have a FAUF Number
                if (b)
                {
                    stmt = new StringBuilder(sqlFauf);
                    stmt.Replace("#Fauf", FAUF_or_Artnr);
                    cmd.prep(stmt.ToString());
                    cmd.Exec();

                    if (cmd.dr.Read())
                    {
                        bIsFauf = true;
                        sFAUF = cmd.dr["DocNum"].ToString();
                        artnr = cmd.dr["ItemCode"].ToString();
                        siFaufQty = Single.Parse(cmd.dr["PlannedQty"].ToString());
                        //string s = cmd.dr["PlannedQty"].ToString();
                    }
                }

                RaisePropertyChanged("FaufQty");

                //2. Get the FERT Articlenumber
                stmt = new StringBuilder(sqlArtStamm);
                stmt.Replace("#ArtNr", artnr);
                cmd.prep(stmt.ToString());
                cmd.Exec();

                if (cmd.dr.Read())
                {
                    setSAPArticle(sapFert, SAPArticleType.SAP_LTS);
                }

                //3. get Stueli either from article or from FAUF
                if (bIsFauf)
                {
                    stmt = new StringBuilder(sqlFaufStueli);
                    stmt.Replace("#Fauf", sFAUF);
                    cmd.prep(stmt.ToString());
                    cmd.Exec();
                }
                else
                {
                    stmt = new StringBuilder(sqlStueli);
                    stmt.Replace("#ArtNr", sapFert.Nr);
                    cmd.prep(stmt.ToString());
                    cmd.Exec();
                }

                while (cmd.dr.Read())
                {
                    artnr = cmd.dr["Itemcode"].ToString();

                    if (artnr.StartsWith("LTE-"))
                    {
                        setSAPArticle(sapLTE, SAPArticleType.SAP_LTE);
                        //sapLTE = new SAPArticle(cmd.dr["Itemcode"].ToString(), cmd.dr["Itemname"].ToString(), cmd.dr["U_PROP1"].ToString(), cmd.dr["U_PROP2"].ToString(), Single.Parse(cmd.dr["Qty"].ToString()));
                    }

                    else if (artnr.StartsWith("LTEF-"))
                    {
                        setSAPArticle(sapLTEF, SAPArticleType.SAP_LTEF);
                        //sapLTEF = new SAPArticle(cmd.dr["Itemcode"].ToString(), cmd.dr["Itemname"].ToString(), cmd.dr["U_PROP1"].ToString(), cmd.dr["U_PROP2"].ToString(), Single.Parse(cmd.dr["Qty"].ToString()));
                    }

                    else if (artnr.StartsWith("LTL-") || (artnr.StartsWith("LTK-")))
                    {
                        setSAPArticle(sapLTL, SAPArticleType.SAP_LTL);
                        //sapLTL = new SAPArticle(cmd.dr["Itemcode"].ToString(), cmd.dr["Itemname"].ToString(), cmd.dr["U_PROP1"].ToString(), cmd.dr["U_PROP2"].ToString(), Single.Parse(cmd.dr["Qty"].ToString()));
                    }

                    else //Labels
                    {

                    }
                }

                //4. get Labels
                stmt = new StringBuilder(sqlLabels3);
                stmt.Replace("#ArtNr", sapFert.Nr);
                cmd.prep(stmt.ToString());
                cmd.Exec();

                while (cmd.dr.Read())
                {
                    b = Int32.TryParse(cmd.dr["Code"].ToString(), out tmp);

                    if (b)
                    {
                        sapLabels.Add(cmd.dr["Code"].ToString(), new SAPArticle(SAPArticleType.SAP_LABEL, cmd.dr["Code"].ToString(), cmd.dr["Itemname"].ToString(), "", "", Single.Parse(cmd.dr["Quantity"].ToString())));
                    }
                }

                cmd.Connection.Close();

                return true;
            }
            catch
            {
                cmd.Connection.Close();
                throw;
            }
        }
    }
}

