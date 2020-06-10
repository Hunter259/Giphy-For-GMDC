using System;
using System.Reflection;
using System.Threading.Tasks;
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
            MainWindow mainWindow = new MainWindow(); // application entry point
            MainWindowViewModel vm = new MainWindowViewModel();
            mainWindow.DataContext = vm;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.Show();
            });

            mainWindow.Closing += (s, e) =>
            {
                cleanup(cacheSession);
            };

            return Task.CompletedTask;
        }
    }
}
