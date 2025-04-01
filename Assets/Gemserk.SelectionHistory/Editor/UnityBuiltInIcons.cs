namespace Gemserk
{
    public static class UnityBuiltInIcons
    {
        public const string pickObjectIconName = "d_pick";//定位icon
        public const string favoriteWindowIconName = "Favorite Icon";
        
        #if UNITY_2022_3_OR_NEWER
        public const string favoriteIconName = "Favorite_colored";
        public const string favoriteEmptyIconName = "Favorite icon";
        #else 
        public const string favoriteIconName = "Toolbar Minus";
        public const string favoriteEmptyIconName = "Toolbar Plus";
        #endif

        
        public const string removeIconName = "Toolbar Minus";
        public const string tagIconName = "AssetLabelIcon";//标签icon
        public const string searchIconName = "Search Icon";
        public const string eyeViewToolIconName = "d_ViewToolOrbit";//可视眼睛icon
        public const string refreshIconName1 = "Refresh";
        public const string refreshIconName2 = "TreeEditor.Refresh";//有描边的刷新icon

        public const string openAssetIconName = "FolderOpened Icon";
        public const string clearSearchToolbarIconName = "d_clear";//关闭icon
        
        
    }
}