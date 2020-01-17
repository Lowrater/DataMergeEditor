using DataMergeEditor.Services;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataMergeEditor.ViewModel.Log
{
    public class LogViewModel : ViewModelBase
    {
        public IDataService dataservice;
        public LogViewModel(IDataService dataservice)
        {
            this.dataservice = dataservice;
            file_location = this.dataservice.LogLocation;
        }
        private string _stringofLog;
        public string StringOfLog
        {
            get => _stringofLog;
            set => Set(ref _stringofLog, value);
        }
        private string _file_location;
        public string file_location
        {
            get => _file_location;
            set
            {
                Set(ref _file_location, value);

                using (var sr = new StreamReader(value))
                {
                    StringOfLog = sr.ReadToEnd();
                    sr.Close();
                }
            }
        }
        private string _searchedfortableword;
        /// <summary>
        /// Ord som bruges til søgemaskinen for at fremsøge specifik tabel
        /// </summary>
        public string SearchedForTableWord
        {
            get => _searchedfortableword;
            set
            {
                Set(ref _searchedfortableword, value);

                //-- Giver Items listen med alle noderne ud, til at sætte på ny.
                FindTableByMatchedWord(value);
            }
        }
        /// <summary>
        /// Til at sortere loggen vist til brugeren
        /// </summary>
        public void FindTableByMatchedWord(string text)
        {
            var logList = new List<string>();
            string line;
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
            {
                using (var sr = new StreamReader(file_location))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if(line.ToLower().Contains(text))
                        {
                            logList.Add(line);
                        }
                    }
                    sr.Close();
                }
                StringOfLog = string.Join(Environment.NewLine, logList.ToArray());
            }
            else
            {
                StringOfLog = new StreamReader(file_location).ReadToEnd();
            }
        }
    }
}
