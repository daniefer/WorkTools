using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeRegDBSettings
{
    /// <summary>
    /// Used to build list for TreeView 
    /// </summary>
    class TreeItem
    {
        public string Name;
        public int Level;

        public TreeItem(string name, int level)
        {
            Name = name;
            Level = level;
        }
    }
}
