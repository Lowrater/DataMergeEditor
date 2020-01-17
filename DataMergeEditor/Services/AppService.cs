using System.Configuration;

namespace DataMergeEditor.Services
{
    /// <summary>
    /// App service class that contains that needs to contain the values from from App.config
    /// ongoing process.
    /// article - best practice: https://blog.submain.com/app-config-basics-best-practices/
    /// </summary>
    public class AppService : IAppService
    {
        //-- Transfering messages header
        public string transfering_Message_header => ConfigurationManager.AppSettings["Transfering_message_header"];
        //-- Transfering messages
        public string transfer_listExistInSelectedTable_msg => ConfigurationManager.AppSettings["Table_transfering_list_exist_msg"];
        public string transfer_valid_table_needed_msg => ConfigurationManager.AppSettings["Table_transfering_valid_table_needed"];
        public string transfer_cannot_transfer_to_main_list_msg => ConfigurationManager.AppSettings["Table_transfering_cannot_transfer_to_main_list"];
        public string transfer_verify_connection_msg => ConfigurationManager.AppSettings["Connection_message_verify_connection"];
        public string transfer_dupliced_file_names_msg => ConfigurationManager.AppSettings["Table_transfering_duplicated_file_names"];

    }
}
