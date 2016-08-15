using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

public class RoomFusion
{
	public int[] REMOTE_BOX_DIM;

	public const int RECT_LT = 0;
	public const int RECT_RT = 1;
	public const int RECT_LD = 2;
	public const int RECT_RD = 3;

	public bool trackingValid;

	public const float UNIT_CONVERT_RATE = 28.0f;

	// tracking
	unsafe private float* path;
	private Matrix4x4 rt;
	public Vector3 translation;
	public Quaternion rotation;

	const string nameDll = "RoomFusionDLL";
	public static RoomFusion instance = null;
	private bool ready = false;

	[DllImport(nameDll,EntryPoint = "rf_init")]
	private static extern void rf_init();

	[DllImport(nameDll,EntryPoint = "rf_destroy")]
	private static extern void rf_destroy();

	[DllImport(nameDll,EntryPoint = "rf_update")]
	private static extern int rf_update();

	[DllImport(nameDll,EntryPoint = "rf_getCulledImagePtr")]
	private static extern IntPtr rf_getCulledImagePtr();

	[DllImport(nameDll,EntryPoint = "rf_getImageSize")]
	private static extern int rf_getImageSize();

	[DllImport(nameDll,EntryPoint = "rf_getImageWidth")]
	private static extern int rf_getImageWidth();

	[DllImport(nameDll,EntryPoint = "rf_getImageHeight")]
	private static extern int rf_getImageHeight();

	[DllImport(nameDll,EntryPoint = "rf_setApplyDepth")]
	private static extern void rf_setApplyDepth(int result);

	[DllImport(nameDll,EntryPoint = "rf_setCorrectionPixel")]
	private static extern void rf_setCorrectionPixel(int position, float w, float h);

	[DllImport(nameDll,EntryPoint = "rf_computeCorrection")]
	private static extern void rf_computeCorrection();

	[DllImport(nameDll,EntryPoint = "rf_getPositionPtr")]
	unsafe private static extern float* rf_getPositionPtr();

	[DllImport(nameDll,EntryPoint = "rf_setD3D11TexturePtr")]
	private static extern void rf_setD3D11TexturePtr(IntPtr ptr);

	[DllImport(nameDll,EntryPoint = "rf_getZedFPS")]
	private static extern float rf_getZedFPS();

	[DllImport(nameDll,EntryPoint = "rf_getRemoteRoomTexturePtr")]
	private static extern IntPtr rf_getRemoteRoomTexturePtr(int side);

	[DllImport(nameDll,EntryPoint = "rf_getSocketDelay")]
	private static extern float rf_getSocketDelay();

	[DllImport(nameDll,EntryPoint = "rf_updateRemoteRoom")]
	private static extern int rf_updateRemoteRoom();

	[DllImport(nameDll,EntryPoint = "rf_isD3DInterop")]
	private static extern int rf_isD3DInterop();

	public static RoomFusion GetInstance()
	{
		if (instance == null)
		{
			instance = new RoomFusion();
		}

		return instance;
	}

	public RoomFusion(){
		REMOTE_BOX_DIM = new int[12]{
			1024, 259, 
			1024, 259,
			826, 259, 
			826, 259, 
			1024, 826, 
			1024, 826
		};
	}

	public void Init(){

		if (!ready) {
			rf_init ();
			ready = true;
		} else {
			//Debug.Log ("Already Initialized");
		}
	}

	public void Destroy(){
		if (ready) {
			rf_destroy ();
		} else {
			Debug.Log ("Not yet Initialized");
		}
	}

	public bool Update(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_update () == 1;
	}

	public IntPtr GetCulledImagePtr(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getCulledImagePtr ();
	}

	public int GetImageSize(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getImageSize ();
	}

	public int GetImageWidth(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getImageWidth ();
	}

	public int GetImageHeight(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getImageHeight ();
	}
		
	public void SetApplyDepth(bool result){
		rf_setApplyDepth (result ? 1 : 0);
	}

	public void SetCorrectionPixel(int position, float w, float h){
		rf_setCorrectionPixel (position, w, h);
	}

	public void ComputeCorrection(){
		rf_computeCorrection ();
	}

	unsafe public float* GetPositionPtr(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getPositionPtr ();
	}

	unsafe public bool UpdateTracking(){
		path = GetPositionPtr ();
		if (path != null) {
			translation.x = path[3] * UNIT_CONVERT_RATE; 
			translation.y = path[7] * UNIT_CONVERT_RATE;
			translation.z = path[11] * UNIT_CONVERT_RATE;
			//Debug.Log ("Tracking Good:" + translation);
			for (int i = 0; i < 4; ++i)
			{
				for (int j = 0; j < 4; ++j)
				{
					rt[i, j] = path[i * 4 + j]; 
				}
			}

			rotation.w = Mathf.Sqrt(Mathf.Max(0, 1 + rt[0, 0] + rt[1, 1] + rt[2, 2])) / 2;
			rotation.x = Mathf.Sqrt(Mathf.Max(0, 1 + rt[0, 0] - rt[1, 1] - rt[2, 2])) / 2;
			rotation.y = Mathf.Sqrt(Mathf.Max(0, 1 - rt[0, 0] + rt[1, 1] - rt[2, 2])) / 2;
			rotation.z = Mathf.Sqrt(Mathf.Max(0, 1 - rt[0, 0] - rt[1, 1] + rt[2, 2])) / 2;
			rotation.x *= Mathf.Sign(rotation.x * (rt[2, 1] - rt[1, 2]));
			rotation.y *= Mathf.Sign(rotation.y * (rt[0, 2] - rt[2, 0]));
			rotation.z *= Mathf.Sign(rotation.z * (rt[1, 0] - rt[0, 1]));

			trackingValid = true;
			return true;
		}
		else {
			trackingValid = false;
			return false;	
		}
	}

	public void SetD3D11TexturePtr(IntPtr ptr){
		rf_setD3D11TexturePtr(ptr);
	}

	public float GetZedPFS(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getZedFPS ();
	}

	public IntPtr GetRemoteRoomTexturePtr(int side){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getRemoteRoomTexturePtr (side);
	}
	public float GetSocketDelay(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_getSocketDelay ();
	}

	public bool UpdateRemoteRoom(){
		if (!ready) {
			throw new System.Exception ("RoomFusion not ready. Must call Init first.");
		}
		return rf_updateRemoteRoom () == 1;
	}

	public bool IsD3DInterop(){
		return rf_isD3DInterop () == 1;
	}
}

