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
    public struct SQLSet
    {
        public string selectSQL;
        public string insertSQL;
        public string updateSQL;
        public string deleteSQL;

        public SQLSet(int n)
        {
            selectSQL = String.Empty;
            insertSQL = String.Empty;
            updateSQL = String.Empty;
            deleteSQL = String.Empty;
        }
    };

    public static class LLSQL
    {
        // Only Select
        public static SQLSet LLRole = new SQLSet();
        public static SQLSet LLRoom = new SQLSet();
        public static SQLSet LLGroup = new SQLSet();
        public static SQLSet LLFixture = new SQLSet();
        public static SQLSet LLScene = new SQLSet();
        public static SQLSet LLData = new SQLSet();

        //Select, Insert, Update, NO Delete
        public static SQLSet LLUser = new SQLSet();
        public static SQLSet LLUSerInfo = new SQLSet();
        
        public static void InitSQLs()
        {
            LLRole.selectSQL = "select * from LLRole oder by RoleID";
            LLRoom.selectSQL = "select * from LLRoom oder by RoomID";
            LLGroup.selectSQL = "select * from LLGroup oder by GroupID";
            LLFixture.selectSQL = "select * from LLFixture oder by FixtureID";
            LLScene.selectSQL = "select * from LLScene oder by SceneID";
            LLData.selectSQL = "select * from LLData oder by DataID";

            LLUser.selectSQL = "select * from LLUser";
            LLUser.insertSQL = "insert into LLUser() values()";
            LLUser.updateSQL = "update LLUser set where ";

            LLUSerInfo.selectSQL = "select * from LLUserInfo";
            LLUSerInfo.insertSQL = "insert into LLUserInfo() values()";
            LLUSerInfo.updateSQL = "update LLUserInfo set where ";
        }
    }
}
