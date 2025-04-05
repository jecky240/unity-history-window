using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk
{
    [FilePath("Gemserk/Gemserk.Favorites.asset", FilePathAttribute.Location.ProjectFolder)]
    public class FavoritesAsset : ScriptableSingleton<FavoritesAsset>
    {
        [Serializable]
        public class Favorite
        {
            public Object reference;
        }
    
        public event Action<FavoritesAsset> OnFavoritesUpdated;
        public event Action<FavoritesAsset> OnFavoritesUpdatedWithNoScroll;

        [SerializeField]
        public List<Favorite> favoritesList = new List<Favorite>();

        public void AddFavorite(Favorite favorite)
        {
            favoritesList.Add(favorite);
            OnFavoritesUpdatedWithNoScroll?.Invoke(this);
            Save(true);
        }

        public bool IsFavorite(Object reference)
        {
            return favoritesList.Count(f => f.reference == reference) > 0;
        }

        public void RemoveFavorite(Object reference)
        {
            favoritesList.RemoveAll(f => f.reference == reference);
            OnFavoritesUpdatedWithNoScroll?.Invoke(this);
            Save(true);
        }

        public void RemoveAll()
        {
            favoritesList.Clear();
            OnFavoritesUpdatedWithNoScroll?.Invoke(this);
            Save(true);
        }

        public void InvokeUpdate()
        {
            OnFavoritesUpdated?.Invoke(this);
        }

        public void InvokeUpdateWithNoScroll()
        {
            OnFavoritesUpdatedWithNoScroll?.Invoke(this);
        }
    }
}