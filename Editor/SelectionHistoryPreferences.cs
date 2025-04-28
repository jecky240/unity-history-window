using UnityEngine;
using UnityEditor;

namespace Gemserk {
    public static class SelectionHistoryPreferences {

        static bool prefsLoaded = false;

       // static int historySize;

        private static bool autoremoveDestroyed;
        private static bool autoremoveUnloaded;

        private static bool autoRemoveDuplicated;

        private static bool onlyRecordPrefabAndSprite;

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
                        onlyRecordPrefabAndSprite = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryOnlyRecordPrefabAndSpritePrefKey, true);
                        orderLastSelectedFirst = EditorPrefs.GetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, false);
                        backgroundRecord = EditorPrefs.GetBool(SelectionHistoryWindowUtils.BackgroundRecordKey, true);
                        prefsLoaded = true;
                    }
                    
                    // if (selectionHistory != null)
                    // {
                    //     selectionHistory.historySize =
                    //         EditorGUILayout.IntField("历史记录数量(0为无限制)", selectionHistory.historySize);
                    // }
                    
                    // autoremoveDestroyed = EditorGUILayout.Toggle("自动移除已销毁的元素", autoremoveDestroyed);
                    // autoremoveUnloaded = EditorGUILayout.Toggle("自动移除已卸载的元素", autoremoveUnloaded);
                    // autoRemoveDuplicated = EditorGUILayout.Toggle("允许重复条目", autoRemoveDuplicated);
                    onlyRecordPrefabAndSprite = EditorGUILayout.Toggle("   只记录预制体和精灵", onlyRecordPrefabAndSprite);
                    // orderLastSelectedFirst = EditorGUILayout.Toggle("从列表头部塞入记录", orderLastSelectedFirst);
                    // backgroundRecord = EditorGUILayout.Toggle("面板关闭时仍能记录", backgroundRecord);

                    if (GUI.changed) {
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDestroyedPrefKey, autoremoveDestroyed);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveUnloadedPrefKey, autoremoveUnloaded);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryOnlyRecordPrefabAndSpritePrefKey, onlyRecordPrefabAndSprite);
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
