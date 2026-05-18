
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class Ruler : UdonSharpBehaviour
{
	[SerializeField] private VRC_Pickup pickup;
	[SerializeField] private Transform child;
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private LineRenderer link;

	private VRCPlayerApi localPlayer;
	private Vector3[] circlePoints = new Vector3[64];
	private Vector3[] linearPoints = new Vector3[] { new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0) };
	private Vector3[] flattenPoints = new Vector3[]
	{
		new Vector3(-0.5f, -0.45f, 0),
		new Vector3(-0.485f, -0.485f, 0),
		new Vector3(-0.45f, -0.5f, 0),
		new Vector3(0.45f, -0.5f, 0),
		new Vector3(0.485f, -0.485f, 0),
		new Vector3(0.5f, -0.45f, 0),
		new Vector3(0.5f, 0.45f, 0),
		new Vector3(0.485f, 0.485f, 0),
		new Vector3(0.45f, 0.5f, 0),
		new Vector3(-0.45f, 0.5f, 0),
		new Vector3(-0.485f, 0.485f, 0),
		new Vector3(-0.5f, 0.45f, 0)
};

	private void Start()
	{
		NextMode();
		NextMode();
		localPlayer = Networking.LocalPlayer;

		syncMode = (int)mode;
	}

	private void Update()
	{
		if (!pickup.IsHeld)
		{
			return;
		}

		if (pickup.currentPlayer != localPlayer)
		{
			Networking.SetOwner(pickup.currentPlayer, gameObject);
			return;
		}
	}

	private Vector3 position;
	private float radius;

	[UdonSynced] private int syncMode = 0;
	private RulerMode mode = RulerMode.Free;

	public override void OnDeserialization()
	{
		mode = (RulerMode)syncMode;
		if (mode == RulerMode.Free)
		{
			lineRenderer.enabled = false;
		}

		if (mode == RulerMode.Linear)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = 2;
			lineRenderer.loop = false;
		}

		if (mode == RulerMode.Circle)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = circlePoints.Length;
			lineRenderer.loop = true;
		}

		if (mode == RulerMode.Flatten)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = flattenPoints.Length;
			lineRenderer.loop = true;
		}
	}

	public void NextMode()
	{
		mode = (RulerMode)(((int)mode + 1) % 4);
		syncMode = (int)mode;

		if (mode == RulerMode.Free)
		{
			lineRenderer.enabled = false;
		}

		if (mode == RulerMode.Linear)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = 2;
			lineRenderer.loop = false;
		}

		if (mode == RulerMode.Circle)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = circlePoints.Length;
			lineRenderer.loop = true;
		}

		if (mode == RulerMode.Flatten)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = flattenPoints.Length;
			lineRenderer.loop = true;
		}
	}

	public void PrevMode()
	{
		mode = (RulerMode)(((int)mode + 3) % 4);
		syncMode = (int)mode;

		if (mode == RulerMode.Free)
		{
			lineRenderer.enabled = false;
		}

		if (mode == RulerMode.Linear)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = 2;
			lineRenderer.loop = false;
		}

		if (mode == RulerMode.Circle)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = circlePoints.Length;
			lineRenderer.loop = true;
		}

		if (mode == RulerMode.Flatten)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = flattenPoints.Length;
			lineRenderer.loop = true;
		}
	}

	public void SetPosition(Vector3 pos)
	{
		link.SetPositions(new Vector3[] { pos, transform.position });
		child.position = pos;
		position = child.localPosition;
		radius = new Vector2(child.localPosition.x, child.localPosition.y).magnitude;

		if (mode == RulerMode.Linear)
		{
			for (int i = 0; i < linearPoints.Length; i++)
			{
				linearPoints[i].y = position.y;
				linearPoints[i].z = position.z;

				lineRenderer.SetPositions(linearPoints);
			}

			return;
		}

		if (mode == RulerMode.Circle)
		{
			for (int i = 0; i < circlePoints.Length; i++)
			{
				circlePoints[i].z = position.z;
				var a = (float)i / (float)circlePoints.Length * Mathf.PI * 2;
				circlePoints[i].x = Mathf.Sin(a) * radius;
				circlePoints[i].y = Mathf.Cos(a) * radius;

				lineRenderer.SetPositions(circlePoints);
			}

			return;
		}

		if (mode == RulerMode.Flatten)
		{
			for (int i = 0; i < flattenPoints.Length; i++)
			{
				flattenPoints[i].z = position.z;
				lineRenderer.SetPositions(flattenPoints);
			}
		}
	}

	public Vector3 GetPosition(Vector3 pos)
	{
		child.position = pos;
		var vec = child.localPosition;

		if (mode == RulerMode.Linear)
		{
			vec.y = position.y;
			vec.z = position.z;
			child.localPosition = vec;
			return child.position;
		}

		if (mode == RulerMode.Circle)
		{
			vec.z = 0;
			vec = vec.normalized * radius;
			vec.z = position.z;
			child.localPosition = vec;
			return child.position;
		}

		if (mode == RulerMode.Flatten)
		{
			vec.z = position.z;
			child.localPosition = vec;
			return child.position;
		}

		if (mode == RulerMode.Point)
		{
			child.localPosition = position;
			return child.position;
		}

		return pos;
	}

	public override void InputUse(bool value, UdonInputEventArgs args)
	{
		if (!value) return;
		if (!pickup.IsHeld) return;
		if (pickup.currentPlayer != Networking.LocalPlayer) return;
		if ((pickup.currentHand == VRC_Pickup.PickupHand.Right) != (args.handType == HandType.RIGHT)) return;

		NextMode();
	}
}

public enum RulerMode
{
	Free,//свободный
	Linear,//линейка
	Circle,//циркуль
	Flatten,//плоский
	Point//точка
}
