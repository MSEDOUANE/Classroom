using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using ExitGames.Client.Photon;
using System;
using System.Linq;
using agora_gaming_rtc;
using System.Runtime.InteropServices;
using static agora_gaming_rtc.ExternalVideoFrame;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	public delegate void PropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged);

	public static event PropertiesChanged RoomPropsChanged;
	public static event Action OnRoomCreated;
	public static event Action OnExistingRoomJoined;
	public static event Action OnPlayersChanged;

	private const string Key = "CHAIR_INDEX";
	private string room = "MeetupV2";
	private string gameVersion = "0.1";

	private bool m_createdRoom = false;
	int i = 100;

	private void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	void Start()
	{

		if (PhotonNetwork.IsConnected)
		{
			OnConnectedToMaster();

		}
		else
		{
			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.GameVersion = gameVersion;
		}



		Debug.Log("Connecting...");
	}
	#region CONNECTION
	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		Debug.Log("Connected to master!");
		Debug.Log("Joining room...");

		//PhotonNetwork.JoinRandomRoom();
		PhotonNetwork.JoinRoom(room);

	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.LogWarningFormat("Disconnected with reason {0}", cause);
	}


	public override void OnJoinedRoom()
	{
		Debug.Log("Joined room!");

		if (m_createdRoom)
		{
			NetworkManager.OnRoomCreated?.Invoke();
		}
		else
		{
			NetworkManager.OnExistingRoomJoined?.Invoke();
		}


		CreatePlayer();
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogWarning("Room join failed " + message);
		m_createdRoom = true;
		Debug.Log("Creating room...");
		PhotonNetwork.CreateRoom(room, new RoomOptions { MaxPlayers = 8, IsOpen = true, IsVisible = true }, TypedLobby.Default);
	}

	void OnTextureRendered(Texture2D texture2D)
	{
		PhotonView[] photonViews = GameObject.FindObjectsOfType<PhotonView>();

		ShareRenderTexture(texture2D);

	}

	public const TextureFormat ConvertFormat = TextureFormat.BGRA32;  // RGBA will be compatible with Web
	public const VIDEO_PIXEL_FORMAT PixelFormat = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA; // note: RGBA is available from v3.0.1 and on


	Texture2D BufferTexture;
	bool _isRunning;


	/// <summary>
	///   Get the image from renderTexture.  (AR Camera must assign a RenderTexture prefab in
	/// its renderTexture field.)
	/// </summary>
	private void ShareRenderTexture(Texture2D texture2D)
	{

		byte[] bytes = texture2D.GetRawTextureData();

		// sends the Raw data contained in bytes
		StartCoroutine(PushFrame(bytes, (int)texture2D.width, (int)texture2D.height,
		 () =>
		 {
			 bytes = null;
			 Destroy(texture2D);
		 }));
		RenderTexture.active = null;
	}

	int frameCnt = 0; // monotonic timestamp counter
	/// <summary>
	/// Push frame to the remote client.  This is the same code that does ScreenSharing.
	/// </summary>
	/// <param name="bytes">raw video image data</param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <param name="onFinish">callback upon finish of the function</param>
	/// <returns></returns>
	IEnumerator PushFrame(byte[] bytes, int width, int height, System.Action onFinish)
	{
		if (bytes == null || bytes.Length == 0)
		{
			Debug.LogError("Zero bytes found!!!!");
			yield break;
		}

		IRtcEngine rtc = IRtcEngine.QueryEngine();
		//if the engine is present
		if (rtc != null)
		{
			//Create a new external video frame
			ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
			//Set the buffer type of the video frame
			externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
			// Set the video pixel format
			externalVideoFrame.format = PixelFormat;
			//apply raw data you are pulling from the rectangle you created earlier to the video frame
			externalVideoFrame.buffer = bytes;
			//Set the width of the video frame (in pixels)
			externalVideoFrame.stride = width;
			//Set the height of the video frame
			externalVideoFrame.height = height;
			//Remove pixels from the sides of the frame
			externalVideoFrame.cropLeft = 10;
			externalVideoFrame.cropTop = 10;
			externalVideoFrame.cropRight = 10;
			externalVideoFrame.cropBottom = 10;
			//Rotate the video frame (0, 90, 180, or 270)
			//externalVideoFrame.rotation = 90;
			externalVideoFrame.rotation = 180;
			// increment i with the video timestamp
			externalVideoFrame.timestamp = frameCnt++;
			//Push the external video frame with the frame we just created
			int a =
			rtc.PushVideoFrame(externalVideoFrame);
			Debug.Log(" pushVideoFrame(" + frameCnt + ") size:" + bytes.Length + " => " + a);

		}
		if (onFinish != null)
		{
			onFinish();
		}
		yield return null;
	}

	void shareScreen(Texture2D mTexture, int width, int height)
	{

		// Get the Raw Texture data from the the from the texture and apply it to an array of bytes
		byte[] bytes = mTexture.GetRawTextureData();
		// Make enough space for the bytes array
		int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
		// Check to see if there is an engine instance already created
		IRtcEngine rtc = IRtcEngine.QueryEngine();
		//if the engine is present
		if (rtc != null)
		{
			//Create a new external video frame
			ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
			//Set the buffer type of the video frame
			externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
			// Set the video pixel format
			externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;
			//apply raw data you are pulling from the rectangle you created earlier to the video frame
			externalVideoFrame.buffer = bytes;
			//Set the width of the video frame (in pixels)
			externalVideoFrame.stride = (int)width;
			//Set the height of the video frame
			externalVideoFrame.height = (int)height;
			//Remove pixels from the sides of the frame
			externalVideoFrame.cropLeft = 10;
			externalVideoFrame.cropTop = 10;
			externalVideoFrame.cropRight = 10;
			externalVideoFrame.cropBottom = 10;
			//Rotate the video frame (0, 90, 180, or 270)
			externalVideoFrame.rotation = 180;
			// increment i with the video timestamp
			externalVideoFrame.timestamp = i++;
			//Push the external video frame with the frame we just created
			int a = rtc.PushVideoFrame(externalVideoFrame);
			Debug.Log(" pushVideoFrame =       " + a);

		}
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
		Debug.Log("Got " + roomList.Count + " rooms.");
		foreach (RoomInfo room in roomList)
		{
			Debug.Log("Room: " + room.Name + ", " + room.PlayerCount);
		}
	}


	public void CreatePlayer()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			Quaternion rot = new(0f, 143.366f, 0f, 0f);
			PhotonNetwork.Instantiate(PlayerPrefs.GetString("CharacterSelectedName"), GameObject.FindGameObjectsWithTag("Teacher chair")[0].transform.position, rot, 0);

		}
		else
		{
			var chairs = GameObject.FindGameObjectsWithTag("chair");

			PhotonNetwork.Instantiate(PlayerPrefs.GetString("CharacterSelectedName"), chairs[PhotonNetwork.CountOfPlayers].transform.position, chairs[PhotonNetwork.CountOfPlayers].transform.localRotation, 0);
		}


	}
	#endregion
	#region ROOM_PROPS

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		OnPlayersChanged?.Invoke();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		OnPlayersChanged?.Invoke();

	}
	public static bool SetCustomPropertySafe(string key, object newValue, WebFlags webFlags = null)
	{
		Room room = PhotonNetwork.CurrentRoom;
		if (room == null || room.IsOffline)
		{
			return false;
		}

		ExitGames.Client.Photon.Hashtable props = room.CustomProperties;

		if (room.CustomProperties.ContainsKey(key))
		{
			props[key] = newValue;
		}
		else
		{
			props.Add(key, newValue);
		}
		//ExitGames.Client.Photon.Hashtable newProps = new ExitGames.Client.Photon.Hashtable(1) { { key, newValue } };
		//Hashtable oldProps = new Hashtable(1) { { key, room.CustomProperties[key] } };
		return room.LoadBalancingClient.OpSetCustomPropertiesOfRoom(props/*, oldProps, webFlags);*/);
	}

	public static object GetCurrentRoomCustomProperty(string key)
	{
		Room room = PhotonNetwork.CurrentRoom;
		if (room == null || room.IsOffline || !room.CustomProperties.ContainsKey(key))
		{
			return null;
		}
		else
		{
			return room.CustomProperties[key];
		}
	}

	public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		RoomPropsChanged?.Invoke(propertiesThatChanged);
	}
	#endregion
}

