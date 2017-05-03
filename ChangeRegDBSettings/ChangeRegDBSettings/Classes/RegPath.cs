using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeRegDBSettings
{
    /// <summary>
    /// User to bind Registry paths to the ConfigWindow listbox
    /// </summary>
    public class RegPath
    {
        public string Path { get; set; }

        public RegPath(string value1)
        {
            Path = value1;
        }
    }
}
