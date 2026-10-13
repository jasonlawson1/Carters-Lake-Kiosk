using System.Configuration;
using System.Data;
using System.Windows;
using Capstone_UI.Services;

namespace Capstone_UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DatabaseService Db { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Db = new DatabaseService();
        }
    }

}
