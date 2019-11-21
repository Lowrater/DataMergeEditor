using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Odbc;
using System.IO;

namespace DataMergeEditor.Model.Exports
{
    //-- Exportering af filer fra databasen
    //-- guide: Oracle - : https://docs.oracle.com/cd/E17952_01/connector-net-en/connector-net-programming-blob-reading.html 
    //-- https://bytes.com/topic/net/answers/818456-c-save-data-db-blob 
    public class DbBlobFile
    {
        //-- Oracle 
        public void ExportBlobOracle(string OracleCommand)
        {
            OdbcCommand Cmd = new OdbcCommand(OracleCommand);
            Cmd.CommandType = CommandType.Text;

            OdbcDataReader Reader = Cmd.ExecuteReader(CommandBehavior.CloseConnection);
            FileStream FStream = null;
            BinaryWriter BWriter = null;

            byte[] Binary = null;
            const int ChunkSize = 100;
            int SizeToWrite = 0;
            MemoryStream MStream = null;

            while (Reader.Read())
            {
                FStream = new FileStream(@"C:\DME\Nomenclature123.xls", FileMode.OpenOrCreate, FileAccess.Write);
                BWriter = new BinaryWriter(FStream);
                Binary = (Reader["blobdata"]) as byte[];
                SizeToWrite = ChunkSize;
                MStream = new MemoryStream(Binary);

                for (int i = 0; i < Binary.GetUpperBound(0) - 1; i = i + ChunkSize)
                {
                    if (i + ChunkSize >= Binary.Length) SizeToWrite = Binary.Length - i;
                    byte[] Chunk = new byte[SizeToWrite];
                    MStream.Read(Chunk, 0, SizeToWrite);
                    BWriter.Write(Chunk);
                    BWriter.Flush();
                }
                BWriter.Close();
                FStream.Close();
            }
            FStream.Dispose();
        }

        //-- Mysql 
        public void ExportBlobMySql(string MySqlcommand)
        {     
            MySqlCommand Cmd = new MySqlCommand(MySqlcommand);
            Cmd.CommandType = CommandType.Text;

            MySqlDataReader Reader = Cmd.ExecuteReader(CommandBehavior.CloseConnection);
            FileStream FStream = null;
            BinaryWriter BWriter = null;
            
             byte[] Binary = null;
             const int ChunkSize = 100;
             int SizeToWrite = 0;
            MemoryStream MStream = null;

            while (Reader.Read())
              {
                FStream = new FileStream(@"C:\DME\Nomenclature123.xls", FileMode.OpenOrCreate, FileAccess.Write);
                BWriter = new BinaryWriter(FStream);
                Binary = (Reader["blobdata"]) as byte[];
                SizeToWrite = ChunkSize;
                MStream = new MemoryStream(Binary);
                
                                 for (int i = 0; i < Binary.GetUpperBound(0) - 1; i = i + ChunkSize)
                                     {
                                         if (i + ChunkSize >= Binary.Length) SizeToWrite = Binary.Length - i;
                                         byte[] Chunk = new byte[SizeToWrite];
                    MStream.Read(Chunk, 0, SizeToWrite);
                    BWriter.Write(Chunk);
                    BWriter.Flush();
               }
                BWriter.Close();
                FStream.Close();
                             }
            FStream.Dispose();
        }
    }
}
