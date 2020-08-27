using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using GMDCGiphyPlugin.Views;
using GMDCGiphyPlugin.ViewModels;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using GalaSoft.MvvmLight.Ioc;

namespace GMDCGiphyPlugin
{
    public class PluginEntry : PluginBase, IGroupChatPlugin
    {
        public string PluginName => this.PluginDisplayName;

        public override string PluginDisplayName => "Giphy Plugin For GMDC";

        public override string PluginVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override Version ApiVersion => new Version(2, 0, 0);

        public Task Activated(IMessageContainer groupOrChat, CacheSession cacheSession, IPluginUIIntegration integration, Action<CacheSession> cleanup)
        {
            string DataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MicroCube", "GroupMe Desktop Client");
            var SettingsManager = new Settings.SettingsManager(Path.Combine(DataRoot, "GiphyPluginSettings.json"));

            if (!SimpleIoc.Default.IsRegistered<Settings.SettingsManager>())
            {
                SimpleIoc.Default.Register(() => SettingsManager);
            }

            MainWindowView mainWindow = new MainWindowView();
            MainWindowViewModel vm = new MainWindowViewModel();
            mainWindow.DataContext = vm;

            mainWindow.Show();

            mainWindow.Closing += (s, e) =>
            {
                cleanup(cacheSession);
                vm.Cleanup();
            };

            Application.Current.MainWindow.Closing += (s, e) =>
            {
                mainWindow.Close();
            };

            return Task.CompletedTask;
        }
    }
}
