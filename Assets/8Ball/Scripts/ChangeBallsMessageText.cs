using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChangeBallsMessageText : MonoBehaviour {

    public Sprite other;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void changeSprite ()
    {
        GetComponent<Image>().sprite = other;
    }
}
