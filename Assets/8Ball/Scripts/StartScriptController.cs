using UnityEngine;
using System.Collections;

public class StartScriptController : MonoBehaviour {

    public GameObject splashCanvas;
    public GameObject LoginCanvas;

    public GameObject menuCanvas;
    public GameObject[] go;

    public GameObject fbButton;

    // Use this for initialization
    void Start() {
        Debug.Log("START SCRIPT");
        if (PlayerPrefs.HasKey("LoggedType")) {
            splashCanvas.SetActive(true);
        } else {
            LoginCanvas.SetActive(true);
        }

#if UNITY_WEBGL
        fbButton.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update() {

    }

    public void HideAllElements() {
        menuCanvas.SetActive(true);
    }
}
