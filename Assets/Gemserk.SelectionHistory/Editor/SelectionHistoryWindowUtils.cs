using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Gemserk
{
	[InitializeOnLoad]
	public static class SelectionHistoryWindowUtils {

		public static readonly string HistoryAutomaticRemoveDestroyedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveDeleted";
		public static readonly string HistoryAutomaticRemoveUnloadedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveUnloaded";
		
		public static readonly string HistoryAllowDuplicatedEntriesPrefKey = "Gemserk.SelectionHistory.AllowDuplicatedEntries";
	    public static readonly string HistoryShowHierarchyObjectsPrefKey = "Gemserk.SelectionHistory.ShowHierarchyObjects";

		public static readonly string HistoryShowOtherPrefKey = "Gemserk.SelectionHistory.ShowOtherHistory";

	    public static readonly string HistoryShowFavoriteButtonPrefKey = "Gemserk.SelectionHistory.ShowFavoritesPinButton";

		public static readonly string HistoryShowPingButtonPrefKey = "Gemserk.SelectionHistory.ShowPingButton";

		public static readonly string HistoryShowOpenButtonPrefKey = "Gemserk.SelectionHistory.ShowOpenButton";

	    public static readonly string ShowUnloadedObjectsKey = "Gemserk.SelectionHistory.ShowUnloadedObjects";
	    public static readonly string ShowDestroyedObjectsKey = "Gemserk.SelectionHistory.ShowDestroyedObjects";
	    
	    public static readonly string OrderLastSelectedFirstKey = "Gemserk.SelectionHistory.OrderLastSelectedFirst";
	    public static readonly string BackgroundRecordKey = "Gemserk.SelectionHistory.BackgroundRecord";

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

		    RecordSelectionChange();
	    }

		[OnOpenAsset]
		public static bool OnOpenAssetCallback(int instanceID, int line)
		{
			// string name = EditorUtility.InstanceIDToObject(instanceID).name;
			if (Selection.activeObject != null)
			{
				string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (assetPath.EndsWith(".prefab") || assetPath.EndsWith(".png"))
				{
					RecordSelectionChange();
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

	    [MenuItem("Window/Selection History/[1] 上一项 %#,")]
	    [Shortcut("Selection History/Previous Selection")]
	    public static void PreviousSelection()
	    {
		    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		    selectionHistory.Previous ();
		    Selection.activeObject = selectionHistory.GetSelection ();
	    }

	    [MenuItem("Window/Selection History/[2] 下一项 %#.")]
	    [Shortcut("Selection History/Next Selection")]
	    public static void NextSelection()
	    {
		    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		    selectionHistory.Next();
		    Selection.activeObject = selectionHistory.GetSelection ();
	    }
		
		public static bool AutomaticRemoveDestroyed =>
			EditorPrefs.GetBool(HistoryAutomaticRemoveDestroyedPrefKey, true);
		
		public static bool AutomaticRemoveUnloaded =>
			EditorPrefs.GetBool(HistoryAutomaticRemoveUnloadedPrefKey, true);
		
		public static bool AllowDuplicatedEntries =>
			EditorPrefs.GetBool(HistoryAllowDuplicatedEntriesPrefKey, false);

		public static bool ShowHierarchyViewObjects =>
			EditorPrefs.GetBool(HistoryShowHierarchyObjectsPrefKey, false);
		
		public static bool ShowUnloadedObjects =>
			EditorPrefs.GetBool(ShowUnloadedObjectsKey, true);
		
		public static bool ShowDestroyedObjects =>
			EditorPrefs.GetBool(ShowDestroyedObjectsKey, false);
		
		public static bool ShowOtherHistory =>
			EditorPrefs.GetBool(HistoryShowOtherPrefKey, true);

		public static bool ShowFavoriteButton =>
			EditorPrefs.GetBool(HistoryShowFavoriteButtonPrefKey, true);

		public static bool ShowPingButton =>
			EditorPrefs.GetBool(HistoryShowPingButtonPrefKey, true);

		public static bool ShowOpenButton =>
			EditorPrefs.GetBool(HistoryShowOpenButtonPrefKey, false);
		
		public static bool OrderLastSelectedFirst =>
			EditorPrefs.GetBool(OrderLastSelectedFirstKey, false);
		
		public static bool RecordInTheBackground =>
			EditorPrefs.GetBool(BackgroundRecordKey, false);
	

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