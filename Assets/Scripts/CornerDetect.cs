using UnityEngine;
using System.Collections;

public class CornerDetect : MonoBehaviour {

	public GameObject[] targets;
	KeyCode[] keycodes;

	Vector3[] corners;
	Vector2[] lastPixelCorners;

	int lastControlIndex = -1;
	int corner_ready = 0x0;

	float backScale = 4.0f;
	float scaleShift;

	int screenWidth;
	int screenHeight;

	// Use this for initialization
	public void Awake () {
		RoomFusion.GetInstance().Init();
	}

	void Start () {
		screenWidth = RoomFusion.GetInstance ().GetImageWidth ();
		screenHeight = RoomFusion.GetInstance ().GetImageHeight ();
		scaleShift = backScale / 2.0f - 0.5f;

		corners = new Vector3[4];
		lastPixelCorners = new Vector2[4];
		keycodes = new KeyCode[4]{ KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V};

	}
	
	// Update is called once per frame
	void DetectCorner () {
		
		for (int i = 0; i < corners.Length; i++) {
			RaycastHit hit;
			Vector3 direction = Camera.main.transform.position - corners[i];
			if (Physics.Raycast (corners[i], direction, out hit, Mathf.Infinity, 0x1 << 8)) {
				Vector3 point = hit.point;
				targets [i].transform.position = point; //Camera.main.transform.position + Camera.main.transform.forward * 3;
				lastPixelCorners[i] = WorldToPixel (point);
				RoomFusion.GetInstance ().SetCorrectionPixel (i, lastPixelCorners[i].x, lastPixelCorners[i].y);
			}
			if (Input.GetKeyDown (KeyCode.A)) {
				Debug.Log ("Cornor[" + i + "] = " + lastPixelCorners[i]);
			}

		}
		if (corner_ready == 0xF) {
			RoomFusion.GetInstance ().SetApplyDepth (true);
			RoomFusion.GetInstance ().ComputeCorrection ();
		} else {
			//Debug.LogError ("No enough!");
			RoomFusion.GetInstance ().SetApplyDepth (false);
			
		}
	}

	void FixedUpdate(){
		DetectCorner ();
	}

	void Update(){
		// set corner
		for(int i=0;i<4;i++){
			if(Input.GetKeyDown(keycodes[i])){
				//targets[i].transform.position = Camera.main.transform.position + Camera.main.transform.forward * 4;
				targets[i].SetActive(true);
				corners[i] = Camera.main.transform.position + Camera.main.transform.forward * 100;
				lastControlIndex = i;
				corner_ready |= 0x1 << i;
			}
		}
		// corner adjust
		if(lastControlIndex >= 0){
			// mouse set corner
			if(Input.GetMouseButtonDown(0)){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				targets [lastControlIndex].SetActive (true);
				//targets[lastControlIndex].transform.position = Camera.main.transform.position + ray.direction * 4;
				corners[lastControlIndex] = Camera.main.transform.position + ray.direction * 100;
			}

		}
	}

	Vector2 WorldToPixel(Vector3 point){
		Vector3 local = transform.InverseTransformPoint (point);
		Vector2 pixel = new Vector2();
		pixel.x = (local.x + 5.0f) * screenWidth * 0.1f * backScale - scaleShift * screenWidth;
		pixel.y = (local.z + 5.0f) * screenHeight * 0.1f * backScale - scaleShift * screenHeight;
		return pixel;
	}
}
