
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Scaler : UdonSharpBehaviour
{
	private VRCPlayerApi localPlayer;

	[SerializeField] private Transform saveSphere;
	private Vector3 saveSphereScale;

	[SerializeField] private Material floor;
	private float gridSize;
	private float fadeDistance;

	private float runSpeed;
	private float walkSpeed;
	private float strafeSpeed;

	private void Start()
	{
		localPlayer = Networking.LocalPlayer;

		saveSphereScale = saveSphere.localScale / 1.5f;

		runSpeed = localPlayer.GetRunSpeed() + 0.5f;
		walkSpeed = localPlayer.GetWalkSpeed() + 0.5f;
		strafeSpeed = localPlayer.GetStrafeSpeed() + 0.5f;

		var scale = localPlayer.GetAvatarEyeHeightAsMeters();

		saveSphere.localScale = saveSphereScale * scale;

		floor.SetFloat("_Scale", scale);

		localPlayer.SetRunSpeed(runSpeed * scale);
		localPlayer.SetWalkSpeed(walkSpeed * scale);
		localPlayer.SetStrafeSpeed(strafeSpeed * scale);
	}

	public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float prevEyeHeightAsMeters)
	{
		if (!player.isLocal) return;

		var scale = player.GetAvatarEyeHeightAsMeters();

		saveSphere.localScale = saveSphereScale * scale;

		floor.SetFloat("_Scale", scale);

		localPlayer.SetRunSpeed(runSpeed * scale);
		localPlayer.SetWalkSpeed(walkSpeed * scale);
		localPlayer.SetStrafeSpeed(strafeSpeed * scale);
	}
}
