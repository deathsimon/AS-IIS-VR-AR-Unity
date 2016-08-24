using UnityEngine;
using System;
using System.Collections;

public class FusedImageToTexture : MonoBehaviour {

	// 是否顯示觀察者房間
	public bool textureVisible = true;
	// 此影像對應的眼睛(0 = 左眼，1 = 右眼)
	public int eye;


	private int textureSize;
	Texture2D texture;
	// Use this for initialization
	public void Awake () {
		RoomFusion.GetInstance().Init();
	}
	public void Start () {
		
		texture = new Texture2D(RoomFusion.GetInstance().GetImageWidth(), RoomFusion.GetInstance().GetImageHeight(), TextureFormat.BGRA32, false);
		textureSize = RoomFusion.GetInstance ().GetImageSize ();
		GetComponent<Renderer>().material.mainTexture = texture;
		RoomFusion.GetInstance ().SetD3D11TexturePtr (eye, texture.GetNativeTexturePtr ());
	}

	// Update is called once per frame
	void Update () {
		// 呼叫更新，如果有影像變更，就會回傳true
		if (RoomFusion.GetInstance ().Update (eye)) {
			if (!RoomFusion.GetInstance ().IsD3DInterop ()) {
				// 如果沒有CUDA-D3D interop，就只能進行CPU複製
				LoadTexture (); // Deprecated: this is the slow way: copy from cpu memory
			}
		}
		// 檢查是不是要隱藏觀察者房間
		if (Input.GetKeyDown (KeyCode.Space)) {
			textureVisible = !textureVisible;
		}
		if (!textureVisible) {
			GetComponent<Renderer> ().material.SetColor ("_Color", new Color (0, 0, 0, 0));
		} else {
			GetComponent<Renderer> ().material.SetColor ("_Color", new Color (1, 1, 1, 1));
		}
	}
	// 進行CPU的記憶體複製，來更新觀察者房間的景象
	public void LoadTexture(){
		// Load data into the texture and upload it to the GPU.
		texture.LoadRawTextureData(RoomFusion.GetInstance().GetCulledImagePtr(eye), textureSize);
		texture.Apply();
	}

	void OnApplicationQuit() {
		if (eye == RoomFusion.EYE_LEFT) {
			RoomFusion.GetInstance ().Destroy ();
		}
	}
		

}
