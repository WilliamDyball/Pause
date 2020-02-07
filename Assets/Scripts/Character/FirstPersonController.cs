using System;
using UnityEngine;
using Luminosity.IO;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour {
    [SerializeField] private bool bIsWalking;
    [SerializeField] private float fWalkSpeed;
    [SerializeField] private float fRunSpeed;
    [SerializeField] [Range(0f, 1f)] private float fRunstepLenghten;
    [SerializeField] private float fJumpSpeed;
    [SerializeField] private float fStickToGroundForce;
    [SerializeField] private float fGravityMultiplier;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private bool bUseFovKick;
    [SerializeField] private FOVKick fovKick = new FOVKick();
    [SerializeField] private bool bUseHeadBob;
    [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
    [SerializeField] private float fStepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    private Camera cam;
    private bool bJump;
    private float fYRotation;
    private Vector2 v2Input;
    private Vector3 v3MoveDir = Vector3.zero;
    private CharacterController charCon;
    private CollisionFlags collFlags;
    private bool bPreviouslyGrounded;
    private Vector3 v3OriginalCameraPosition;
    private float fStepCycle;
    private float fNextStep;
    private bool bJumping;
    private AudioSource m_AudioSource;

    bool bTimeStopped;

    public bool bCarrying;
    public GameObject carriedObj;
    Quaternion storedRot;
    public float throwForce;
    public float holdingDistance;

    public float fTimer;
    float freezeTimeMax;
    public TextMeshProUGUI timerText;
    private string strTimer;
    [SerializeField]
    TimeGroupController timeCont;

    UnityEvent pausedEvent;

    private GameObject[] pickupables;

    // Use this for initialization
    private void Start() {
        charCon = GetComponent<CharacterController>();
        cam = Camera.main;
        v3OriginalCameraPosition = cam.transform.localPosition;
        fovKick.Setup(cam);
        headBob.Setup(cam, fStepInterval);
        fStepCycle = 0f;
        fNextStep = fStepCycle / 2f;
        bJumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        mouseLook.Init(transform, cam.transform);
        pausedEvent = new UnityEvent();
        pickupables = GameObject.FindGameObjectsWithTag("Pickupable") as GameObject[];
        foreach (GameObject pickup in pickupables) {
            if (pickup.GetComponent<Pickupable>()) {
                pausedEvent.AddListener(pickup.GetComponent<Pickupable>().StoreForce);
            }
        }
    }

    // Update is called once per frame
    private void Update() {
        mouseLook.LookRotation(transform, cam.transform);

        //Pick up objects
        #region pickup
        if (InputManager.GetButtonDown("PickUp")) {
            if (!bCarrying) {
                holdingDistance = 3;
                PickupObj();
            } else {
                DropObj();
            }
        }
        if (InputManager.GetButtonDown("Throw") && bCarrying) {
            ThrowObj();
        }
        if (bCarrying) {
            CarryObj(carriedObj);
            if (InputManager.GetAxis("Zoom") > 0f) {
                holdingDistance += 0.1f;
                holdingDistance = Mathf.Clamp(holdingDistance, 1.4f, 3.5f);
            }
            if (InputManager.GetAxis("Zoom") < 0f) {
                holdingDistance -= 0.1f;
                holdingDistance = Mathf.Clamp(holdingDistance, 1f, 3.5f);
            }
        }
        #endregion

        //Stop time
        #region TimeControl
        if (InputManager.GetButtonDown("PauseTime")) {
            PauseTime();
        }
        #endregion

        // the jump state needs to read here to make sure it is not missed
        if (!bJump && charCon.isGrounded) {
            bJump = InputManager.GetButtonDown("Jump");
        }

        if (!bPreviouslyGrounded && charCon.isGrounded) {
            StartCoroutine(jumpBob.DoBobCycle());
            PlayLandingSound();
            v3MoveDir.y = 0f;
            bJumping = false;
        }
        if (!charCon.isGrounded && !bJumping && bPreviouslyGrounded) {
            v3MoveDir.y = 0f;
        }
        bPreviouslyGrounded = charCon.isGrounded;
    }

    private void PlayLandingSound() {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        fNextStep = fStepCycle + .5f;
    }

    private void FixedUpdate() {
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * v2Input.y + transform.right * v2Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, charCon.radius, Vector3.down, out hitInfo, charCon.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        v3MoveDir.x = desiredMove.x * speed;
        v3MoveDir.z = desiredMove.z * speed;

        if (charCon.isGrounded) {
            v3MoveDir.y = -fStickToGroundForce;
            if (bJump) {
                v3MoveDir.y = fJumpSpeed;
                PlayJumpSound();
                bJump = false;
                bJumping = true;
            }
        } else {
            v3MoveDir += Physics.gravity * fGravityMultiplier * Time.fixedDeltaTime;
        }
        collFlags = charCon.Move(v3MoveDir * Time.fixedDeltaTime);
        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);
        mouseLook.UpdateCursorLock();
    }

    private void UpdateCameraPosition(float speed) {
        Vector3 newCameraPosition;
        if (!bUseHeadBob) {
            return;
        }
        if (charCon.velocity.magnitude > 0 && charCon.isGrounded) {
            cam.transform.localPosition = headBob.DoHeadBob(charCon.velocity.magnitude + (speed * (bIsWalking ? 1f : fRunstepLenghten)));
            newCameraPosition = cam.transform.localPosition;
            newCameraPosition.y = cam.transform.localPosition.y - jumpBob.Offset();
        } else {
            newCameraPosition = cam.transform.localPosition;
            newCameraPosition.y = v3OriginalCameraPosition.y - jumpBob.Offset();
        }
        cam.transform.localPosition = newCameraPosition;
    }

    private void GetInput(out float speed) {
        // Read input
        float horizontal = InputManager.GetAxis("Horizontal");
        float vertical = InputManager.GetAxis("Vertical");

        bool waswalking = bIsWalking;

        // set the desired speed to be walking or running
        if (bIsWalking) {
            speed = fWalkSpeed;
        } else {
            speed = fRunSpeed;
        }
        speed = bIsWalking ? fWalkSpeed : fRunSpeed;
        v2Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (v2Input.sqrMagnitude > 1) {
            v2Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (bIsWalking != waswalking && bUseFovKick && charCon.velocity.sqrMagnitude > 0) {
            StopAllCoroutines();
            StartCoroutine(!bIsWalking ? fovKick.FOVKickUp() : fovKick.FOVKickDown());
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (collFlags == CollisionFlags.Below) {
            return;
        }

        if (body == null || body.isKinematic) {
            return;
        }
        body.AddForceAtPosition(charCon.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    void PauseTime() {
        if (bTimeStopped) {
            bTimeStopped = false;
            timeCont.ResumeGroupTime();
        } else {
            bTimeStopped = true;
            timeCont.PauseGroupTime();
            pausedEvent.Invoke();
        }
    }

    //Sends out a ray cast from the centre of the screen to detect an object
    void PickupObj() {
        int x = Screen.width / 2;
        int y = Screen.height / 2;

        Ray pRay = Camera.main.ScreenPointToRay(new Vector3(x, y));
        RaycastHit pHit;

        if (Physics.Raycast(pRay, out pHit, 5)) { //The third parameter is the length of the ray cast or the players reach
            if (pHit.collider.GetComponent<Pickupable>()) {
                Pickupable p = pHit.collider.GetComponent<Pickupable>();
                if (p != null) {
                    bCarrying = true;
                    carriedObj = p.gameObject;
                    storedRot = carriedObj.transform.rotation;
                    carriedObj.GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
    }

    //Allows the player to carry the object by updating the objects position
    void CarryObj(GameObject cObj) {
        cObj.GetComponent<Rigidbody>().useGravity = false;
        cObj.GetComponent<Transform>().position = Vector3.Lerp(cObj.GetComponent<Transform>().position, Camera.main.transform.position + Camera.main.transform.forward * holdingDistance, Time.deltaTime * 4);
        if (Quaternion.Angle(cObj.GetComponent<Transform>().rotation, transform.rotation) > 1.0f) {
            cObj.GetComponent<Transform>().rotation = Quaternion.Lerp(cObj.GetComponent<Transform>().rotation, transform.rotation, Time.deltaTime * 4); // Keeps the object facing the same way as the player
        }
    }

    //Resets the values changed to allow the object to drop
    void DropObj() {
        bCarrying = false;
        carriedObj.GetComponent<Rigidbody>().useGravity = true;
        carriedObj = null;
    }

    //Same as the DropObj but adds a force to it
    void ThrowObj() {
        bCarrying = false;
        if (!bTimeStopped) {
            carriedObj.GetComponent<Rigidbody>().useGravity = true;
            carriedObj.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
        } else {
            carriedObj.GetComponent<Pickupable>().storedForce = (Camera.main.transform.forward * throwForce);
            carriedObj.GetComponent<Pickupable>().bForceStored = true;
        }
        carriedObj = null;
    }

    private void PlayJumpSound() {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    private void ProgressStepCycle(float speed) {
        if (charCon.velocity.sqrMagnitude > 0 && (v2Input.x != 0 || v2Input.y != 0)) {
            fStepCycle += (charCon.velocity.magnitude + (speed * (bIsWalking ? 1f : fRunstepLenghten))) *
                         Time.fixedDeltaTime;
        }

        if (!(fStepCycle > fNextStep)) {
            return;
        }

        fNextStep = fStepCycle + fStepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio() {
        if (!charCon.isGrounded) {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    public bool GetTimeStopped() {
        return bTimeStopped;
    }
}