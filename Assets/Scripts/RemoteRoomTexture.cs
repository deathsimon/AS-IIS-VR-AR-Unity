using UnityEngine;
using System.Collections;

public class RemoteRoomTexture : MonoBehaviour {

	public Renderer[] roomSideRenderers;
	int[] roomSideTextureSizes;

	public void Awake () {
		RoomFusion.GetInstance().Init();
	}
	// Use this for initialization
	void Start () {
		roomSideTextureSizes = new int[6];
		InitTextures ();
	}
	
	// Update is called once per frame
	void Update () {
		if (RoomFusion.GetInstance ().UpdateRemoteRoom ()) {
			LoadTextures ();
		}
	}

	void InitTextures(){
		for (int i = 0; i < 6; i++) {
			Texture2D texture = new Texture2D(RoomFusion.GetInstance().REMOTE_BOX_DIM[i * 2 + 0], RoomFusion.GetInstance().REMOTE_BOX_DIM[i * 2 + 1], TextureFormat.BGRA32, false);
			roomSideRenderers [i].material.mainTexture = texture;
			roomSideTextureSizes[i] = RoomFusion.GetInstance().REMOTE_BOX_DIM[i * 2 + 0] * RoomFusion.GetInstance().REMOTE_BOX_DIM[i * 2 + 1] * 4;
		}
	}

	void LoadTextures(){
		for (int i = 0; i < 6; i++) {
			Texture2D texture = (Texture2D)(roomSideRenderers [i].material.mainTexture);
			texture.LoadRawTextureData(RoomFusion.GetInstance().GetRemoteRoomTexturePtr(i), roomSideTextureSizes[i]);	
			texture.Apply();
		}
	}
}
