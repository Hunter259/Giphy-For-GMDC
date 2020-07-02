using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using GMDCGiphyPlugin.ViewModel;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;

namespace GMDCGiphyPlugin
{
    public class PluginEntry : PluginBase, IGroupChatPlugin
    {
        public string PluginName => this.PluginDisplayName;

        public override string PluginDisplayName => "Gighy Plugin For GMDC";

        public override string PluginVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override Version ApiVersion => new Version(2, 0, 0);

        public Task Activated(IMessageContainer groupOrChat, CacheSession cacheSession, IPluginUIIntegration integration, Action<CacheSession> cleanup)
        {
            MainWindow mainWindow = new MainWindow();
            MainWindowViewModel vm = new MainWindowViewModel();
            mainWindow.DataContext = vm;

            mainWindow.Show();

            mainWindow.Closing += (s, e) =>
            {
                cleanup(cacheSession);
                vm.Cleanup();
            };

            System.Windows.Threading.Dispatcher.Run();

            return Task.CompletedTask;
        }
    }
}
