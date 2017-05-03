using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChangeRegDBSettings.Properties;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace ChangeRegDBSettings
{
    public partial class Form1 : Form
    {
        private BindingList<RegConfiguration> _settingList;
        private BindingList<string> _dblist2;
        private BindingList<string> _dblist3;
        private BindingList<string> _dblist4;
        private readonly BackgroundWorker _bwOnLeave = new BackgroundWorker();
        private readonly BackgroundWorker _bwOnLoad = new BackgroundWorker();
        private delegate int AsyncCall(List<string> x);


        /// <summary>
        ///     Form Constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            StupidProofTool();
            _bwOnLeave.WorkerSupportsCancellation = true;
            _bwOnLeave.DoWork += new DoWorkEventHandler(ConnectLoadAsync);
            _bwOnLeave.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ConnectLoadAsyncCompleted);
            _bwOnLoad.WorkerSupportsCancellation = true;
            _bwOnLoad.DoWork += new DoWorkEventHandler(ConnectLoadAsync);
            _bwOnLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ConnectLoadAsyncCompletedWithIndexSet);
            LoadSettings();
            var treeItems = BuildTreeView();
            if (treeItems != null)
            {
                PopulateTreeView(treeView1, treeItems);
            }
            textBox6.AppendText("Settings are saved in " + System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
        }

        private void StupidProofTool()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.RegPaths))
            {
                label8.BackColor = Color.Yellow;
            }
            else
            {
                label1.BackColor = Color.Transparent;
            }
        }

        /// <summary>
        ///     Builds Tree View for current Registry folders
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<TreeItem> BuildTreeView()
        {
            try
            {
                var ls = new List<TreeItem>();
                var regPaths =
                    Settings.Default.RegPaths.Split(";".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                var r64Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                var r32Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey rKeySubKey;
                // Gets Registry entries

                foreach (var item in regPaths)
                {
                    rKeySubKey = r64Key.OpenSubKey(item) ?? r32Key.OpenSubKey(item);
                    
                    if (rKeySubKey == null) continue;
                    
                    ls.Add(new TreeItem(item, 0));
                    ls.Add(new TreeItem((string) rKeySubKey.GetValue("SqlServer"), 1));
                    ls.Add(new TreeItem((string) rKeySubKey.GetValue("SQLDatabase"), 1));
                    ls.Add(new TreeItem((string) rKeySubKey.GetValue("SQLDatabaseLets"), 1));
                    ls.Add(new TreeItem((string) rKeySubKey.GetValue("SqlDatabaseLogs"), 1));
                }
                return ls;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///     Populates the Tree in the view
        /// </summary>
        /// <param name="tv">TreeView object to be populated</param>
        /// <param name="items">The List Built with the BuildTreeView function</param>
        /// <returns></returns>
        private static int PopulateTreeView(TreeView tv, IEnumerable<TreeItem> items)
        {
            try
            {
                tv.Nodes.Clear();
                var roots = new List<TreeNode>();
                foreach (var item in items)
                {
                    if (roots.Count == 0)
                    {
                        roots.Add(tv.Nodes.Add(item.Name));
                    }
                    else
                    {
                        if (item.Level == 0)
                        {
                            roots.Add(tv.Nodes.Add(item.Name));
                        }
                        else
                        {
                            if (item.Level == roots.Count + 1) roots.Add(roots[roots.Count].LastNode);
                            roots.Last().Nodes.Add(item.Name);
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        /// <summary>
        ///     Updates the Registry Entries that are saved in Settings.
        /// </summary>
        /// <param name="serverName">Name of Server</param>
        /// <param name="dbName">Name of content database</param>
        /// <param name="letsDbName">Name of LETS database</param>
        /// <param name="logsDbName">Name of Logs database</param>
        /// <returns></returns>
        private static int UpdateReg(string serverName, string dbName, string letsDbName, string logsDbName)
        {
            try
            {
                var regPaths = Settings.Default.RegPaths.Split(";".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                var r64Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey r64KeySubKey;

                // Gets 64 bit Registry;
                foreach (var item in regPaths)
                {
                    r64KeySubKey = r64Key.OpenSubKey(item, true);
                    
                    if (r64KeySubKey == null) continue;

                    r64KeySubKey.SetValue("SqlServer", serverName, RegistryValueKind.String);
                    r64KeySubKey.SetValue("SQLDatabase", dbName, RegistryValueKind.String);
                    r64KeySubKey.SetValue("SQLDatabaseLets", letsDbName, RegistryValueKind.String);
                    r64KeySubKey.SetValue("SqlDatabaseLogs", logsDbName, RegistryValueKind.String);
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        /// <summary>
        ///     Loads the initail settings into the application.
        /// </summary>
        /// <returns></returns>
        private int LoadSettings()
        {
            try
            {
                if (Properties.Settings.Default.Configurations == null) Properties.Settings.Default.Configurations =  new StringCollection();
                _settingList = new BindingList<RegConfiguration>();

                foreach (var item in Settings.Default.Configurations)
                {
                    var tmp = item.Split(";".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    _settingList.Add(new RegConfiguration(tmp[0], tmp[1], tmp[2], tmp[3], tmp[4]));
                }


                comboBox1.DataSource = _settingList;
                comboBox1.DisplayMember = "ConfigurationName";

                //_dblist2 = new BindingList<string>();
                //_dblist3 = new BindingList<string>();
                //_dblist4 = new BindingList<string>();
                //comboBox2.DataSource = _dblist2;
                //comboBox3.DataSource = _dblist3;
                //comboBox4.DataSource = _dblist4;

                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        /// <summary>
        ///     Saves a new configuration to user Settings
        /// </summary>
        /// <returns></returns>
        private int SaveConfigurations()
        {
            try
            {
                Settings.Default["Configurations"] = new StringCollection();
                foreach (RegConfiguration item in _settingList)
                {
                    Settings.Default.Configurations.Add(item.ServerName + ";" + item.DBName + ";" + item.LETS_DBName +
                                                        ";" + item.LogsDBName + ";" + item.ConfigurationName);
                }
                Settings.Default.Save();
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        /// <summary>
        ///     Save Button Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            _settingList.Add(new RegConfiguration(textBox1.Text.Trim(), comboBox2.Text.Trim(), comboBox3.Text.Trim(),
                comboBox4.Text.Trim(), textBox5.Text.Trim()));
            SaveConfigurations();
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
        }

        /// <summary>
        ///     Change Registry Button Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            int saveSuccess;
            var valuesNotBlank = textBox1.Text.Trim() != String.Empty &&
                                  comboBox2.Text.Trim() != String.Empty &&
                                  comboBox3.Text.Trim() != String.Empty &&
                                  comboBox4.Text.Trim() != String.Empty;
            if (valuesNotBlank)
            {
                saveSuccess = UpdateReg(textBox1.Text.Trim(), comboBox2.Text.Trim(), comboBox3.Text.Trim(),
                    comboBox4.Text.Trim());
            }
            else
            {
                MessageBox.Show(this, @"All Registry Configuration fields must have values.", @"Error on change!", MessageBoxButtons.OK);
                return;
            }

            if (saveSuccess == 0)
            {
                MessageBox.Show(this, @"Values changed successfully.", @"Save Result", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show(this, @"Values not saved. Error during registry update.", @"Error During Save!",
                    MessageBoxButtons.OK);
            }

            var treeItems = BuildTreeView();
            if (treeItems != null)
            {
                PopulateTreeView(treeView1, treeItems);
            }
        }

        /// <summary>
        ///     Refreshes the tree view Button Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            IEnumerable<TreeItem> treeItems = BuildTreeView();
            if (treeItems != null)
            {
                PopulateTreeView(treeView1, treeItems);
            }
        }

        /// <summary>
        ///     Form Closing Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;

            e.Cancel = true;
            this.Hide();
        }

        /// <summary>
        ///     Deletes a configuration from the combobox and user settings. Button Click event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            var dr = new DialogResult();
            dr = MessageBox.Show(this, @"Are you sure you want to delete this configuration?", @"Attention!",
                MessageBoxButtons.YesNo);

            if (dr != DialogResult.Yes) return;

            _settingList.Remove((RegConfiguration) comboBox1.SelectedItem);
            textBox1.Text = string.Empty;
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            comboBox4.SelectedIndex = -1;
            textBox5.Text = string.Empty;
            comboBox1.SelectedIndex = -1;
            SaveConfigurations();
        }

        /// <summary>
        ///     ComboBox Selection Index Change Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox) sender).SelectedIndex == -1) return;

            var selected = (RegConfiguration) ((ComboBox) sender).SelectedItem;
            ;
            if (_dblist2 == null || _dblist3 == null || _dblist4 == null || textBox1.Text != selected.ServerName)
            {
                _bwOnLoad.RunWorkerAsync(selected.ServerName);
            }
            else if (comboBox2.Text != selected.DBName || comboBox3.Text != selected.LETS_DBName || comboBox4.Text != selected.LogsDBName)
            {
                comboBox2.SelectedIndex = _dblist2 != null ? _dblist2.IndexOf(selected.DBName) : -1;
                comboBox3.SelectedIndex = _dblist3 != null ? _dblist3.IndexOf(selected.LETS_DBName) : -1;
                comboBox4.SelectedIndex = _dblist4 != null ? _dblist4.IndexOf(selected.LogsDBName) : -1;
            }
            textBox1.Text = selected.ServerName;
            textBox5.Text = selected.ConfigurationName;
        }

        /// <summary>
        ///     File>Configure Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var config = new ConfigWindow();
            config.Show();
        }

        /// <summary>
        /// Opens window when doubleclicking on the notifyicon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Focus();
        }

        /// <summary>
        /// Exits application 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dr = new DialogResult();
            dr = MessageBox.Show(this, @"Are you sure you want to close?", @"Confirm", MessageBoxButtons.YesNo);

            if (dr == DialogResult.No) return;

            notifyIcon1 = null;
            Application.Exit();
        }

        /// <summary>
        /// Catches the notifyicon right click event. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_Click(object sender, System.EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show();
            }
        }

        /// <summary>
        /// Opens the configuration window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var config = new ConfigWindow();
            config.Show();
        }

        /// <summary>
        /// Exits the Application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var dr = new DialogResult();
            dr = MessageBox.Show(this, @"Are you sure you want to close?", @"Confirm", MessageBoxButtons.YesNo);

            if (dr == DialogResult.No) return;

            Application.Exit();
        }

        /// <summary>
        /// Shows the main window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Focus();
        }

        /// <summary>
        /// Catches the minimization event and removes the dock icon since the notify icon will still exist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (RemoveAllConnectionStrings() != 0) MessageBox.Show(@"Error. Unable to remove Connection Strings.");
        }

        private int RemoveAllConnectionStrings()
        {
            try
            {
                textBox6.AppendText("*****************************************************\r\n");
                textBox6.AppendText("*           Deleting connection strings...          *\r\n");
                textBox6.AppendText("*****************************************************\r\n");
                string output;
                int backupCounter;
                foreach (var file in Properties.Settings.Default.ConfigFiles)
                {
                    try
                    {
                        backupCounter = 0;
                        var streamRead = new System.IO.StreamReader(file);
                        var filestring = streamRead.ReadToEnd();
                        streamRead.Close();
                        streamRead.Dispose();

                        while (true)
                        {
                            if (!File.Exists(file + ".back" + backupCounter))
                            {
                                var streamWrite = new System.IO.StreamWriter(file + ".back" + backupCounter);
                                streamWrite.WriteLine(filestring);
                                streamWrite.Close();
                                streamWrite.Dispose();
                                break;
                            }
                            backupCounter ++;
                        }
                        

                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(filestring)))
                        {
                            var xml = new XmlTextReader((Stream)stream);

                            var xmlDoc = new XmlDocument();
                            xmlDoc.Load(xml);
                            var xmlNode = xmlDoc.SelectSingleNode("configuration/connectionStrings/add");


                            if (xmlNode != null &&
                                xmlNode.Attributes != null &&
                                xmlNode.Attributes["connectionString"] != null)
                            {
                                xmlNode.Attributes["connectionString"].Value = "";
                            }
                            xmlDoc.Save(file);
                        }

                        output = file + " : Successful\r\n";
                    }
                    catch (Exception)
                    {
                        output = file + " : Failed\r\n";
                    }
                    textBox6.AppendText(output);

                }

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// Deletes all .back files on button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            textBox6.AppendText("*****************************************************\r\n");
            textBox6.AppendText("*              Deleting .back files...              *\r\n");
            textBox6.AppendText("*****************************************************\r\n");
            var DeleteCount = 0;
            var output = new StringBuilder();
            foreach (var file in Properties.Settings.Default.ConfigFiles)
            {
                var dir = Directory.GetParent(file);
                var files = Directory.GetFiles(dir.FullName, "*.config.back*");
                foreach (var back in files)
                {
                    try
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(back, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        output.Append(back + " : Removed.\r\n");
                        DeleteCount ++;
                    }
                    catch (Exception)
                    {
                        output.Append(back + " : Error Occured.\r\n");
                    }
                    textBox6.AppendText(output.ToString());
                    output.Clear();
                }
            }
            textBox6.AppendText(@"Files Removed : " + DeleteCount + "\r\n");
        }

        /// <summary>
        /// Loads the DB list from the provided server name and populates the three DB ComboBoxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  void textBox1_Leave(object sender, EventArgs e)
        {
            if (!_bwOnLeave.IsBusy)
            {
                _bwOnLeave.RunWorkerAsync(textBox1.Text);
            }
        }

        private void ConnectLoadAsync(object sender, DoWorkEventArgs e)
        {
            var dblist = Connect_Load((string)e.Argument);

            e.Result = dblist;
        }

        private void ConnectLoadAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                var wrapper = new DispatchWrapper(textBox6);
                ((TextBox)wrapper.WrappedObject).AppendText(e.Error.Message + "\r\n");
            }
            else if(e.Result == null)
            {
                var wrapper = new DispatchWrapper(textBox6);
                ((TextBox)wrapper.WrappedObject).AppendText(@"Error. Unable to bind DB list to ComboBoxes." + "\r\n");
            }
            else if (((List<string>)e.Result).Count == 1)
            {
                textBox6.AppendText(((List<string>)e.Result)[0] + "\r\n");
            }
            else
            {
                var ans = BindDbCombos((List<string>)e.Result);
                comboBox2.SelectedIndex = -1;
                comboBox3.SelectedIndex = -1;
                comboBox4.SelectedIndex = -1;
            }
        }

        private void ConnectLoadAsyncCompletedWithIndexSet(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                textBox6.AppendText(e.Error.Message + "\r\n");
            }
            else if (e.Result == null)
            {
                textBox6.AppendText(@"Error. Unable to bind DB list to ComboBoxes." + "\r\n");
            }
            else if (((List<string>)e.Result).Count == 1)
            {
                textBox6.AppendText(((List<string>)e.Result)[0] + "\r\n");
            }
            else
            {
                var ans = BindDbCombos((List<string>)e.Result);
                var selected = (RegConfiguration)comboBox1.SelectedItem;
                if (selected != null)
                {
                    comboBox2.SelectedIndex = _dblist2 != null ? _dblist2.IndexOf(selected.DBName) : -1;
                    comboBox3.SelectedIndex = _dblist3 != null ? _dblist3.IndexOf(selected.LETS_DBName) : -1;
                    comboBox4.SelectedIndex = _dblist4 != null ? _dblist4.IndexOf(selected.LogsDBName) : -1;
                }
                else
                {
                    comboBox1.SelectedIndex = 0;
                }
                
            }
        }

        /// <summary>
        /// Connects to the SQL instance specified and brings back a list of all DB names
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="CallBack"></param>
        /// <returns></returns>
        private List<string> Connect_Load(string serverName)
        {
            try
            {
                var connectionBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = serverName,
                    UserID = "pssqluser",
                    Password = "police"
                };
                var ds = new DataSet();
                using (var con = new SqlConnection(connectionBuilder.ToString()))
                using (var cmd = new SqlDataAdapter("SELECT Name FROM master.sys.databases", con))
                {
                    cmd.Fill(ds);
                    //var tmp = ds.Tables[0].Rows.Cast<DataRow>().Select(x => x["Name"].ToString());//.Select(x => x["Name"].ToString());
                }

                return ds.Tables[0].Rows.Cast<DataRow>().Select(x => x["Name"].ToString()).ToList();
            }
            catch (SqlException ex)
            {
                return new List<string>() {ex.Message};
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Binds a list to the three database comboBoxes
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private int BindDbCombos(List<string> list)
        {
            try
            {
                if (list != null && (_dblist2 == null || !list.OrderBy(i => i).SequenceEqual(_dblist2.ToList().OrderBy(i => i))))
                {
                    _dblist2 = new BindingList<string>(list);
                    _dblist3 = new BindingList<string>(list);
                    _dblist4 = new BindingList<string>(list);

                    comboBox2.DataSource = _dblist2;
                    comboBox3.DataSource = _dblist3;
                    comboBox4.DataSource = _dblist4;
                }
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

    }
}