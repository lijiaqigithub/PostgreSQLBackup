﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;

namespace PostgreSQLBackup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private string xmlName = "settings";

        public string backupFilePath = "";

        public static string pgDumpPath,
                             host,
                             dbname,
                             port,
                             user,
                             password;

        public MainWindow()
        {
            InitializeComponent();
        }

        void onLoad(object sender, RoutedEventArgs e)
        {
            if(File.Exists(xmlName + ".xml"))
                ReadData();
        }

        private void backupButton_Click(object sender, RoutedEventArgs e)
        {
            backupButton.IsEnabled = false;

            PgDump pgObject = new PgDump();

            pgObject.pgDumpPath = pgDumpPath;
            pgObject.password = password;
            pgObject.host = host;
            pgObject.port = port;
            pgObject.username = user;
            pgObject.dbname = dbname;

            if(string.IsNullOrEmpty(backupFileNameTextbox.Text))
            {
                MessageBox.Show("Alege numele fisierului backup");
                backupButton.IsEnabled = true;
                return;
            } else
            {
                if (string.IsNullOrEmpty(backupFilePath))
                {
                    pgObject.path = backupFileNameTextbox.Text;
                }
                else
                {
                    pgObject.path = backupFilePath + "\\" + backupFileNameTextbox.Text;
                }
            }

            Thread thread = new Thread(() => PostgreSqlDump(pgObject));
            thread.Start();
        }

        private void openFileExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                backupFilePath = dialog.FileName;
            } else
            {
                MessageBox.Show("Alege locatie de salvare (Default: /bin)");
            }
        }
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow settingsWindow = new settingsWindow();
            this.Hide();
            settingsWindow.ShowDialog();
            if (File.Exists(xmlName + ".xml"))
                ReadData();
            this.Show();
        }

        private void ReadData()
        {
            var reader = XmlReader.Create(xmlName + ".xml");

            reader.ReadToFollowing("pgDumpPath");
            pgDumpPath = reader.ReadElementContentAsString();
            dbname = reader.ReadElementContentAsString();
            host = reader.ReadElementContentAsString();
            port = reader.ReadElementContentAsString();
            user = reader.ReadElementContentAsString();
            password = reader.ReadElementContentAsString();

            reader.Dispose();
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
                System.Windows.MessageBox.Show("Eroare la crearea fisierului: " + ex.Message);
            }
            finally
            {
                if (File.Exists(batFilePath))
                    File.Delete(batFilePath);

                if (File.Exists(passFilePath))
                    File.Delete(passFilePath);

                Dispatcher.Invoke(new Action(() => 
                {
                    backupButton.IsEnabled = true;
                    MessageBox.Show("Fisier salvat cu succes!");
                }));

            }
        }

    }
}
        