using UnityEngine;
using System.Collections;

public class DontGoThroughtThings : MonoBehaviour {
    // Careful when setting this to true - it might cause double
    // events to be fired - but it won't pass through the trigger
    public bool sendTriggerMessage = false;

    public LayerMask layerMask = -1; //make sure we aren't in this layer 
    public float skinWidth = 0.1f; //probably doesn't need to be changed 

    private float minimumExtent;
    private float partialExtent;
    private float sqrMinimumExtent;
    private Vector3 previousPosition;
    private Rigidbody myRigidbody;
    private Collider myCollider;
    private float radius;
    private string TAG;
    //initialize values 
    void Start() {
        radius = GetComponent<SphereCollider>().radius * transform.localScale.x;
        myRigidbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        previousPosition = myRigidbody.position;
        minimumExtent = Mathf.Min(Mathf.Min(myCollider.bounds.extents.x, myCollider.bounds.extents.y), myCollider.bounds.extents.z);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;


    }

    void Awake() {
        TAG = transform.tag;
    }
    bool aa = false;
    void FixedUpdate() {
        //have we moved more than our minimum extent? 
        Vector3 movementThisStep = myRigidbody.position - previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;

        if (movementSqrMagnitude > sqrMinimumExtent) {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
            RaycastHit hitInfo;



            if (Physics.SphereCast(previousPosition, radius, movementThisStep, out hitInfo, movementMagnitude, layerMask.value)) {

                if (!hitInfo.transform.tag.Equals(transform.tag)) {




                    if (!hitInfo.collider)
                        return;

                    if (hitInfo.collider.isTrigger) {
                        //hitInfo.collider.SendMessage ("OnTriggerEnter", myCollider);

                    }

                    if (!hitInfo.collider.isTrigger) {
                        Vector3 endPosition = previousPosition + (movementThisStep.normalized * hitInfo.distance);
                        myRigidbody.position = endPosition;
                    }

                }
            }
        }

        previousPosition = myRigidbody.position;
    }
}