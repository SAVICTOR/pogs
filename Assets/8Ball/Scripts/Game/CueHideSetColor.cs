using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class CueHideSetColor : MonoBehaviour {

    void Start() {

        Color color = Camera.main.backgroundColor;
        color.a = 1;
        GetComponent<SpriteRenderer>().color = color;
    }

}
