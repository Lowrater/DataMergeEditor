using DataMergeEditor.Model;
using DataMergeEditor.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DataMergeEditor.ViewModel
{
    //-- relay command = eks:
    //-- RelayCommand<int>(o => SaveScript(o));   - Her har brugeren indtastet en værdi, som kan benyttes.
    //-- RelayCommand(SaveScript) - er også gyldigt til at kalde metoder

    //-- Delegate command =
    //-- VIewModelBase frem for Inotifypropertychanged

    //-- Pagining - Vist x antal rækker, og bladre igennem
    //-- https://www.codeproject.com/Articles/20463/DataView-Paging-in-WPF
    //-- https://social.msdn.microsoft.com/Forums/vstudio/en-US/18c1a6b9-7873-4673-bd20-3046fa20f0be/how-to-write-click-event-for-label-in-wpf-not-win-form?forum=wpf 

    public class NewScriptNoteViewModel : ViewModelBase
    {
        public ICommand SaveScriptCommand => new RelayCommand(SaveScript);
        public ICommand OpenScriptsCommand => new RelayCommand(OpenSQLFile);
        public ICommand RunThisScriptCommand => new RelayCommand(ExecuteScript);
        private string CurrentDBName { get; set; }

        //-- Dataservice for at dele data mellem viewmodels
        private readonly IDataService dataservice;
        public NewScriptNoteViewModel(IDataService dataService)
        {
            this.dataservice = dataService;
        }

        /// <summary>
        /// Køre scripts
        /// </summary>
        public void ExecuteScript()
        {
            //-- Modtager database string fra NewQueryTabItem --- PULL
            MessengerInstance.Send<NotificationMessageAction<string>>(
                                     new NotificationMessageAction<string>
                                     ("", ActiveConnection => CurrentDBName = ActiveConnection)
                                      );

            if (!string.IsNullOrWhiteSpace(CurrentDBName) && CurrentDBName.ToLower() != "none")
            {
                dataservice.ConnectionList.FirstOrDefault(x => x.Key == CurrentDBName).Value.UnknownReadCreateToDatabaes(ScriptContentString);
                TableAddons.writeLogFile($"{ScriptContentString} executed from the ScriptNote", dataservice.LogLocation);
            }
            else
            {
                MessageBox.Show($"{ConfigurationManager.AppSettings["New_script_note_verify_connection_msg"]}",
                                $"{ConfigurationManager.AppSettings["New_script_note_execute_header"]}",
                             MessageBoxButton.OK,
                             MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// saves the content
        /// </summary>
        public void SaveScript()
        {
            //-- Open dialog
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "SQL(SQL(*.sql)|*.sql"
            };

            if (sfd.ShowDialog() == true)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(sfd.FileName)))
                {
                    outputFile.WriteLine(ScriptContentString);
                }
            }
        }

        /// <summary>
        /// åbner sql filer i nye text felter
        /// </summary>
        public void OpenSQLFile()
        {
            OpenFileDialog file = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "SQL(SQL(*.sql)|*.sql"
            };

            if (file.ShowDialog() == true)
            {
                if (file.FileName.Contains(".sql"))
                {
                    FileInfo sqlfil = new FileInfo(file.FileName);
                    string scriptet = sqlfil.OpenText().ReadToEnd();
                    TableAddons.writeLogFile($"{file.FileName} sql script has been opened in the ScriptNote", dataservice.LogLocation);

                    ScriptContentString = scriptet;
                    sqlfil.OpenText().Close();
                }
                else
                {
                    MessageBox.Show($"{ConfigurationManager.AppSettings["script_only_sql_files_msg"]}",
                                    $"{ConfigurationManager.AppSettings["New_script_note_open_script_header"]}",
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Warning);
                }
            }
        }

        private string _scriptContentString;
        //-- Sætter værdien for query feltet  
        //-- Lambda
        public string ScriptContentString
        {
            get => _scriptContentString;
            set => Set(ref _scriptContentString, value);
        }
    }
}
