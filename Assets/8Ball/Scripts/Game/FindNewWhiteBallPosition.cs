using UnityEngine;
using System.Collections;

public class FindNewWhiteBallPosition : MonoBehaviour {

    public GameObject ballHand;

    public bool triggerCollides = true;

    public bool findNew = false;
    private Vector3 newPos;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (findNew) {
            

            if (!triggerCollides) {
                findNew = false;
                GameManager.Instance.whiteBall.GetComponent<Rigidbody>().transform.position = newPos;
                GameManager.Instance.whiteBall.GetComponent<LockZPosition>().ballActive = true;
                GameManager.Instance.whiteBall.SetActive(true);
                Vector3 newBallPos1 = GameManager.Instance.whiteBall.transform.position;
                newBallPos1.z = ballHand.transform.position.z;
                ballHand.transform.position = newBallPos1;
            } else {
                newPos.x -= 0.1f;
                transform.position = newPos;
            }
        }
	}

    void OnTriggerEnter(Collider other) {
        if(other.transform.tag.Contains("Ball")) {
            triggerCollides = true;
            Debug.Log("Trigger enter");
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.transform.tag.Contains ("Ball")) {
            triggerCollides = false;
            Debug.Log("Trigger exit");

        }
    }

    public void findNewPosition ()
    {

        GameManager.Instance.whiteBall.GetComponent<Rigidbody> ().velocity = Vector3.zero;
        GameManager.Instance.whiteBall.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
        GameManager.Instance.whiteBall.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionZ;
        newPos = new Vector3 (-0f, -0.69f, -0.24f);
        transform.position = newPos;
        findNew = true;

    }


}
