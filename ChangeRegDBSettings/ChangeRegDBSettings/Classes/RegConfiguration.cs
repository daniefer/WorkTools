using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeRegDBSettings
{
    class RegConfiguration
    {
        public string ServerName { get; set; }
        public string DBName { get; set; }
        public string LETS_DBName { get; set; }
        public string LogsDBName { get; set; }
        public string ConfigurationName { get; set; }
        public Guid id { get; set; }

        public RegConfiguration(string servername, string dbname, string letsdbname, string logsdbname, string configname)
        {
            ServerName = servername;
            DBName = dbname;
            LETS_DBName = letsdbname;
            LogsDBName = logsdbname;
            ConfigurationName = configname;
            id = Guid.NewGuid();
        }
    }
}
