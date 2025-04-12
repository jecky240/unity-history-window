using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Gemserk
{
	[InitializeOnLoad]
	public static class SelectionHistoryWindowUtils {

		public static readonly string HistoryAutomaticRemoveDestroyedPrefKey = "SelectionHistoryRecord.AutomaticRemoveDeleted";
		public static readonly string HistoryAutomaticRemoveUnloadedPrefKey = "SelectionHistoryRecord.AutomaticRemoveUnloaded";
		
		public static readonly string HistoryAllowDuplicatedEntriesPrefKey = "SelectionHistoryRecord.AllowDuplicatedEntries";
	    public static readonly string HistoryShowHierarchyObjectsPrefKey = "SelectionHistoryRecord.ShowHierarchyObjects";

		public static readonly string HistoryOnlyRecordPrefabAndSpritePrefKey = "SelectionHistoryRecord.OnlyRecordPrefabAndSprite";

	    public static readonly string HistoryShowFavoriteButtonPrefKey = "SelectionHistoryRecord.ShowFavoritesPinButton";
		public static readonly string HistoryShowPingButtonPrefKey = "SelectionHistoryRecord.ShowPingButton";
		public static readonly string HistoryShowOpenButtonPrefKey = "SelectionHistoryRecord.ShowOpenButton";

		public static readonly string HistoryShowFavoriteButtonPrefKey2 = "SelectionHistoryRecord.ShowFavoritesPinButton2";
		public static readonly string HistoryShowPingButtonPrefKey2 = "SelectionHistoryRecord.ShowPingButton2";
		public static readonly string HistoryShowOpenButtonPrefKey2 = "SelectionHistoryRecord.ShowOpenButton2";

	    public static readonly string ShowUnloadedObjectsKey = "SelectionHistoryRecord.ShowUnloadedObjects";
	    public static readonly string ShowDestroyedObjectsKey = "SelectionHistoryRecord.ShowDestroyedObjects";
	    
	    public static readonly string OrderLastSelectedFirstKey = "SelectionHistoryRecord.OrderLastSelectedFirst";
	    public static readonly string BackgroundRecordKey = "SelectionHistoryRecord.BackgroundRecord";

	    public const float distanceToConsiderDrag = 10.0f;
	    
	    private static readonly bool debugEnabled = false;
	    
	    static SelectionHistoryWindowUtils()
	    {
		    Selection.selectionChanged += SelectionRecorder;
	    }
		
	    private static void SelectionRecorder ()
	    {
		    if (!RecordInTheBackground)
			    return;

		    Record();
	    }

		public static void Record()
		{
			if (Selection.activeObject != null)
			{
                var needRecord = false;
                if(SelectionHistoryWindowUtils.OnlyRecordPrefabAndSprite)
                {
                    needRecord = SelectionHistoryUtils.isSprite(Selection.activeObject);
                } else {
                    needRecord = SelectionHistoryUtils.isOther(Selection.activeObject);
                }
                if(needRecord)
                {
                    SelectionHistoryWindowUtils.RecordSelectionChange(); 
                }
				FavoritesAsset.instance.InvokeUpdate();
			}
		}

		[OnOpenAsset(0)]
		public static bool OnOpenAssetCallback(int instanceID, int line)
		{
			// string name = EditorUtility.InstanceIDToObject(instanceID).name;
			if (Selection.activeObject != null)
			{
				if(SelectionHistoryUtils.isPrefab(Selection.activeObject))
				{
					SelectionHistoryWindowUtils.RecordSelectionChange();
				}
			}
			return false;
		}

	    public static void RecordSelectionChange()
	    {
		    if (Selection.activeObject != null)
		    {
			    if (debugEnabled)
			    {
				    Debug.Log("Recording new selection: " + Selection.activeObject.name);
			    }

			    var isSceneObject = SelectionHistoryUtils.IsSceneObject(Selection.activeObject); 
			    
			    if (!SelectionHistoryWindowUtils.ShowHierarchyViewObjects)
			    {
				    if (isSceneObject)
				    {
					    return;
				    }
			    }

			    if (Application.isPlaying && isSceneObject)
			    {
				    return;
			    }

			    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
			    selectionHistory.UpdateSelection(Selection.activeObject);
		    }
	    }

	    // [MenuItem("Window/Selection History/[1] 上一项 %#,")]
	    // [Shortcut("Selection History/Previous Selection")]
	    // public static void PreviousSelection()
	    // {
		//     var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		//     selectionHistory.Previous ();
		//     Selection.activeObject = selectionHistory.GetSelection ();
	    // }

	    // [MenuItem("Window/Selection History/[2] 下一项 %#.")]
	    // [Shortcut("Selection History/Next Selection")]
	    // public static void NextSelection()
	    // {
		//     var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		//     selectionHistory.Next();
		//     Selection.activeObject = selectionHistory.GetSelection ();
	    // }
		
		public static bool AutomaticRemoveDestroyed =>
			EditorPrefs.GetBool(HistoryAutomaticRemoveDestroyedPrefKey, true);
		
		public static bool AutomaticRemoveUnloaded =>
			EditorPrefs.GetBool(HistoryAutomaticRemoveUnloadedPrefKey, true);
		
		public static bool AllowDuplicatedEntries =>
			EditorPrefs.GetBool(HistoryAllowDuplicatedEntriesPrefKey, false);

		public static bool ShowHierarchyViewObjects =>
			EditorPrefs.GetBool(HistoryShowHierarchyObjectsPrefKey, false);
		
		public static bool OnlyRecordPrefabAndSprite =>
			EditorPrefs.GetBool(HistoryOnlyRecordPrefabAndSpritePrefKey, true);

		public static bool ShowUnloadedObjects =>
			EditorPrefs.GetBool(ShowUnloadedObjectsKey, true);
		
		public static bool ShowDestroyedObjects =>
			EditorPrefs.GetBool(ShowDestroyedObjectsKey, false);

		public static bool ShowFavoriteButton =>
			EditorPrefs.GetBool(HistoryShowFavoriteButtonPrefKey, true);

		public static bool ShowPingButton =>
			EditorPrefs.GetBool(HistoryShowPingButtonPrefKey, true);

		public static bool ShowOpenButton =>
			EditorPrefs.GetBool(HistoryShowOpenButtonPrefKey, true);

		public static bool ShowFavoriteButton2 =>
			EditorPrefs.GetBool(HistoryShowFavoriteButtonPrefKey2, true);

		public static bool ShowPingButton2 =>
			EditorPrefs.GetBool(HistoryShowPingButtonPrefKey2, true);

		public static bool ShowOpenButton2 =>
			EditorPrefs.GetBool(HistoryShowOpenButtonPrefKey2, true);
		
		public static bool OrderLastSelectedFirst =>
			EditorPrefs.GetBool(OrderLastSelectedFirstKey, false);
		
		public static bool RecordInTheBackground =>
			EditorPrefs.GetBool(BackgroundRecordKey, true);
	

	    public static void PingEntry(SelectionHistory.Entry e)
	    {
		    if (e.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
		    {
			    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(e.scenePath);
			    EditorGUIUtility.PingObject(sceneAsset);
		    } else
		    {
			    EditorGUIUtility.PingObject(e.Reference);
		    }
	    }
	}
}