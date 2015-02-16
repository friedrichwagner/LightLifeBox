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
using System.Data;



namespace LightLifeAdminConsole.Content
{
    /// <summary>
    /// Interaktionslogik für Database_info.xaml
    /// </summary>
    //  public enum E_Gender {Male, Female};

    public class Proband
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Birthday { get; set; }
        public string Remark { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RoleID { get; set; }
        public string added { get; set; }
    }

    public partial class Database_info : UserControl
    {

        public Database_info()
        {
            InitializeComponent();
            ObservableCollection<Proband> passdata = GetData();

            //Bind the DataGrid to the Proband data
            DG1.DataContext = passdata;
        }


        private ObservableCollection<Proband> GetData()
        {
            DataTable dataTable = new DataTable();
            var Probands = new ObservableCollection<Proband>();

           SQL_Functions.Refresh(ref dataTable);

            Probands.Add(new Proband { FirstName = "Peter", LastName = "Huber", Gender = "Male", Birthday = "12.12.1986", Remark = "" });
            Probands.Add(new Proband { FirstName = "Hans", LastName = "Günther", Gender = "Male", Birthday = "13.05.1999", Remark = "" });

            try 
            {
                Probands.Add(new Proband { FirstName = dataTable.Rows[0][1].ToString(),
                                           LastName = dataTable.Rows[0][2].ToString(),
                                           Gender = dataTable.Rows[0][4].ToString(),
                                           Birthday = dataTable.Rows[0][3].ToString(),
                                           Password = dataTable.Rows[0][7].ToString(),
                                           UserName = dataTable.Rows[0][6].ToString()});
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
            return Probands;
        }



    }
}
