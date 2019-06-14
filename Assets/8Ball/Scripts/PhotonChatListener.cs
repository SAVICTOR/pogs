using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class PhotonChatListener : MonoBehaviour {

    private Animator animator;
    public Text text;
    private string senderID;
    private string roomName;
    // "invited"
    // "accepted"
    public string type;
    public GameObject okButton;
    public GameObject rejectButton;
    public GameObject acceptButton;
    public GameObject matchPlayersCanvas;
    public GameObject friendsCanvas;
    public GameObject menuCanvas;
    public GameObject gameTitle;
    public GameObject payoutCoinsText;

    // Use this for initialization
    void Start() {
        GameManager.Instance.invitationDialog = this.gameObject;
        animator = GetComponent<Animator>();

    }

    //type
    // 0 - invite received
    // 1 - invite rejected
    // 2 - invite accepted
    // 3 - start game
    public void showInvitationDialog(int type, string name, string id, string room) {

        if (type == 0) {
            payoutCoinsText.GetComponent<Text>().text = "" + GameManager.Instance.payoutCoins;
            rejectButton.SetActive(true);
            acceptButton.SetActive(true);
            okButton.SetActive(false);

            this.type = "invited";
            senderID = id;
            roomName = room;

            text.text = name + " invited you to game";
            animator.Play("InvitationDialogShow");
        } else if (type == 1) {
            payoutCoinsText.GetComponent<Text>().text = "" + GameManager.Instance.payoutCoins;
            rejectButton.SetActive(false);
            acceptButton.SetActive(false);
            okButton.SetActive(true);
            text.text = name + " can't play right now";
            animator.Play("InvitationDialogShow");
        } else if (type == 2) {
            payoutCoinsText.GetComponent<Text>().text = "" + GameManager.Instance.payoutCoins;
            rejectButton.SetActive(true);
            acceptButton.SetActive(true);
            okButton.SetActive(false);

            this.type = "accepted";
            senderID = id;
            roomName = room;

            text.text = name + " accepted your invitation";
            animator.Play("InvitationDialogShow");
        }
    }





    public void hideDialog(string a) {

        if (type.Equals("invited")) {
            if (a.Equals("accepted")) {
                if (GameManager.Instance.coinsCount >= GameManager.Instance.payoutCoins) {
                    GameManager.Instance.chatClient.SendPrivateMessage(senderID, "INVITE_ACCEPT;" + roomName + ";" + GameManager.Instance.nameMy);
                    RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };


                    PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
                    //				menuCanvas.SetActive (false);
                    //				gameTitle.SetActive (false);


                    matchPlayersCanvas.GetComponent<SetMyData>().MatchPlayer();
                    matchPlayersCanvas.GetComponent<SetMyData>().setBackButton(false);
                    //				friendsCanvas.SetActive (false);
                    //				menuCanvas.SetActive (false);
                    //				gameTitle.SetActive (false);
                } else {
                    GameManager.Instance.chatClient.SendPrivateMessage(senderID, "INVITE_REJECT;" + roomName + ";" + GameManager.Instance.nameMy);
                    GameManager.Instance.dialog.SetActive(true);
                }
            } else if (a.Equals("rejected")) {
                GameManager.Instance.chatClient.SendPrivateMessage(senderID, "INVITE_REJECT;" + roomName + ";" + GameManager.Instance.nameMy);
            }
        } else if (type.Equals("accepted")) {
            if (a.Equals("accepted")) {
                if (GameManager.Instance.coinsCount >= GameManager.Instance.payoutCoins) {


                    GameManager.Instance.chatClient.SendPrivateMessage(senderID, "INVITE_START;" + roomName + ";" + GameManager.Instance.nameMy);
                    matchPlayersCanvas.GetComponent<SetMyData>().MatchPlayer();
                    matchPlayersCanvas.GetComponent<SetMyData>().setBackButton(false);
                    //				friendsCanvas.SetActive (false);
                    //				menuCanvas.SetActive (false);
                    //				gameTitle.SetActive (false);
                    PhotonNetwork.JoinRoom(roomName);
                } else {
                    GameManager.Instance.chatClient.SendPrivateMessage(senderID, "INVITE_STOP;" + roomName + ";" + GameManager.Instance.nameMy);
                    GameManager.Instance.dialog.SetActive(true);
                }
            } else if (a.Equals("rejected")) {
                GameManager.Instance.chatClient.SendPrivateMessage(senderID, "INVITE_STOP;" + roomName + ";" + GameManager.Instance.nameMy);

            }
        }
        //Debug.Log ("Dialog: " + a);
        animator.Play("InvitationDialogHide");
    }

}
