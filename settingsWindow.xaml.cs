using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;


namespace PostgreSQLBackup
{
    /// <summary>
    /// Interaction logic for settingsWindow.xaml
    /// </summary>
    public partial class settingsWindow : Window
    {
        public static string lastDirectory;
        private string xmlName = "settings";



        public settingsWindow()
        {
            InitializeComponent();
        }

        void onLoad(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void pgdumpFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                lastDirectory = openFileDialog.FileName;
                pgdumpFilePathButton.Background = Brushes.LightGray;
            } else
            {
                MessageBox.Show("FILEPATH CAN'T BE EMPTY");
            }
        }   

        private void LoadSettings()
        {
            lastDirectory = MainWindow.pgDumpPath;
            pgdumpFilePathButton.ToolTip = lastDirectory;
            dbnamesTextbox.Text = MainWindow.dbnames;
            hostTextbox.Text = MainWindow.host;
            portTextbox.Text = MainWindow.port;
            userTextbox.Text = MainWindow.user;
            passwordTextbox.Text = MainWindow.password;
        }

        private void WriteXML(string xmlName)
        {
            using (XmlWriter writer = XmlWriter.Create(xmlName+ ".xml"))
            {
                writer.WriteStartElement("settings");
                writer.WriteElementString("pgDumpPath", lastDirectory);
                writer.WriteElementString("dbNames", dbnamesTextbox.Text);
                writer.WriteElementString("host", hostTextbox.Text);
                writer.WriteElementString("port", portTextbox.Text);
                writer.WriteElementString("user", userTextbox.Text);
                writer.WriteElementString("password", passwordTextbox.Text);
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if(errorDesign())
            {
                return;
            }

            WriteXML(xmlName);
            this.Close();
        }

        #region DESIGN
        private void hostTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            hostTextbox.BorderBrush = Brushes.Gray;
            hostTextbox.BorderThickness = new Thickness(1.0);
        }

        private void portTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            portTextbox.BorderBrush = Brushes.Gray;
            portTextbox.BorderThickness = new Thickness(1.0);
        }

        private void userTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            userTextbox.BorderBrush = Brushes.Gray;
            userTextbox.BorderThickness = new Thickness(1.0);
        }

        private void passwordTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            passwordTextbox.BorderBrush = Brushes.Gray;
            passwordTextbox.BorderThickness = new Thickness(1.0);
        }

        private void dbnamesTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            dbnamesTextbox.BorderBrush = Brushes.Gray;
            dbnamesTextbox.BorderThickness = new Thickness(1.0);
        }

        private bool errorDesign()
        {
            bool notOk = false;

            if (string.IsNullOrEmpty(lastDirectory))
            {
                pgdumpFilePathButton.Background = Brushes.Red;
                notOk = true;
            }
            if (string.IsNullOrEmpty(hostTextbox.Text))
            {
                hostTextbox.BorderBrush = Brushes.Red;
                hostTextbox.BorderThickness = new Thickness(2.0);
                notOk = true;
            }
            if (string.IsNullOrEmpty(portTextbox.Text))
            {
                portTextbox.BorderBrush = Brushes.Red;
                portTextbox.BorderThickness = new Thickness(2.0);
                notOk = true;
            }
            if (string.IsNullOrEmpty(userTextbox.Text))
            {
                userTextbox.BorderBrush = Brushes.Red;
                userTextbox.BorderThickness = new Thickness(2.0);
                notOk = true;
            }
            if (string.IsNullOrEmpty(passwordTextbox.Text))
            {
                passwordTextbox.BorderBrush = Brushes.Red;
                passwordTextbox.BorderThickness = new Thickness(2.0);
                notOk = true;
            }
            if (string.IsNullOrEmpty(dbnamesTextbox.Text))
            {
                dbnamesTextbox.BorderBrush = Brushes.Red;
                dbnamesTextbox.BorderThickness = new Thickness(2.0);
                notOk = true;
            }

            return notOk;
        }

        #endregion
    }
}
