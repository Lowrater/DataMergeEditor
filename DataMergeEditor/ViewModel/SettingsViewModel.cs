using DataMergeEditor.Model;
using GalaSoft.MvvmLight;
using DataMergeEditor.Services;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.Windows;
using System.Configuration;

namespace DataMergeEditor.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsWindowModel SettingsWindowModel = new SettingsWindowModel();
        public ICommand SetNewLogPathCommand => new RelayCommand(SetNewLogPath);
        public ICommand ResetCommand => new RelayCommand(ResetSettings);

        public IDataService dataservice;
        public SettingsViewModel(IDataService dataservice)
        {
            this.dataservice = dataservice;
            AskOnInsert = true;
            AskOnDrop = true;
            AskOnAcceptChanges = true;
            DefaultTheme = true;
            RowLimit = 100;
            LogFile = @"C:\DME\log.txt";
        }

        /// <summary>
        /// resetter alle settings til default
        /// </summary>
        public void ResetSettings()
        {
            AskOnInsert = true;
            AskOnDrop = true;
            AskOnAcceptChanges = true;
            DarkTheme = false;
            DefaultTheme = true;
            RowLimit = 100;
            LogFile = @"C:\DME\log.txt";
            //-- Message
            MessageBox.Show($"{ConfigurationManager.AppSettings["Settings_all_settings_reset_message"]}",
                            $"{ConfigurationManager.AppSettings["Settings_reset_header"]}",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        /// <summary>
        /// Metode til at sætte Log filen / stien
        /// </summary>
        public void SetNewLogPath()
        {
            OpenFileDialog file = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Text Files(*.txt)|*.txt;|Text Files(*.txt)|*.txt"
            };

            if (file.ShowDialog() == true)
            {
                LogFile = file.FileName;
            }
        }

        /// <summary>
        /// Row limiter der bestemmer hvor mange rækker man vil se
        /// </summary>
        public int RowLimit
        {
            get => SettingsWindowModel._rowlimit;
            set
            {
                Set(ref SettingsWindowModel._rowlimit, value);
                dataservice.RowLimiter = value;
                TableAddons.writeLogFile($"Row limiter has been changed to {value}", dataservice.LogLocation);
            }
        }


        /// <summary>
        /// Placering for logs, som kan skiftes af brugeren
        /// Sender værdien, hvis den rettes fra default
        /// sendes til TableAddons, som kaldes via. Dataservice, på alle klasser
        /// </summary>
        public string LogFile
        {
            get => SettingsWindowModel._logFileLocation;
            set
            {
                Set(ref SettingsWindowModel._logFileLocation, value);
                TableAddons.writeLogFile($"Log file has changed to  {value}", dataservice.LogLocation);
                dataservice.LogLocation = value;
            }
        }

        /// <summary>
        /// Til at bekræfte om de vil have popup beskeder ved insert statements
        /// </summary>
        public bool AskOnInsert
        {
            get => SettingsWindowModel._askOnInsert;
            set
            {
                Set(ref SettingsWindowModel._askOnInsert, value);
                dataservice.AskOnInsert = value;
                TableAddons.writeLogFile($"The insert warning has been changed to {value}", dataservice.LogLocation);
            }
        }

        /// <summary>
        /// Til at bekræfte om de vil have popup beskeder ved drop/delete statements
        /// </summary>
        public bool AskOnDrop
        {
            get => SettingsWindowModel._askOnDrop;
            set
            {
                Set(ref SettingsWindowModel._askOnDrop, value);
                dataservice.AskOnDrop = value;
                TableAddons.writeLogFile($"The drop security warning has been changed to {value}", dataservice.LogLocation);
            }
        }

        /// <summary>
        /// Til at bekræfte om de vil have popup beskeder ved at lave akut 'apply changes' mod databasen efter redigeret felter
        /// </summary>
        public bool AskOnAcceptChanges
        {
            get => SettingsWindowModel._askOnAcceptChanges;
            set
            {
                Set(ref SettingsWindowModel._askOnAcceptChanges, value);
                dataservice.AskOnAcceptChanges = value;
                TableAddons.writeLogFile($"The Accept changes security warning has been changed to {value}",
                    dataservice.LogLocation);
            }
        }


        /// <summary>
        /// Standard Tema
        /// </summary>
        public bool DefaultTheme
        {
            get => SettingsWindowModel._defaultTheme;
            set
            {
                if(!DarkTheme.Equals(true))
                {
                    Set(ref SettingsWindowModel._defaultTheme, value);
                }
                else
                {
                    MessageBox.Show($"{ConfigurationManager.AppSettings["Settings_theme_limit_message"]}",
                        $"{ConfigurationManager.AppSettings["Settings_theme_message_header"]}",
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
        }


        /// <summary>
        /// Dark Tema
        /// </summary>
        public bool DarkTheme
        {
            get => SettingsWindowModel._darkTheme;
            set
            {
                if (!DefaultTheme.Equals(true))
                {
                    Set(ref SettingsWindowModel._darkTheme, value);
                }
                else
                {
                    MessageBox.Show($"{ConfigurationManager.AppSettings["Settings_theme_limit_message"]}",
                        $"{ConfigurationManager.AppSettings["Settings_theme_message_header"]}",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
    }
}

