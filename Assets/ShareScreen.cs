using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
using System.Globalization;
using System.Runtime.InteropServices;
using System;
using Photon.Pun;
using System.IO;

public class ShareScreen : MonoBehaviour
{
	Texture2D mTexture;
	Rect mRect;
	[SerializeField]
	private string appId = "b814293d74bb43d4bc2642f6fb8070b6";
	[SerializeField]
	private string channelName;
	public IRtcEngine mRtcEngine;
	int i = 100;
	void Start()
	{
		Debug.Log("ScreenShare Activated");
		//Create a rectangle width and height of the screen
		mRect = new Rect(0, 0, Screen.width, Screen.height);
		//Create a texture the size of the rectangle you just created
		mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.BGRA32, false);
		mRtcEngine = IRtcEngine.getEngine(appId);
		// enable log
		mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
		// set callbacks (optional)
		mRtcEngine.SetParameters("{\"rtc.log_filter\": 65535}");
		//Configure the external video source
		mRtcEngine.SetExternalVideoSource(true, false);
		// Start video mode
		mRtcEngine.EnableVideo();
		// allow camera output callback
		mRtcEngine.EnableVideoObserver();
		// join channel
		mRtcEngine.JoinChannel(PhotonNetwork.CurrentRoom.Name, null, 0);


	}
	void Update()
	{
		//Start the screenshare Coroutine
		StartCoroutine(shareScreen());
	}
	//Screen Share
	IEnumerator shareScreen()
	{
		yield return new WaitForEndOfFrame();
		byte[] bytes = null;

		//Read the Pixels inside the Rectangle
		mTexture.ReadPixels(mRect, 0, 0);
		//Apply the Pixels read from the rectangle to the texture
		mTexture.Apply();
		// Get the Raw Texture data from the the from the texture and apply it to an array of bytes
		bytes = mTexture.GetRawTextureData();
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
			externalVideoFrame.stride = (int)mRect.width;
			//Set the height of the video frame
			externalVideoFrame.height = (int)mRect.height;
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
			File.WriteAllBytes("Foo.png", externalVideoFrame.buffer);
			Debug.Log(" pushVideoFrame =       " + a);

		}
	}
}