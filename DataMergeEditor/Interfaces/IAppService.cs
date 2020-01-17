namespace DataMergeEditor.Services
{
    public interface IAppService
    {
        string transfer_cannot_transfer_to_main_list_msg { get; }
        string transfer_dupliced_file_names_msg { get; }
        string transfer_listExistInSelectedTable_msg { get; }
        string transfer_valid_table_needed_msg { get; }
        string transfer_verify_connection_msg { get; }
        string transfering_Message_header { get; }
    }
}