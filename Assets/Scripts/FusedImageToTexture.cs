using UnityEngine;
using System;
using System.Collections;

public class FusedImageToTexture : MonoBehaviour {


	public bool textureVisible = true;
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
		if (RoomFusion.GetInstance ().Update (eye)) {
			if (!RoomFusion.GetInstance ().IsD3DInterop ()) {
				LoadTexture (); // Deprecated: this is the slow way: copy from cpu memory
			}
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			textureVisible = !textureVisible;
		}
		if (!textureVisible) {
			GetComponent<Renderer> ().material.SetColor ("_Color", new Color (0, 0, 0, 0));
		} else {
			GetComponent<Renderer> ().material.SetColor ("_Color", new Color (1, 1, 1, 1));
		}
	}
		
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
