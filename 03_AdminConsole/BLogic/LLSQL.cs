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
using System.Data;
using Lumitech.Helpers;

namespace LightLife
{
    public class SQLSet
    {
        public string tablename;
        public string selectSQL;
        public string insertSQL;
        public string updateSQL;
        public string deleteSQL;

        public SQLSet(string tname)
        {
            tablename = tname;
            selectSQL = String.Empty;
            insertSQL = String.Empty;
            updateSQL = String.Empty;
            deleteSQL = String.Empty;
        }
    };

    public static class LLSQL
    {
        public static SqlConnection sqlCon;
        public static LTSQLCommand cmd;
        public static IDictionary<string, SQLSet> tables;
        public static IDictionary<int, string> llgroups;
        public static IDictionary<int, string> llrooms;
        
        public static void InitSQLs()
        {
            Settings ini = Settings.GetInstance();
            
            SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();
            sqlsb.DataSource = ini.ReadString("Database", "DataSource", "");
            sqlsb.InitialCatalog = ini.ReadString("Database", "InitialCatalog", "");
            sqlsb.UserID = ini.ReadString("Database", "UserID", "");
            sqlsb.Password = ini.ReadString("Database", "Password", "");

            sqlCon = new SqlConnection(sqlsb.ConnectionString);
            cmd = new LTSQLCommand(sqlCon);

            tables = new Dictionary<string, SQLSet>();
            tables.Add("LLRole", new SQLSet("LLRole"));
            tables.Add("LLRoom", new SQLSet("LLRoom"));
            tables.Add("LLGroup", new SQLSet("LLGroup"));
            tables.Add("LLFixture", new SQLSet("LLFixture"));
            tables.Add("LLScene", new SQLSet("LLScene"));
            tables.Add("LLData", new SQLSet("LLData"));
            tables.Add("LLUser", new SQLSet("LLUser"));
            tables.Add("LLUserInfo", new SQLSet("LLUSerInfo"));

            tables["LLRole"].selectSQL = "select * from LLRole order by RoleID";
            tables["LLRoom"].selectSQL = "select * from LLRoom order by RoomID";
            tables["LLGroup"].selectSQL = "select * from LLGroup order by GroupID";
            tables["LLFixture"].selectSQL = "select * from LLFixture order by FixtureID";
            tables["LLScene"].selectSQL = "select * from LLScene order by SceneID";
            tables["LLData"].selectSQL = "select * from LLData order by DataID";

            tables["LLUser"].selectSQL = "select * from LLUser";
            tables["LLUser"].insertSQL = "insert into LLUser() values()";
            tables["LLUser"].updateSQL = "update LLUser set where ";

            tables["LLUserInfo"].selectSQL = "select * from LLUserInfo";
            tables["LLUserInfo"].insertSQL = "insert into LLUserInfo() values()";
            tables["LLUserInfo"].updateSQL = "update LLUserInfo set where ";

            if (sqlCon.State == ConnectionState.Closed) sqlCon.Open();

            llgroups = new Dictionary<int, string>();
            getDict(ref llgroups, tables["LLGroup"], "GroupID, Name");

            llrooms = new Dictionary<int, string>();
            getDict(ref llrooms, tables["LLRoom"], "RoomID, Name");
        }

        private static void getDict(ref IDictionary<int, string> dict, SQLSet s, string fields)
        {
            try
            {
                string stmt = "select " + fields + " from " + s.tablename;
                cmd.prep(stmt);
                cmd.Exec();

                //Always add a "-1" entry
                dict.Add(-1, "");
                
                while (cmd.dr.Read())
                {
                    dict.Add(cmd.dr.GetInt32(0), cmd.dr.GetString(1));
                }

                cmd.dr.Close();

            }
            catch
            {
                throw;
            }


        }
    }

    public static class Extensions
    {
        public static Dictionary<TKey, TRow> TableToDictionary<TKey,TRow>(
            this DataTable table,
            Func<DataRow, TKey> getKey,
            Func<DataRow, TRow> getRow)
        {
            return table
                .Rows
                .OfType<DataRow>()
                .ToDictionary(getKey, getRow);
        }
    }

    /*public static void SampleUsage()
        {
            DataTable t = new DataTable();

            var dictionary = t.TableToDictionary(
                row => row.Field<int>("ID"),
                row => new {
                    Age = row.Field<int>("Age"),
                    Name = row.Field<string>("Name"),
                    Address = row.Field<string>("Address"),
                });
        }*/
}
