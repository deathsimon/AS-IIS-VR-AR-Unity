using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class CameraTracking : MonoBehaviour {
	public void Awake () {
		RoomFusion.GetInstance().Init();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		// 利用ZED的position tracking來更新此物件的位置
		if (RoomFusion.GetInstance ().UpdateTracking ()) {
			transform.localPosition = RoomFusion.GetInstance ().translation;
			//transform.rotation = Quaternion.Inverse (UnityEngine.VR.InputTracking.GetLocalRotation (UnityEngine.VR.VRNode.CenterEye)) * RoomFusion.GetInstance ().rotation;
		}
	}

	void Update () {
		// 重新定位偵測
		if (Input.GetKeyDown (KeyCode.N)) {
			RoomFusion.GetInstance ().ResetTracking ();
			Debug.Log ("Recentering");
			InputTracking.Recenter ();
		}
	}
}
