using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ReadyPlayerMe;

public class VoiceChatManager : MonoBehaviourPunCallbacks
{
	string appID = "b814293d74bb43d4bc2642f6fb8070b6";

	public static VoiceChatManager Instance;

	IRtcEngine rtcEngine;
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

	void Awake()
	{
		if (Instance)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	void Start()
	{
		if (string.IsNullOrEmpty(appID))
		{
			Debug.LogError("App ID not set in VoiceChatManager script");
			return;
		}

		rtcEngine = IRtcEngine.GetEngine(appID);

		rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
		rtcEngine.OnLeaveChannel += OnLeaveChannel;
		rtcEngine.OnError += OnError;
		rtcEngine.EnableLocalVoicePitchCallback(100);
		rtcEngine.OnLocalVoicePitchInHz += OnLocalVoicePitchInHz;

		rtcEngine.EnableSoundPositionIndication(true);
	}
	void OnLocalVoicePitchInHz( int pitch)
	{
		Debug.LogError(pitch);
		PhotonView[] photonViews = GameObject.FindObjectsOfType<PhotonView>();

		foreach (var pv in photonViews)
		{
			if(pv.IsMine)
			{
				pv.RPC("ChatMessage", RpcTarget.All, pv.ViewID, pitch);
			}
		}

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
			Debug.LogError("index :"+index);

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
	void OnError(int error, string msg)
	{
		Debug.LogError("Error with Agora: " + msg+" Error code :"+error);
	}

	void OnLeaveChannel(RtcStats stats)
	{
		Debug.Log("Left channel with duration " + stats.duration);
	}

	void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
	{
		Debug.Log("Joined channel " + channelName);

		Hashtable hash = new Hashtable();
		hash.Add("agoraID", uid.ToString());
		PhotonNetwork.SetPlayerCustomProperties(hash);
	}

	public IRtcEngine GetRtcEngine()
	{
		return rtcEngine;
	}

	public override void OnJoinedRoom()
	{
		rtcEngine.JoinChannel(PhotonNetwork.CurrentRoom.Name);
	}

	public override void OnLeftRoom()
	{
		rtcEngine.LeaveChannel();
	}

	void OnDestroy()
	{
		IRtcEngine.Destroy();
	}
}