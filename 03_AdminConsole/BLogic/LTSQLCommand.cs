using System;
using System.Collections;
using System.Data.SqlClient;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lumitech.Helpers
{
    
    public class LTSQLCommand
    {

        public SqlCommand cmd= new SqlCommand();
        public ArrayList Params= new ArrayList();
        private int AnzParams;
        private string stmt;
        public string stmtText { get { return stmt; } }

        public SqlDataReader dr;
        private Stopwatch sw = new Stopwatch();
        //private Logger log;

        public SqlConnection Connection
        {
            get { return cmd.Connection; }
            set { cmd.Connection = value; }
        }

        public SqlTransaction Transaction
        {
            get { return cmd.Transaction; }
            set { cmd.Transaction = value; }
        }

        public LTSQLCommand()
        {
            //Database
            Settings ini = Settings.GetInstance();
            String s = ini.ReadString("Database", "DataSource", "");
            SqlConnectionStringBuilder sqlsb= new SqlConnectionStringBuilder();     
            sqlsb.DataSource = s;
            sqlsb.InitialCatalog = ini.ReadString("Database", "InitialCatalog", "");
            sqlsb.UserID = ini.ReadString("Database", "UserID", "");
            sqlsb.Password = ini.ReadString("Database", "Password", "");
            cmd.Connection = new SqlConnection(sqlsb.ConnectionString);

            //log = Logger.GetInstance();
        }

        public LTSQLCommand(SqlConnection con)
        {
            cmd.Connection = con;
            //cmd.Transaction = null;
            //log = Logger.GetInstance();
        }

        public LTSQLCommand(SqlConnection con, SqlTransaction tr)
        {
            cmd.Connection = con;
            cmd.Transaction = tr;
            //log = Logger.GetInstance();
        }

        public int prep(string cmdText)
        {
            stmt = cmdText;

            Params.Clear();
            AnzParams = 0;


            //FW 19.3.2014 - DAs kann ein Problem sein, wenn in einerm String wirklich ein ":" vorkommt
            //AnzParams = cmdText.Count(x => x == ':');
            //Params.Capacity = AnzParams;            
            
            for (int i = 1; i < 100; i++)
            {
                //if (cmdText.IndexOf(":" + i + ",") > 0 || cmdText.IndexOf(":" + i + " ") > 0 || cmdText.IndexOf(":" + i + ")") > 0) 

                //Was ist mit :1 udn :10
                if (cmdText.IndexOf(":" + i) > 0)
                {
                    Params.Add("");
                    AnzParams++;
                }
                else break;
            }                    

            return AnzParams;
        }

        public string ReplaceParams()
        {
            //Falls keine Parameter sind
            this.cmd.CommandText = stmt;
            StringBuilder sb = new StringBuilder(stmt);

            //Von oben nach unten ersetzen, damit ":10" nicht bei ":1" mit ersetzt wird
            for (int i = Params.Count-1; i>=0; i--)
            {
                //Parameter Array started bei 0, Parameter in stmt starten  bei :1
                if (Params[i] is string)
                {
                    string s = Params[i] as String;
                    s.ToLower();
                    if (s.CompareTo("null") == 0)
                        sb.Replace(":" + (i + 1), "null");
                    else
                        sb.Replace(":" + (i + 1), "'" + Params[i] + "'");
                }
                else if (Params[i] is Boolean)
                {
                    sb.Replace(":" + (i + 1), "'" + Params[i] + "'");
                }
                else if (Params[i] is DateTime)
                {
                    sb.Replace(":" + (i + 1), "'" + Params[i] + "'");
                }
                else //alle anderen Parametertypen numerisch
                {
                    //Wird "." in "," umgewandelt ?
                    string s = String.Format("{0}", Params[i]).Replace(",", ".");
                    sb.Replace(":" + (i + 1), s);
                    //sb = Regex.Replace(sb.ToString(), ":" + (i + 1)+"[, s);
                }

            }

            this.cmd.CommandText = sb.ToString();
            return this.cmd.CommandText;
        }

        public int Exec()
        {
            int ret = -1;

            ReplaceParams();
            //cmd.CommandText=stmt;
            string s = cmd.CommandText.ToLower();
            s.TrimStart(' ');
            sw.Reset();
            sw.Start();

            if (s.StartsWith("select"))
            {
                //returns SqlDataReader
                if (dr!= null) dr.Close();

                //if (log !=  null) log.Debug(cmd.CommandText);
                Debug.Print(cmd.CommandText);

                dr=cmd.ExecuteReader();

                if (dr.HasRows) ret=1;
                else ret=0;
            }
            else
            {
                //return RowsAffected (int)
                if (dr != null) dr.Close();
                //if (log != null) log.Debug(cmd.CommandText);
                Debug.Print(cmd.CommandText);
                ret=cmd.ExecuteNonQuery();
            }
            sw.Stop();
            //Debug.Print(String.Format("{0} ms *************************************", sw.Elapsed.TotalMilliseconds));

            return ret;
        }
    }
}
