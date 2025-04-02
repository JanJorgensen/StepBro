using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StepBro.Core.Host.Presentation
{
    internal class UserDataStation
    {
        [JsonIgnore]
        internal bool IsEmpty
        {
            get
            {
                return (m_recentFiles == null || m_recentFiles.Count == 0) && (m_favoriteFiles == null || m_favoriteFiles.Count == 0);
            }
        }
        [JsonIgnore]
        public bool Changed { get; set; } = false;

        public int Version { get; set; } = 1;

        [JsonIgnore]
        internal List<string> m_recentFiles = null;
        public List<string> RecentFiles
        {
            get { return m_recentFiles; }
            set { m_recentFiles = value; }
        }
        [JsonIgnore]
        internal List<string> m_favoriteFiles = null;
        public List<string> FavoriteFiles
        {
            get { return m_favoriteFiles; }
            set { m_favoriteFiles = value; }
        }
    }
}
