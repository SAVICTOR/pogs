using UnityEngine;
using System.Collections;

public class SpinController : MonoBehaviour {

    public GameObject circle1;
    public GameObject circle2;

    private bool mouseDown;
    private Vector3 initialPosStart;
    private Vector3 initialPos;
    private Vector3 initialPosCircle2;
    private bool firstTouchDone;
    private Animator anim;
    private CueController cueControllerScript;
    public GameObject blackRect;
    private Animator anim2;
    private Vector3 localPosInit;
    private bool fT;
    private Vector3 centerStart;
    private BoxCollider rectCollider;
    // Use this for initialization
    void Start() {
        anim2 = blackRect.GetComponent<Animator>();
        anim = GameObject.Find("cueSpinBallBig").GetComponent<Animator>();
        cueControllerScript = GameObject.Find("WhiteBall").GetComponent<CueController>();
        initialPosStart = circle2.transform.localPosition;
        localPosInit = circle1.transform.localPosition;
        rectCollider = blackRect.GetComponent<BoxCollider>();
    }


    public void resetPositions() {
        circle1.transform.localPosition = localPosInit;
        circle2.transform.localPosition = initialPosStart;
        if (cueControllerScript == null)
            cueControllerScript = GameObject.Find("WhiteBall").GetComponent<CueController>();
        cueControllerScript.trickShotAdd = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update() {

    }

    void OnMouseDown() {
        if (cueControllerScript.isServer) {
            if (!firstTouchDone) {
                firstTouchDone = true;
                initialPos = circle1.transform.position;
                initialPosCircle2 = circle2.transform.localPosition;
            }

            if (!fT) {

                fT = true;
                centerStart = circle1.transform.position;
                Debug.Log(centerStart.x + "  " + centerStart.y);
            }

            mouseDown = true;
        }
    }

    void OnMouseUp() {
        hideController();
    }

    public void hideController() {
        if (GameManager.Instance.cueController.spinShowed) {
            //if (cueControllerScript.isServer) {
            mouseDown = false;
            firstTouchDone = false;
            GameManager.Instance.cueController.spinShowed = false;
            //cueControllerScript.spinShowed = false;
            GameObject.Find("cueSpinBallBig").GetComponent<Animator>().Play("CueSpinFadeOut");
            blackRect.GetComponent<Animator>().Play("ChangeAlphaBackwards");
            blackRect.GetComponent<BoxCollider>().enabled = false;
            //}
        }

    }

    void OnMouseOver() {
        if (cueControllerScript.isServer) {
            if (mouseDown) {
                Vector3 pos = circle1.transform.position;
                float initZ = pos.z;
                pos.x = Input.mousePosition.x;
                pos.y = Input.mousePosition.y;

                pos = Camera.main.ScreenToWorldPoint(pos);
                pos.z = initZ;



                float dist = Vector3.Distance(pos, centerStart);

                if (dist > 1.65f) {
                    pos = pos - centerStart;
                    pos.z = 0;
                    pos.Normalize();
                    pos *= 1.65f;
                    pos = pos + centerStart;
                    pos.z = initZ;
                }

                circle1.transform.position = pos;


                //			localPosInit.z = circle1.transform.localPosition.z;
                //
                //			float dist = Vector3.Distance (, localPosInit);
                //
                //
                //			if (dist > 1.63f) {
                //				Vector3 newPos = circle1.transform.localPosition - localPosInit;
                //				newPos.Normalize ();
                //				newPos *= 1.63f;
                //				circle1.transform.position = newPos;
                //			}

                //			if (dist > 1.63f) {
                //				pos.x = pos.x * (1.63f / dist);
                //				pos.y = pos.y * (1.63f / dist);
                //			}
                //				
                //			circle1.transform.position = pos;

                //			Debug.Log ("" + Vector3.Distance (circle1.transform.localPosition, localPosInit));


                //			float dist = Vector3.Distance (circle2.transform.localPosition, initialPosStart);
                //
                //			Vector3 offset = pos - initialPos;
                //
                //			if (dist > 1.6f) {
                //				Vector3 newpos = circle1.transform.localPosition - initialPosStart;
                //				newpos.Normalize ();
                //				newpos *= 1.6f;
                //				circle1.transform.localPosition = newpos;
                //				offset = newpos - initialPos;
                //			} 



                Vector3 offset = pos - initialPos;
                offset.z = 0;
                circle2.transform.localPosition = initialPosCircle2 + (offset);

                if (!GameManager.Instance.offlineMode)
                    PhotonNetwork.RaiseEvent(11, circle2.transform.localPosition, true, null);


                Vector3 offsetSpin = circle2.transform.localPosition - initialPosStart;



                cueControllerScript.trickShotAdd = new Vector3(0, -offsetSpin.x / 20.0f, -offsetSpin.y / 20.0f);

            }
        }
    }

    public void changePositionOpponent(Vector3 pos) {
        circle2.transform.localPosition = pos;
        circle1.transform.localPosition = pos;
    }
}
