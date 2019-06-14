using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AssemblyCSharp;
using PlayFab.ClientModels;
using Facebook.Unity;
using System;

public class WinnerControllerScript : MonoBehaviour {

    public GameObject myObject;
    public GameObject opponentObject;
    public GameObject shareButton;

    public bool isGameScene = false;

    public Image myImage;
    public Image oppoImage;

    public Text myName;
    public Text oppoText;

    public GameObject myMessageBubble;
    public GameObject oppoMessageBubble;

    public GameObject rematchButton;

    public bool rematchRequest = false;
    public bool sentRematch = false;

    public GameObject ChatMessagesList;
    public GameObject ChatMessageButtonPrefab;

    public GameObject ChatMessagesObject;

    public GameObject prizeText;
    private AudioSource[] audioSources;
    public GameObject reardShareText;

    public bool messageDialogVisible = false;
    // Use this for initialization
    void Start() {



        audioSources = GetComponents<AudioSource>();

        if (GameManager.Instance.playerDisconnected) {
            GameManager.Instance.playerDisconnected = false;
            if (!isGameScene) {
                rematchButton.SetActive(false);
            }
        }



        if (!isGameScene) {


            PhotonNetwork.BackgroundTimeout = 0;

            if (GameManager.Instance.payoutCoins > GameManager.Instance.coinsCount) {
                rematchButton.SetActive(false);
            }

            if (reardShareText != null)
                reardShareText.GetComponent<Text>().text = "+" + StaticStrings.rewardCoinsForShareViaFacebook;

            if (StaticStrings.showAdOnGameOverScene)
                GameManager.Instance.adsScript.ShowAd();

            if (!PlayerPrefs.GetString("LoggedType").Equals("Facebook")) {
                shareButton.SetActive(false);
            }

            rematchRequest = false;
            sentRematch = false;

            if (GameManager.Instance.iWon) {
                myObject.GetComponent<Animator>().Play("WinnerOpponentAnimation");
                audioSources[0].Play();
                GameManager.Instance.playfabManager.addCoinsRequest(GameManager.Instance.payoutCoins * 2);
            } else {
                opponentObject.GetComponent<Animator>().Play("WinnerOpponentAnimation");
                audioSources[1].Play();
            }

            if (GameManager.Instance.avatarMy != null)
                myImage.sprite = GameManager.Instance.avatarMy;
            if (GameManager.Instance.avatarOpponent != null)
                oppoImage.sprite = GameManager.Instance.avatarOpponent;

            myName.text = GameManager.Instance.nameMy;
            oppoText.text = GameManager.Instance.nameOpponent;

            int prizeCoins = GameManager.Instance.payoutCoins * 2;

            if (prizeCoins >= 1000) {
                if (prizeCoins >= 1000000) {
                    if (prizeCoins % 1000000.0f == 0) {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000000.0f).ToString("0") + "M";

                    } else {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000000.0f).ToString("0.0") + "M";

                    }

                } else {
                    if (prizeCoins % 1000.0f == 0) {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000.0f).ToString("0") + "k";
                    } else {
                        prizeText.GetComponent<Text>().text = (prizeCoins / 1000.0f).ToString("0.0") + "k";
                    }

                }
            } else {
                prizeText.GetComponent<Text>().text = prizeCoins + "";
            }
        }

        for (int i = 0; i < StaticStrings.chatMessages.Length; i++) {
            GameObject button = Instantiate(ChatMessageButtonPrefab);
            button.transform.GetChild(0).GetComponent<Text>().text = StaticStrings.chatMessages[i];
            button.transform.parent = ChatMessagesList.transform;
            button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            string index = StaticStrings.chatMessages[i];
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => SendMessageEvent(index));
        }

        for (int i = 0; i < StaticStrings.chatMessagesExtended.Length; i++) {
            if (GameManager.Instance.ownedChats.Contains("'" + i + "'")) {
                for (int j = 0; j < StaticStrings.chatMessagesExtended[i].Length; j++) {
                    GameObject button = Instantiate(ChatMessageButtonPrefab);
                    button.transform.GetChild(0).GetComponent<Text>().text = StaticStrings.chatMessagesExtended[i][j];
                    button.transform.parent = ChatMessagesList.transform;
                    button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    string index = StaticStrings.chatMessagesExtended[i][j];
                    button.GetComponent<Button>().onClick.RemoveAllListeners();
                    button.GetComponent<Button>().onClick.AddListener(() => SendMessageEvent(index));
                }
            }

        }

    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    // void Update() {

    //     if (Input.anyKey && messageDialogVisible) {
    //         ChatMessagesObject.GetComponent<Animator>().Play("hideMessageDialog");
    //         messageDialogVisible = false;
    //     }
    // }

    public void share() {
        if (PlayerPrefs.GetString("LoggedType").Equals("Facebook")) {

            Uri myUri = new Uri(StaticStrings.facebookShareLinkAndroid);
#if UNITY_IPHONE
            myUri = new Uri(StaticStrings.facebookShareLinkAppStore);
#endif

            FB.ShareLink(
                myUri,
                StaticStrings.facebookShareLinkTitle,
                callback: ShareCallback
            );
        }
    }

    private void ShareCallback(IShareResult result) {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error)) {
            Debug.Log("ShareLink Error: " + result.Error);
        } else if (!String.IsNullOrEmpty(result.PostId)) {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        } else {
            // Share succeeded without postID
            GameManager.Instance.playfabManager.addCoinsRequest(StaticStrings.rewardCoinsForShareViaFacebook);
            Debug.Log("ShareLink success!");
        }
    }


    void OnDestroy() {
        removeOnEventCall();
    }

    public void SendMessageEvent(string index) {
        Debug.Log("Button Clicked " + index);
        if (!GameManager.Instance.offlineMode)
            PhotonNetwork.RaiseEvent(193, index, true, null);
        ChatMessagesObject.GetComponent<Animator>().Play("hideMessageDialog");
        messageDialogVisible = false;

        if (isGameScene) {
            myMessageBubble.SetActive(true);
            myMessageBubble.transform.GetChild(0).GetComponent<Text>().text = index;
            if (isGameScene) {
                CancelInvoke("hideMyMessageBubble");
                Invoke("hideMyMessageBubble", 6.0f);
            }
        }

    }

    public void loadMenuScene() {
        if (GameManager.Instance.offlineMode && StaticStrings.showAdWhenLeaveGame)
            GameManager.Instance.adsScript.ShowAd();
        SceneManager.LoadScene("Menu");
        Debug.Log("Timeout 6");
        PhotonNetwork.BackgroundTimeout = 0;
        if (!GameManager.Instance.offlineMode)
            PhotonNetwork.RaiseEvent(194, 1, true, null);
        removeOnEventCall();

        GameManager.Instance.cueController.removeOnEventCall();
        PhotonNetwork.LeaveRoom();

        GameManager.Instance.playfabManager.roomOwner = false;
        GameManager.Instance.roomOwner = false;
        GameManager.Instance.resetAllData();

    }

    public void sendRematchRequest() {
        if (!rematchRequest) {
            sentRematch = true;
            Debug.Log("Send message");
            if (!GameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(195, 1, true, null);
            myMessageBubble.SetActive(true);
            myMessageBubble.transform.GetChild(0).GetComponent<Text>().text = StaticStrings.IWantPlayAgain;
            rematchButton.SetActive(false);
        } else {
            Debug.Log("Send message");
            if (!GameManager.Instance.offlineMode)
                PhotonNetwork.RaiseEvent(195, 1, true, null);
            rematchButton.SetActive(false);
            GameManager.Instance.resetAllData();
            SceneManager.LoadScene("GameScene");
            removeOnEventCall();
        }
    }

    public void sendMessageButton() {
        ChatMessagesObject.GetComponent<Animator>().Play("showMessagesDialog");
        messageDialogVisible = true;

    }

    void Awake() {
        PhotonNetwork.OnEventCall += this.OnEvent;

    }

    public void removeOnEventCall() {
        PhotonNetwork.OnEventCall -= this.OnEvent;
    }

    // Multiplayer data received
    private void OnEvent(byte eventcode, object content, int senderid) {
        Debug.Log("Received message");
        if (eventcode == 195) {
            if (sentRematch) {
                GameManager.Instance.resetAllData();
                SceneManager.LoadScene("GameScene");
                removeOnEventCall();
            } else {
                rematchRequest = true;
                if (GameManager.Instance.payoutCoins <= GameManager.Instance.coinsCount) {
                    oppoMessageBubble.SetActive(true);
                    oppoMessageBubble.transform.GetChild(0).GetComponent<Text>().text = StaticStrings.IWantPlayAgain;
                }
            }
        } else if (eventcode == 194) {
            rematchButton.SetActive(false);
            oppoMessageBubble.SetActive(true);
            oppoMessageBubble.transform.GetChild(0).GetComponent<Text>().text = StaticStrings.cantPlayRightNow;

        } else if (eventcode == 193) {
            string index = (string)content;
            Debug.Log("INDEX: " + index);
            oppoMessageBubble.SetActive(true);
            oppoMessageBubble.transform.GetChild(0).GetComponent<Text>().text = index;
            if (isGameScene) {
                CancelInvoke("hideOppoMessageBubble");
                Invoke("hideOppoMessageBubble", 6.0f);
            }

        }
    }

    public void hideOppoMessageBubble() {
        oppoMessageBubble.SetActive(false);
    }

    public void hideMyMessageBubble() {
        myMessageBubble.SetActive(false);
    }
}
