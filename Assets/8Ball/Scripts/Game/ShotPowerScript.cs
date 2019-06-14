using UnityEngine;
using System.Collections;

public class ShotPowerScript : MonoBehaviour {

    CueController cueScript;
    public GameObject shotColorIndicator;
    private Color initColor;
    public GameObject posEnd;
    public bool mouseDown = false;
    public GameObject moveTo;
    public GameObject cue;
    public GameObject cueMain;
    public GameObject cueMainMoveTo;
    private Vector3 initialPos;
    private float initYPos;
    // Use this for initialization
    public GameObject mainObject;
    public Animator anim;
    private bool deactivateDone = false;
    private Vector3 initMainCuePos;
    private Vector3 initMainCouPositionDetector;
    GameManager gameManager;
    void Start() {
        gameManager = GameManager.Instance;
        cueScript = GameObject.Find("WhiteBall").GetComponent<CueController>();

        initialPos = cue.transform.position;
        setIndicatorColor();
        initColor = shotColorIndicator.GetComponent<SpriteRenderer>().color;
        anim = mainObject.GetComponent<Animator>();
        if (GameManager.Instance.roomOwner)
            anim.Play("MakeVisible");
        else {
            anim.Play("ShotPowerAnimation");
        }

    }


    void OnMouseDown() {
        mouseDown = true;
        initYPos = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        deactivateDone = false;
        initMainCuePos = cueMain.transform.position;
        initMainCouPositionDetector = cueMainMoveTo.transform.position;
        GameManager.Instance.ballHand.SetActive(false);
    }

    void OnMouseUp() {

        if (!GameManager.Instance.stopTimer && cueScript.isServer) {
            Invoke("deactivate", 0.5f);
            deactivateDone = true;
            cueScript.steps = (int)(50 * Mathf.Abs((cue.transform.position.y - initialPos.y) / (posEnd.transform.position.y - initialPos.y)));
            if (cueScript.steps > 0) {
                anim.Play("ShotPowerAnimation");
                cueScript.shouldShot = true;
                cueScript.startShoot = true;
                cueMain.GetComponent<Renderer>().enabled = false;
            }

            cue.transform.position = initialPos;
            cueMain.transform.position = initMainCuePos;
            shotColorIndicator.GetComponent<SpriteRenderer>().color = initColor;
        } else {
            Invoke("deactivate", 0.5f);
            deactivateDone = true;
            resetCue();
        }

    }

    public void resetCue() {
        if (mouseDown) {
            Vector3 newPos = cue.transform.position;
            newPos.y = initialPos.y;
            cue.transform.position = newPos;
            cueMain.transform.position = initMainCuePos;
            shotColorIndicator.GetComponent<SpriteRenderer>().color = initColor;
        }
    }

    public void deactivate() {
        mouseDown = false;

    }


    void Update() {
        if (mouseDown && !deactivateDone && GameManager.Instance.cueController.isServer) {
            Vector3 cuePos = cue.transform.position;
            float newYPos = initialPos.y + (Camera.main.ScreenToWorldPoint(Input.mousePosition).y - initYPos);


            if (newYPos < initialPos.y && newYPos > posEnd.transform.position.y) {
                cuePos.y = newYPos;
                cue.transform.position = cuePos;

            } else {

                if (newYPos > initialPos.y)
                    cue.transform.position = initialPos;
                else if (newYPos < posEnd.transform.position.y) {
                    Vector3 pos = cue.transform.position;
                    pos.y = posEnd.transform.position.y;
                    cue.transform.position = pos;
                }
            }

            cueMain.transform.position = Vector3.MoveTowards(initMainCuePos, initMainCouPositionDetector, (initialPos.y - cue.transform.position.y) / 2);
            if (!GameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(8, cueMain.transform.position, true, null);

            setIndicatorColor();

        }
    }

    // Sets indicator color when cue is moving
    private void setIndicatorColor() {
        float add = (Mathf.Abs((cue.transform.position.y - initialPos.y)) * 45 + 80) / 255.0f;
        Color color = shotColorIndicator.GetComponent<SpriteRenderer>().color;
        color.r = add;
        color.g = add;
        color.b = add;

        shotColorIndicator.GetComponent<SpriteRenderer>().color = color;
    }


}
