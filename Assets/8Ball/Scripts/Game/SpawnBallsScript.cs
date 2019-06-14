using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnBallsScript : MonoBehaviour {

    public GameObject ballPrefab;
    public GameObject whiteBallPrefab;
    public GameObject[] balls;
    private bool ballsInMotion;
    private Animator animator;
    public Texture[] texturesArray;
    private MeshRenderer[] renderers;
    private Rigidbody[] rigidbodies;
    private LockZPosition[] enabledScripts;
    Renderer renderer;
    private int updateCount;
    private CueController cueControllerScript;
    private SpinController spinControl;
    // Use this for initialization
    void Start() {
        animator = GameObject.Find("ShotPower").GetComponent<Animator>();
        ballsInMotion = true;

        spinControl = GameObject.Find("cueSpinBallBig").GetComponent<SpinController>();

        cueControllerScript = whiteBallPrefab.GetComponent<CueController>();
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;


        //cueControllerScript = GameObject.Find ("WhiteBall").GetComponent <CueController> ();
        //16
        balls = new GameObject[16];
        renderers = new MeshRenderer[16];
        rigidbodies = new Rigidbody[16];
        enabledScripts = new LockZPosition[16];

        whiteBallPrefab.transform.tag = "WhiteBall";
        balls[0] = whiteBallPrefab;
        renderers[0] = whiteBallPrefab.GetComponent<MeshRenderer>();
        rigidbodies[0] = whiteBallPrefab.GetComponent<Rigidbody>();
        enabledScripts[0] = whiteBallPrefab.GetComponent<LockZPosition>();

        GameManager.Instance.whiteBall = balls[0];

        Rigidbody rigid = balls[0].GetComponent<Rigidbody>();
        rigid.maxAngularVelocity = 150;
        //		rigid.useConeFriction = false;
        //rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;

        Debug.Log(GameManager.Instance.whiteBall.GetComponent<Rigidbody>().maxAngularVelocity + " MAX ANG");

        float radius = ballPrefab.GetComponent<SphereCollider>().radius;
        int counter = 1;
        //5



        List<int> freeI = new List<int>();
        freeI.Add(0);
        freeI.Add(1);
        freeI.Add(1);
        freeI.Add(2);
        freeI.Add(2);
        freeI.Add(3);
        freeI.Add(3);
        freeI.Add(3);
        freeI.Add(3);

        freeI.Add(4);
        freeI.Add(4);
        freeI.Add(4);



        freeI.Add(4);
        freeI.Add(4);
        freeI.Add(2);


        List<int> freeJ = new List<int>();
        freeJ.Add(0);

        freeJ.Add(0);
        freeJ.Add(1);


        freeJ.Add(0);
        freeJ.Add(2);

        freeJ.Add(0);
        freeJ.Add(1);
        freeJ.Add(2);
        freeJ.Add(3);

        freeJ.Add(1);
        freeJ.Add(2);
        freeJ.Add(3);


        freeJ.Add(0);
        freeJ.Add(4);
        freeJ.Add(1);

        Vector3[] positions = new Vector3[15];

        for (int i = 0; i < 5; i++) {

            for (int j = 0; j < i + 1; j++) {

                GameObject ball = Instantiate(ballPrefab);
                ball.GetComponent<Renderer>().material.SetTexture("_MainTex", texturesArray[counter - 1]);
                ball.transform.tag = "Ball" + counter;
                balls[counter] = ball;
                renderers[counter] = ball.GetComponent<MeshRenderer>();
                rigidbodies[counter] = ball.GetComponent<Rigidbody>();
                enabledScripts[counter] = ball.GetComponent<LockZPosition>();

                Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();


                ballRigidbody.maxAngularVelocity = 150;
                //				ballRigidbody.useConeFriction = false;

                //ballRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                if (GameManager.Instance.roomOwner) {

                    int minus = 4;
                    if (counter > 9)
                        minus = 1;

                    int iii = Random.Range(0, freeI.Count - minus);



                    if (counter == 1)
                        iii = freeI.Count - 3;
                    else if (counter == 9)
                        iii = freeI.Count - 2;
                    else if (counter == 8)
                        iii = freeI.Count - 1;

                    float posY = ballPrefab.GetComponent<Rigidbody>().transform.position.y + freeI[iii] * (radius * ballPrefab.transform.localScale.x/*0.325f*/);

                    Vector3 position = ballRigidbody.transform.position;


                    position.x += freeI[iii] * (radius * (ball.transform.localScale.x * 2 * 0.9f) * 1.0f);
                    position.y = posY - freeJ[iii] * (radius * (ball.transform.localScale.x * 2) * 1.0f);

                    freeI.RemoveAt(iii);
                    freeJ.RemoveAt(iii);

                    ballRigidbody.transform.position = position;
                    positions[counter - 1] = position;
                } else {
                    ball.SetActive(false);
                }

                //ball.SetActive (false);


                ballRigidbody.maxAngularVelocity = 150;
                counter++;
                //ballRigidbody.Sleep ();
            }
        }

        GameManager.Instance.balls = balls;

        if (GameManager.Instance.roomOwner) {
            Debug.Log("Raise 198");
            if (!GameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(198, positions, true, null);
        } else {
            for (int i = 0; i < GameManager.Instance.initPositions.Length; i++) {
                GameManager.Instance.balls[i + 1].GetComponent<Rigidbody>().transform.position = GameManager.Instance.initPositions[i];
                GameManager.Instance.balls[i + 1].SetActive(true);
            }
        }



    }

    int updatesCount = 0;
    // Update is called once per frame
    void Update() {
        if (cueControllerScript.isServer == false && cueControllerScript.opponentShotStart == true) {

            bool canSendFinishData = true;
            for (int i = 0; i < balls.Length; i++) {
                //              if ((balls[i].GetComponent <MeshRenderer>().enabled) &&  (balls [i].GetComponent<Rigidbody> ().velocity != Vector3.zero ||
                //                  balls [i].GetComponent<Rigidbody> ().angularVelocity != Vector3.zero)) {
                if ((enabledScripts[i].ballActive) && (rigidbodies[i].velocity != Vector3.zero ||
                    rigidbodies[i].angularVelocity != Vector3.zero)) {
                    canSendFinishData = false;
                    break;
                }
            }

            if (canSendFinishData && updatesCount > 10) {
                Debug.Log("Raise event 10");
                cueControllerScript.opponentShotStart = false;
                if (!GameManager.Instance.offlineMode)
                    PhotonNetwork.RaiseEvent(10, null, true, null);

                updatesCount = 0;
                cueControllerScript.isServer = false;
            }
            updatesCount++;
        }

        updateCount = (updateCount + 1) % 10;

        if (cueControllerScript.opponentBallsStoped || GameManager.Instance.offlineMode) {
            //            if ((updateCount++ % 10) == 0) {
            if (updateCount == 0) {
                if (cueControllerScript.dataSent) {
                    //			if (rigidbodies [0].velocity == Vector3.zero && rigidbodies [0].angularVelocity == Vector3.zero) {
                    bool canSendPositions = true;
                    for (int i = 0; i < balls.Length; i++) {
                        //				if ((balls[i].GetComponent <MeshRenderer>().enabled) &&  (balls [i].GetComponent<Rigidbody> ().velocity != Vector3.zero ||
                        //					balls [i].GetComponent<Rigidbody> ().angularVelocity != Vector3.zero)) {
                        if ((enabledScripts[i].ballActive) && (rigidbodies[i].velocity != Vector3.zero ||
                            rigidbodies[i].angularVelocity != Vector3.zero)) {
                            canSendPositions = false;
                            break;
                        }
                    }

                    if (canSendPositions) {
                        cueControllerScript.ballsInMovement = false;

                        //Time.timeScale = 1;
                        cueControllerScript.canRun = false;
                        a = 2;
                        cueControllerScript.dataSent = false;
                        cueControllerScript.cue.transform.position = new Vector3(balls[0].transform.position.x, balls[0].transform.position.y, cueControllerScript.cue.transform.position.z);


                        GameManager.Instance.cueController.setMyTurn(false);

                        //cueControllerScript.cue.GetComponent<Renderer> ().enabled = true;
                        //spinControl.resetPositions ();
                        Invoke("showLines", 0.1f);
                        //if (cueControllerScript.isServer) 
                        //    animator.Play ("MakeVisible");

                        Debug.Log("STOP - Data Sent");
                        cueControllerScript.shotMyTurnDone = false;
                        Vector3[] data = new Vector3[balls.Length * 3];


                        for (int i = 0; i < balls.Length; i++) {
                            data[i * 3] = balls[i].transform.position;
                            data[i * 3 + 1] = balls[i].GetComponent<Rigidbody>().velocity;
                            data[i * 3 + 2] = balls[i].GetComponent<Rigidbody>().angularVelocity;
                        }

                        dd.Add(data);
                        //cueControllerScript.isServer = false;
                        if (!GameManager.Instance.offlineMode)
                            PhotonNetwork.RaiseEvent(6, data, true, null);
                        dd.Clear();

                        if (GameManager.Instance.offlineMode) {
                            GameManager.Instance.cueController.setTurnOffline(true);
                        }

                    }
                    //			}
                }
            }
        }
    }

    private void showLines() {
        //cueControllerScript.targetLine.SetActive (true);


        if (GameManager.Instance.wasFault) {
            Debug.Log("Raise 9");
            if (GameManager.Instance.faultMessage.Length > 0) {
                GameManager.Instance.gameControllerScript.showMessage(GameManager.Instance.faultMessage);
                if (!GameManager.Instance.offlineMode)
                    PhotonNetwork.RaiseEvent(13, GameManager.Instance.faultMessage, true, null);
            }
            if (!GameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(9, cueControllerScript.cue.transform.position, true, null);
            GameManager.Instance.faultMessage = "";
        } else {
            Debug.Log("Raise 12");
            if (!GameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(12, cueControllerScript.cue.transform.position, true, null);
        }


    }

    //	int a = 2;
    int a = 1;
    ArrayList dd = new ArrayList();
    void FixedUpdate() {

        if (cueControllerScript.ballsInMovement) {

            if (a == 1 && cueControllerScript.canRun && cueControllerScript.isServer) {
                sendData();

            }
            a++;
            if (a > 15)
                a = 0;
        }
    }

    public void sendData() {
        Vector3[] data = new Vector3[balls.Length * 3];


        for (int i = 0; i < balls.Length; i++) {
            data[i * 3] = balls[i].transform.position;
            data[i * 3 + 1] = balls[i].GetComponent<Rigidbody>().velocity;
            data[i * 3 + 2] = balls[i].GetComponent<Rigidbody>().angularVelocity;
        }
        if (!GameManager.Instance.offlineMode)
            PhotonNetwork.RaiseEvent(5, data, true, null);
    }

}
