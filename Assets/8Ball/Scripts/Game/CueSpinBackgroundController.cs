using UnityEngine;
using System.Collections;

public class CueSpinBackgroundController : MonoBehaviour {

    private BoxCollider coll;
    // Use this for initialization
    void Start() {
        coll = GetComponent<BoxCollider>();
    }

    public void enableCollider() {
        coll.enabled = true;
    }

    public void disableCollider() {
        coll.enabled = false;
    }
}
