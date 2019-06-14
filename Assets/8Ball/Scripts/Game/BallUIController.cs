using UnityEngine;
using System.Collections;

public class BallUIController : MonoBehaviour {

    // Use this for initialization
    private PotedBallsGUIController cont;
    void Start() {
        cont = GameObject.Find("PotedBallsGUI").GetComponent<PotedBallsGUIController>();
    }

    // Update is called once per frame
    void Update() {

    }

    public void moveOthersAfterPot() {
        cont.moveOtherBalls();
    }


}
