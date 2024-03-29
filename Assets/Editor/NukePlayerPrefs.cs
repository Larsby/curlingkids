using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NUnit.Framework;
using System.IO;


internal static class NukePlayerPrefs  {

	private const string MenuRoot = "Pastille/"; 
	[MenuItem (MenuRoot + "Nuke Playerprefs")]
	static bool Nuke() {
		Debug.Log ("Nuking playerprefs baby");
		PlayerPrefs.DeleteAll ();
		return true;
	}


   
    [MenuItem(MenuRoot + "Nuke Persistant Storage")]
    static bool NukePS()
    {
        Debug.Log("Nuking persistant storage baby");
        DirectoryInfo dataDir = new DirectoryInfo(Application.persistentDataPath);
        dataDir.Delete(true);
        return true;
    }



}
