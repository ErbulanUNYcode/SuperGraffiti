
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CameraShaderSetter : UdonSharpBehaviour
{
	[SerializeField]
	private Shader shader;
	[SerializeField]
	private Camera[] cams;
	private void Start()
	{
		foreach (var cam in cams)
			cam.SetReplacementShader(shader, null);
	}
}
