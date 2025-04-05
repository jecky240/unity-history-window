using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk
{
    public class FavoriteAssetsWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/Selection History/[3] 打开收藏夹 %#/")]
        public static void OpenWindow()
        {
            var window = GetWindow<FavoriteAssetsWindow>();
            var titleContent = EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteWindowIconName);
            titleContent.text = "收藏夹";
            titleContent.tooltip = "收藏夹窗口";
            window.titleContent = titleContent;
        }

        // [MenuItem("Window/Selection History/[5] 加入收藏夹")]
        // [Shortcut("Gemserk/Favorite Item", null, KeyCode.F, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        // public static void Favorite()
        // { 
        //     FavoriteElements(Selection.objects);
        // }

        private static bool CanBeFavorite(Object reference)
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(reference)))
            {
                return true;
            }
            return false;
        }

        private static void FavoriteElements(Object[] references)
        {
            var favorites = FavoritesAsset.instance;

            foreach (var reference in references)
            {
                if (favorites.IsFavorite(reference))
                    continue;
            
                if (CanBeFavorite(reference))
                {
                    favorites.AddFavorite(new FavoritesAsset.Favorite
                    {
                        reference = reference
                    });   
                }
            }
        }

        private FavoritesAsset _favorites;

        private StyleSheet styleSheet;

        private VisualTreeAsset favoriteElementTreeAsset;

        private ToolbarSearchField searchToolbar;
        private VisualElement favoritesParent;
        
        private string searchText;
        
        private void GetDefaultElements()
        {
            if (styleSheet == null)
            {
                styleSheet = AssetDatabaseExt.FindAssets(typeof(StyleSheet), "SelectionHistoryStylesheet")
                    .OfType<StyleSheet>().FirstOrDefault();
            }
            
            if (favoriteElementTreeAsset == null)
            {
                favoriteElementTreeAsset = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "FavoriteElement")
                    .OfType<VisualTreeAsset>().FirstOrDefault();
            }
        }
        
        private void OnDisable()
        {
            if (_favorites != null)
            {
                _favorites.OnFavoritesUpdated -= OnFavoritesUpdated;
                _favorites.OnFavoritesUpdatedWithNoScroll -= OnFavoritesUpdated;
            }
            
            styleSheet = null;
            favoriteElementTreeAsset = null;
        }

        public void OnEnable()
        {
            GetDefaultElements();
            
            _favorites = FavoritesAsset.instance;
            _favorites.OnFavoritesUpdated += OnFavoritesUpdated;
            _favorites.OnFavoritesUpdatedWithNoScroll += OnFavoritesUpdated;
            
            var root = rootVisualElement;
            if(styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
                root.Add(CreateSearchToolbar());
                ReloadRoot();
            }

            root.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();
                FavoriteElements(DragAndDrop.objectReferences);
            });
            
            root.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            });
        }

        private void OnFavoritesUpdated(FavoritesAsset favorites)
        {
            // var root = rootVisualElement;
            // root.Clear();
            ReloadRoot();
        }

        private VisualElement CreateSearchToolbar()
        {
            searchToolbar = new ToolbarSearchField();
            searchToolbar.AddToClassList("searchToolbar");
            searchToolbar.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue;
                ReloadRoot();
            });

            return searchToolbar;
        }
        
        private void ReloadRoot()
        {
            var root = rootVisualElement;

            // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FavoriteElement.uxml");
            if (favoritesParent == null)
            {
                favoritesParent = new ScrollView(ScrollViewMode.Vertical);
                root.Add(favoritesParent);
                var clearButton = new Button(delegate
                {
                    FavoritesAsset.instance.RemoveAll();
                }) {text = "清空收藏夹"};
                root.Add(clearButton);
            }
            else
            {
                favoritesParent.Clear();
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

            for (var i = 0; i < _favorites.favoritesList.Count; i++)
            {
                var assetReference = _favorites.favoritesList[i].reference;

                if (assetReference == null)
                    continue;

                var testName = assetReference.name.ToLower();
                    
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
                        continue;
                    }
                }
                
                var elementTree = favoriteElementTreeAsset.CloneTree();
                var favoriteRoot = elementTree.Q<VisualElement>("Root");
                
                var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
                if(selectionHistory != null){
                    var selectReference = selectionHistory.GetSelection();
                    if(assetReference == selectReference){
                        favoriteRoot.style.backgroundColor = new Color(0.22f, 0.24f, 0.29f, 1f);
                    }
                }

                var dragArea = elementTree.Q<VisualElement>("DragArea");
                
                var isSceneAsset = assetReference is SceneAsset;
                var isAsset = !isSceneAsset;

                if (dragArea != null)
                {
                    dragArea.AddManipulator(new FavoriteElementDragManipulator(assetReference));
                }
                
                var icon = elementTree.Q<Image>("Icon");
                if (icon != null)
                {
                    icon.image = AssetPreview.GetMiniThumbnail(assetReference);
                }
                
                var pingIcon = elementTree.Q<Image>("PingIcon");
                if (pingIcon != null)
                {
                    pingIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.searchIconName).image;
                    pingIcon.tooltip = "定位";
                    pingIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        EditorGUIUtility.PingObject(assetReference);
                    });
                    if (SelectionHistoryUtils.isOther(assetReference) || !SelectionHistoryWindowUtils.ShowPingButton2)
                    {
                        pingIcon.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        pingIcon.style.display = DisplayStyle.Flex;
                    }
                }

                var removeIcon = elementTree.Q<Image>("RemoveIcon");
                if (removeIcon != null)
                {
                    // removeIcon.image = AssetPreview.GetMiniThumbnail(assetReference);
                    removeIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.removeIconName).image;
                    removeIcon.tooltip = "移除";
                    
                    removeIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        FavoritesAsset.instance.RemoveFavorite(assetReference);
                    });
                }
                
                var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
                if (openPrefabIcon != null)
                {
                    // removeIcon.image = AssetPreview.GetMiniThumbnail(assetReference);
                    openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openAssetIconName).image;
                    openPrefabIcon.tooltip = "打开";

                    openPrefabIcon.RemoveFromClassList("hidden");

                    openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        AssetDatabase.OpenAsset(assetReference);
                    });
                    if (SelectionHistoryUtils.isPrefab(assetReference) || !SelectionHistoryWindowUtils.ShowOpenButton2)
                    {
                        openPrefabIcon.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        openPrefabIcon.style.display = DisplayStyle.Flex;
                    }
                }
                
                var label = elementTree.Q<Label>("Favorite");
                if (label != null)
                {
                    label.text = assetReference.name;
                }

                favoritesParent.Add(favoriteRoot);
            }

            var receiveDragArea = new VisualElement();
            receiveDragArea.style.flexGrow = 1;
            root.Add(receiveDragArea);
        }

        public void AddItemsToMenu(GenericMenu menu)
        {		             
            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.HistoryShowPingButtonPrefKey2, " [定位] 按钮", 
                "Toggle to show/hide ping button.", true);

            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.HistoryShowOpenButtonPrefKey2, " [打开] 按钮", 
                "Toggle to show/hide open button.", true);
            menu.AddItem(new GUIContent("一键清除所有历史"), false, delegate
            {
                var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
                if(selectionHistory != null){
                    selectionHistory.Clear();
                }
                FavoritesAsset.instance.InvokeUpdate(); 
            });
            menu.AddItem(new GUIContent("一键清除 [历史&&收藏夹]"), false, delegate 
            {
                var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
                if(selectionHistory != null){
                    selectionHistory.Clear();
                }
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
                ReloadRoot();
            });
        }

        private static void ToggleBoolEditorPref(string preferenceName, bool defaultValue)
        {
            var newValue = !EditorPrefs.GetBool(preferenceName, defaultValue);
            EditorPrefs.SetBool(preferenceName, newValue);
        }
    }
}