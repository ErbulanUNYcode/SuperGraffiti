using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ReadRenderTexture : UdonSharpBehaviour
{
	[SerializeField] private CustomRenderTexture CRT;
	[SerializeField] private Material CRTMaterial;
	private Texture2D syncTex;
	private Color32[] data;

	private void Awake()
	{
		syncTex = new Texture2D(CRT.width, CRT.height, TextureFormat.ARGB32, false);
		syncTex.filterMode = FilterMode.Bilinear;
		syncTex.wrapMode = TextureWrapMode.Clamp;
		data = new Color32[CRT.width * CRT.height];
	}
	private bool taskComplete;
	public void Read(Vector2 tile)
	{
		taskComplete = false;
		CRTMaterial.SetVector("_Tile", (Vector4)tile);
		CRT.Update();
		VRCAsyncGPUReadback.Request(CRT, 0, (IUdonEventReceiver)this);
	}
	public override void OnAsyncGpuReadbackComplete(VRCAsyncGPUReadbackRequest request)
	{
		if (request.hasError)
		{
			Debug.LogError("GPU readback error!");
			taskComplete = true;
			return;
		}
		else
		{
			request.TryGetData(data);
			//myComponent.SendData(data);
		}
		taskComplete = true;
	}

	public override void OnDeserialization()
	{
		syncTex.SetPixels32(data);
		syncTex.Apply();
	}

	private bool synced;

	private void Update()
	{
		if (synced) return;
		if (!taskComplete) return;
	}

	private PlayerSyncComponent myComponent;

	public void SetOwnComponent(PlayerSyncComponent myComponent)
	{
		this.myComponent = myComponent;
	}

	private PlayerSyncComponent[] players;

	public void AddPlayer(PlayerSyncComponent player)
	{
		var newPlayers = new PlayerSyncComponent[players.Length + 1];
		Array.Copy(players, newPlayers, players.Length);
		newPlayers[players.Length] = player;
		players = newPlayers;
	}
	public bool RemovePlayer(PlayerSyncComponent player)
	{
		int index = Array.IndexOf(players, player);

		if (index < 0)
			return false;

		var newPlayers = new PlayerSyncComponent[players.Length - 1];

		if (index > 0)
			Array.Copy(players, 0, newPlayers, 0, index);

		int tailLength = players.Length - index - 1;

		if (tailLength > 0)
			Array.Copy(players, index + 1, newPlayers, index, tailLength);

		players = newPlayers;

		return true;
	}
}