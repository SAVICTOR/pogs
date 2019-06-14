using UnityEngine;
using System.Collections;

public class lockHandRotation : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
    void Update () {
        var rotation = Quaternion.LookRotation(Vector3.zero , Vector3.forward);
        transform.rotation = rotation;
    }
}
