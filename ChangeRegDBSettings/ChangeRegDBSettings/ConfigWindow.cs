using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChangeRegDBSettings
{
    public partial class ConfigWindow : Form
    {
        /// <summary>
        /// Public Variables for Binding
        /// </summary>
        private BindingList<RegPath> _regPaths;
        public BindingList<RegPath> RegPaths { get { return _regPaths; } set { _regPaths = value; } }
        private BindingList<Label> _filePaths;
        public BindingList<Label> FilePaths { get { return _filePaths; } set { _filePaths = value; } } 

        /// <summary>
        /// Constructor for ConfigWindow
        /// </summary>
        public ConfigWindow()
        {
            if (Properties.Settings.Default.ConfigFiles == null) Properties.Settings.Default.ConfigFiles = new StringCollection();
            InitializeComponent();
            LoadRegPathSettings();
            LoadConfigPathSettings();
            
            if (listBox1.Items.Count == 0)
            {
                button2.Enabled = false;
            }
            
        }

        /// <summary>
        /// Loads the Registry Paths from user Settings into the listbox
        /// </summary>
        /// <returns></returns>
        private int LoadRegPathSettings()
        {
            try
            {
                List<string> tmp;
                _regPaths = new BindingList<RegPath>();
                tmp = Properties.Settings.Default.RegPaths.Split(";".ToArray(),StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                foreach (string item in tmp)
                {
                    _regPaths.Add(new RegPath(item));
                }
                
                listBox1.DataSource = _regPaths;
                listBox1.DisplayMember = "Path";
                listBox1.ValueMember = null;

                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        private int LoadConfigPathSettings()
        {
            try
            {
                textBox3.Text = @"C:\Program Files\tylertechnologies\";
                _filePaths =  new BindingList<Label>();
                foreach (var item in Properties.Settings.Default.ConfigFiles)
                {
                    FilePaths.Add(File.Exists(item)
                        ? new Label() { Text = item }
                        : new Label() { Text = item, BackColor = Color.Orange });
                }
                listBox2.DataSource = FilePaths;
                listBox2.DisplayMember = "Text";
                
                listBox2.ValueMember = null;
                return 0;
            }
            catch (Exception)
            {
                return 1;
                throw;
            }
        }

        /// <summary>
        /// Saves a new Path to the listbox. Save Button Click Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (AddRegistryPath(textBox1.Text.Trim()) != 0)
            {
                MessageBox.Show(@"Path not saved. An error occured.");
            }

            if (button2.Enabled == false)
            {
                button2.Enabled = true;
            }
            textBox1.Text = string.Empty;
        }

        /// <summary>
        /// Does the work of adding a new registery path to Settings
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private int AddRegistryPath(string path)
        {
            try
            {
                _regPaths.Clear();
                var tmp = Properties.Settings.Default.RegPaths.Split(";".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                foreach (var item in tmp)
                {
                    _regPaths.Add(new RegPath(item));
                }
                _regPaths.Add(new RegPath(path));
                Properties.Settings.Default["RegPaths"] = ((string)Properties.Settings.Default["RegPaths"]) + ";" + path;
                Properties.Settings.Default.Save();
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        /// <summary>
        /// Removes the selected registry path from the listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            _regPaths.Remove((RegPath)listBox1.SelectedItem);
            var sb = new StringBuilder();
            foreach (RegPath item in _regPaths)
            {
                sb.Append(item.Path).Append(";");
            }

            Properties.Settings.Default["RegPaths"] = sb.ToString();
            Properties.Settings.Default.Save();

            if (_regPaths.Count ==0)
            {
                button2.Enabled = false;
            }
        }

        /// <summary>
        /// Opens the FileDialog box so the user can select which files should be saved to the configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog() { Multiselect = true };
            var filesToAddList = new List<string>();
            
            textBox2.Text = string.Empty;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text =  String.Join(@";", dialog.FileNames);
            }
        }

        /// <summary>
        /// Adds Config File paths in the inputString to User Settings and to the listbox2.items collection
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private int AddConfigFiles(string inputString)
        {
            try
            {
                foreach (var fileString in inputString.Split(@";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    Properties.Settings.Default.ConfigFiles.Add(fileString);
                    FilePaths.Add(File.Exists(fileString)
                        ? new Label() {Text = fileString}
                        : new Label() {Text = fileString, BackColor = Color.Orange});
                }
                Properties.Settings.Default.Save();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }   
        }

        /// <summary>
        /// Adds the items in textbox2 to the Settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            if (AddConfigFiles(textBox2.Text) != 0)
            {
                MessageBox.Show(@"An error occured while saving. Unable to add selected path(s) to settings.");
            }
            if (FilePaths.Count(itm => itm.BackColor == Color.Orange) > 0)
            {
                MessageBox.Show(@"Warning. Some file paths could not be verified. Please ensure the file exists before running tool.");
            }
        }

        /// <summary>
        /// Removes the selected Config File Path items from Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (RemoveSelectedConfigFiles((Label) listBox2.SelectedItem) != 0)
            {
                MessageBox.Show(@"Error. Unable to remove selected item.");
            }
        }

        /// <summary>
        /// Removes the selectedItem from the listbox2 and the content of the Label from Settings.
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns></returns>
        private int RemoveSelectedConfigFiles(Label selectedItem)
        {
            try
            {
                Properties.Settings.Default.ConfigFiles.Remove(selectedItem.Text);
                FilePaths.Remove(selectedItem);
                Properties.Settings.Default.Save();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets all .config files from the Dir listed in textbox3 recursively
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            var rootpath = textBox3.Text;
            GetFilesRecursive(rootpath);
        }

        /// <summary>
        /// Recursive function for getting .config files from a provided dir path
        /// </summary>
        /// <param name="rootpath"></param>
        /// <returns></returns>
        private int GetFilesRecursive(string rootpath)
        {
            try
            {
                //rootpath may not be a file so only use these two cases
                if (Directory.Exists(rootpath))
                {
                    AddConfigFiles(String.Join(";", Directory.GetFiles(rootpath).Where(str => str.EndsWith(".config"))));
                    foreach (var path in Directory.GetDirectories(rootpath))
                    {
                        GetFilesRecursive(path);
                    }
                }
                else if (File.Exists(rootpath))
                {
                    AddConfigFiles(String.Join(";", Directory.GetFiles(rootpath).Where(str => str.EndsWith(".config"))));
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
