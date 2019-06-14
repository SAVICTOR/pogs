using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SetMyData : MonoBehaviour {

	public GameObject avatar;
	public GameObject name;
	public GameObject matchCanvas;
	public GameObject controlAvatars;
	public GameObject backButton;
	// Use this for initialization


	public void MatchPlayer() {
		
		name.GetComponent <Text>().text = GameManager.Instance.nameMy;
		if(GameManager.Instance.avatarMy != null)
			avatar.GetComponent <Image>().sprite = GameManager.Instance.avatarMy;


		controlAvatars.GetComponent <ControlAvatars> ().reset ();
		//matchCanvas.SetActive (true);

	}

	public void setBackButton(bool active) {
		backButton.SetActive (active);
	}
}
