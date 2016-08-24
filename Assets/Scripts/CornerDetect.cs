using UnityEngine;
using System.Collections;

public class CornerDetect : MonoBehaviour {

	// 四個定位角落的物件 (TargetLT那些)
	public GameObject[] targets;
	// 四個定位點用的按鍵
	KeyCode[] keycodes;

	// 四個角落
	Vector3[] corners;
	Vector2[] lastPixelCorners;

	// 上一個設定的角落
	int lastControlIndex = -1;
	// 檢測是不是4個角落都有設定了
	int corner_ready = 0x0;

	// 3D到2D轉換的相關變數
	float backScale = 4.0f;
	float scaleShift;

	// 螢幕寬高
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
	
	// 偵測並找出四個點
	void DetectCorner () {
		
		for (int i = 0; i < corners.Length; i++) {
			// 從各角落射出射線到相機，找出與顯示平面的交叉點
			RaycastHit hit;
			Vector3 direction = Camera.main.transform.position - corners[i];
			if (Physics.Raycast (corners[i], direction, out hit, Mathf.Infinity, 0x1 << 8)) {
				Vector3 point = hit.point;
				targets [i].transform.position = point; 
				lastPixelCorners[i] = WorldToPixel (point);
				// 把轉出來的2D螢幕座標送到C extension
				RoomFusion.GetInstance ().SetCorrectionPixel (i, lastPixelCorners[i].x, lastPixelCorners[i].y);
			}
			if (Input.GetKeyDown (KeyCode.A)) {
				Debug.Log ("Cornor[" + i + "] = " + lastPixelCorners[i]);
			}

		}
		// 檢查是否四個角落都有設定了，若有才計算四邊形並套用深度，否則關閉深度套用
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
			// 檢查按鍵輸入，看看是否有要設定角落
			if(Input.GetKeyDown(keycodes[i])){
				targets[i].SetActive(true);
				corners[i] = Camera.main.transform.position + Camera.main.transform.forward * 100;
				lastControlIndex = i;
				corner_ready |= 0x1 << i;
			}
		}
		// 滑鼠點擊設定角落
		// corner adjust
		if(lastControlIndex >= 0){
			// mouse set corner
			if(Input.GetMouseButtonDown(0)){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				targets [lastControlIndex].SetActive (true);

				corners[lastControlIndex] = Camera.main.transform.position + ray.direction * 100;
			}

		}
	}

	// 把打在平面上的世界座標轉換為2D畫面座標
	Vector2 WorldToPixel(Vector3 point){
		Vector3 local = transform.InverseTransformPoint (point);
		Vector2 pixel = new Vector2();
		pixel.x = (local.x + 5.0f) * screenWidth * 0.1f * backScale - scaleShift * screenWidth;
		pixel.y = (local.z + 5.0f) * screenHeight * 0.1f * backScale - scaleShift * screenHeight;
		return pixel;
	}
}
