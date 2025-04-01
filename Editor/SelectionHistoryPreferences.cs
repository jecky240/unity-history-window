using UnityEngine;
using UnityEditor;

namespace Gemserk {
    public static class SelectionHistoryPreferences {

        static bool prefsLoaded = false;

       // static int historySize;

        static bool autoremoveDestroyed;
        static bool autoremoveUnloaded;

        static bool autoRemoveDuplicated;

        private static bool drawFavorites = true;

        private static bool orderLastSelectedFirst = true;

        private static bool backgroundRecord;

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Selection History", SettingsScope.User) {
                label = "Selection History",
                guiHandler = (searchContext) =>
                {
                    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
                    
                    if (!prefsLoaded) {
                       // historySize = EditorPrefs.GetInt(SelectionHistoryWindowUtils.HistorySizePrefKey, defaultHistorySize);
                        autoremoveDestroyed = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDestroyedPrefKey, true);
                        autoremoveUnloaded = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveUnloadedPrefKey, true);
                        autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, false);
                        drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, true);
                        orderLastSelectedFirst = EditorPrefs.GetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, false);
                        backgroundRecord = EditorPrefs.GetBool(SelectionHistoryWindowUtils.BackgroundRecordKey, false);
                        prefsLoaded = true;
                    }
                    
                    if (selectionHistory != null)
                    {
                        selectionHistory.historySize =
                            EditorGUILayout.IntField("历史记录数量(0为无限制)", selectionHistory.historySize);
                    }
                    
                    autoremoveDestroyed = EditorGUILayout.Toggle("自动移除已销毁的元素", autoremoveDestroyed);
                    autoremoveUnloaded = EditorGUILayout.Toggle("自动移除已卸载的元素", autoremoveUnloaded);
                    autoRemoveDuplicated = EditorGUILayout.Toggle("允许重复条目", autoRemoveDuplicated);
                    drawFavorites = EditorGUILayout.Toggle("显示收藏夹按钮", drawFavorites);
                    orderLastSelectedFirst = EditorGUILayout.Toggle("倒序选择条目", orderLastSelectedFirst);
                    backgroundRecord = EditorGUILayout.Toggle("关闭时保存记录", backgroundRecord);

                    if (GUI.changed) {
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDestroyedPrefKey, autoremoveDestroyed);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveUnloadedPrefKey, autoremoveUnloaded);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, drawFavorites);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, orderLastSelectedFirst);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.BackgroundRecordKey, backgroundRecord);

                        // var window = EditorWindow.GetWindow<SelectionHistoryWindow>();
                        // if (window != null)
                        // {
                        //     window.ReloadRootAndRemoveUnloadedAndDuplicated();
                        // }

                        SelectionHistoryAsset.instance.ForceSave();
                    }
                },

            };
            return provider;
        }
    }
}
