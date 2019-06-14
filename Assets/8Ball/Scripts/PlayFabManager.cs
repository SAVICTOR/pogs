using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using System.Collections.Generic;
using ExitGames.Client.Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using AssemblyCSharp;
using System.Globalization;

public class PlayFabManager : Photon.PunBehaviour, IChatClientListener
{

    public string PlayFabId;
    public string PlayFabTitleID;
    public string PhotonAppID;
    public string PhotonChatID;
    public bool multiGame = true;
    public bool roomOwner = false;
    private FacebookManager fbManager;
    public GameObject fbButton;
    private FacebookFriendsMenu facebookFriendsMenu;
    public ChatClient chatClient;
    private bool alreadyGotFriends = false;
    public GameObject menuCanvas;
    public GameObject MatchPlayersCanvas;
    public GameObject splashCanvas;
    public bool opponentReady = false;
    public bool imReady = false;
    public GameObject playerAvatar;
    public GameObject playerName;
    public GameObject backButtonMatchPlayers;


    public GameObject loginEmail;
    public GameObject loginPassword;
    public GameObject loginInvalidEmailorPassword;
    public GameObject loginCanvas;


    public GameObject regiterEmail;
    public GameObject registerPassword;
    public GameObject registerNickname;
    public GameObject registerInvalidInput;
    public GameObject registerCanvas;

    public GameObject resetPasswordEmail;
    public GameObject resetPasswordInformationText;

    public bool isInLobby = false;
    public bool isInMaster = false;

    void Awake()
    {


        PlayFabTitleID = StaticStrings.PlayFabTitleID;
        PhotonAppID = StaticStrings.PhotonAppID;
        PhotonChatID = StaticStrings.PhotonChatID;
        PlayFabSettings.TitleId = PlayFabTitleID;
        PhotonNetwork.OnEventCall += this.OnEvent;
        DontDestroyOnLoad(transform.gameObject);
        //GameManager.Instance.playfabManager = this;
        //        if (GameManager.Instance.logged) {
        //            showMenu();
        //        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        PhotonNetwork.OnEventCall -= this.OnEvent;
    }

    public void destroy()
    {
        if(this.gameObject != null)
            DestroyImmediate(this.gameObject);
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("Timeout 4");
        PhotonNetwork.BackgroundTimeout = 0.0f;
        //PhotonNetwork.SwitchToProtocol(ConnectionProtocol.Tcp);
        PlayFabTitleID = StaticStrings.PlayFabTitleID;
        PhotonAppID = StaticStrings.PhotonAppID;
        PhotonChatID = StaticStrings.PhotonChatID;

        GameManager.Instance.playfabManager = this;

        fbManager = GameObject.Find("FacebookManager").GetComponent<FacebookManager>();
        facebookFriendsMenu = GameManager.Instance.facebookFriendsMenu;//fbButton.GetComponent <FacebookFriendsMenu> ();


        //		if (multiGame)
        //			Login ();
        //		else
        //			SceneManager.LoadScene ("GameScene");
    }

    // Update is called once per frame
    void Update()
    {

        if (chatClient != null) { chatClient.Service(); }
        

    }


    


    // handle events:
    private void OnEvent(byte eventcode, object content, int senderid)
    {
        if (eventcode == 199)
        {
            string dd = (string)content;
            string[] dd1 = dd.Split('-');
            GameManager.Instance.opponentCueIndex = Int32.Parse(dd1[0]);
            GameManager.Instance.opponentCueTime = Int32.Parse(dd1[1]);
            opponentReady = true;
            StartCoroutine(waitAndStartGame());
        }
        else if (eventcode == 198)
        {
            Debug.Log("Received 198");
            GameManager.Instance.initPositions = (Vector3[])content;

            if (GameManager.Instance.initPositions == null) Debug.Log("null pos");
            else Debug.Log("not null pos");
            GameManager.Instance.receivedInitPositions = true;
        } 
        else if (eventcode == 141 && GameManager.Instance.MatchPlayersCanvas.activeSelf)
        {
            GameManager.Instance.controlAvatars.waitingOpponentTime = StaticStrings.photonDisconnectTimeout;
            GameManager.Instance.controlAvatars.opponentActive = false;
            GameManager.Instance.controlAvatars.messageBubbleText.GetComponent<Text>().text = StaticStrings.waitingForOpponent + " " + StaticStrings.photonDisconnectTimeout;
            GameManager.Instance.controlAvatars.messageBubble.GetComponent<Animator>().Play("ShowBubble");
            
            StartCoroutine(GameManager.Instance.controlAvatars.updateMessageBubbleText());
        }
        else if (eventcode == 142 && GameManager.Instance.MatchPlayersCanvas.activeSelf)
        {
            GameManager.Instance.controlAvatars.CancelInvoke("showLongTimeMessage");
            GameManager.Instance.controlAvatars.opponentActive = true;
            GameManager.Instance.controlAvatars.messageBubble.GetComponent<Animator>().Play("HideBubble");
        }


    }

    private IEnumerator waitAndStartGame()
    {
        while (!opponentReady || !imReady || (!GameManager.Instance.roomOwner && !GameManager.Instance.receivedInitPositions))
        {
            yield return 0;
        }

        startGameScene();
        //Invoke ("startGameScene", 2);

        opponentReady = false;
        imReady = false;
    }

    public void startGameScene()
    {

        SceneManager.LoadScene("GameScene");
    }

    public void resetPassword()
    {
        resetPasswordInformationText.SetActive(false);

        SendAccountRecoveryEmailRequest request = new SendAccountRecoveryEmailRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = resetPasswordEmail.GetComponent<Text>().text
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, (result) =>
        {
            resetPasswordInformationText.SetActive(true);
            resetPasswordInformationText.GetComponent<Text>().text = "Email sent to your address. Check your inbox";


        }, (error) =>
        {
            resetPasswordInformationText.SetActive(true);
            resetPasswordInformationText.GetComponent<Text>().text = "Account with specified email doesn't exist";
        });
    }

    public void setInitNewAccountData() {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Cues", "'0'");
        data.Add("Chats", "");
        data.Add("UsedCue", "'0';'0';'0';'0'");
        GameManager.Instance.ownedCues = "'0'";
        GameManager.Instance.ownedChats = "";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest() {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Initial data added");
        }, (error1) => {
            Debug.Log("Initial data add error " + error1.ErrorMessage);
        }, null);
    }

    public void setUsedCue(int index, int power, int aim, int time) {
        GameManager.Instance.cueIndex = index;
        GameManager.Instance.cuePower = power;
        GameManager.Instance.cueAim = aim;
        GameManager.Instance.cueTime = time;

        if(GameManager.Instance.cueController != null) {
            GameManager.Instance.cueController.changeCueImage(index);
        }

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("UsedCue", "'" + index + "';" + "'" + power + "';" + "'" + aim + "';" + "'" + time + "'");
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest() {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Cue changed playfab");
        }, (error1) => {
            Debug.Log("Cue changed playfab error " + error1.ErrorMessage);
        }, null);
    }

    public void updateBoughtCues(int index) {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Cues", GameManager.Instance.ownedCues + ";'" + index + "'");
        GameManager.Instance.ownedCues = GameManager.Instance.ownedCues + ";'" + index + "'";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest() {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Bought cue added");
        }, (error1) => {
            Debug.Log("Bought cue error " + error1.ErrorMessage);
        }, null);
    }

    public void updateBoughtChats(int index) {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Chats", GameManager.Instance.ownedChats + ";'" + index + "'");

        GameManager.Instance.ownedChats = GameManager.Instance.ownedChats + ";'" + index + "'";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest() {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Bought chat added");
        }, (error1) => {
            Debug.Log("Bought chat error " + error1.ErrorMessage);
        }, null);
    }

    public void addCoinsRequest(int count)
    {
        if (!GameManager.Instance.offlineMode) {


            GameManager.Instance.coinsCount += count;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Coins", "" + GameManager.Instance.coinsCount);
            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest() {
                Data = data,
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
                if (GameManager.Instance.coinsTextMenu != null) {
                    updateCoinsTextMenu();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }

                if (GameManager.Instance.coinsTextShop != null) {
                    updateCoinsTextShop();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }


                Debug.Log("Coins updated successfull ");
            }, (error1) => {
                Debug.Log("Coins updated error " + error1.ErrorMessage);
            }, null);
        }
    }


    public void updateCoinsTextMenu()
    {
        if(GameManager.Instance.coinsCount != 0) {
            GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount.ToString("0,0", CultureInfo.InvariantCulture).Replace(',', ' ');
        } else {
            GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = "0";
        }
        
    }

    public void updateCoinsTextShop()
    {
        if (GameManager.Instance.coinsCount != 0) {
            GameManager.Instance.coinsTextShop.GetComponent<Text>().text = GameManager.Instance.coinsCount.ToString("0,0", CultureInfo.InvariantCulture).Replace(',', ' ');
        } else {
            GameManager.Instance.coinsTextShop.GetComponent<Text>().text = "0";
        }
    }

    public void getPlayerDataRequest()
    {
        GetUserDataRequest getdatarequest = new GetUserDataRequest()
        {
            PlayFabId = GameManager.Instance.playfabManager.PlayFabId,
        };

        PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
        {

            Dictionary<string, UserDataRecord> data = result.Data;

            if (data.ContainsKey("Coins"))
            {
                Debug.Log("Got coins from playfab");
                GameManager.Instance.coinsCount = Int32.Parse(data["Coins"].Value);
                // if (GameManager.Instance.coinsTextMenu != null)
                // {
                //     GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                // }
            }
            else
            {
                GameManager.Instance.coinsCount = 0;
            }

            if(data.ContainsKey("UsedCue")) {
                string[] d = data["UsedCue"].Value.Split(';');
                GameManager.Instance.cueIndex = Int32.Parse(d[0].Replace("'", ""));
                GameManager.Instance.cuePower = Int32.Parse(d[1].Replace("'", ""));
                GameManager.Instance.cueAim = Int32.Parse(d[2].Replace("'", ""));
                GameManager.Instance.cueTime = Int32.Parse(d[3].Replace("'", ""));

                Debug.Log("Using cue: " + GameManager.Instance.cueIndex);
            }

            if(data.ContainsKey("Chats")) {
                if(data["Chats"].Value != null)
                    GameManager.Instance.ownedChats = data["Chats"].Value;
            }

            if(data.ContainsKey("Cues")) {
               
                GameManager.Instance.ownedCues = data["Cues"].Value;
                Debug.Log("Owned Cues: " + GameManager.Instance.ownedCues);
            }

            //SceneManager.LoadScene("Menu");
            StartCoroutine(loadSceneMenu());
        }, (error) =>
        {
            Debug.Log("Data updated error " + error.ErrorMessage);
        }, null);
    }


    private IEnumerator loadSceneMenu() {
        yield return new WaitForSeconds(0.1f);

        if(isInMaster && isInLobby) {
            SceneManager.LoadScene("Menu");
        } else {
            StartCoroutine(loadSceneMenu());
        }

    }

    public void RegisterNewAccountWithID()
    {
        string email = regiterEmail.GetComponent<Text>().text;
        string password = registerPassword.GetComponent<Text>().text;
        string nickname = registerNickname.GetComponent<Text>().text;

        registerInvalidInput.SetActive(false);

        if (Regex.IsMatch(email, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$") && password.Length >= 6 && nickname.Length > 0)
        {



            RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                Email = email,
                Password = password,
                RequireBothUsernameAndEmail = false
            };

            PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
            {
                PlayFabId = result.PlayFabId;
                Debug.Log("Got PlayFabID: " + PlayFabId);

                registerCanvas.SetActive(false);
                PlayerPrefs.SetString("email_account", email);
                PlayerPrefs.SetString("password", password);
                PlayerPrefs.SetString("LoggedType", "EmailAccount");
                PlayerPrefs.Save();
                GameManager.Instance.nameMy = nickname;

                UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
                {
                    //DisplayName = GameManager.Instance.nameMy,
                    DisplayName = GameManager.Instance.playfabManager.PlayFabId
                };

                PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
                {
                    Debug.Log("Title Display name updated successfully");
                }, (error) =>
                {
                    Debug.Log("Title Display name updated error: " + error.Error);

                }, null);


                Dictionary<string, string> data = new Dictionary<string, string>();

                data.Add("LoggedType", "EmailAccount");
                //data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
                data.Add("PlayerName", GameManager.Instance.nameMy);
                //data.Add("PlayerAvatarUrl", GameManager.Instance.avatarMyUrl);

                UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
                {
                    Data = data,
                    Permission = UserDataPermission.Public
                };

                PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
                {
                    Debug.Log("Data updated successfull ");
                }, (error1) =>
                {
                    Debug.Log("Data updated error " + error1.ErrorMessage);
                }, null);



                fbManager.showLoadingCanvas();
                GetPhotonToken();




                //GetFriends ();
            },
                (error) =>
                {

                    //                    
                    if (error.ErrorMessage.Equals("EmailAddressNotAvailable"))
                    {
                        registerInvalidInput.SetActive(true);
                        registerInvalidInput.GetComponent<Text>().text = "Email already registered";
                    }
                    Debug.Log("Error registering new account with email: " + error.ErrorMessage + "\n" + error.ErrorDetails);
                    //Debug.Log(error.ErrorMessage);
                });
        }
        else
        {
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = "Invalid input specified";
        }


    }

    public void LoginWithFacebook()
    {
        LoginWithFacebookRequest request = new LoginWithFacebookRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            AccessToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString
        };

        PlayFabClientAPI.LoginWithFacebook(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData();
                addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                Debug.Log("(existing account)");
            }


            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                //DisplayName = GameManager.Instance.nameMy,
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.Log("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.Log("Title Display name updated error: " + error.Error);

            }, null);


            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("LoggedType", "Facebook");
            data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
            if(result.NewlyCreated)
                data.Add("PlayerName", GameManager.Instance.nameMy);
            else {
                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = result.PlayFabId,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                {

                    Dictionary<string, UserDataRecord> data2 = result2.Data;


                    GameManager.Instance.nameMy = data2["PlayerName"].Value;
                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);
            }
            data.Add("PlayerAvatarUrl", GameManager.Instance.avatarMyUrl);

            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                Debug.Log("Data updated successfull ");
            }, (error1) =>
            {
                Debug.Log("Data updated error " + error1.ErrorMessage);
            }, null);




            GetPhotonToken();




            //GetFriends ();
        },
            (error) =>
            {
                Debug.Log("Error logging in player with custom ID: " + error.ErrorMessage + "\n" + error.ErrorDetails);
                GameManager.Instance.connectionLost.showDialog();
                //Debug.Log(error.ErrorMessage);
            });
    }

    private string androidUnique()
    {
        AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityPlayerActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject unityPlayerResolver = unityPlayerActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass androidSettingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        return androidSettingsSecure.CallStatic<string>("getString", unityPlayerResolver, "android_id");
    }

    public void LoginWithEmailAccount()
    {


        loginInvalidEmailorPassword.SetActive(false);



        string email = "";
        string password = "";
        if (PlayerPrefs.HasKey("email_account"))
        {
            email = PlayerPrefs.GetString("email_account");
            password = PlayerPrefs.GetString("password");
        }
        else
        {
            email = loginEmail.GetComponent<Text>().text;
            password = loginPassword.GetComponent<Text>().text;

        }






        LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = email,
            Password = password
        };


        PlayFabClientAPI.LoginWithEmailAddress(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            Dictionary<string, string> data = new Dictionary<string, string>();

            loginCanvas.SetActive(false);
            PlayerPrefs.SetString("email_account", email);
            PlayerPrefs.SetString("password", password);
            PlayerPrefs.SetString("LoggedType", "EmailAccount");
            PlayerPrefs.Save();



            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData();
                addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                Debug.Log("(existing account)");
            }


            GetUserDataRequest getdatarequest = new GetUserDataRequest()
            {
                PlayFabId = result.PlayFabId,

            };

            PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
            {

                Dictionary<string, UserDataRecord> data2 = result2.Data;


                GameManager.Instance.nameMy = data2["PlayerName"].Value;
            }, (error) =>
            {
                Debug.Log("Data updated error " + error.ErrorMessage);
            }, null);





            fbManager.showLoadingCanvas();


            GetPhotonToken();

        },
             (error) =>
             {
                 loginInvalidEmailorPassword.SetActive(true);
                 Debug.Log("Error logging in player with custom ID:");
                 Debug.Log(error.ErrorMessage);

                 GameManager.Instance.connectionLost.showDialog();
             });
    }

    public void Login()
    {
        string customId = "";
        if (PlayerPrefs.HasKey("unique_identifier"))
        {
            customId = PlayerPrefs.GetString("unique_identifier");
        }
        else
        {
            customId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("unique_identifier", customId);
        }




        Debug.Log("UNIQUE IDENTIFIER: " + customId);

        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = customId //SystemInfo.deviceUniqueIdentifier
        };



        PlayFabClientAPI.LoginWithCustomID(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            Dictionary<string, string> data = new Dictionary<string, string>();

            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData();
                addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                Debug.Log("(existing account)");
            }



            string name = result.PlayFabId;
            if (PlayerPrefs.HasKey("GuestPlayerName"))
            {
                name = PlayerPrefs.GetString("GuestPlayerName");
            }
            else
            {
                name = "Guest";
                for (int i = 0; i < 6; i++)
                {
                    name += UnityEngine.Random.Range(0, 9);
                }
                PlayerPrefs.SetString("GuestPlayerName", name);
                PlayerPrefs.Save();
            }


            data.Add("LoggedType", "Guest");
            data.Add("PlayerName", name);


            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                //DisplayName = name,
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.Log("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.Log("Title Display name updated error: " + error.Error);

            }, null);


            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                Debug.Log("Data updated successfull ");
            }, (error1) =>
            {
                Debug.Log("Data updated error " + error1.ErrorMessage);
            }, null);

            GameManager.Instance.nameMy = name;

            PlayerPrefs.SetString("LoggedType", "Guest");
            PlayerPrefs.Save();

            fbManager.showLoadingCanvas();


            GetPhotonToken();

        },
            (error) =>
            {
                Debug.Log("Error logging in player with custom ID:");
                Debug.Log(error.ErrorMessage);
                GameManager.Instance.connectionLost.showDialog();
            });
    }


    // List<string> playfabFriendsName = new List<string>();
    // public void GetPlayfabFriends()
    // {
    //     if (alreadyGotFriends)
    //     {
    //         Debug.Log("show firneds FFFF");
    //         if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //         {
    //             fbManager.getFacebookInvitableFriends();
    //         }
    //         else
    //         {

    //             facebookFriendsMenu.showFriends(null, null, null);
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("IND");
    //         GetFriendsListRequest request = new GetFriendsListRequest();
    //         request.IncludeFacebookFriends = true;
    //         PlayFabClientAPI.GetFriendsList(request, (result) =>
    //         {

    //             Debug.Log("Friends list Playfab: " + result.Friends.Count);
    //             var friends = result.Friends;

    //             List<string> playfabFriends = new List<string>();
    //             playfabFriendsName = new List<string>();
    //             List<string> playfabFriendsFacebookId = new List<string>();


    //             chatClient.RemoveFriends(GameManager.Instance.friendsIDForStatus.ToArray());

    //             List<string> friendsToStatus = new List<string>();


    //             if(friends.Count > 0) {
    //                 for(int i=0; i<friends.Count; i++)
    //                 //foreach (var friend in friends)
    //                 {

    //                     var friend = friends[i];


    //                     playfabFriends.Add(friend.FriendPlayFabId);

    //                     Debug.Log("Title: " + friend.TitleDisplayName);
    //                     GetUserDataRequest getdatarequest = new GetUserDataRequest()
    //                     {
    //                         PlayFabId = friend.TitleDisplayName,
    //                     };

    //                     int ii = i;

    //                     PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
    //                     {

    //                         Dictionary<string, UserDataRecord> data2 = result2.Data;

    //                         playfabFriendsName.Add(data2["PlayerName"].Value);
    //                         Debug.Log("Added " + data2["PlayerName"].Value);

                            
    //                         if(ii == friends.Count - 1) {
    //                             GameManager.Instance.friendsIDForStatus = friendsToStatus;

    //                             chatClient.AddFriends(friendsToStatus.ToArray());



    //                             GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
    //                             //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

    //                             if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //                             {
    //                                 fbManager.getFacebookInvitableFriends();
    //                             }
    //                             else
    //                             {
    //                                 GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
    //                                 //facebookFriendsMenu.showFriends (null, null, null);
    //                             }
    //                             //alreadyGotFriends = true;
    //                         }
    //                         //GameManager.Instance.nameMy = data2["PlayerName"].Value;

    //                     }, (error) =>
    //                     {
    //                         playfabFriendsName.Add("Unknown");
    //                         if(ii == friends.Count - 1) {
    //                             GameManager.Instance.friendsIDForStatus = friendsToStatus;

    //                             chatClient.AddFriends(friendsToStatus.ToArray());



    //                             GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
    //                             //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

    //                             if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //                             {
    //                                 fbManager.getFacebookInvitableFriends();
    //                             }
    //                             else
    //                             {
    //                                 GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
    //                                 //facebookFriendsMenu.showFriends (null, null, null);
    //                             }
    //                             //alreadyGotFriends = true;
    //                         }
    //                         Debug.Log("Data updated error " + error.ErrorMessage);
    //                     }, null);


    //                     friendsToStatus.Add(friend.FriendPlayFabId);
    //                 }
    //             } else {
    //                 GameManager.Instance.friendsIDForStatus = friendsToStatus;

    //                         chatClient.AddFriends(friendsToStatus.ToArray());



    //                         GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
    //                         //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

    //                         if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //                         {
    //                             fbManager.getFacebookInvitableFriends();
    //                         }
    //                         else
    //                         {
    //                             GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
    //                             //facebookFriendsMenu.showFriends (null, null, null);
    //                         }
    //                         //alreadyGotFriends = true;
    //             }
                
    //         }, OnPlayFabError);
    //     }

        
    // }

    public void GetPlayfabFriends()
    {
        if (alreadyGotFriends)
        {
            Debug.Log("show firneds FFFF");
            if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
            {
                fbManager.getFacebookInvitableFriends();
            }
            else
            {

                facebookFriendsMenu.showFriends(null, null, null);
            }
        }
        else
        {
            Debug.Log("IND");
            GetFriendsListRequest request = new GetFriendsListRequest();
            request.IncludeFacebookFriends = true;
            PlayFabClientAPI.GetFriendsList(request, (result) =>
            {

                Debug.Log("Friends list Playfab: " + result.Friends.Count);
                var friends = result.Friends;

                List<string> playfabFriends = new List<string>();
                List<string> playfabFriendsName = new List<string>();
                List<string> playfabFriendsFacebookId = new List<string>();


                chatClient.RemoveFriends(GameManager.Instance.friendsIDForStatus.ToArray());

                List<string> friendsToStatus = new List<string>();


                int index = 0;
                foreach (var friend in friends)
                {

                
                    playfabFriends.Add(friend.FriendPlayFabId);

                    Debug.Log("Title: " + friend.TitleDisplayName);
                    GetUserDataRequest getdatarequest = new GetUserDataRequest()
                    {
                        PlayFabId = friend.TitleDisplayName,
                    };


                    int ind2 = index;

                    PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                    {

                        Dictionary<string, UserDataRecord> data2 = result2.Data;
                        playfabFriendsName[ind2] = data2["PlayerName"].Value;
                        Debug.Log("Added " + data2["PlayerName"].Value);
                        GameManager.Instance.facebookFriendsMenu.updateName(ind2, data2["PlayerName"].Value, friend.TitleDisplayName);

                    }, (error) =>
                    {
                        
                        Debug.Log("Data updated error " + error.ErrorMessage);
                    }, null);

                    playfabFriendsName.Add("");

                    friendsToStatus.Add(friend.FriendPlayFabId);

                    index++;
                }
            
                GameManager.Instance.friendsIDForStatus = friendsToStatus;

                chatClient.AddFriends(friendsToStatus.ToArray());



                GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
                //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

                if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
                {
                    fbManager.getFacebookInvitableFriends();
                }
                else
                {
                    GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
                    //facebookFriendsMenu.showFriends (null, null, null);
                }
                //alreadyGotFriends = true;
                
                
            }, OnPlayFabError);
        }

        
    }



    

    // Generic PlayFab callback for errors.
    void OnPlayFabError(PlayFabError error)
    {
        Debug.Log("Playfab Error: " + error.ErrorMessage);
    }

    // #######################  PHOTON  ##########################

    void GetPhotonToken()
    {
        GetPhotonAuthenticationTokenRequest request = new GetPhotonAuthenticationTokenRequest();
        request.PhotonApplicationId = PhotonAppID.Trim();//GameConstants.PhotonAppId.Trim();
                                                         // get an authentication ticket to pass on to Photon
        PlayFabClientAPI.GetPhotonAuthenticationToken(request, OnPhotonAuthenticationSuccess, OnPlayFabError);
    }


    public string authToken;
    // callback on successful GetPhotonAuthenticationToken request 
    void OnPhotonAuthenticationSuccess(GetPhotonAuthenticationTokenResult result)
    {
        string photonToken = result.PhotonCustomAuthenticationToken;
        Debug.Log(string.Format("Yay, logged in session token: {0}", photonToken));
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
        PhotonNetwork.AuthValues.AddAuthParameter("username", this.PlayFabId);
        PhotonNetwork.AuthValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
        PhotonNetwork.AuthValues.UserId = this.PlayFabId;
        PhotonNetwork.ConnectUsingSettings("1.0");
        PhotonNetwork.playerName = this.PlayFabId;

        //		PhotonNetwork.JoinLobby ();


        authToken = result.PhotonCustomAuthenticationToken;
        // chatClient = new ChatClient(this);
        // GameManager.Instance.chatClient = chatClient;
        // // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
        // ExitGames.Client.Photon.Chat.AuthenticationValues authValues = new ExitGames.Client.Photon.Chat.AuthenticationValues();
        // authValues.UserId = this.PlayFabId;
        // authValues.AuthType = ExitGames.Client.Photon.Chat.CustomAuthenticationType.Custom;
        // authValues.AddAuthParameter("username", this.PlayFabId);
        // authValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
        // chatClient.Connect(this.PhotonChatID, "1.0", authValues);
        getPlayerDataRequest();
        connectToChat();

    }

    public void connectToChat() {
        chatClient = new ChatClient(this);
        GameManager.Instance.chatClient = chatClient;
        // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
        ExitGames.Client.Photon.Chat.AuthenticationValues authValues = new ExitGames.Client.Photon.Chat.AuthenticationValues();
        authValues.UserId = this.PlayFabId;
        authValues.AuthType = ExitGames.Client.Photon.Chat.CustomAuthenticationType.Custom;
        authValues.AddAuthParameter("username", this.PlayFabId);
        authValues.AddAuthParameter("Token", authToken);
        chatClient.Connect(this.PhotonChatID, "1.0", authValues);
    }

    public void OnConnected()
    {
        Debug.Log("Photon Chat connected!!!");
        chatClient.Subscribe(new string[] { "invitationsChannel" });
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        GameManager.Instance.opponentDisconnected = true;



        if(GameManager.Instance.controlAvatars != null) {
            if(GameManager.Instance.readyToAnimateCoins) {
                GameManager.Instance.controlAvatars.playerDisconnected();
                
            } else {
                GameManager.Instance.controlAvatars.playerRejected = true;
            }

            GameManager.Instance.controlAvatars.hideMessageBubble();

            //GameManager.Instance.controlAvatars.playerRejected = true;
        }

        if(GameManager.Instance.cueController != null) {
            GameManager.Instance.cueController.HideAllControllers();
            Debug.Log("Player disconnected. You won");
            GameManager.Instance.playerDisconnected = true;
            PhotonNetwork.LeaveRoom();
            GameManager.Instance.iWon = true;
            GameManager.Instance.gameControllerScript.showMessage(GameManager.Instance.nameOpponent + " disconnected from room");
            GameManager.Instance.stopTimer = true;
            GameManager.Instance.cueController.youWonMessage.SetActive(true);
            GameManager.Instance.audioSources[3].Play();
            GameManager.Instance.cueController.youWonMessage.GetComponent<Animator>().Play("YouWinMessageAnimation");
        } 
    }

    public void showMenu()
    {

        menuCanvas.gameObject.SetActive(true);

        playerName.GetComponent<Text>().text = GameManager.Instance.nameMy;

        if (GameManager.Instance.avatarMy != null)
            playerAvatar.GetComponent<Image>().sprite = GameManager.Instance.avatarMy;

        splashCanvas.SetActive(false);
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed to a new channel - set online status!");

        //splashCanvas.SetActive (false);

        chatClient.SetOnlineStatus(ChatUserStatus.Online);


        // getPlayerDataRequest();
        //SceneManager.LoadScene("Menu");

        //menuCanvas.gameObject.SetActive (true);

        //playerName.GetComponent <Text> ().text = GameManager.Instance.nameMy;

        //if(GameManager.Instance.avatarMy != null)
        //	playerAvatar.GetComponent <Image> ().sprite = GameManager.Instance.avatarMy;


    }


    public void challengeFriend(string id, string message)
    {
        chatClient.SendPrivateMessage(id, "INVITE_SEND;" + id + this.PlayFabId + ";" + GameManager.Instance.nameMy + ";" + message);

        //chatClient.PublishMessage( "invitationsChannel", "So Long, and Thanks for All the Fish!" );
        Debug.Log("Send invitation to: " + id);

        //		RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        //		PhotonNetwork.JoinOrCreateRoom(id+this.PlayFabId, roomOptions, TypedLobby.Default);

    }




    string roomname;
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (!sender.Equals(this.PlayFabId))
        {
            if (message.ToString().Contains("INVITE_SEND"))
            {
                string roomName = message.ToString().Split(';')[1];
                int payout = Int32.Parse(message.ToString().Split(';')[3]);
                GameManager.Instance.tableNumber = Int32.Parse(message.ToString().Split(';')[4]);
                Debug.Log("INVITE_SEND " + message + "  " + sender + " room: " + roomName);
                GameManager.Instance.payoutCoins = payout;
                GameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(0, message.ToString().Split(';')[2], sender, roomName);

                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_ACCEPT"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_ACCEPT " + message + "  " + sender + " room: " + roomName);
                GameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(2, message.ToString().Split(';')[2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_REJECT"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_REJECT " + message + "  " + sender + " room: " + roomName);
                GameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(1, message.ToString().Split(';')[2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_START"))
            {
                string roomName = message.ToString().Split(';')[1];
                //				PhotonNetwork.JoinRoom (roomName);

                Debug.Log("INVITE_START " + message + "  " + sender + " room: " + roomName);
                //				GameManager.Instance.invitationDialog.GetComponent <PhotonChatListener> ().showInvitationDialog (1, message.ToString ().Split (';') [2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_STOP"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_STOP " + message + "  " + sender + " room: " + roomName);


                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().playerRejected = true;

                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = sender,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result) => {

                    Dictionary<string, UserDataRecord> data = result.Data;


                    if (data.ContainsKey("LoggedType")) { 
                        if (data["LoggedType"].Value.Equals("Facebook")) {
                            //callApiToGetOpponentData (data ["FacebookID"].Value);
                            getOpponentData(data);
                        } else {
                            Debug.Log("DUPADUPA");
                            if (data.ContainsKey("PlayerName")) {
                                GameManager.Instance.nameOpponent = data["PlayerName"].Value;
                                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                                backButtonMatchPlayers.SetActive(false);
                            } else {
                                GameManager.Instance.nameOpponent = "Guest453678";
                                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                                GameManager.Instance.controlAvatars.playerRejected = true;
                            }
                                
                            
                            //SceneManager.LoadScene ("GameScene");
                        }
                    } else {
                        GameManager.Instance.nameOpponent = "Guest453678";
                        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        GameManager.Instance.controlAvatars.playerRejected = true;
                    }

                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);



                //				GameManager.Instance.invitationDialog.GetComponent <PhotonChatListener> ().showInvitationDialog (1, message.ToString ().Split (';') [2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
        }
        //		Debug.Log ("INVITE RECEIVED " + message + "  " + sender);
        //
        //		roomname = message.ToString ();
        //
        //		Invoke ("join", 3.0f);

    }

    public void join()
    {
        PhotonNetwork.JoinRoom(roomname);
    }

    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnChatStateChange(ChatState state)
    {

    }

    
    public override void OnDisconnectedFromPhoton() {
        Debug.Log("Disconnected from photon");
        //GameManager.Instance.connectionLost.showDialog();
        switchUser();
    }

    public void DisconnecteFromPhoton() {
        PhotonNetwork.Disconnect();
    }

    public void switchUser() {
        //if(GameManager.Instance.playfabManager != null)
            GameManager.Instance.playfabManager.destroy();
        //if(GameManager.Instance.facebookManager != null)    
            GameManager.Instance.facebookManager.destroy();
        //if(GameManager.Instance.connectionLost != null)
            GameManager.Instance.connectionLost.destroy();
        //if(GameManager.Instance.adsScript != null)
            GameManager.Instance.adsScript.destroy();
        GameManager.Instance.avatarMy = null;
        GameManager.Instance.logged = false;

        //PlayerPrefs.DeleteAll();
        GameManager.Instance.resetAllData();
        GameManager.Instance.coinsCount = 0;
        SceneManager.LoadScene("LoginSplash");
    }

    public void OnDisconnected()
    {
        Debug.Log("Chat disconnected called!!!!!!!!!!! Reconnect");
        connectToChat();

        //GameManager.Instance.connectionLost.showDialog();
    }

    

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {

    }

    public void OnUnsubscribed(string[] channels)
    {

    }


    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log("STATUS UPDATE CHAT!");
        Debug.Log("Status change for: " + user + " to: " + status);

        bool foundFriend = false;
        for (int i = 0; i < GameManager.Instance.friendsStatuses.Count; i++)
        {
            string[] friend = GameManager.Instance.friendsStatuses[i];
            if (friend[0].Equals(user))
            {
                GameManager.Instance.friendsStatuses[i][1] = "" + status;
                foundFriend = true;
                break;
            }
        }

        if (!foundFriend)
        {
            GameManager.Instance.friendsStatuses.Add(new string[] { user, "" + status });
        }

        if (GameManager.Instance.facebookFriendsMenu != null)
            GameManager.Instance.facebookFriendsMenu.updateFriendStatus(status, user);
    }



    public override void OnJoinedLobby()
    {
        //getCoinsRequest();
        Debug.Log("OnJoinedLobby");

        isInLobby = true;
        //		PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        isInMaster = true;
        // when AutoJoinLobby is off, this method gets called when PUN finished the connection (instead of OnJoinedLobby())
        //PhotonNetwork.JoinRandomRoom();
        //		RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        //		PhotonNetwork.JoinOrCreateRoom("debugRoom", roomOptions, TypedLobby.Default);

        PhotonNetwork.JoinLobby();

    }

    public void JoinRoomAndStartGame()
    {
        //		RoomOptions roomOptions = new RoomOptions () { isVisible = false, maxPlayers = 2 };
        //		//PhotonNetwork.Joi
        //		PhotonNetwork.JoinOrCreateRoom("debugRoom", roomOptions, TypedLobby.Default);

        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "tbl", GameManager.Instance.tableNumber }, { "isAvailable", true} };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnPhotonRandomJoinFailed()
    {

        //RoomOptions roomOptions = new RoomOptions () { isVisible = true, maxPlayers = 2 };
        // PhotonNetwork.CreateRoom (null, roomOptions, TypedLobby.Default);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new String[] { "tbl", "isAvailable" };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "tbl", GameManager.Instance.tableNumber }, { "isAvailable", true} };
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
        

    }




    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        Debug.Log("Owner room: " + roomOwner);
        
        GameManager.Instance.avatarOpponent = null;

        if (!roomOwner)
        {

            GameManager.Instance.backButtonMatchPlayers.SetActive(false);

            GetUserDataRequest getdatarequest = new GetUserDataRequest()
            {
                PlayFabId = PhotonNetwork.otherPlayers[0].name,

            };

            PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
            {

                Dictionary<string, UserDataRecord> data = result.Data;


                if (data.ContainsKey("LoggedType")) {
                    if (data["LoggedType"].Value.Equals("Facebook")) {
                        //callApiToGetOpponentData (data ["FacebookID"].Value);
                        getOpponentData(data);
                    } else {
                        Debug.Log("DUPADUPA");
                        if (data.ContainsKey("PlayerName")) {
                            GameManager.Instance.nameOpponent = data["PlayerName"].Value;
                            GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        } else {
                            GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                            GameManager.Instance.controlAvatars.playerRejected = true;
                            GameManager.Instance.nameOpponent = "Guest568253";
                        }


                        //SceneManager.LoadScene ("GameScene");
                    }
                } else {
                    GameManager.Instance.nameOpponent = "Guest453678";
                    GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                    GameManager.Instance.controlAvatars.playerRejected = true;
                }

            }, (error) =>
            {
                Debug.Log("Data updated error " + error.ErrorMessage);
            }, null);


        }



    }


    public override void OnCreatedRoom()
    {
        roomOwner = true;
        GameManager.Instance.roomOwner = true;
        Debug.Log("OnCreatedRoom");

    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom called");
        roomOwner = false;
        GameManager.Instance.roomOwner = false;
        GameManager.Instance.resetAllData();

    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {

        GameManager.Instance.controlAvatars.hideLongTimeMessage();
        //SceneManager.LoadScene ("GameScene");
        PhotonNetwork.room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isAvailable", false } });
        Debug.Log("New player joined");
        GameManager.Instance.backButtonMatchPlayers.SetActive(false);
        //backButtonMatchPlayers.SetActive (false);
        GetUserDataRequest getdatarequest = new GetUserDataRequest()
        {
            PlayFabId = PhotonNetwork.otherPlayers[0].name,

        };

        PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
        {
            Debug.Log("Data updated successfull ");
            Dictionary<string, UserDataRecord> data = result.Data;



            if (data.ContainsKey("LoggedType")) {
                if (data["LoggedType"].Value.Equals("Facebook")) {

                    getOpponentData(data);
                    //callApiToGetOpponentData (data["FacebookID"].Value);

                    //				data.Add("PlayerName", GameManager.Instance.nameMy);
                    //				data.Add("PlayerAvatarUrl"

                } else {
                    if (data.ContainsKey("PlayerName")) {
                        GameManager.Instance.nameOpponent = data["PlayerName"].Value;
                        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                    } else {
                        GameManager.Instance.nameOpponent = "Guest675824";
                        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        GameManager.Instance.controlAvatars.playerRejected = true;
                    }




                    //SceneManager.LoadScene ("GameScene");

                }
            } else {
                GameManager.Instance.nameOpponent = "Guest453678";
                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                GameManager.Instance.controlAvatars.playerRejected = true;
            }

        }, (error) =>
        {
            Debug.Log("Data updated error " + error.ErrorMessage);
        }, null);
    }

    private void getOpponentData(Dictionary<string, UserDataRecord> data)
    {
        if(data.ContainsKey("PlayerName"))
            GameManager.Instance.nameOpponent = data["PlayerName"].Value;
        else
            GameManager.Instance.nameOpponent = "Guest857643";
        if(data.ContainsKey("PlayerAvatarUrl")) {
            StartCoroutine(loadImageOpponent(data["PlayerAvatarUrl"].Value));
        } else {
            GameManager.Instance.avatarOpponent = null;
            GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
            GameManager.Instance.backButtonMatchPlayers.SetActive(false);
        } 
        
    }

    public IEnumerator loadImageOpponent(string url)
    {
        // Load avatar image

        Debug.Log("Opponent image url: " + url);
        // Start a download of the given URL
        WWW www = new WWW(url);

        // Wait for download to complete
        yield return www;


        GameManager.Instance.avatarOpponent = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f), 32);

        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
        GameManager.Instance.backButtonMatchPlayers.SetActive(false);

        //		SceneManager.LoadScene ("GameScene");
    }

    //	private void callApiToGetOpponentData(string id)
    //	{
    //
    //
    //		FB.API(id + "?fields=first_name", Facebook.Unity.HttpMethod.GET, delegate(IGraphResult result) {
    //			GameManager.Instance.nameOpponent = result.ResultDictionary ["first_name"].ToString ();
    //
    //			FB.API("/" + id + "/picture?type=square&height=92&width=92", Facebook.Unity.HttpMethod.GET, delegate(IGraphResult result2) {
    //				if (result2.Texture != null) {
    //					// use texture
    //					GameManager.Instance.avatarOpponent = Sprite.Create(result2.Texture, new Rect(0, 0, result2.Texture.width, result2.Texture.height), new Vector2(0.5f, 0.5f), 32);
    //					SceneManager.LoadScene ("GameScene");
    //				}
    //			});
    //		});
    //	}









    //	public void OnGUI()
    //	{
    //		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    //	}





}
