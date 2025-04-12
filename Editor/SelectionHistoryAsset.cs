using UnityEditor;
using UnityEngine;

namespace Gemserk
{
    [FilePath("Logs/SelectionHistoryRecord.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SelectionHistoryAsset : ScriptableSingleton<SelectionHistoryAsset>
    {
        [SerializeField]
        public SelectionHistory selectionHistory = new SelectionHistory();
        
        private void OnEnable()
        {
            if (selectionHistory != null)
            {
                selectionHistory.OnNewPrefabAdded += OnNewEntryAdded;
                selectionHistory.OnNewEntryAdded += OnNewEntryAdded;
            }
        }
        
        private void OnDisable()
        {
            if (selectionHistory != null)
            {
                selectionHistory.OnNewPrefabAdded -= OnNewEntryAdded;
                selectionHistory.OnNewEntryAdded -= OnNewEntryAdded;
            }
        }

        private void OnNewEntryAdded(SelectionHistory obj)
        {
            // EditorUtility.SetDirty(this);
            Save(true);
            // Debug.Log("Saved to: " + GetFilePath());
        }

        public void ForceSave()
        {
            Save(true);
        }
    }
}