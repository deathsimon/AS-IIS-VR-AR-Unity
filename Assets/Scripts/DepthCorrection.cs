using UnityEngine;
using System.Collections;

public class DepthCorrection : MonoBehaviour {

	public float depthThreshold;
	public float initDepthThreshold;
	public Vector3 initPosition;
	public Vector3 wallDirection;

	public GameObject targetBall;
	public GameObject cornerDetectPlane;

	float backScale = 4.0f;
	float scaleShift;

	int screenWidth;
	int screenHeight;

	// Use this for initialization
	void Start () {
		initPosition = transform.position;
		screenWidth = RoomFusion.GetInstance ().GetImageWidth ();
		screenHeight = RoomFusion.GetInstance ().GetImageHeight ();
		scaleShift = backScale / 2.0f - 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.B)) {
			PerformCorrection ();
		}
		// update movement correction
		Vector3 displacement = transform.position - initPosition;
		Vector3 projection = Vector3.Project (displacement, wallDirection);
		float distance = projection.magnitude / RoomFusion.UNIT_CONVERT_RATE;
		if (Vector3.Dot (displacement, wallDirection) > 0) {
			depthThreshold = initDepthThreshold - distance;	
		} else {
			depthThreshold = initDepthThreshold + distance;	
		}

		// set RF depth
		RoomFusion.GetInstance ().SetDepthThreshold (depthThreshold);
	}
		
	void PerformCorrection(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (ray.origin, ray.direction, out hit, Mathf.Infinity, 0x1 << 10)) {
			Vector3 point = hit.point;
			targetBall.transform.position = ray.origin + ray.direction * 5;
			Vector2 pos = WorldToPixel (point);
			initDepthThreshold = RoomFusion.GetInstance ().GetDepth (pos[0], pos[1]);
			initPosition = transform.position;
			targetBall.SetActive (true);
			wallDirection = ray.direction;
		}
	}

	Vector2 WorldToPixel(Vector3 point){
		Vector3 local = cornerDetectPlane.transform.InverseTransformPoint (point);
		Vector2 pixel = new Vector2();
		pixel.x = (local.x + 5.0f) * screenWidth * 0.1f * backScale - scaleShift * screenWidth;
		pixel.y = (local.z + 5.0f) * screenHeight * 0.1f * backScale - scaleShift * screenHeight;
		return pixel;
	}
}
