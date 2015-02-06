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
    public class AdminBase
    {
        private LTSQLCommand _cmd;
        private SqlConnection _con;
        private SQLSet _sqlset;
        private LTSQLCommand _sql;

        public AdminBase(SqlConnection con, SQLSet sqlset)
        {
            _cmd = new LTSQLCommand(con);
            _cmd.Connection.Open();
            this._sqlset = sqlset;
            this._con = con;

            if (_con.State != ConnectionState.Open)
                _con.Open();

            _sql = new LTSQLCommand(con);

            
        }

        public virtual SqlDataReader select(string filter)
        {
            if (_sqlset.selectSQL != string.Empty)
            {
                _sql.prep(_sqlset.selectSQL + " " + filter);
                _sql.Exec();
                return _sql.dr;
            }

            return null;
        }

        public virtual int insert(string filter)
        {
            if (_sqlset.insertSQL != string.Empty)
            {
                _sql.prep(_sqlset.deleteSQL + " " + filter);
                return _sql.Exec();
            }

            return 0;
        }

        public virtual int update(string filter)
        {
            if (_sqlset.updateSQL != string.Empty)
            {
                _sql.prep(_sqlset.deleteSQL + " " + filter);
                return _sql.Exec();
            }

            return 0;
        }

        public virtual int delete(string filter)
        {
            if (_sqlset.deleteSQL != string.Empty)
            {
                _sql.prep(_sqlset.deleteSQL + " " + filter);
                return _sql.Exec();
            }

            return 0;
        }
        

    }
}
