using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;

namespace PostgreSQLBackup
{
    public partial class MainWindow : Window
    {
        private string xmlName = "settings";

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
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Substring(6);

            this.Visibility = Visibility.Collapsed;
            if (File.Exists(path + "\\" + xmlName + ".xml"))
            {
                Thread t = new Thread(new ThreadStart(AppThread));
                t.Start();
            }
            else
            {
                MessageBox.Show("Fisierul Setari.xml nu exista!");
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
            var cs = "Host=" + pgObject.host + ";Username=" + pgObject.username +";Password="+ pgObject.password +";Database=" +pgObject.dbname;

            var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT datname FROM pg_database WHERE datistemplate = false;";

            var cmd = new NpgsqlCommand(sql, con);

            var names = cmd.ExecuteReader();
            var nameArray = new List<string>();
            while(names.Read())
            {
                nameArray.Add(names[0].ToString());
            }
            
            return nameArray;
        }

        private void ReadData()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Substring(6);

            var reader = XmlReader.Create(path + "\\" + xmlName + ".xml");

            reader.ReadToFollowing("pgDumpPath");
            pgDumpPath = reader.ReadElementContentAsString();
            reader.ReadToFollowing("backupFolderPath");
            backupFolderPath = reader.ReadElementContentAsString();
            reader.ReadToFollowing("dbName");
            dbname = reader.ReadElementContentAsString();
            reader.ReadToFollowing("host");
            host = reader.ReadElementContentAsString();
            reader.ReadToFollowing("port");
            port = reader.ReadElementContentAsString();
            reader.ReadToFollowing("user");
            user = reader.ReadElementContentAsString();
            reader.ReadToFollowing("password");
            password = reader.ReadElementContentAsString();
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

            if(string.IsNullOrEmpty(backupFolderPath))
            {
                backupFolderPath = "C:\\Users\\" + Environment.UserName + "\\Documents\\Backups";
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
                pgObject.path = backupFolderPath + "\\"+ name +"\\" + name + "_backup_" + System.DateTime.Now.Day + "_" + System.DateTime.Now.Month +
                                "_" + System.DateTime.Now.Year + "_" + System.DateTime.Now.Hour + "_" + System.DateTime.Now.Minute;

                PostgreSqlDump(pgObject);
            }
        }

        public void PostgreSqlDump(PgDump pgObj)
        {
            String dumpCommand = "\"" + pgObj.pgDumpPath + "\"" + " -Fc" + " -h " + pgObj.host + " -p " + pgObj.port + " -d " + pgObj.dbname + " -U " + pgObj.username + "";
            String passFileContent = "" + pgObj.host + ":" + pgObj.port + ":" + pgObj.dbname + ":" + pgObj.username + ":" + pgObj.password + "";

            String batFilePath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".bat");

            String passFilePath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".conf");

            try
            {
                String batchContent = "";
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
            catch(Exception ex)
            {
                MessageBox.Show("Eroare la crearea fisierului: " + ex.Message);
            }
            finally
            {
                if (File.Exists(batFilePath))
                    File.Delete(batFilePath);

                if (File.Exists(passFilePath))
                    File.Delete(passFilePath);


                var name = names.LastOrDefault();
                if (pgObj.dbname == name)
                {
                    EndProcess("PostgreSQLBackup");
                }
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
            if (!result.Contains("'BackupPostgre'"))
            {
                CMDWithResponse(@"schtasks /create /sc hourly /tn BackupPostgres /tr """ + System.Reflection.Assembly.GetEntryAssembly().Location+ " argument=nodesign\"");
            }
        }
        public void EndProcess(string process)
        {
            Process[] p = Process.GetProcessesByName(process);
            if (p.Length >= 1)
            {
                Environment.Exit(0);
            }
        }
    }
}
        
