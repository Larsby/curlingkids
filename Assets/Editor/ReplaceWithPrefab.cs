using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab : EditorWindow
{
	[SerializeField] private GameObject prefab;
	[SerializeField] private bool retainComponents = false;

	[MenuItem("Tools/Replace With Prefab")]
	static void CreateReplaceWithPrefab()
	{
		EditorWindow.GetWindow<ReplaceWithPrefab>();
	}

	public void CopyComponent<T>(GameObject selected, GameObject newObject, bool removeOldComponent = true) where T : Component
	{
		if (removeOldComponent)
			if (newObject.GetComponent<T> () != null) GameObject.DestroyImmediate (newObject.GetComponent<T>());

		T comp = selected.GetComponent<T> ();
		if (comp != null) GameUtil.CopyComponent<T> (newObject, comp);
	}

	private void OnGUI()
	{
		prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
		retainComponents = EditorGUILayout.Toggle("AttemptRetainComponents_TopLevelOnly", retainComponents);

		if (GUILayout.Button("Replace"))
		{
			var selection = Selection.gameObjects;
			//var selection = GameObject.FindObjectsOfType<Cone>();

			for (var i = selection.Length - 1; i >= 0; --i)
			{
				var selected = selection[i].gameObject;
				var prefabType = PrefabUtility.GetPrefabType(prefab);
				GameObject newObject;

				if (prefabType == PrefabType.Prefab)
				{
					newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
				}
				else
				{
					newObject = Instantiate(prefab);
					newObject.name = prefab.name;
				}

				if (newObject == null)
				{
					Debug.LogError("Error instantiating prefab");
					break;
				}

				Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
				newObject.transform.parent = selected.transform.parent;
				newObject.transform.localPosition = selected.transform.localPosition;
				newObject.transform.localRotation = selected.transform.localRotation;
				newObject.transform.localScale = selected.transform.localScale;

				if (retainComponents) { // add as needed
					CopyComponent<Goal>(selected, newObject);
					CopyComponent<SimpleTransform>(selected, newObject);
					CopyComponent<SoundEmitter>(selected, newObject);
					CopyComponent<ToonDollHelper>(selected, newObject);
					CopyComponent<RandomAnimatorStart>(selected, newObject);
					CopyComponent<Rigidbody>(selected, newObject);
				}

				// case specific
				//newObject.transform.localScale = new Vector3 (3,3,3);

				newObject.name = newObject.name + (i + 1);

				newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
				Undo.DestroyObjectImmediate(selected);
			}
		}

		GUI.enabled = false;
		EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
	}
}