using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using Lumitech.Helpers;
using LightLife.Data;

namespace LightLifeAdminConsole.Data
{
    public class SQLSet
    {
        public string tablename;
        public string selectSQL;
        public string insertSQL;
        public string updateSQL;
        public string deleteSQL;
        public string sqlCmd1;
        public string sqlCmd2;

        public SQLSet(string tname)
        {
            tablename = tname;
            selectSQL = String.Empty;
            insertSQL = String.Empty;
            updateSQL = String.Empty;
            deleteSQL = String.Empty;
            sqlCmd1 = String.Empty;
            sqlCmd2 = String.Empty;
        }
    };

    public class PILEDScene
    {
        public int ID;
        public string Name {get; private set;}
        public PILEDData piledata;

        public PILEDScene(int id, string name, PILEDData d)
        {
            ID = id;
            Name = name;
            piledata = d;
        }
    }

    public static class LLSQL
    {
        public static SqlConnection sqlCon;
        public static LTSQLCommand cmd;
        public static IDictionary<string, SQLSet> tables;
        public static IDictionary<int, string> llgroups;
        public static IDictionary<int, string> llrooms;
        public static IDictionary<int, string> lllights;
        public static IDictionary<int, string> sequences;
        public static IDictionary<int, PILEDScene> llscenes;
        public static DataTable llroomgroup;
        public static DataTable llusers;
        public static IDictionary<int, string> probanden;
        public static IDictionary<int, string> llactivationstate;
        public static DataTable llstep;
        public static DataTable V_BoxState;
        
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
            tables.Add("LLRoomGroup", new SQLSet("LLRoomGroup"));
            tables.Add("LLTestSequenceHead", new SQLSet("LLTestSequenceHead"));
            tables.Add("LLTestSequencePos", new SQLSet("LLTestSequencePos"));
            tables.Add("LLBox", new SQLSet("LLBox"));

            tables.Add("LLActivationState", new SQLSet("LLActivationState"));
            tables.Add("LLStep", new SQLSet("LLStep"));
            tables.Add("LLBoxState", new SQLSet("V_BOXSTATE"));
            tables.Add("VLLTestSequence", new SQLSet("V_TestSequence"));

            tables["LLRole"].selectSQL = "select * from LLRole order by RoleID";
            tables["LLRoom"].selectSQL = "select * from LLRoom order by RoomID";
            tables["LLGroup"].selectSQL = "select * from LLGroup order by GroupID";
            tables["LLFixture"].selectSQL = "select * from LLFixture order by FixtureID";
            tables["LLScene"].selectSQL = "select * from LLScene order by SceneID";
            tables["LLData"].selectSQL = "select * from LLData order by DataID";
            tables["LLRoomGroup"].selectSQL = "select * from LLRoomGroup order by RoomID, GroupID";

            tables["LLUser"].selectSQL = "select * from LLUser";
            tables["LLUser"].insertSQL = "insert into LLUser() values()";
            tables["LLUser"].updateSQL = "update LLUser set where ";

            tables["LLUserInfo"].selectSQL = "select * from LLUserInfo";
            tables["LLUserInfo"].insertSQL = "insert into LLUserInfo() values()";
            tables["LLUserInfo"].updateSQL = "update LLUserInfo set where ";

            tables["LLTestSequenceHead"].selectSQL = "select * from LLTestSequenceHead";
            tables["LLTestSequenceHead"].insertSQL = "insert into LLTestSequenceHead(SequenceID, BoxID, UserID, VLId, TestStateID, ActualPosID, remark) values(:1,:2,:3,:4,:5,:6,:7)";
            tables["LLTestSequenceHead"].updateSQL = "update LLTestSequenceHead set TestStateID=:1 where SequenceID=:2";
            tables["LLTestSequenceHead"].sqlCmd1 = "update LLTestSequenceHead set Remark=:1 where sequenceID=:2";

            tables["LLTestSequencePos"].selectSQL = "select * from LLTestSequencePos";
            tables["LLTestSequencePos"].insertSQL = "insert into LLTestSequencePos(SequenceID, ActivationID, StepID, pimode, Brightness, CCT, duv, x,y, remark) values(:1,:2,:3,:4,:5,:6,:7,:8, :9, :10)";
            tables["LLTestSequencePos"].updateSQL = "update LLTestSequencePos set Brightness=:1, CCT=:2, duv=:3 x=:4, y=:5, remark=:6 where PosID=:7";

            tables["LLBox"].selectSQL = "select * from LLBox";
            tables["LLBox"].updateSQL = "update LLBox set active=:1 where boxid=:2";

            tables["LLActivationState"].selectSQL = "select * from LLActivationState";
            tables["LLStep"].selectSQL = "select * from LLStep";
            tables["LLBoxState"].selectSQL = "select * from V_BOXSTATE";
            tables["VLLTestSequence"].selectSQL = "select * from V_TestSequence";

            if (sqlCon.State == ConnectionState.Closed) sqlCon.Open();

            llgroups = new Dictionary<int, string>();
            getDict(ref llgroups, tables["LLGroup"], "GroupID, Name");

            llrooms = new Dictionary<int, string>();
            getDict(ref llrooms, tables["LLRoom"], "RoomID, Name");

            lllights = new Dictionary<int, string>();
            getDict(ref lllights, tables["LLFixture"], "FixtureID, Name");

            sequences = new Dictionary<int, string>();
            getSequences(ref sequences);

            llscenes = new Dictionary<int, PILEDScene>();
            getScenes(ref llscenes, tables["LLScene"]);

            llroomgroup = new DataTable(tables["LLRoomGroup"].tablename);
            getDataTable(ref llroomgroup, tables["LLRoomGroup"]);

            llusers = new DataTable(tables["LLUser"].tablename);
            getDataTable(ref llusers, tables["LLUser"]);

            probanden = new Dictionary<int, string>();
            getDict(ref probanden, tables["LLUser"], "UserID,  (FirstName+ ' '+ Lastname)", " where RoleId=100");

            llactivationstate = new Dictionary<int, string>();
            getDict(ref llactivationstate, tables["LLActivationState"], "ActivationID, Name", "order by 1");

            llstep = new DataTable(tables["LLStep"].tablename);
            getDataTable(ref llstep, tables["LLStep"]);

            V_BoxState = new DataTable("V_BoxState");
            getDataTable(ref V_BoxState, tables["LLBoxState"]);
        }

        public static void Done()
        {
            if (LLSQL.sqlCon.State == System.Data.ConnectionState.Open)
                LLSQL.sqlCon.Close();
        }

        private static void getDict(ref IDictionary<int, string> dict, SQLSet s, string fields, string filter="")
        {
            try
            {
                string stmt = "select " + fields + " from " + s.tablename;
                if (filter.Length > 0) stmt += " " + filter;
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

        private static void getScenes(ref IDictionary<int, PILEDScene> dict, SQLSet s)
        {
            try
            {
                string stmt = "select SceneID, SceneName, pimode, Brightness, CCT, duv, x, y from " + s.tablename;
                cmd.prep(stmt);
                cmd.Exec();

                
                while (cmd.dr.Read())
                {
                    dict.Add(cmd.dr.GetInt32(0), new PILEDScene(cmd.dr.GetInt32(0), cmd.dr.GetString(1),
                        new PILEDData((PILEDMode)(cmd.dr.GetInt32(2)), (byte)cmd.dr.GetInt32(3), cmd.dr.GetInt32(4), (float)cmd.dr.GetDouble(5), (float)cmd.dr.GetDouble(6), (float)cmd.dr.GetDouble(7), (byte)0, (byte)0, (byte)0, "AdminConsole", "Lights")));
                }

                cmd.dr.Close();

            }
            catch
            {
                throw;
            }
        }

        private static void getSequences(ref IDictionary<int, string> dict)
        {
            dict.Add(0, "Sequenzen Stoppen");
            dict.Add(1, "Standard Tagesverlauf");
            dict.Add(2, "RGB Sequenz");
            dict.Add(3, "Demo-Tagesverlauf");
        }

        private static void getDataTable(ref DataTable t, SQLSet s)
        {
            cmd.prep(s.selectSQL);
            cmd.Exec();

            t.Load(cmd.dr);         
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

