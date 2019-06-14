using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AssemblyCSharp;

public class ControlAvatars : MonoBehaviour {

    public GameObject prefab;
    public List<Sprite> sprites;
    private List<GameObject> avatars;
    private GameObject lastAvatar;
    private Sprite restoreSprite;
    private int addAvatars = 10;
    public bool foundPlayer = false;
    public bool playerRejected = false;
    public float speed;
    private float speed1;
    public bool foundCancel = false;
    public Sprite prite;
    private GameObject OpponentAvatar;
    public Text opponentNameText;
    public Sprite noAvatarSprite;
    public GameObject cancelGameButton;

    public GameObject menuCanvas;
    public GameObject titleCanvas;
    public GameObject matchPlayersCanvas;

    public GameObject AvatarFrameMy;
    public GameObject AvatarFrameOpponent;
    public GameObject vsText;
    public GameObject centerCoins;
    public GameObject leftcoins;
    public GameObject rightCoins;

    public GameObject oppontentCoinImage;
    public GameObject myCoinImage;
    public GameObject oppontentPayoutCoins;
    public GameObject myPayoutCoins;
    public GameObject centerPayoutCoins;

    private float rightCoinsPos = 500;
    private float leftCoinsPos = -500;
    private float centerCoinsPos = -550;
    private float avatrLeftPos = -500;
    private float avatarRightPos = 500;
    private float vsTextpos = 0;
    private AudioSource[] audioSources;
    public GameObject cantPlayNowOppo;
    public GameObject longTimeMessage;
    // Use this for initialization
    public float waitingOpponentTime = 0;
    public GameObject messageBubbleText;
    public GameObject messageBubble;
    public bool opponentActive = true;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake() {
        audioSources = GetComponents<AudioSource>();
    }

    void Start() {
        GameManager.Instance.controlAvatars = this;
        speed1 = speed;
        avatars = new List<GameObject>();


        //		rightCoinsPos = rightCoins.GetComponent <RectTransform> ().anchoredPosition.x;
        //		leftCoinsPos = leftcoins.GetComponent <RectTransform> ().anchoredPosition.x;
        //		centerCoinsPos = centerCoins.GetComponent <RectTransform> ().anchoredPosition.y;
        //		avatrLeftPos = AvatarFrameMy.GetComponent <RectTransform> ().anchoredPosition.x;
        //		avatarRightPos = AvatarFrameOpponent.GetComponent <RectTransform> ().anchoredPosition.x;
        //		vsTextpos = vsText.GetComponent <RectTransform> ().anchoredPosition.y;

        for (int i = 0; i < addAvatars; i++) {
            GameObject avatar = Instantiate(prefab);
            avatar.transform.parent = gameObject.transform;
            int pos = Random.Range(0, sprites.Count);
            avatar.GetComponent<Image>().sprite = sprites[pos];
            sprites.RemoveAt(pos);
            RectTransform transform = avatar.GetComponent<RectTransform>();
            transform.localScale = new Vector2(1f, 1f);
            transform.anchoredPosition = new Vector2(0.0f, 0 + i * transform.sizeDelta.x * transform.localScale.x);
            avatars.Add(avatar);
            if (i == addAvatars - 1) {
                lastAvatar = avatar;
            }
        }

    }

    public void reset() {
        CancelInvoke();
        opponentActive = true;
        longTimeMessage.SetActive(false);

        Invoke("showLongTimeMessage", 30.0f);

        Debug.Log("Timeout BIG");
        PhotonNetwork.BackgroundTimeout = StaticStrings.photonDisconnectTimeout;

        Debug.Log("TIMEOUT: " + PhotonNetwork.BackgroundTimeout);
        Debug.Log("Right " + rightCoinsPos);
        rightCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(rightCoinsPos, rightCoins.GetComponent<RectTransform>().anchoredPosition.y);
        leftcoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(leftCoinsPos, leftcoins.GetComponent<RectTransform>().anchoredPosition.y);
        centerCoins.GetComponent<RectTransform>().anchoredPosition = new Vector2(centerCoins.GetComponent<RectTransform>().anchoredPosition.x, centerCoinsPos);
        AvatarFrameMy.GetComponent<RectTransform>().anchoredPosition = new Vector2(avatrLeftPos, AvatarFrameMy.GetComponent<RectTransform>().anchoredPosition.y);
        AvatarFrameOpponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(avatarRightPos, AvatarFrameOpponent.GetComponent<RectTransform>().anchoredPosition.y);
        vsText.GetComponent<RectTransform>().anchoredPosition = new Vector2(vsText.GetComponent<RectTransform>().anchoredPosition.x, vsTextpos);
        //cancelGameButton.SetActive (true);


        Color c1 = leftcoins.GetComponent<Image>().color;
        c1.a = 1;
        leftcoins.GetComponent<Image>().color = c1;
        Color c2 = rightCoins.GetComponent<Image>().color;
        c2.a = 1;
        rightCoins.GetComponent<Image>().color = c2;


        speed1 = speed;
        opponentNameText.text = "";
        startedGame = false;
        foundPlayer = false;
        if (OpponentAvatar != null)
            OpponentAvatar.GetComponent<Image>().sprite = restoreSprite;
        OpponentAvatar = null;
        playerRejected = false;
        changedAvatar = false;

        if (avatars != null) {
            foreach (GameObject avatar in avatars) {
                avatar.SetActive(true);
            }
        }

        rightCoins.SetActive(true);
        leftcoins.SetActive(true);
        matchPlayersCanvas.SetActive(true);
        GameManager.Instance.readyToAnimateCoins = false;
        AvatarFrameMy.GetComponent<Animator>().Play("MySelector");
        AvatarFrameOpponent.GetComponent<Animator>().Play("OpponentSelector");


    }

    public void showLongTimeMessage() {
        if (!foundPlayer && gameObject.activeSelf)
            longTimeMessage.SetActive(true);
    }



    public void hideLongTimeMessage() {
        longTimeMessage.SetActive(false);
    }

    // Update is called once per frame
    bool changedAvatar = false;
    bool startedGame = false;
    void Update() {
        if (!startedGame) {
            if (foundPlayer) {

                if (speed1 > 3f)
                    speed1 -= speed / 200.0f;//0.25f;

                if (speed1 < speed * (2.8f / 4.0f) && !changedAvatar) {
                    changedAvatar = true;
                    OpponentAvatar = lastAvatar;
                    restoreSprite = lastAvatar.GetComponent<Image>().sprite;
                    if (GameManager.Instance.avatarOpponent != null)
                        lastAvatar.GetComponent<Image>().sprite = GameManager.Instance.avatarOpponent;
                    else
                        lastAvatar.GetComponent<Image>().sprite = noAvatarSprite;
                }

                if (speed1 <= 0) {
                    speed1 = speed / 100.0f;
                }

                if (OpponentAvatar != null && OpponentAvatar.GetComponent<RectTransform>().anchoredPosition.y <= 0) {
                    speed1 = 0.0f;

                    foreach (GameObject avatar in avatars) {
                        avatar.SetActive(false);
                    }


                    audioSources[1].Play();

                    OpponentAvatar.SetActive(true);
                    OpponentAvatar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    opponentNameText.text = GameManager.Instance.nameOpponent;

                    startedGame = true;




                    if (PhotonNetwork.playerList.Length < 2 || playerRejected) {
                        playerDisconnected();
                    } else {
                        coinsAnimate();
                        AvatarFrameMy.GetComponent<Animator>().Play("MySelectorMoveOut");
                        AvatarFrameOpponent.GetComponent<Animator>().Play("OpponentFrameMoveOut");
                        vsText.GetComponent<Animator>().Play("VsTextAnim");
                        centerCoins.GetComponent<Animator>().Play("CoinsCenter");
                        leftcoins.GetComponent<Animator>().Play("OppontentCoins");
                        rightCoins.GetComponent<Animator>().Play("MyCoinsA");

                        StartCoroutine(countDownCoins(GameManager.Instance.payoutCoins));
                    }
                    GameManager.Instance.readyToAnimateCoins = true;
                    //StartCoroutine (waitSecs (1));



                    // if (!playerRejected) {
                    //     coinsAnimate();
                    //     AvatarFrameMy.GetComponent<Animator>().Play("MySelectorMoveOut");
                    //     AvatarFrameOpponent.GetComponent<Animator>().Play("OpponentFrameMoveOut");
                    //     vsText.GetComponent<Animator>().Play("VsTextAnim");
                    //     centerCoins.GetComponent<Animator>().Play("CoinsCenter");
                    //     leftcoins.GetComponent<Animator>().Play("OppontentCoins");
                    //     rightCoins.GetComponent<Animator>().Play("MyCoinsA");

                    //     StartCoroutine(countDownCoins(GameManager.Instance.payoutCoins));
                    // } else {
                    //     playerDisconnected();
                    //     // cantPlayNowOppo.SetActive(true);
                    //     // PhotonNetwork.LeaveRoom();
                    //     // Invoke("cancelGame", 5.0f);
                    // }
                    //					else {
                    //						startGame ();
                    //						//Invoke ("startGame", 5.0f);
                    //					}


                }
            }


            if (!startedGame) {
                for (int i = 0; i < avatars.Count; i++) {
                    GameObject avatar = avatars[i];
                    RectTransform trans = avatar.GetComponent<RectTransform>();
                    Vector2 pos = trans.anchoredPosition;
                    pos.y -= speed1;

                    Debug.Log("AUDIO");



                    if (pos.y < -200) {
                        float min = 0;
                        if (i == 0)
                            min = speed1;
                        pos.y = lastAvatar.GetComponent<RectTransform>().anchoredPosition.y + trans.localScale.x * trans.sizeDelta.x - min;
                        lastAvatar = avatar;

                        audioSources[0].Play();
                    }


                    trans.anchoredPosition = pos;
                }
            }
        }
    }

    Text oppontent;
    Text my;
    Text center;
    Image opImage;
    Image myImage;
    Image lc;
    private void coinsAnimate() {

        oppontent = oppontentPayoutCoins.GetComponent<Text>();
        my = myPayoutCoins.GetComponent<Text>();
        center = centerPayoutCoins.GetComponent<Text>();
        opImage = oppontentCoinImage.GetComponent<Image>();
        myImage = myCoinImage.GetComponent<Image>();
        lc = leftcoins.GetComponent<Image>();

        my.color = new Color(my.color.r, my.color.g, my.color.b, 1);
        oppontent.color = new Color(oppontent.color.r, oppontent.color.g, oppontent.color.b, 1);
        opImage.color = new Color(opImage.color.r, opImage.color.g, opImage.color.b, 1);
        myImage.color = new Color(myImage.color.r, myImage.color.g, myImage.color.b, 1);

        if (GameManager.Instance.payoutCoins >= 1000) {
            if (GameManager.Instance.payoutCoins >= 1000000) {
                if (GameManager.Instance.payoutCoins % 1000000.0f == 0) {
                    my.text = (GameManager.Instance.payoutCoins / 1000000.0f).ToString("0") + "M";
                    oppontent.text = (GameManager.Instance.payoutCoins / 1000000.0f).ToString("0") + "M";
                } else {
                    my.text = (GameManager.Instance.payoutCoins / 1000000.0f).ToString("0.0") + "M";
                    oppontent.text = (GameManager.Instance.payoutCoins / 1000000.0f).ToString("0.0") + "M";
                }

            } else {
                if (GameManager.Instance.payoutCoins % 1000.0f == 0) {
                    my.text = (GameManager.Instance.payoutCoins / 1000.0f).ToString("0") + "k";
                    oppontent.text = (GameManager.Instance.payoutCoins / 1000.0f).ToString("0") + "k";
                } else {
                    my.text = (GameManager.Instance.payoutCoins / 1000.0f).ToString("0.0") + "k";
                    oppontent.text = (GameManager.Instance.payoutCoins / 1000.0f).ToString("0.0") + "k";
                }

            }
        } else {
            oppontent.text = GameManager.Instance.payoutCoins + "";
            my.text = GameManager.Instance.payoutCoins + "";
        }

        center.text = "0";

    }

    private IEnumerator countDownCoins(int count) {

        Debug.Log("STAET");

        StartCoroutine(waitSecs(5));
        Debug.Log("END");
        int loops = 50;
        int minus = count / loops;
        int current = count;
        int centerCurrent = 0;

        float minusAlpha = 1.0f / loops;

        yield return new WaitForSeconds(2);

        audioSources[2].Play();

        for (int i = 0; i < loops; i++) {


            my.color = new Color(my.color.r, my.color.g, my.color.b, my.color.a - minusAlpha);
            oppontent.color = new Color(oppontent.color.r, oppontent.color.g, oppontent.color.b, oppontent.color.a - minusAlpha);
            opImage.color = new Color(opImage.color.r, opImage.color.g, opImage.color.b, opImage.color.a - minusAlpha);
            myImage.color = new Color(myImage.color.r, myImage.color.g, myImage.color.b, myImage.color.a - minusAlpha);

            current -= minus;
            centerCurrent += minus * 2;


            if (count >= 1000) {
                if (count >= 1000000) {
                    my.text = (current / 1000000.0f).ToString("0.0") + "M";
                    oppontent.text = (current / 1000000.0f).ToString("0.0") + "M";


                    center.text = (centerCurrent / 1000000.0f).ToString("0.0") + "M";


                } else {
                    my.text = (current / 1000.0f).ToString("0.0") + "k";
                    oppontent.text = (current / 1000.0f).ToString("0.0") + "k";
                    if (centerCurrent >= 1000000) {

                        center.text = (centerCurrent / 1000000.0f).ToString("0.0") + "M";


                    } else {

                        center.text = (centerCurrent / 1000.0f).ToString("0.0") + "k";


                    }
                }
            } else {
                my.text = current + "";
                oppontent.text = current + "";
                if (centerCurrent >= 1000) {

                    center.text = (centerCurrent / 1000.0f).ToString("0.0") + "k";


                } else {
                    center.text = centerCurrent + "";
                }
            }



            if (current > 0) {
                //				StartCoroutine (waitSecs (1));
                yield return new WaitForSeconds(0.04f);
                //yield return new WaitForSeconds (1f);
            }

        }

        if (centerCurrent >= 1000) {
            if (centerCurrent >= 1000000) {
                if (centerCurrent % 1000000.0f == 0) {
                    center.text = (centerCurrent / 1000000.0f).ToString("0") + "M";
                }
            } else {
                if (centerCurrent % 1000.0f == 0) {
                    center.text = (centerCurrent / 1000.0f).ToString("0") + "k";
                }
            }
        }

        float alpha = 1.0f;



        for (int i = 0; i < 20; i++) {

            alpha -= (1 / 20.0f);
            //Color c1 = leftcoins.GetComponent <Image> ().color;
            //c1.a = alpha;
            //Debug.Log ("AAAA + " + c1.a);
            lc.color = new Color(1, 1, 1, alpha);

            Color c2 = rightCoins.GetComponent<Image>().color;
            c2.a = alpha;
            rightCoins.GetComponent<Image>().color = c2;

            yield return new WaitForSeconds(0.01f);
        }



        startGame();
    }



    private IEnumerator waitSecs(float milis) {
        yield return new WaitForSeconds(milis);
    }

    public void playerDisconnected() {
        StopAllCoroutines();
        rightCoins.SetActive(false);
        leftcoins.SetActive(false);
        cantPlayNowOppo.SetActive(true);
        PhotonNetwork.LeaveRoom();
        Invoke("cancelGame", 5.0f);
    }

    private void cancelGame() {
        cantPlayNowOppo.SetActive(false);
        matchPlayersCanvas.SetActive(false);
        PhotonNetwork.BackgroundTimeout = 0;
        Debug.Log("Timeout 1");
        //reset ();
    }

    private void startGame() {
        GameObject.Find("PlayFabManager").GetComponent<PlayFabManager>().imReady = true;
        if (!GameManager.Instance.offlineMode) {

            PhotonNetwork.RaiseEvent(199, GameManager.Instance.cueIndex + "-" + GameManager.Instance.cueTime, true, null);
        }

        if (PhotonNetwork.playerList.Length < 2) {
            playerDisconnected();
        }

        //SceneManager.LoadScene ("GameScene");
        //reset ();
    }

    void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            PhotonNetwork.RaiseEvent(141, 1, true, null);

            PhotonNetwork.SendOutgoingCommands();
            Debug.Log("Application pause");
        } else {
            PhotonNetwork.RaiseEvent(142, 1, true, null);
            PhotonNetwork.SendOutgoingCommands();
            Debug.Log("Application resume");
        }
    }

    public void hideMessageBubble() {
        messageBubble.GetComponent<Animator>().Play("HideBubble");
    }

    public IEnumerator updateMessageBubbleText() {
        yield return new WaitForSeconds(1.0f * 2);
        waitingOpponentTime -= 1;
        if (!GameManager.Instance.opponentDisconnected)
            messageBubbleText.GetComponent<Text>().text = StaticStrings.waitingForOpponent + " " + waitingOpponentTime;
        if (waitingOpponentTime > 0 && !opponentActive && !GameManager.Instance.opponentDisconnected) {
            StartCoroutine(updateMessageBubbleText());
        }
    }

    public void test() {

    }

    public void cancelMatching() {
        //		if (!foundPlayer && PhotonNetwork.otherPlayers.Length == 0) {
        //			PhotonNetwork.LeaveRoom ();
        //			matchPlayersCanvas.SetActive (false);
        //		}
        cancelGameButton.SetActive(false);
        Invoke("cancelGameInvoke", 3.0f);
    }

    public void cancelGameInvoke() {
        Debug.Log("Length: " + PhotonNetwork.otherPlayers.Length);
        if (!foundPlayer && PhotonNetwork.otherPlayers.Length == 0) {

            PhotonNetwork.LeaveRoom();
            matchPlayersCanvas.SetActive(false);
            PhotonNetwork.BackgroundTimeout = 0;
            Debug.Log("Timeout 2");
            GameManager.Instance.playfabManager.imReady = false;
            //GameObject.Find ("PlayFabManager").GetComponent <PlayFabManager> ().imReady = false;
        }
    }


}
