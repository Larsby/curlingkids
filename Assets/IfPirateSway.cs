using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfPirateSway : MonoBehaviour {
	
	// Use this for initialization
	float x = 32.9f;
	float zstart = -5.5f;
	float zend = 5.5f;
	float toZ = 0.0f;
	int i = 0;
	void Start () {
		bool isPirate = StaticManager.GetNumberOfCredits() > 200000;

 		if(isPirate) {
			float chanceOfGettingHaxEffect = Random.Range(0.0f, 1.0f);
			if (chanceOfGettingHaxEffect >= 0.5f)
			{

				float fxSelector = Random.Range(0.0f, 1.0f);
				if (fxSelector < 0.8f)
				{
					toZ = zstart;
					rotateZ();
				} else {
					rotateX();
				}
			}
		}
	}
	public void rotateX()
	{
		i++;
		if(i%2==0) {
			x = 36.9f;

		} else {
			x= 32.9f;
		}
		if (i > 999) i = 0;
		iTween.RotateTo(gameObject,iTween.Hash("x",x,"time",10,"delay",1,"oncomplete","rotateX","looptype",iTween.LoopType.none));

	}
	public void rotateZ()
	{

		float to = toZ;

		iTween.RotateTo(gameObject, iTween.Hash("z", to, "time", 10, "oncomplete", "rotateZ", "looptype", iTween.LoopType.none,"easetype",iTween.EaseType.linear));
		i++;
		if (i % 2 == 0)
		{
			toZ = zstart;

		}
		else
		{
			toZ = zend;
		}

	}
	// Update is called once per frame
	void Update () {
		
	}
}
