using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerSyncComponent : UdonSharpBehaviour
{
	[UdonSynced] private bool[] isSynced;
	[SerializeField] private ReadRenderTexture sync;

	void Start()
	{
		if (!Networking.IsOwner(gameObject))
		{
			sync.AddPlayer(this);
			return;
		}

		isSynced = new bool[144];
		sync.SetOwnComponent(this);
	}

	public override void OnDeserialization()
	{
		Debug.Log("OnDeserialization and " + (Networking.IsOwner(gameObject) ? "owner" : "not owner"));
	}
}