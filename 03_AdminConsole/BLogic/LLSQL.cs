﻿using System;
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
using PILEDServer;

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
            tables["LLTestSequenceHead"].insertSQL = "insert into LLTestSequenceHead(SequenceID, BoxID, UserID, VLId, remark) values(:1,:2,:3,:4, :5)";
            tables["LLTestSequenceHead"].updateSQL = "update LLTestSequenceHead set Remark=:1 where sequenceID=:2";

            tables["LLTestSequencePos"].selectSQL = "select * from LLTestSequencePos";
            tables["LLTestSequencePos"].insertSQL = "insert into LLTestSequencePos(SequenceID, StepID, pimode, Brightness, CCT, x,y, remark) values(:1,:2,:3,:4,:5,:6,:7,:8)";
            tables["LLTestSequencePos"].updateSQL = "update LLTestSequencePos set Brightness=:1, CCT=:2, x=:3, y=:4, remark=:5 where sequenceID=:6 and stepid=:7";

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
                string stmt = "select SceneID, SceneName, pimode, Brightness, CCT, x, y from " + s.tablename;
                cmd.prep(stmt);
                cmd.Exec();

                
                while (cmd.dr.Read())
                {
                    dict.Add(cmd.dr.GetInt32(0), new PILEDScene(cmd.dr.GetInt32(0), cmd.dr.GetString(1),
                        new PILEDData((PILEDMode)(cmd.dr.GetInt32(2)), (byte)cmd.dr.GetInt32(3), cmd.dr.GetInt32(4), (float)cmd.dr.GetDouble(5), (float)cmd.dr.GetDouble(6), (byte)0, (byte)0, (byte)0, "AdminConsole", "Lights")));
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
