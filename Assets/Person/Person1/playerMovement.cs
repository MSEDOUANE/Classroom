using Photon.Pun;
using Photon.Voice.PUN;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playerMovement : MonoBehaviour
{


	//camera
	public GameObject cameraOne;
	public GameObject cameraTwo;

	AudioListener cameraOneAudioLis;
	AudioListener cameraTwoAudioLis;


	//Moving
	public float speed = 1f;
	public float gravity = 20f;
	CharacterController Controller;
	Animator anim;
	public float rotSpeed = 80f;
	float rot = 0f;


	float horizontal;
	float vertical;

	Vector3 moveDirection = Vector3.zero;
	private PhotonView photonView;

	private bool isClassroomScene;


	// Start is called before the first frame update
	void Start()
	{
		//camera
		cameraOneAudioLis = cameraOne.GetComponent<AudioListener>();
		cameraTwoAudioLis = cameraTwo.GetComponent<AudioListener>();
		// cameraPositionChange(PlayerPrefs.GetInt("CameraPosition"));





		//moving
		Controller = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();
		photonView = GetComponent<PhotonView>();
		isClassroomScene = SceneManager.GetActiveScene().name == "SampleScene";


		Camera camera = this.GetComponentInChildren<Camera>(true);
		AudioListener audioListener = this.GetComponentInChildren<AudioListener>(true);
		var FocusCamera = GameObject.Find("FocusCamera");
		FocusCamera.GetComponent<Camera>().enabled = false;

		if (isClassroomScene && photonView.IsMine)
		{
			//camera.enabled = true;
			audioListener.enabled = true;
			cameraOne.SetActive(true);
			cameraTwo.SetActive(false);
		}
		else if (isClassroomScene)
		{
			cameraOne.SetActive(false);
			cameraTwo.SetActive(false);
			// camera.enabled = false;
			audioListener.enabled = false;

		}
	}

	// Update is called once per frame
	void Update()
	{





		//Character animation
		if (isClassroomScene && photonView.IsMine)
		{
			move();
			//camera
			switchCamera();
			FocusCamera();
			if (Input.GetKey(KeyCode.U))
			{
				anim.SetBool("isSitting", false);
				anim.SetBool("isTalking", false);
				anim.SetBool("isStandClap", false);
				anim.SetBool("isThankful", false);
				anim.SetBool("isQueshada", false);
			}
			else if (Input.GetKey(KeyCode.T))
			{
				anim.SetBool("isTalking", true);
			}
			else if (Input.GetKey(KeyCode.I))
			{
				anim.SetBool("isStandClap", true);
			}
			else if (Input.GetKey(KeyCode.Y))
			{
				anim.SetBool("isThankful", true);
				// anim.SetBool("isThankful", false);
			}
			else if (Input.GetKey(KeyCode.E))
			{
				anim.SetBool("isQueshada", true);
			}
			else if (Input.GetKey(KeyCode.L))
			{
				Sitting();
			}
			else if (Input.GetKey(KeyCode.K))
			{
				anim.SetBool("isAsking", true);
			}
			else if (Input.GetKey(KeyCode.F))
			{
				anim.SetBool("isfemaleSit", true);
			}
			else if (Input.GetKey(KeyCode.M))
			{
				anim.SetBool("isMaleSit", true);
			}
			else if (Input.GetKey(KeyCode.N))
			{
				anim.SetBool("isDrinking", true);
			}
			else if (Input.GetKey(KeyCode.P))
			{
				anim.SetBool("isTyping", true);
			}
			else if (Input.GetKey(KeyCode.R))
			{
				anim.SetBool("isWriting", true);
			}
			else if (Input.GetKey(KeyCode.C))
			{
				anim.SetBool("isClapping", true);

			}
		}
	}
	void Sitting()
	{
		initialisation();
		anim.SetBool("isSitting", true);
	}
	void Standing()
	{

		anim.SetBool("isSitting", true);
	}
	void initialisation()
	{
		anim.SetBool("isWalking", false);
		anim.SetBool("isAsking", false);
		anim.SetBool("isTalking", false);
		anim.SetBool("isfemaleSit", false);
		anim.SetBool("isMaleSit", false);
		anim.SetBool("isDrinking", false);
		anim.SetBool("isTyping", false);
		anim.SetBool("isWriting", false);
		anim.SetBool("isClapping", false);
	}

	void Asking()
	{
		anim.SetBool("isAsking", true);
	}






	void move()
	{
		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		if (Controller.isGrounded)
		{
			moveDirection = new Vector3(0, 0.0f, vertical);
			moveDirection *= speed;
			moveDirection = transform.TransformDirection(moveDirection);
			if (Input.GetKey("up") || Input.GetKey("down"))
			{
				anim.SetBool("isWalking", true);
			}
			else
			{
				anim.SetBool("isWalking", false);
			}
		}
		rot += Input.GetAxis("Horizontal") * rotSpeed * Time.deltaTime;
		transform.eulerAngles = new Vector3(0, rot, 0);
		moveDirection.y -= gravity * Time.deltaTime;
		Controller.Move(moveDirection * Time.deltaTime);
	}

	bool isPlaying(Animator anim, string stateName)
	{
		if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
			return true;
		else
			return false;
	}


	public void switchCamera()
	{

		if (Input.GetKeyDown(KeyCode.H))
		{
			cameraChangeCounter();
		}

	}

	void cameraChangeCounter()
	{
		int cameraPositionCounter = PlayerPrefs.GetInt("CameraPosition");
		cameraPositionCounter++;
		cameraPositionChange(cameraPositionCounter);

	}

	void cameraPositionChange(int camPosition)
	{

		if (camPosition > 1)
		{
			camPosition = 0;
		}

		//Set camera position database
		PlayerPrefs.SetInt("CameraPosition", camPosition);

		if (camPosition == 0)
		{
			cameraOne.SetActive(true);
			// cameraOneAudioLis.enable = true;

			//cameraTwoAudioLis.enable = false;
			cameraTwo.SetActive(false);
		}

		if (camPosition == 1)
		{
			cameraTwo.SetActive(true);
			//cameraTwoAudioLis.enable = true;

			//cameraOneAudioLis.enable = false;
			cameraOne.SetActive(false);
		}

	}

	void FocusCamera()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			var FocusCamera = GameObject.Find("FocusCamera");
			cameraOne.SetActive(FocusCamera.activeSelf);
			cameraTwo.SetActive(FocusCamera.activeSelf);
			FocusCamera.GetComponent<Camera>().enabled = !FocusCamera.GetComponent<Camera>().enabled;
		}


	}


}