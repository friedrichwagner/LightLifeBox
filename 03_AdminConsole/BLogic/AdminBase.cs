using System.Data.SqlClient;
using System.Data;
using Lumitech.Helpers;
using LightLifeAdminConsole.Data;

namespace LightLifeAdminConsole
{
    public class AdminBase
    {
        private SqlConnection _con;
        private SQLSet _sqlset;
        private LTSQLCommand _sql;
        
        public SqlTransaction Transaction
        {
            get { return _sql.Transaction; }
            set { _sql.Transaction = value; }
        }

        public AdminBase(SqlConnection con, SQLSet sqlset)
        {
            this._sqlset = sqlset;
            this._con = con;

            if (_con.State != ConnectionState.Open)
                _con.Open();

            _sql = new LTSQLCommand(con);

        }

        public virtual DataTable select(string filter)
        {
            DataTable table = new DataTable();

            if (_sqlset.selectSQL != string.Empty)
            {
                _sql.prep(_sqlset.selectSQL + " " + filter);
                _sql.Exec();
                
                table.Load(_sql.dr);                
            }

            _sql.dr.Close();

            return table;
        }

        public virtual int insert(string[] p)
        {
            if (_sqlset.insertSQL != string.Empty)
            {
                _sql.prep(_sqlset.insertSQL);
                for (int i = 0; i < p.Length; i++)
                    _sql.Params[i] = p[i];

                return _sql.Exec();
            }

            return 0;
        }

        public virtual int update(string filter, string[] p)
        {
            if (_sqlset.updateSQL != string.Empty)
            {
                _sql.prep(_sqlset.updateSQL + " " + filter);
                for (int i = 0; i < p.Length; i++)
                    _sql.Params[i] = p[i];

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

        public virtual DataTable execQuery(string sqlCmd, string[] p, string filter)
        {
            DataTable table = new DataTable();
            if (sqlCmd != string.Empty)
            {
                _sql.prep(sqlCmd + " " + filter);
                for (int i = 0; i < p.Length; i++)
                    _sql.Params[i] = p[i];

                _sql.Exec();
                table.Load(_sql.dr);  
            }

            _sql.dr.Close();

            return table;
        }
    }
}
