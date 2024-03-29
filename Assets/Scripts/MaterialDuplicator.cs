using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialDuplicator : MonoBehaviour {

	public Color materialColor = Color.red;
	public Texture altTexture = null;
	public Shader altShader = null;

	void Start () {
		Renderer rend = GetComponent<Renderer> ();

		if (rend != null) {
			if (rend.material != null) {
				Material duplMat = new Material (rend.material);
				duplMat.color = materialColor;
				if (altTexture != null)
					duplMat.mainTexture = altTexture;
				if (altShader != null)
					duplMat.shader = altShader;
				rend.material = duplMat;
			}
		}
	}
}
