using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCount : MonoBehaviour{

	int frameCount = 0;
	int fps;
	float lastTime = 0;

	public UnityEngine.UI.Text text;

	void Awake(){
		lastTime = Time.realtimeSinceStartup;
	}

	void Update(){
		double time_now = Time.realtimeSinceStartup;

		frameCount++;
		if (time_now - lastTime > 1.0f){
			fps = frameCount;
			lastTime += 1.0f;
			frameCount = 0;
		}

		text.text = "FPS " + fps;
	}
}