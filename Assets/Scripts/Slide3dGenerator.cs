using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.IO;

public class Slide3dGenerator : MonoBehaviour {

	public bool interactiveEditorMode = false;

	public int NOF_CURVE_POINTS = 30;
	public int NOF_SEGMENTS = 2;

	public float X_SIZE = 10;
	public float Y_SIZE = 4f;
	public float Z_SIZE = 10;
	public float Y_DELTA_PER_SEGMENT = 0f;

	public float X_BEND_SHARPNESS = 0;
	public float X_BEND_START = 0;
	public float X_BEND_MUL = 0;
	public float X_BEND_VOLUME_CHANGE = 0f;

	public float Y_BEND_SHARPNESS = 0;
	public float Y_BEND_START = 0;
	public float Y_BEND_MUL = 0;
	public float Y_BEND_VOLUME_CHANGE = 0f;

	public float Z_BEND_SHARPNESS = 0;
	public float Z_BEND_START = 0;
	public float Z_BEND_MUL = 0;
	public float Z_BEND_VOLUME_CHANGE = 0f;

	public float END_BEND_MUL = 1f;

	public float Y_CURVE_START = 0;
	public float Y_CURVE_LENGTH = Mathf.PI;
	public float Y_CURVE_START_DELTA = 0;

	public bool progressiveCurve = false;
	public bool progressiveMul = false;
	public float xDiv = 3;
	public float zDiv = 3;

	public PhysicMaterial physMat = null;

	public bool faceDoubling = false;

	public Vector2 tiling = Vector2.one;

	public string exportAs = string.Empty;
	public bool exportNow = false;


	private void Start () {
		CreateObject ();
	}

	private void CreateObject (bool editorModeEdit = false) {
		if (NOF_SEGMENTS < 2 || NOF_CURVE_POINTS < 2) {
			print ("Segment and/or curve points values less than 2. Mesh was not (re)generated.");
			return;
		}

		var mesh = GetComponent<MeshFilter>().mesh;

		var newVerts = new Vector3[NOF_CURVE_POINTS * NOF_SEGMENTS];
		var newUv = new Vector2[NOF_CURVE_POINTS * NOF_SEGMENTS];
		var newNorms = new Vector3[NOF_CURVE_POINTS * NOF_SEGMENTS];
		var newTris = new int[(NOF_CURVE_POINTS*2 - 2) * NOF_SEGMENTS * 3];

		float curveXDelta = X_SIZE / ((float) (NOF_CURVE_POINTS - 1));
		float curveYDelta = Y_CURVE_LENGTH / ((float) (NOF_CURVE_POINTS - 1));
		float curveZDelta = Z_SIZE / ((float) (NOF_SEGMENTS - 1));

		float curveTXDelta = 1.0f / ((float) (NOF_CURVE_POINTS - 1));
		float curveTYDelta = 1.0f / ((float) (NOF_SEGMENTS - 1));

		float curveZPoint = -Z_SIZE / 2;
		float curveTYPoint = 0;

		float XbendVolumeDelta = X_BEND_VOLUME_CHANGE / ((float) (NOF_CURVE_POINTS - 1));
		float YbendVolumeDelta = Y_BEND_VOLUME_CHANGE / ((float) (NOF_CURVE_POINTS - 1));
		float ZbendVolumeDelta = Z_BEND_VOLUME_CHANGE / ((float) (NOF_CURVE_POINTS - 1));

		float endBendDelta = (END_BEND_MUL-1f) / ((float) (NOF_SEGMENTS - 1));
		float endBend = 1f;

		for (var j = 0; j < NOF_SEGMENTS; j++) {
			float curveXPoint = -X_SIZE / 2;
			float curveYPoint = Y_CURVE_START + Y_CURVE_START_DELTA * j;
			float curveTXPoint = 0;

			float XbendVolumeChange = 0;
			float YbendVolumeChange = 0;
			float ZbendVolumeChange = 0;

			for (var i = 0; i < NOF_CURVE_POINTS; i++) {

				if (!progressiveCurve)
					newVerts [j * NOF_CURVE_POINTS + i] = new Vector3 (curveXPoint + Mathf.Sin (j * X_BEND_SHARPNESS + X_BEND_START) * (X_BEND_MUL*endBend-XbendVolumeChange), Y_DELTA_PER_SEGMENT * j + Mathf.Sin (curveYPoint) * Y_SIZE + Mathf.Sin (j * Y_BEND_SHARPNESS + Y_BEND_START) * (Y_BEND_MUL*endBend-YbendVolumeChange), curveZPoint + Mathf.Sin (j * Z_BEND_SHARPNESS + Z_BEND_START) * (Z_BEND_MUL*endBend-ZbendVolumeChange));
				else {
					if (progressiveMul)
						newVerts [j * NOF_CURVE_POINTS + i] = new Vector3 (curveXPoint + Mathf.Sin (j * X_BEND_SHARPNESS + X_BEND_START) * (X_BEND_MUL*endBend-XbendVolumeChange), Y_DELTA_PER_SEGMENT * j + Mathf.Sin (curveYPoint) + Mathf.Sin ((float)((float)i / xDiv * (float)j / zDiv)) * Y_SIZE + Mathf.Sin (j * Y_BEND_SHARPNESS + Y_BEND_START) * (Y_BEND_MUL*endBend-YbendVolumeChange), curveZPoint + Mathf.Sin (j * Z_BEND_SHARPNESS + Z_BEND_START) * (Z_BEND_MUL*endBend-ZbendVolumeChange));
					else
						newVerts [j * NOF_CURVE_POINTS + i] = new Vector3 (curveXPoint + Mathf.Sin (j * X_BEND_SHARPNESS + X_BEND_START) * (X_BEND_MUL*endBend-XbendVolumeChange), Y_DELTA_PER_SEGMENT * j + Mathf.Sin (curveYPoint) + Mathf.Sin ((float)((float)i / xDiv + (float)j / zDiv)) * Y_SIZE + Mathf.Sin (j * Y_BEND_SHARPNESS + Y_BEND_START) * (Y_BEND_MUL*endBend-YbendVolumeChange), curveZPoint + Mathf.Sin (j * Z_BEND_SHARPNESS + Z_BEND_START) * (Z_BEND_MUL*endBend-ZbendVolumeChange));
				}

				XbendVolumeChange += XbendVolumeDelta;
				YbendVolumeChange += YbendVolumeDelta;
				ZbendVolumeChange += ZbendVolumeDelta;

				newUv [j * NOF_CURVE_POINTS + i] = new Vector3 (curveTXPoint * tiling.x, curveTYPoint * tiling.y);

				newNorms [j * NOF_CURVE_POINTS + i] = Vector3.one;

				curveXPoint += curveXDelta;
				curveYPoint += curveYDelta;

				curveTXPoint += curveTXDelta;
			}

			curveZPoint += curveZDelta;
			curveYPoint += curveYDelta;

			curveTYPoint += curveTYDelta;

			endBend += endBendDelta;
		}

		mesh.triangles = newTris; // set here first to avoid complaints that newTris contains vertices that don't exist (yet)

		int k = 0;
		for (var j = 0; j < NOF_SEGMENTS - 1; j++) {
			for (var i = 0; i < NOF_CURVE_POINTS - 1; i++) {

				newTris [k++] = j * NOF_CURVE_POINTS + i + 1;
				newTris [k++] = j * NOF_CURVE_POINTS + i;
				newTris [k++] = j * NOF_CURVE_POINTS + i + NOF_CURVE_POINTS;

				newTris [k++] = j * NOF_CURVE_POINTS + i + 1;
				newTris [k++] = j * NOF_CURVE_POINTS + i + NOF_CURVE_POINTS;
				newTris [k++] = j * NOF_CURVE_POINTS + i + NOF_CURVE_POINTS + 1;
			}
		}

		mesh.vertices = newVerts;
		mesh.uv = newUv;
		mesh.normals = newNorms;
		mesh.triangles = newTris; // assign triangles last!

		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		mesh.RecalculateTangents ();

		if (faceDoubling) {
			DoubleFaces ();
		}

		if (editorModeEdit == false) {
			MeshCollider eraseMe = gameObject.GetComponent<MeshCollider> ();
			if (eraseMe != null)
				Destroy (eraseMe);
			
			MeshCollider mc = gameObject.AddComponent<MeshCollider> (); // collider shape is created automatically (!)
			if (physMat != null)
				mc.material = physMat;
		}

		if (exportAs.Length > 0 && exportNow) {
			//PrefabUtility.CreatePrefab ("Assets/" + exportAs + ".prefab", gameObject); // saves the gameobject with colliders, this, etc, but not the mesh

			string outFile = "Assets/" + exportAs + ".obj";

			MeshToFile (GetComponent<MeshFilter> (), outFile);
			print ("Object exported as " + outFile);

			exportNow = false;
		}
	}

	// called whenever a variable of the object is changed in the editor
	void OnValidate()
	{
		if (interactiveEditorMode)
			CreateObject (true);
	}


	// borrowed from http://wiki.unity3d.com/index.php?title=ObjExporter
	public static string MeshToString(MeshFilter mf) {
		Mesh m = mf.mesh;
		Renderer rend = mf.GetComponent<Renderer> ();
		Material[] mats = rend.sharedMaterials;

		StringBuilder sb = new StringBuilder();

		sb.Append("g ").Append(mf.name).Append("\n");
		foreach(Vector3 v in m.vertices) {
			sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach(Vector3 v in m.normals) {
			sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
		}
		sb.Append("\n");
		foreach(Vector3 v in m.uv) {
			sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
		}
		for (int material=0; material < m.subMeshCount; material ++) {
			sb.Append("\n");
			sb.Append("usemtl ").Append(mats[material].name).Append("\n");
			sb.Append("usemap ").Append(mats[material].name).Append("\n");

			int[] triangles = m.GetTriangles(material);
			for (int i=0;i<triangles.Length;i+=3) {
				sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
					triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
			}
		}
		return sb.ToString();
	}

	public static void MeshToFile(MeshFilter mf, string filename) {
		using (StreamWriter sw = new StreamWriter(filename)) 
		{
			sw.Write(MeshToString(mf));
		}
	}

	private void DoubleFaces () {
		var mesh = GetComponent<MeshFilter>().mesh;
		var vertices = mesh.vertices;
		var uv = mesh.uv;
		var normals = mesh.normals;
		var szV = vertices.Length;
		var newVerts = new Vector3[szV*2];
		var newUv = new Vector2[szV*2];
		var newNorms = new Vector3[szV*2];
		for (var j=0; j< szV; j++){
			newVerts[j] = newVerts[j+szV] = vertices[j];
			newUv[j] = newUv[j+szV] = uv[j];
			newNorms[j] = normals[j];
			newNorms[j+szV] = -normals[j];
		}
		var triangles = mesh.triangles;
		var szT = triangles.Length;
		var newTris = new int[szT*2];
		for (var i=0; i< szT; i+=3){
			newTris[i] = triangles[i];
			newTris[i+1] = triangles[i+1];
			newTris[i+2] = triangles[i+2];
			var j = i+szT; 
			newTris[j] = triangles[i]+szV;
			newTris[j+2] = triangles[i+1]+szV;
			newTris[j+1] = triangles[i+2]+szV;
		}
		mesh.vertices = newVerts;
		mesh.uv = newUv;
		mesh.normals = newNorms;
		mesh.triangles = newTris; // assign triangles last!
	}

}
