using System.Collections.Generic;
using System.Linq;

namespace AAI_Repo.Models
{
    class InstallItemList
    {
        /// <summary>
        /// 各リポジトリ
        /// </summary>
        private static List<InstallItem> _installItemList = new List<InstallItem>();

        /// <summary>
        /// インストールアイテム一覧を取得
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <returns></returns>
        public static InstallItem[] GetInstalItemList()
        {
            return _installItemList.ToArray();
        }

        /// <summary>
        /// インストールアイテムを追加
        /// </summary>
        /// <param name="repoType">リポジトリの選択</param>
        /// <param name="item"></param>
        public static void AddInstallItem(InstallItem item)
        {
            _installItemList.Add(item);
        }

        public static void SortMakerName()
        {
            var orderedByMakerName = new List<InstallItem>(_installItemList.OrderByDescending(x => x.MakerName));
            _installItemList = orderedByMakerName;
        }

        public static InstallItem[] GetItemListFromMakerName(string makerName)
        {
            var itemList = _installItemList.Where(x => { return x.MakerName == makerName; });

            return itemList.ToArray();
        }
    }
}
