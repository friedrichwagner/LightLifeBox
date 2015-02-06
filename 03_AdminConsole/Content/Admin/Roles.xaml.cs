using System;
using System.Collections.Generic;
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
using LightLife;

namespace LightLifeAdminConsole.Content.Admin
{
    /// <summary>
    /// Interaction logic for Roles.xaml
    /// </summary>
    public partial class Roles : UserControl
    {
        private AdminBase ab;
        private AdminVM dc;

        public Roles()
        {
            InitializeComponent();

            LLSQL.InitSQLs();
            dc = AdminVM.GetInstance();

            ab = new AdminBase(dc.sqlCon,LLSQL.LLRole);

            //dgRoles.DataContext = ab.select();

        }
    }
}
