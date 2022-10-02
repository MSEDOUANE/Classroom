using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ReadyPlayerMe;
using UnityEngine.UI;

public class VoiceChatManager : MonoBehaviourPunCallbacks
{
	string appID = "33679a9cddbd40e39240679a66422e24";

	public static VoiceChatManager Instance;
	protected Dictionary<uint, VideoSurface> UserVideoDict = new Dictionary<uint, VideoSurface>();

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
	private string token = "007eJxTYPg1J/K2y4qcxtSHS9j55pqLHr62p+Q3u7yCitYV+wTOn2oKDEkWhiZGlsYp5iZJSSbGKSZJyUZmJkZpZmlJFgbmBklmSlPNkxc+s0jeN/M6MyMDBIL4HAy+qaklpQVhRgwMALL6IhY=";

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
		rtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
		// set callbacks (optional)
		rtcEngine.SetParameters("{\"rtc.log_filter\": 65535}");




		// join channel
		rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
		rtcEngine.OnLeaveChannel += OnLeaveChannel;
		rtcEngine.OnUserJoined = OnUserJoined;
		rtcEngine.OnUserOffline = OnUserOffline;
		rtcEngine.OnError += OnError;
		rtcEngine.EnableLocalVoicePitchCallback(100);
		rtcEngine.OnLocalVoicePitchInHz += OnLocalVoicePitchInHz;
		rtcEngine.OnStreamMessageError += OnStreamMessageError;

		rtcEngine.EnableSoundPositionIndication(true);
		GameObject quad = GameObject.Find("DisplayPlane");
		if (ReferenceEquals(quad, null))
		{
			Debug.Log("Error: failed to find DisplayPlane");
			return;
		}
		else
		{
			UserVideoDict[0] = quad.AddComponent<VideoSurface>();
		}

		var button = GameObject.Find("ShareDisplayButton").GetComponent<Button>();
		if (button != null)
		{
			button.onClick.AddListener(ShareDisplayScreen);
		}
	}

	protected virtual void OnVideoSizeChanged(uint uid, int width, int height, int rotation)
	{
		Debug.LogWarningFormat("OnVideoSizeChanged width = {0} height = {1} for rotation:{2}", width, height, rotation);
		if (UserVideoDict.ContainsKey(uid))
		{
			GameObject go = UserVideoDict[uid].gameObject;
			Vector2 v2 = new Vector2(width, height);
			RawImage image = go.GetComponent<RawImage>();
			image.rectTransform.sizeDelta = v2;
		}
	}


	void OnLocalVoicePitchInHz(int pitch)
	{
		PhotonView[] photonViews = GameObject.FindObjectsOfType<PhotonView>();

		foreach (var pv in photonViews)
		{
			if (pv.IsMine)
			{
				pv.RPC("ChatMessage", RpcTarget.All, pv.ViewID, pitch);
			}
		}

	}
	// When a remote user joined, this delegate will be called. Typically
	// create a GameObject to render video on it
	protected virtual void OnUserJoined(uint uid, int elapsed)
	{
		Debug.LogError("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
		Debug.LogError("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
		string masterUid =string.Empty;

		foreach (var item in PhotonNetwork.PlayerList)
		{
			if (item.IsMasterClient)
			{
				Debug.Log(item.CustomProperties["agoraID"]);
				masterUid = item.CustomProperties["agoraID"].ToString();
			}
		}
		if (!PhotonNetwork.IsMasterClient)
		{       // configure videoSurface
			GameObject quad = GameObject.Find("DisplayPlane");

			VideoSurface videoSurface = quad.GetComponent<VideoSurface>();

			videoSurface.SetForUser(uint.Parse(masterUid));
			videoSurface.SetEnable(true);
			videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer);
			videoSurface.SetGameFps(30);
			videoSurface.EnableFilpTextureApply(enableFlipHorizontal: true, enableFlipVertical: false);
			UserVideoDict[uid] = videoSurface;

		}

	}


	protected VideoSurface makeImageSurface(string goName)
	{
		GameObject go = new GameObject();

		if (go == null)
		{
			return null;
		}

		go.name = goName;

		// to be renderered onto
		RawImage image = go.AddComponent<RawImage>();
		image.rectTransform.sizeDelta = new Vector2(1, 1);// make it almost invisible

		// make the object draggable
		GameObject canvas = GameObject.Find("Canvas");
		if (canvas != null)
		{
			go.transform.SetParent(canvas.transform);
		}
		// set up transform
		go.transform.Rotate(0f, 0.0f, 180.0f);
		go.transform.localScale = Vector3.one;

		// configure videoSurface
		VideoSurface videoSurface = go.AddComponent<VideoSurface>();
		return videoSurface;
	}

	// When remote user is offline, this delegate will be called. Typically
	// delete the GameObject for this user
	protected virtual void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
	{
		// remove video stream
		Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
		if (UserVideoDict.ContainsKey(uid))
		{
			var surface = UserVideoDict[uid];
			surface.SetEnable(false);
			UserVideoDict.Remove(uid);
			GameObject.Destroy(surface.gameObject);
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
	void OnError(int error, string msg)
	{
		Debug.LogError("Error with Agora: " + msg + " Error code :" + error);
	}

	void OnStreamMessageError(uint userId, int streamId, int code, int missed, int cached)
	{
		Debug.LogError("Error with Agora: " + userId + " Error code :" + code + " missed code :" + missed + " streamId code :" + streamId);
	}

	void OnLeaveChannel(RtcStats stats)
	{
		Debug.Log("Left channel with duration " + stats.duration);
	}

	void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
	{
		Debug.Log("Joined channel " + channelName + "uid :" + uid);

		Hashtable hash = new Hashtable();
		hash.Add("agoraID", uid.ToString());
		PhotonNetwork.SetPlayerCustomProperties(hash);
	}

	public IRtcEngine GetRtcEngine()
	{
		return rtcEngine;
	}

	int displayID0or1 = 0;

	void ShareDisplayScreen()
	{
		int a = rtcEngine.EnableVideo();
		int b = rtcEngine.EnableVideoObserver();

		ScreenCaptureParameters sparams = new ScreenCaptureParameters
		{
			captureMouseCursor = true,
			frameRate = 30
		};

		rtcEngine.StopScreenCapture();

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        mRtcEngine.StartScreenCaptureByDisplayId(getDisplayId(displayID0or1), default(Rectangle), sparams);  // 
#else
		TestRectCrop(displayID0or1);
#endif
		displayID0or1 = 1 - displayID0or1;
	}

	void TestRectCrop(int order)
	{
		// Assuming you have two display monitors, each of 1920x1080, position left to right:
		Rectangle screenRect = new Rectangle() { x = 0, y = 0, width = 1920 * 2, height = 1080 };
		Rectangle regionRect = new Rectangle() { x = order * 1920, y = 0, width = 1920, height = 1080 };

		int rc = rtcEngine.StartScreenCaptureByScreenRect(screenRect,
			regionRect,
			default(ScreenCaptureParameters)
			);
		if (rc != 0) Debug.LogWarning("rc = " + rc);
	}

	public override void OnJoinedRoom()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			var share = GameObject.Find("ShareDisplayButton");
			share.SetActive(false);
		}


		if (PhotonNetwork.IsMasterClient)
		{
			GameObject gameObject = GameObject.Find("DisplayPlane");
			if (ReferenceEquals(gameObject, null))
			{
				Debug.Log("Error: failed to find DisplayPlane");
				return;
			}
			else
			{
				gameObject.AddComponent<VideoSurface>();
			}
		}
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