
namespace LightLifeAdminConsole
{
    class AdminVM //: NotifyPropertyChanged
    {
        //Singleton
        private static AdminVM _instance;

        private AdminVM()
        {           
        }

        public static AdminVM GetInstance()
        {
            if (_instance == null)
                _instance = new AdminVM();

            return _instance;
        }
    }
}
