using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace BaseProject.Extension
{
    public abstract class HelperExtension
    {
        [MenuItem("[Helper]/Select NULL in Hierarchy %g")] // %g means CTRL + G
        static void SelectNull() => Selection.activeObject = null;
        
        [MenuItem("[Helper]/Clear Player Prefs %t")] // %t means CTRL + T
        static void ClearPlayerPrefs(){PlayerPrefs.DeleteAll(); PlayerPrefs.DeleteAll(); PlayerPrefs.Save();}
        
        [MenuItem("[Helper]/Toggle Inspector Lock %l")] // %l means CTRL + L
        static void ToggleInspectorLock()
        {
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
    }
}
#endif