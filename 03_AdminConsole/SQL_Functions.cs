using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;


namespace LightLifeAdminConsole
{
    static class SQL_Functions
    {

        static private SqlConnection connection = new SqlConnection(@"Data Source=DAVID-VAIO\SQLEXPRESS;Initial Catalog=LightLife;Integrated Security=False;User ID=LightLife;Password=test;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False");

        static public void Refresh (ref DataTable _dataTable)
        {
            try
            {
                connection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM LLUser", connection);
                dataAdapter.Fill(_dataTable);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                connection.Close();
            }

        }
    }
}
