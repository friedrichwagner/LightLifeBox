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

        public SQLSet(int n);

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
        public static IDictionary<string, SQLSet> tables;
        public static SqlConnection sqlCon;
        
        public static void InitSQLs()
        {
            

            Settings ini = Settings.GetInstance();

            SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder();
            sqlsb.DataSource = ini.ReadString("Database", "DataSource", "");
            sqlsb.InitialCatalog = ini.ReadString("Database", "InitialCatalog", "");
            sqlsb.UserID = ini.ReadString("Database", "UserID", "");
            sqlsb.Password = ini.ReadString("Database", "Password", "");

            sqlCon = new SqlConnection(sqlsb.ConnectionString);

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
        }
    }
}
