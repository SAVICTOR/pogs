using UnityEngine;
using System.Collections;

public class MoveAvatar : MonoBehaviour {

	private RectTransform transform;
	public GameObject lastAvatar;

//	// Use this for initialization
//	void Start () {
//		transform = GetComponent <RectTransform> ();
//		if (lastAvatar) {
//			GameManager.Instance.lastAvatar = lastAvatar;
//		}	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//		Vector2 pos = transform.anchoredPosition;
//		pos.y -= GameManager.Instance.avatarMoveSpeed;
//
//		if (pos.y <= -70) {
//			pos.y = GameManager.Instance.lastAvatar.GetComponent <RectTransform> ().anchoredPosition.y + 100 * GameManager.Instance.lastAvatar.GetComponent <RectTransform> ().localScale.x;
//			GameManager.Instance.lastAvatar = this.gameObject;
//		}
//
//		transform.anchoredPosition = pos;
//
//
//	}


}
