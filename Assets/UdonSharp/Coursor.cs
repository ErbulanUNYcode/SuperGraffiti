
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Coursor : UdonSharpBehaviour
{
	[SerializeField]
	private LineRenderer laser;
	[SerializeField]
	private Image point;
	[SerializeField]
	private Transform direction;

	public Vector2 GetPos(Canvas canvas, Vector3 handPos, Quaternion dir)
	{
		transform.rotation = dir;

		Vector3 forward = direction.position.normalized;

		RectTransform rect = canvas.GetComponent<RectTransform>();

		// Плоскость канваса
		Plane plane = new Plane(rect.forward, rect.position);

		float enter;
		Ray ray = new Ray(handPos, forward);

		if (plane.Raycast(ray, out enter))
		{
			Vector3 hitPoint = ray.GetPoint(enter);

			// локальные координаты относительно канваса
			Vector3 local = rect.InverseTransformPoint(hitPoint);

			Vector2 size = rect.rect.size;

			// перевод в координаты UI (0..width, 0..height)
			Vector2 result = new Vector2(
				local.x + size.x * 0.5f,
				local.y + size.y * 0.5f
			);

			Visualize(handPos, result, canvas, true);
			return result - Vector2.up * 110;
		}

		Visualize(Vector3.zero, Vector2.zero, canvas, false);
		return Vector2.zero;
	}

	private void Visualize(Vector3 handPos, Vector2 pos, Canvas canvas, bool isHit)
	{
		if (!isHit)
		{
			point.enabled = false;
			return;
		}

		point.enabled = true;
		point.transform.parent = canvas.transform;
		point.transform.localRotation = Quaternion.identity;
		point.transform.localPosition = new Vector3(pos.x - canvas.GetComponent<RectTransform>().rect.width / 2f, pos.y - canvas.GetComponent<RectTransform>().rect.height / 2f, 0f);
		laser.SetPositions(new Vector3[] { handPos, canvas.transform.TransformPoint(point.transform.localPosition) });
	}

	private void Update()
	{
		laser.enabled = point.enabled && point.gameObject.activeInHierarchy;
	}
}
