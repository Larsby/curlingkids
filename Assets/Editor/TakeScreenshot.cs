using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NUnit.Framework;
using System.IO;
using UnityEngine.SceneManagement;


internal static class TakeScreenshot  {

	private const string MenuRoot = "Pastille/"; 
    [MenuItem(MenuRoot + "Take Screenshot _%w")]
 
    static bool Nuke()
    {
        string fileName = SceneManager.GetActiveScene().name + " " + System.DateTime.UtcNow.ToString("s") + "Z" + ".png";
        Debug.Log("Saved FileName: " + fileName);

        ScreenCapture.CaptureScreenshot(fileName);


        return true;
    }




}
