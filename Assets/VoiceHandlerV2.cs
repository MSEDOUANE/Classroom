// ReSharper disable RedundantUsingDirective

using System;
using UnityEngine;
using System.Collections;
using static ReadyPlayerMe.ExtensionMethods;
using UnityEngine.SceneManagement;
using Photon.Pun;
using ReadyPlayerMe;

public class VoiceHandlerV2 : MonoBehaviour
{
	private const string MOUTH_OPEN_BLEND_SHAPE_NAME = "mouthOpen";
	private const int AMPLITUDE_MULTIPLIER = 10;
	private const int AUDIO_SAMPLE_LENGTH = 4096;

	private float[] audioSample = new float[AUDIO_SAMPLE_LENGTH];
	private SkinnedMeshRenderer headMesh;
	private SkinnedMeshRenderer beardMesh;
	private SkinnedMeshRenderer teethMesh;

	private const float MOUTH_OPEN_MULTIPLIER = 100f;

	private int mouthOpenBlendShapeIndexOnHeadMesh = -1;
	private int mouthOpenBlendShapeIndexOnBeardMesh = -1;
	private int mouthOpenBlendShapeIndexOnTeethMesh = -1;
	private void Start()
	{


	}

	private void Update()
	{

	}

	[PunRPC]
	void ChatMessage(int a, int b, PhotonMessageInfo info)
	{
		// the photonView.RPC() call is the same as without the info parameter.
		// the info.Sender is the player who called the RPC.
		Debug.LogFormat("Info: {0} {1} {2}", info.Sender, info.photonView, info.SentServerTime);
		PhotonView pv = PhotonView.Find(a);
		headMesh = GetMeshAndSetIndex(pv.gameObject, MeshType.HeadMesh, ref mouthOpenBlendShapeIndexOnHeadMesh);
		beardMesh = GetMeshAndSetIndex(pv.gameObject, MeshType.BeardMesh, ref mouthOpenBlendShapeIndexOnBeardMesh);
		teethMesh = GetMeshAndSetIndex(pv.gameObject, MeshType.TeethMesh, ref mouthOpenBlendShapeIndexOnTeethMesh);

		SetBlendShapeWeights(b);
	}
	private void SetBlendShapeWeights(float weight)
	{
		SetBlendShapeWeight(headMesh, mouthOpenBlendShapeIndexOnHeadMesh);
		SetBlendShapeWeight(beardMesh, mouthOpenBlendShapeIndexOnBeardMesh);
		SetBlendShapeWeight(teethMesh, mouthOpenBlendShapeIndexOnTeethMesh);

		void SetBlendShapeWeight(SkinnedMeshRenderer mesh, int index)
		{
			Debug.LogError("index :" + index);

			if (index >= 0)
			{
				mesh.SetBlendShapeWeight(index, weight);
			}
		}
	}

	private SkinnedMeshRenderer GetMeshAndSetIndex(GameObject player, MeshType meshType, ref int index)
	{
		var mesh = player.GetMeshRenderer(meshType);
		if (mesh != null)
		{
			index = mesh.sharedMesh.GetBlendShapeIndex(MOUTH_OPEN_BLEND_SHAPE_NAME);
		}

		return mesh;
	}

}

