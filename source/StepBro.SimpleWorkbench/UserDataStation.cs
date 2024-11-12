using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StepBro.SimpleWorkbench
{
    internal class UserDataStation
    {
        [JsonIgnore]
        internal bool IsEmpty
        {
            get
            {
                return (FavoriteFiles == null || FavoriteFiles.Count == 0) && (RecentFiles == null || RecentFiles.Count == 0);
            }
        }
        [JsonIgnore]
        public bool Updated { get; set; } = false;


        public int Version { get; set; } = 1;
        public List<string> FavoriteFiles { get; set; } = null;
        public List<string> RecentFiles { get; set; } = null;

        public void AddRecentFile(string file)
        {
            if (this.RecentFiles == null)
            {
                this.RecentFiles = new List<string>();
            }
            var index = this.RecentFiles.IndexOf(file);
            if (index == 0) return; // Note: If index == 0, don't do anything because its already fine.
            if (index > 0)  
            {
                this.RecentFiles.RemoveAt(index);
            }
            this.RecentFiles.Add(file);
            this.Updated = true;
        }
    }
}
