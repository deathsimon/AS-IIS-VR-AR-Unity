using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
	public GameObject trackingArea;

	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	void OnGUI()
	{
		drawFPS ();
		drawTracking ();
	}

	void drawFPS(){

		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 50;
		style.normal.textColor = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("Player: {0:0.0} ms ({1:0.} fps), Socket: {2:0.0} ms, Threshold: {3:0.0} meter", msec, fps, RoomFusion.GetInstance().GetSocketDelay() * 1000.0f, trackingArea.GetComponent<DepthCorrection>().depthThreshold);
		GUI.Label(rect, text, style);
	}

	void drawTracking(){
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, h * 2 / 50, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 50;
		style.normal.textColor = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		string text = "";
		if (!RoomFusion.GetInstance ().trackingValid) {
			text = "Tracking LOST!";
		}
		GUI.Label(rect, text, style);
	}
}