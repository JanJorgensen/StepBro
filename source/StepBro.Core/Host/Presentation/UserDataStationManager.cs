using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StepBro.Core.Host.Presentation
{
    public static class UserDataStationManager
    {
        private static string m_userFileStationPath = null;
        private static UserDataStation m_userDataStation = new UserDataStation() { Changed = true };   // Note: New object is created if loading existing settings.

        public static string UserFileStationPath
        {
            get
            {
                if (m_userFileStationPath == null)
                {
                    m_userFileStationPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StepBro", "StepBro.Workbench.user.json");
                }
                return m_userFileStationPath;
            }
        }

        public static void SaveUserSettingsOnStation(bool force = false)
        {
            if (force || (!m_userDataStation.IsEmpty && m_userDataStation.Changed))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

                var folder = Path.GetDirectoryName(UserFileStationPath);
                if (!Directory.Exists(folder))
                {
                    var root = Path.GetDirectoryName(folder);
                    if (!Directory.Exists(root))
                    {
                        return; // Something wrong!!
                    }
                    Directory.CreateDirectory(folder);
                }

                using (FileStream createStream = System.IO.File.Create(UserFileStationPath))
                {
                    JsonSerializer.Serialize(createStream, m_userDataStation, options);
                    m_userDataStation.Changed = false;
                }
            }
        }

        public static void LoadUserSettingsOnStation()
        {
            if (System.IO.File.Exists(UserFileStationPath))
            {
                m_userDataStation = JsonSerializer.Deserialize<UserDataStation>(System.IO.File.ReadAllText(UserFileStationPath));
            }
        }

        private static void AddFileToList(ref List<string> list, string file, bool moveToTop)
        {
            if (list == null)
            {
                list = new List<string>();
            }
            var index = list.IndexOf(file);
            if (index == 0) return; // Note: If index == 0, don't do anything, because its already fine.
            if (moveToTop || index < 0)
            {
                if (index > 0)
                {
                    list.RemoveAt(index);
                }
                list.Insert(0, file);
                m_userDataStation.Changed = true;
            }
        }

        private static void RemoveFileFromList(ref List<string> list, string file)
        {
            if (list == null) return;
            var index = list.IndexOf(file);
            if (index < 0) return;
            list.RemoveAt(index);
            m_userDataStation.Changed = true;
        }

        public static void AddRecentFile(string file)
        {
            AddFileToList(ref m_userDataStation.m_recentFiles, file, moveToTop: true);
        }

        public static void RemoveRecentFile(string file)
        {
            RemoveFileFromList(ref m_userDataStation.m_recentFiles, file);
        }

        public static void AddFavoriteFile(string file)
        {
            AddFileToList(ref m_userDataStation.m_favoriteFiles, file, moveToTop: false);
        }

        public static void RemoveFavoriteFile(string file)
        {
            RemoveFileFromList(ref m_userDataStation.m_favoriteFiles, file);
        }

        public static IEnumerable<string> ListRecentFiles()
        {
            if (m_userDataStation == null || m_userDataStation.RecentFiles == null || m_userDataStation.RecentFiles.Count == 0) yield break;
            foreach (var file in m_userDataStation.RecentFiles) yield return file;
        }

        public static IEnumerable<string> ListFavoriteFiles()
        {
            if (m_userDataStation == null || m_userDataStation.FavoriteFiles == null || m_userDataStation.FavoriteFiles.Count == 0) yield break;
            foreach (var file in m_userDataStation.FavoriteFiles) yield return file;
        }
    }
}
