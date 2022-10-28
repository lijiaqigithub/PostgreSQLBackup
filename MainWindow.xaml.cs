using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;

namespace PostgreSQLBackup
{
    public partial class MainWindow : Window
    {
        private string xmlName = "settings";
        private string directoryPath = string.Empty;
        private string exePath = string.Empty;

        public static string pgDumpPath,
                             backupFolderPath,
                             host,
                             dbname,
                             port,
                             user,
                             password;

        public List<string> names;

        public MainWindow()
        {
            InitializeComponent();
        }

        void onLoad(object sender, RoutedEventArgs e)
        {
            exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

            directoryPath = exePath.Substring(0, exePath.LastIndexOf('\\'));

            if (File.Exists(directoryPath + "\\" + xmlName + ".xml"))
            {
                Thread t = new Thread(new ThreadStart(AppThread));
                t.Start();
            }
            else
            {
                MessageBox.Show("Fisierul settings.xml nu exista!");
                return;
            }
        }

        private void AppThread()
        {
            TaskScheduler_Task();
            ReadData();
            StartBackup();
        }

        private List<string> GetAllDatabaseNames(PgDump pgObject)
        {
            var nameArray = new List<string>();


            var cs = "Host=" + pgObject.host + ";Username=" + pgObject.username + ";Password=" + pgObject.password + ";Database=" + pgObject.dbname;

            var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT datname FROM pg_database WHERE datistemplate = false;";

            var cmd = new NpgsqlCommand(sql, con);

            var names = cmd.ExecuteReader();

            while (names.Read())
            {
                nameArray.Add(names[0].ToString());
            }


            return nameArray;
        }

        private void ReadData()
        {
            var reader = XmlReader.Create(directoryPath + "\\" + xmlName + ".xml");
            while (reader.Read())
            {
                if (reader.Name == "pgDumpPath")
                {
                    pgDumpPath = reader.ReadElementContentAsString();
                }
                if (reader.Name == "backupFolderPath")
                {
                    backupFolderPath = reader.ReadElementContentAsString();
                }
                if (reader.Name == "dbName")
                {
                    dbname = reader.ReadElementContentAsString();
                }
                if (reader.Name == "host")
                {
                    host = reader.ReadElementContentAsString();
                }
                if (reader.Name == "port")
                {
                    port = reader.ReadElementContentAsString();
                }
                if (reader.Name == "user")
                {
                    user = reader.ReadElementContentAsString();
                }
                if (reader.Name == "password")
                {
                    password = reader.ReadElementContentAsString();
                }

            }

            reader.Dispose();
        }

        private void StartBackup()
        {
            PgDump pgObject = new PgDump();

            pgObject.pgDumpPath = pgDumpPath;
            pgObject.password = password;
            pgObject.host = host;
            pgObject.port = port;
            pgObject.username = user;

            if (string.IsNullOrEmpty(backupFolderPath))
            {
                backupFolderPath = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) + "Users\\" + Environment.UserName + "\\Documents\\Backups";
            }

            pgObject.path = backupFolderPath;


            names = GetAllDatabaseNames(pgObject);
            foreach (var name in names)
            {
                string pathString = backupFolderPath + "\\" + name;
                if (!Directory.Exists(pathString))
                {
                    Directory.CreateDirectory(pathString);
                }

                pgObject.dbname = name;
                pgObject.path = backupFolderPath + "\\" + name + "\\" + name + "_backup_" + System.DateTime.Now.Day + "_" + System.DateTime.Now.Month +
                                "_" + System.DateTime.Now.Year + "_" + System.DateTime.Now.Hour + "_" + System.DateTime.Now.Minute;

                PostgreSqlDump(pgObject);
            }

            EndProcess();
        }

        public void PostgreSqlDump(PgDump pgObj)
        {
            string dumpCommand = "\"" + pgObj.pgDumpPath + "\"" + " -Fc" + " -h " + pgObj.host + " -p " + pgObj.port + " -d " + pgObj.dbname + " -U " + pgObj.username + "";
            string passFileContent = "" + pgObj.host + ":" + pgObj.port + ":" + pgObj.dbname + ":" + pgObj.username + ":" + pgObj.password + "";

            string batFilePath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".bat");

            string passFilePath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".conf");

            try
            {
                string batchContent = "";
                batchContent += "@" + "set PGPASSFILE=" + passFilePath + "\n";
                batchContent += "@" + dumpCommand + "  > " + "\"" + pgObj.path + "\"" + "\n";

                File.WriteAllText(
                    batFilePath,
                    batchContent,
                    Encoding.ASCII);

                File.WriteAllText(
                    passFilePath,
                    passFileContent,
                    Encoding.ASCII);

                if (File.Exists(pgObj.path))
                    File.Delete(pgObj.path);

                ProcessStartInfo oInfo = new ProcessStartInfo(batFilePath);
                oInfo.UseShellExecute = false;
                oInfo.CreateNoWindow = true;

                using (Process proc = System.Diagnostics.Process.Start(oInfo))
                {
                    proc.WaitForExit();
                    proc.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la crearea fisierului pentru db: " + pgObj.dbname + " --error message:" + ex.Message);
            }
            finally
            {
                if (File.Exists(batFilePath))
                    File.Delete(batFilePath);

                if (File.Exists(passFilePath))
                    File.Delete(passFilePath);
            }
        }

        public static string CMDWithResponse(string Comanda, string compiler = "cmd")
        {
            string CMDcuRSP = "";
            try
            {
                Process myprocess = new Process();
                myprocess.StartInfo.FileName = compiler;
                myprocess.StartInfo.RedirectStandardInput = true;
                myprocess.StartInfo.RedirectStandardOutput = true;
                myprocess.StartInfo.UseShellExecute = false;
                myprocess.StartInfo.CreateNoWindow = true;
                myprocess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myprocess.Start();
                System.IO.StreamReader SR = myprocess.StandardOutput;
                System.IO.StreamWriter SW = myprocess.StandardInput;
                SW.WriteLine(Comanda);
                SW.WriteLine("exit" + (char)(13));
                CMDcuRSP = (SR.ReadToEnd());
                SW.Close();
                SR.Close();
                myprocess.Dispose();
            }
            catch (Exception ex)
            {
                CMDcuRSP = "ERROR:" + ex.Message;
            }
            return CMDcuRSP;
        }

        public void TaskScheduler_Task()
        {
            string result = CMDWithResponse("schtasks /query");
            if (!result.Contains("BackupPostgre"))
            {
                CMDWithResponse(@"schtasks /create /sc DAILY /tn BackupPostgres /tr """ + exePath + "");
            }
        }
        public void EndProcess()
        {
            Environment.Exit(0);
        }
    }
}

