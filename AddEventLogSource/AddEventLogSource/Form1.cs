using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddEventLogSource
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox2.AppendText(string.Format("Adding Event Log sources listed above.{0}", Environment.NewLine));
            var eventSources = this.textBox1.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string eventSource in eventSources)
            {
                if (!EventLog.SourceExists(eventSource))
                {
                    try
                    {
                        EventLog.CreateEventSource(eventSource, "Application");
                        this.textBox2.AppendText(string.Format("Added Event Log source '{0}'.{1}", eventSource, Environment.NewLine));
                    }
                    catch (Exception ex)
                    {
                        this.textBox2.AppendText(string.Format("An error occured while trying to add the Event Log source {0}. See Exception:{1}{2}{1}", eventSource, Environment.NewLine, ex));
                    }
                }
                else
                {
                    this.textBox2.AppendText(string.Format("Event Log source '{0}' already exists. Skipping. {1}", eventSource, Environment.NewLine));
                }
            }
            this.textBox2.AppendText(string.Format("Completed adding Event Log sources at {0}.{1}", DateTime.Now.ToString("G"), Environment.NewLine));
            this.textBox2.AppendText(string.Format("-------------------------------------{0}", Environment.NewLine));
        }
    }
}
