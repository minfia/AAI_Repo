using System.Data.SQLite;

namespace AAI_Repo.Models
{
    class PreRepoFileR
    {
        private static readonly string _installTableName = "InstallList";
        private static readonly string _makerTableName = "MakerName";
        private static SQLiteConnection connection;

        public PreRepoFileR(string databaseFilePath)
        {
            SQLiteConnectionStringBuilder connectionSB = new SQLiteConnectionStringBuilder() { DataSource = databaseFilePath };
            connection = new SQLiteConnection(connectionSB.ToString());
        }

        public void Open()
        {
            connection.Open();
        }

        public void Close()
        {
            connection.Close();
            connection = null;
        }

        /// <summary>
        /// インストールリストを読み出す
        /// </summary>
        /// <returns>成否</returns>
        public bool ReadInstallItemList()
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = $"select * from {_installTableName} natural join {_makerTableName}";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InstallItem item = new InstallItem
                            {
                                ItemName = reader["name"].ToString(),
                                MakerName = reader["maker_name"].ToString(),
                                URL = reader["url"].ToString(),
                                Version = reader["version"].ToString(),
                            };
                            InstallItemList.AddInstallItem(item);
                        }
                    }
                }

                InstallItemList.SortMakerName();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
