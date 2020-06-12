using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            Thread thread = new Thread(() =>
            {
                DispatcherHelper.Initialize();
                MainWindow mainWindow = new MainWindow();
                MainWindowViewModel vm = new MainWindowViewModel();
                mainWindow.DataContext = vm;

                mainWindow.Show();

                mainWindow.Closing += (s, e) =>
                {
                    mainWindow.Dispatcher.InvokeShutdown();
                    cleanup(cacheSession);
                };

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            Application.Current.MainWindow.Closing += (s, e) =>
            {
                thread.Abort();
            };

            return Task.CompletedTask;
        }
    }
}
