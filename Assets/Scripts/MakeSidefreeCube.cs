using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeSidefreeCube : MonoBehaviour {

	// this assumes the mesh is a typical cube object. Unfortunately depending on *which* cube it is, side 1,2,3 etc may refer to different sides. This is why they are not named sideLeft, sideTop etc

	public bool side1 = true;	
	public bool side2 = true;	
	public bool side3 = true;	
	public bool side4 = true;	
	public bool side5 = true;	
	public bool side6 = true;	

	private void Start () {
		var mesh = GetComponent<MeshFilter>().mesh;
		var triangles = mesh.triangles;

		int nofNew = 0;
		nofNew += side1 ? 2 : 0;
		nofNew += side2 ? 2 : 0;
		nofNew += side3 ? 2 : 0;
		nofNew += side4 ? 2 : 0;
		nofNew += side5 ? 2 : 0;
		nofNew += side6 ? 2 : 0;
		nofNew *= 3;

		var newTris = new int[nofNew];
		int j = 0;

		if (side1) {
			for (int i = 0; i < 3 * 2; i++) {
				newTris [j++] = triangles [i];
			}
		}
		if (side2) {
			for (int i = 3 * 2; i < 3 * 4; i++) {
				newTris [j++] = triangles [i];
			}
		}
		if (side3) {
			for (int i = 3 * 4; i < 3 * 6; i++) {
				newTris [j++] = triangles [i];
			}
		}
		if (side4) {
			for (int i = 3 * 6; i < 3 * 8; i++) {
				newTris [j++] = triangles [i];
			}
		}
		if (side5) {
			for (int i = 3 * 8; i < 3 * 10; i++) {
				newTris [j++] = triangles [i];
			}
		}
		if (side6) {
			for (int i = 3 * 10; i < 3 * 12; i++) {
				newTris [j++] = triangles [i];
			}
		}

		mesh.triangles = newTris;
	}

}
