using UnityEngine;
using System.Collections;

public class EyeDisplaySelector : MonoBehaviour {


	public bool useDoubleEye = false;
	public GameObject leftCameraAnchor;
	public GameObject rightCameraAnchor;

	void Start () {
		updateEyeMode ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.M)) {
			// 切換單眼、雙眼模式
			useDoubleEye = !useDoubleEye;
			updateEyeMode ();

		}
	}

	void updateEyeMode(){
		if (useDoubleEye) {
			switchToDoubleEye ();
		} else {
			switchToSingleEye ();
		}
	}

	void switchToDoubleEye(){
		Debug.Log ("Use double Eye");
		leftCameraAnchor.GetComponent<Camera> ().stereoTargetEye = StereoTargetEyeMask.Left;
		rightCameraAnchor.GetComponent<Camera> ().stereoTargetEye = StereoTargetEyeMask.Right;
	}

	void switchToSingleEye(){
		Debug.Log ("Use single Eye");	
		leftCameraAnchor.GetComponent<Camera> ().stereoTargetEye = StereoTargetEyeMask.Both;
		rightCameraAnchor.GetComponent<Camera> ().stereoTargetEye = StereoTargetEyeMask.None;
	}
}
