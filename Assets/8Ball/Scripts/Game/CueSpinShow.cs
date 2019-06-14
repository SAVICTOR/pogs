using UnityEngine;
using System.Collections;

public class CueSpinShow : MonoBehaviour {

    // Use this for initialization
    public GameObject bigCueSpin;
    public GameObject blackRect;
    private Animator anim;
    private Animator anim2;
    private CueController cueControllerScript;

    void Start() {
        anim = bigCueSpin.GetComponent<Animator>();
        anim2 = blackRect.GetComponent<Animator>();
        cueControllerScript = GameObject.Find("WhiteBall").GetComponent<CueController>();

    }

    void OnMouseDown() {
        if (cueControllerScript.isServer && !cueControllerScript.shotMyTurnDone) {
            cueControllerScript.spinShowed = true;
            anim.Play("CueSpinFade");
            anim2.Play("ChangeAlpha");
        }
    }


}
