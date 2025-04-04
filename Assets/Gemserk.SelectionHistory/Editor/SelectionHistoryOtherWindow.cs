using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Gemserk
{   
    public class SelectionHistoryOtherWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/Selection History/[2] 打开资源历史记录 %#.")]
        public static void OpenWindow()
        {
            var window = GetWindow<SelectionHistoryOtherWindow>();
            var titleContent = EditorGUIUtility.IconContent(UnityBuiltInIcons.plusIconName);
            titleContent.text = "资源";
            titleContent.tooltip = "资源历史记录";
            window.titleContent = titleContent;
        }
        
        private StyleSheet styleSheet;
        private VisualTreeAsset historyElementViewTree;

        private SelectionHistory selectionHistory;

        private ToolbarSearchField searchToolbar;
        private ScrollView mainScrollElement;
        private List<VisualElement> visualElements = new List<VisualElement>();

        private Button removeUnloadedButton;
        private Button removeDestroyedButton;

        private string searchText;

        private void GetDefaultElements()
        {
            if (styleSheet == null)
            {
                styleSheet = AssetDatabaseExt.FindAssets(typeof(StyleSheet), "SelectionHistoryStylesheet")
                    .OfType<StyleSheet>().FirstOrDefault();
            }
            
            if (historyElementViewTree == null)
            {
                historyElementViewTree = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "SelectionHistoryElement")
                    .OfType<VisualTreeAsset>().FirstOrDefault();
            }
        }

        private void OnDisable()
        {
            //EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded -= OnHistoryEntryAdded;
            }
            
            Selection.selectionChanged -= OnSelectionChanged;

            styleSheet = null;
            historyElementViewTree = null;
        }

        public void OnEnable()
        {
            GetDefaultElements();  
            
            EditorSceneManager.sceneOpened += OnSceneOpened;
            
            selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
            
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded += OnHistoryEntryAdded;
            }

            FavoritesAsset.instance.OnFavoritesUpdated += delegate
            {
                ReloadRootAndRemoveUnloadedAndDuplicated();
            };
            
            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);

            RegenerateUI();
            
            ReloadRootAndRemoveUnloadedAndDuplicated();
            
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void RegenerateUI()
        {
            var root = rootVisualElement;
            root.Clear();
            
            visualElements.Clear();
            
            root.Add(CreateSearchToolbar());
            
            mainScrollElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "MainScroll"
            };
            
            root.Add(mainScrollElement);
            
            CreateMaxElements(selectionHistory, mainScrollElement);
            
            var clearButton = new Button(delegate
            {
                selectionHistory.ClearOther();
                FavoritesAsset.instance.InvokeUpdate();
            }) {text = "清空资源历史记录"};
            
            root.Add(clearButton);
            
            // // this is just for development
            // var refreshButton = new Button(delegate
            // {
            //     ReloadRoot();
            // }) {text = "Refresh (dev)"};
            //
            // root.Add(refreshButton);
            
            removeUnloadedButton = new Button(delegate
            {
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceUnloaded);
                // ReloadRootAndRemoveUnloadedAndDuplicated();
                ReloadRoot();
            }) {text = "清空已卸载元素"};
            root.Add(removeUnloadedButton);
            
            removeDestroyedButton = new Button(delegate
            {
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);
                // ReloadRootAndRemoveUnloadedAndDuplicated();
                ReloadRoot();
            }) {text = "清空已删除元素"};
            root.Add(removeDestroyedButton);
        }
        
        private VisualElement CreateSearchToolbar()
        {
            searchToolbar = new ToolbarSearchField();
            searchToolbar.AddToClassList("searchToolbar");
            searchToolbar.SetValueWithoutNotify(searchText);
            searchToolbar.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue;
                ReloadRoot(false);
            });

            return searchToolbar;
        }

        private void CreateMaxElements(SelectionHistory selectionHistory, VisualElement parent)
        {
            var size = selectionHistory.GetHistoryCount();
            if(size == 0){
                size = 1;
            }

            for (int i = 0; i < size; i++)
            {
                var elementTree = CreateHistoryVisualElement(i);
                parent.Add(elementTree);
                
                visualElements.Add(elementTree);
            }
        }

        private VisualElement CreateHistoryVisualElement(int index)
        {
            if(historyElementViewTree == null){
                GetDefaultElements();
            }
            var elementTree = historyElementViewTree.CloneTree();
            var selectionElementRoot = elementTree.Q<VisualElement>("Root");
            
            var historyIndex = index;
            
            var dragArea = selectionElementRoot.Q<VisualElement>("DragArea");
            if (dragArea != null)
            {
                dragArea.AddManipulator(new HistoryElementDragManipulator(selectionHistory, historyIndex));
            }
            
            var openPrefabIcon = selectionElementRoot.Q<Image>("OpenPrefabIcon");
            if (openPrefabIcon != null)
            {
                openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openAssetIconName).image;
                openPrefabIcon.tooltip = "打开";
                openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);

                    if (entry == null)
                    {
                        return;
                    }
                    
                    if (entry.isAsset || entry.IsSceneAsset())
                    {
                        AssetDatabase.OpenAsset(entry.Reference);
                    } else if (entry.isUnloadedHierarchyObject)
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(entry.scenePath);
                        }
                    }
                });
            }

            var favoriteAsset = selectionElementRoot.Q<Image>("Favorite");
            if (favoriteAsset != null)
            {
                favoriteAsset.tooltip = "收藏";
                favoriteAsset.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);
                    if (entry == null)
                        return;
                        
                    if (FavoritesAsset.instance.IsFavorite(entry.Reference))
                    {
                        FavoritesAsset.instance.RemoveFavorite(entry.Reference);
                    } else {
                        FavoritesAsset.instance.AddFavorite(new FavoritesAsset.Favorite
                        {
                            reference = entry.Reference
                        });
                    }
                });
            }

            var removeIcon = selectionElementRoot.Q<Image>("RemoveIcon");
            if (removeIcon != null)
            {
                removeIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.clearSearchToolbarIconName).image;
                removeIcon.tooltip = "关闭";
                
                removeIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);
                    if (entry == null)
                        return;
                    selectionHistory.Remove(entry);
                    FavoritesAsset.instance.InvokeUpdate();
                });
            }

            return selectionElementRoot;
        }

        private void OnSelectionChanged()
        {
            if (SelectionHistoryWindowUtils.RecordInTheBackground)
            {
                return;
            }

            SelectionHistoryWindowUtils.Record();
        }

        private void OnHistoryEntryAdded(SelectionHistory selectionHistory)
        {
            ReloadRootAndRemoveUnloadedAndDuplicated();
            ScrollToLatestSelection();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ReloadRootAndRemoveUnloadedAndDuplicated();
        }

        public void ReloadRootAndRemoveUnloadedAndDuplicated()
        {
            if (SelectionHistoryWindowUtils.AutomaticRemoveDestroyed)
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);
            
            if (SelectionHistoryWindowUtils.AutomaticRemoveUnloaded)
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceUnloaded);

            if (!SelectionHistoryWindowUtils.AllowDuplicatedEntries)
                selectionHistory.RemoveDuplicated();
            
            ReloadRoot();
        }

        private void ReloadRoot(bool needRegenerateUI = true)
        {
            //if (visualElements.Count != selectionHistory.historySize)
            //{
            if(needRegenerateUI)
                RegenerateUI(); 
            //}
            
            var showHierarchyViewObjects =
                EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowHierarchyObjectsPrefKey, false);
            
            var showUnloadedObjects = showHierarchyViewObjects && SelectionHistoryWindowUtils.ShowUnloadedObjects 
                                                               && !SelectionHistoryWindowUtils.AutomaticRemoveUnloaded;
            var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects && !SelectionHistoryWindowUtils.AutomaticRemoveDestroyed;
            
            var currentEntry = -1;

            if (removeUnloadedButton != null)
            {
                removeUnloadedButton.style.display = showUnloadedObjects ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            if (removeDestroyedButton != null)
            {
                removeDestroyedButton.style.display = showDestroyedObjects ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            string[] searchTexts = null;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.TrimStart().TrimEnd();
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchTexts = searchText.Split(' ');
                }
            }
            
            for (var i = 0; i < visualElements.Count; i++)
            {
                var visualElement = visualElements[i];
                visualElement.style.backgroundColor = new Color(0.196f, 0.196f, 0.196f, 1f);

                var entry = selectionHistory.GetEntry(i);
                
                var isOther = entry != null && SelectionHistoryUtils.isOther(entry.Reference);
                if (entry == null || !isOther)
                {
                    visualElement.style.display = DisplayStyle.None;
                }
                else
                {
                    var selectReference = selectionHistory.GetSelection();
                    if(entry.Reference == selectReference){
                        visualElement.style.backgroundColor = new Color(0.22f, 0.24f, 0.29f, 1f);
                    }

                    var testName = entry.GetName(false).ToLower();

                    if (searchTexts != null && searchTexts.Length > 0)
                    {
                        var match = true;
                        
                        foreach (var text in searchTexts)
                        {
                            if (!testName.Contains(text.ToLower()))
                            {
                                match = false;
                            }
                        }

                        if (!match)
                        {
                            visualElement.style.display = DisplayStyle.None;
                            continue;
                        }
                    }
                    
                    currentEntry = i;
                    
                    // var isPrefabAsset = entry.isReferenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
                    var isAsset = entry.isReferenced && entry.isAsset;
                    var isSceneAsset = entry.isReferenced && entry.isAsset && entry.reference is SceneAsset;
                    
                    visualElement.style.display = DisplayStyle.Flex;
                    
                    // since now I am using the root element, remove each specific class to avoid
                    // losing the base ones defined in the uxml file.
                    
                    visualElement.RemoveFromClassList("unreferencedObject");
                    visualElement.RemoveFromClassList("sceneObject");
                    visualElement.RemoveFromClassList("assetObject");

                    // visualElement.AddToClassList("history");
                    
                    if (!entry.isReferenced)
                    {
                        visualElement.AddToClassList("unreferencedObject");

                        if (!showDestroyedObjects)
                        {
                            visualElement.style.display = DisplayStyle.None;
                            continue;
                        }
                    }
                    else if (entry.isSceneInstance)
                    {
                        visualElement.AddToClassList("sceneObject");
                    }
                    else
                    {
                        visualElement.AddToClassList("assetObject");
                    }
                    
                    var label = visualElement.Q<Label>("Name");
                    if (label != null)
                    {
                        label.text = entry.GetName(true);
                    }
                    
                    var icon = visualElement.Q<Image>("Icon");
                    if (icon != null)
                    {
                        icon.image = AssetPreview.GetMiniThumbnail(entry.Reference);
                    }
                    
                    var openPrefabIcon = visualElement.Q<Image>("OpenPrefabIcon");
                    if (openPrefabIcon != null)
                    {
                        openPrefabIcon.ClearClassList();
                        
                        if (isAsset || isSceneAsset || entry.isUnloadedHierarchyObject)
                        {
                            openPrefabIcon.RemoveFromClassList("hidden");
                        }
                        else
                        {
                            openPrefabIcon.AddToClassList("hidden");
                        }
                        if (!SelectionHistoryWindowUtils.ShowOpenButton || !SelectionHistoryWindowUtils.ShowFavoriteButton2 ||!entry.isReferenced)
                        {
                            openPrefabIcon.style.display = DisplayStyle.None;
                        }
                        else
                        {
                            openPrefabIcon.style.display = DisplayStyle.Flex;
                        }
                    }
                    
                    var favoriteAsset = visualElement.Q<Image>("Favorite");
                    if (!SelectionHistoryWindowUtils.ShowFavoriteButton2 || !entry.isReferenced)
                    {
                        favoriteAsset.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        favoriteAsset.style.display = DisplayStyle.Flex;
                         
                        var isFavorite = FavoritesAsset.instance.IsFavorite(entry.Reference);
                        
                        favoriteAsset.image = isFavorite
                            ? EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteIconName).image
                            : EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteEmptyIconName).image;
                    }

                    var removeIcon = visualElement.Q<Image>("RemoveIcon");
                    if (removeIcon != null)
                    {
                        if (!entry.isReferenced)
                        {
                            removeIcon.style.display = DisplayStyle.None;
                        }
                        else
                        {
                            removeIcon.style.display = DisplayStyle.Flex; 
                        }
                    }
                }
                
                // now update values
                
                // depending configuration, hide elements
            }
            
            if (mainScrollElement != null)
            {
                mainScrollElement.contentContainer.style.flexDirection = SelectionHistoryWindowUtils.OrderLastSelectedFirst ? FlexDirection.ColumnReverse : FlexDirection.Column;
            }
        }

        public void ScrollToLatestSelection()
        {
            var index = selectionHistory.GetSelectedIndex();

            if (mainScrollElement != null)
            {
                mainScrollElement.contentContainer.style.flexDirection = SelectionHistoryWindowUtils.OrderLastSelectedFirst ? FlexDirection.ColumnReverse : FlexDirection.Column;

                if (index >= 0 && index <= visualElements.Count - 1)
                {
                    mainScrollElement.ScrollTo(visualElements[index]);
                }
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {      
            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.HistoryShowFavoriteButtonPrefKey2, " [收藏&&打开] 按钮", 
                "Toggle to show/hide favorite & open button.", true);

            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.HistoryShowOpenButtonPrefKey, " [打开] 按钮", 
                "Toggle to show/hide open button.", true); 
            menu.AddItem(new GUIContent("一键清除所有历史"), false, delegate
            {
                selectionHistory.Clear();
                FavoritesAsset.instance.InvokeUpdate(); 
            });
            menu.AddItem(new GUIContent("一键清除 [历史&&收藏夹]"), false, delegate 
            {
                selectionHistory.Clear();
                FavoritesAsset.instance.RemoveAll();
            });
        }

        private void AddMenuItemForPreference(GenericMenu menu, string preference, string text, string tooltip, bool defaultValue)
        {
            var value = EditorPrefs.GetBool(preference, defaultValue);
            var name = value ? $"隐藏{text}" : $"显示{text}";
            menu.AddItem(new GUIContent(name, tooltip), false, delegate
            {
                ToggleBoolEditorPref(preference, defaultValue);
                ReloadRootAndRemoveUnloadedAndDuplicated();
            });
        }

        private static void ToggleBoolEditorPref(string preferenceName, bool defaultValue)
        {
            var newValue = !EditorPrefs.GetBool(preferenceName, defaultValue);
            EditorPrefs.SetBool(preferenceName, newValue);
        }
    }
}
