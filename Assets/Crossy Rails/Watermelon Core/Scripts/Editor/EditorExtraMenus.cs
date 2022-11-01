using UnityEngine;
using UnityEditor;
using System;

namespace Watermelon.Core
{
    public static class EditorExtraMenus
    {
        [MenuItem("Tools/Editor/Clear PlayerPrefs")]
        public static void ClearPrefs()
        {
            if(EditorUtility.DisplayDialog("Clear Prefs", "Do you really want to delete all saved PlayerPrefs?", "Yes", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
            }
        }

#if UNITY_EDITOR_WIN
        [MenuItem("Help/Open Persistent Folder", priority = 151)]
        public static void OpenPersistentFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.persistentDataPath.Replace("/", @"\"));
        }

        [MenuItem("Help/Open Project Folder", priority = 152)]
        public static void OpenProjectFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.dataPath.Replace("/Assets", "/").Replace("/", @"\"));
        }

        [MenuItem("Help/Open Register Path", priority = 153)]
        public static void OpenRegisterPath()
        {
            //Project location path
            string registryLocation = @"HKEY_CURRENT_USER\Software\Unity\UnityEditor\" + Application.companyName + @"\" + Application.productName + @"\";
            //Last location path
            string registryLastKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";

            try
            {
                //Set LastKey value that regedit will go directly to
                Microsoft.Win32.Registry.SetValue(registryLastKey, "LastKey", registryLocation);
                System.Diagnostics.Process.Start("regedit.exe");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
#endif
    }
}
