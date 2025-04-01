using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gemserk.Tools
{
    public static class ExportPackageAction
    {
        [MenuItem("Window/Selection History/[6] 导出包")]
        public static void ExportPackage()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Exporting package", "", 0.5f);

                var rootPath = Application.dataPath.Replace("/Assets", "");
                var packageFile = Path.Combine(rootPath, "release/unity-selection-history.unitypackage");
                AssetDatabase.ExportPackage("Assets/Gemserk.SelectionHistory", packageFile,
                    ExportPackageOptions.Recurse);
                
                EditorUtility.DisplayProgressBar("Exporting package", "", 1);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            
        }
    }
}