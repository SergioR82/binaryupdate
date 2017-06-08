using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.IO.Compression;

namespace BinaryMigration
{
    class Program
    {

        const string query = "SELECT VERSION, BINARY FROM BINPACKAGES";
	const string fileName = "binary.exe";
        const string binpackage = "binary.zip";

        public static void InsertFileintoSqlDatabase(SqlConnection sqlconnection)
        {
            string filePath = (ConfigurationManager.AppSettings["sourcePath"] + binpackage);

                // Converts text file(.txt) into byte[]
                byte[] fileData = File.ReadAllBytes(filePath);

                SqlCommand insertCommand = new SqlCommand(@"DELETE FROM BINPACKAGES", sqlconnection);
                insertCommand.ExecuteNonQuery();

                string insertQuery = @"INSERT INTO BINPACKAGES (VERSION,BINARY) VALUES(1,@FileData)";

                // Insert text file Value into Sql Table by SqlParameter
                insertCommand = new SqlCommand(insertQuery, sqlconnection);
                SqlParameter sqlParam = insertCommand.Parameters.AddWithValue("@FileData", fileData);
                sqlParam.DbType = DbType.Binary;
                insertCommand.ExecuteNonQuery();
        }

        public static void ExportFileFromSqlDatabase(byte[] zip)
        {
            var PrgmPath = ConfigurationManager.AppSettings["newPath"].ToString();

                // Write/Export File content into new text file
                File.WriteAllBytes(PrgmPath + binpackage, zip);               
                ZipFile.ExtractToDirectory(PrgmPath + binpackage, PrgmPath);
        }


        static void Main(string[] args)
        {
            string newFile, destFile;
            
            string newPath = ConfigurationManager.AppSettings["newPath"];
            string PrgmPath = ConfigurationManager.AppSettings["PrgmPath"];

            newFile = System.IO.Path.Combine(newPath, fileName);
            destFile = System.IO.Path.Combine(PrgmPath, fileName);
            
            AssemblyName currentAssemblyName = AssemblyName.GetAssemblyName(destFile);
            AssemblyName updatedAssemblyName = AssemblyName.GetAssemblyName(newFile);

            Console.WriteLine(currentAssemblyName.Version);
            Console.WriteLine(updatedAssemblyName.Version);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            if (updatedAssemblyName.Version.CompareTo(currentAssemblyName.Version) > 0)
            {
                // To copy a folder's contents to a new location:
                // Create a new target folder, if necessary.
                if (!System.IO.Directory.Exists(PrgmPath))
                    System.IO.Directory.CreateDirectory(PrgmPath);

                if (System.IO.Directory.Exists(newPath))
                {
                    string[] files = System.IO.Directory.GetFiles(newPath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        fileName = System.IO.Path.GetFileName(s);
                        destFile = System.IO.Path.Combine(PrgmPath, fileName);
                        System.IO.File.Copy(s, destFile, true);
                    }
                }
                else
                    Console.WriteLine("Source path does not exist!");
                
            }
            Process.Start(ConfigurationManager.AppSettings["PrgmPath"] + fileName);
        }
    }
}
