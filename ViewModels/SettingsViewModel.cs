using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using GMDCGiphyPlugin.Settings;

namespace GMDCGiphyPlugin.ViewModels
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private SettingsManager SettingsManager;

        public SettingsViewModel()
        {
            this.SettingsManager = SimpleIoc.Default.GetInstance<Settings.SettingsManager>();
        }

        public CopyBehaviorTypes CopyBehavior
        {
            get => SettingsManager.Settings.CopyBehavior;
            set
            {
                SettingsManager.Settings.CopyBehavior = value;
                this.RaisePropertyChanged(nameof(CopyBehavior));
                SettingsManager.SaveDatabase();
            }
        }

        public GIF_Control.GIFSizeType CopySizeBehavior
        {
            get => SettingsManager.Settings.CopySizeBehavior;
            set
            {
                SettingsManager.Settings.CopySizeBehavior = value;
                this.RaisePropertyChanged(nameof(CopySizeBehavior));
                SettingsManager.SaveDatabase();
            }
        }
        
    }
}
