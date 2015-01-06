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

namespace LightLife
{

    class ConsoleLogin
    {
        private const string sqlCheckLogin = "select * from LLUser where Username='#Username' and Password='#Password'";
        private const string sqlGetRole = "select * from LLRole where RolID=#RoleID";

        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public int UserId { get; private set; }

        public int RoleId { get; private set; }
        public string RoleName { get; private set; }
        public string Rights { get; private set; }
        public bool IsLoggedIn { get; set; }

        private LTSQLCommand cmd;

        public ConsoleLogin(SqlConnection con)
        {
            cmd = new LTSQLCommand(con);
            cmd.Connection.Open();
            IsLoggedIn = false;
            FirstName = String.Empty;
            LastName = String.Empty;
            RoleName = String.Empty;
            Rights = String.Empty;
        }

        public bool CheckUser(string uname, string pwd)
        {             
            StringBuilder stmt = new StringBuilder(sqlCheckLogin);
            stmt.Replace("#Username", uname);
            stmt.Replace("#Password", pwd);
            cmd.prep(stmt.ToString());
            cmd.Exec();

            if (cmd.dr.Read())
            {
                IsLoggedIn = true;
                FirstName = cmd.dr["FirstName"].ToString();
                LastName = cmd.dr["LastName"].ToString();
                UserId = Int32.Parse(cmd.dr["UserId"].ToString());
                RoleId = Int32.Parse(cmd.dr["RoleId"].ToString());
            }

            return IsLoggedIn;
        }
    }
}
