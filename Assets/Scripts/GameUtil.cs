using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;

public static class GameUtil : System.Object {

	// Set or modify single value in vector (returns new vector)
	public static Vector3 SetX(Vector3 vector, float value) {
		return new Vector3 (value, vector.y, vector.z);
	}

	public static Vector3 SetY(Vector3 vector, float value) {
		return new Vector3 (vector.x, value, vector.z);
	}

	public static Vector3 SetZ(Vector3 vector, float value) {
		return new Vector3 (vector.x, vector.y, value);
	}

	public static Vector3 AddX(Vector3 vector, float value) {
		return new Vector3 (vector.x + value, vector.y, vector.z);
	}

	public static Vector3 AddY(Vector3 vector, float value) {
		return new Vector3 (vector.x, vector.y + value, vector.z);
	}

	public static Vector3 AddZ(Vector3 vector, float value) {
		return new Vector3 (vector.x, vector.y,  + vector.z + value);
	}
		
	public static Vector3 CloneVector3(Vector3 inv) {
		return new Vector3(inv.x, inv.y, inv.z);
	}

	// Aim object (like a camera) at given position. To animate instead of setting, set doRotation=false and use returned Vector3 with LeanTween/iTween. OR use lower step params progressively
	public static Vector3 AimTowards(GameObject obj, Vector3 pos, bool doRotation = true, float step = 1000) { // 1000 = high value to ensure that entire rotation is done at once, otherwise it starts "moving towards" the target
		Vector3 targetDir = pos - obj.transform.position;
		Vector3 newDir = Vector3.RotateTowards(obj.transform.forward, targetDir, step, 0);
		// Debug.DrawRay(transform.position, newDir, Color.red); Debug.Break ();

		if (doRotation)
			obj.transform.rotation = Quaternion.LookRotation(newDir);
		
		return Quaternion.LookRotation(newDir).eulerAngles;
	}

	// Set color based on 0-255 values instead of 0...1
	public static Color IntColor(float r, float g, float b, float a = 255) {
		return new Color (r/255f, g/255f, b/255f, a/255f);
	}

	// Find parent object with a given tag
	public static GameObject FindParentWithTag(GameObject childObject, string tag)
	{
		Transform t = childObject.transform;
		while (t != null)
		{
			if (t.tag == tag)
			{
				return t.gameObject;
			}
			if (t.parent == null)
				return null;
			t = t.parent.transform;
		}
		return null;
	}
		
	// Find parent object with a given name
	public static GameObject FindParentWithName(GameObject childObject, string name)
	{
		Transform t = childObject.transform;
		while (t != null)
		{
			if (t.name == name)
			{
				return t.gameObject;
			}
			if (t.parent == null)
				return null;
			t = t.parent.transform;
		}
		return null;
	}


	// Find nested child with specified name (breadth-first search). Normal "Find" method is only one level deep
	public static Transform FindDeepChild(Transform aParent, string aName)
	{
		var result = aParent.Find(aName);
		if (result != null)
			return result;
		foreach(Transform child in aParent)
		{
			result = FindDeepChild(child, aName);
			if (result != null)
				return result;
		}
		return null;
	}


	// Set layer on this transform and all its children
	public static void SetDeepLayer(Transform tCurrent, int layer)
	{
		tCurrent.gameObject.layer = layer;

		foreach(Transform child in tCurrent)
		{
			SetDeepLayer(child, layer);
		}
	}


	public enum ShaderMode { OPAQUE=0, CUTOUT=1, FADE=2, TRANSP=3 };

	// Change the shader material blend mode
	public static void SetMaterialBlendMode(Material material, ShaderMode blendMode )
	{
		switch (blendMode)
		{
			case ShaderMode.OPAQUE:
				material.SetFloat("_Mode", 0);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;
			case ShaderMode.CUTOUT:
				material.SetFloat("_Mode", 1);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 2450;
				break;
			case ShaderMode.FADE:
				material.SetFloat("_Mode", 2);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
			case ShaderMode.TRANSP:
				material.SetFloat("_Mode", 3);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
		}
	}

	// If width OR height is less than given value (in inches), then the device is considered small
	private static bool IsSmallDevice(float treshold=4f, bool debug=false) {
		float width = Screen.width / Screen.dpi;
		float height = Screen.height / Screen.dpi;

		if (debug) {
			Debug.Log ("DPI: " + Screen.dpi + "  W: " + Screen.width + "  H: " + Screen.height);
			Debug.Log ("Measured: " + width + " x " + height);
		}

		if (width < treshold || height < treshold)
			return true;
		else
			return false;
	}
	public static void DisableButton(Button b, Image img,float disabledAlphaValue) {
		
		b.interactable = false;

		Color dis = img.color;
		dis.a = disabledAlphaValue;
		img.color = dis;
	}

	// Find the edge TOP-LEFT coordinates of a specified camera's viewport in world coordinates, given a specified z. Assume z pos 0 if not set, can be set to an object's pos
	// Can be used e.g. to place an object at edges of screen. Invert e.g. x in returned vector to place in top-right corner.
	// Can also be used to calculate screen width in world coordinates at a certain depth (default is z=0)
	//
	// Example: GameObject go = GameObject.Find ("Test"); Vector3 pos = GameUtil.GetCameraWorldPosEdges (Camera.main, go.gameObject.transform.position.z); go.transform.position = new Vector3(pos.x, -pos.y, pos.z); // place object in BOTTOM-LEFT visible corner from perspective of main camera
	// Example2: Vector3 pos = GameUtil.GetCameraWorldPosEdges (Camera.main); int screenWidthWorld = pos.x * 2 // calculate screen width in world coordinates at Z=0 from view of main camera
	public static Vector3 GetCameraWorldPosEdges(Camera cam, float z = 0, float xEdge = 0, float yEdge = 0) {
		float dist = -cam.transform.position.z + z;
		return cam.ViewportToWorldPoint (new Vector3 (xEdge, 1-yEdge, dist));
	}
		

	// !! WARNING: does not seem to work on device (iOS), and disabling "Strip engine code" in Player settings did not seem to help reflection to work either!

	// Copy a component (https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html)
	public static T CopyComponent<T>(this GameObject go, T toAdd) where T : Component
	{
		return go.AddComponent<T>().GetCopyOf(toAdd) as T;
	}

	public static T GetCopyOf<T>(this Component comp, T other) where T : Component
	{
		Type type = comp.GetType();
		if (type != other.GetType()) return null; // type mis-match
//		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default; // removing DeclaredOnly seems to copy more stuff, such as Materials etc. Don't know if it has any unwanted side-effects.
		PropertyInfo[] pinfos = type.GetProperties(flags);
		foreach (var pinfo in pinfos) {
			if (pinfo.CanWrite) {
				try {
					pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
				}
				catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
			}
		}
		FieldInfo[] finfos = type.GetFields(flags);
		foreach (var finfo in finfos) {
			finfo.SetValue(comp, finfo.GetValue(other));
		}
		return comp as T;
	}

}
