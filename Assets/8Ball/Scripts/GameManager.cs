﻿using System;
using UnityEngine;
using ExitGames.Client.Photon.Chat;
using System.Collections.Generic;


public class GameManager {
    private static GameManager instance;
    public bool offlineMode = false;
    public float playerTime = 30.0f * 2; // player time in seconds
    public bool readyToAnimateCoins = false;
    public bool showTargetLines = false;
    public bool callPocketBlack = false;
    public bool callPocketAll = false;
    public bool inviteFriendActivated = false;
    public InitMenuScript initMenuScript;
    public string challengedFriendID;
    public GameObject tablesCanvas;
    public bool stopTimer = false;
    public bool ownSolids = false;
    public bool playersHaveTypes = false;
    public bool firstBallTouched = false;
    public bool wasFault = false;
    public bool validPot = false;
    public int validPotsCount = 0;
    public string ownedCues = "'0'";
    public string ownedChats = "";
    public string faultMessage = "";
    public FacebookFriendsMenu facebookFriendsMenu;
    public GameObject matchPlayerObject;
    public GameObject backButtonMatchPlayers;
    public GameObject MatchPlayersCanvas;
    public GameControllerScript gameControllerScript;
    public FacebookManager facebookManager;
    public GameObject whiteBall;
    public bool testValue;
    public bool hasCueInHand = false;
    public int shotPower;
    public bool ballsStriked = false;
    public List<String> ballTouchBeforeStrike = new List<String>();
    public GameObject ballHand;
    public bool iWon = false;
    public bool iLost = false;
    public bool calledPocket = false;
    public int solidPoted = 0;
    public int stripedPoted = 0;
    public bool noTypesPotedStriped = false;
    public bool noTypesPotedSolid = false;
    public GameObject usingCueText;
    public int ballTouchedBand = 0;
    public bool receivedInitPositions = false;
    public Vector3[] initPositions;
    public GameObject[] balls;
    public bool logged = false;
    public List<string> friendsIDForStatus = new List<string>();

    public string nameMy;
    public Sprite avatarMy;
    public string avatarMyUrl;
    public GameObject dialog;

    public string nameOpponent;
    public Sprite avatarOpponent;

    public string opponentPlayFabID;
    public int offlinePlayerTurn = 1;
    public bool offlinePlayer1OwnSolid = true;
    public string facebookIDMy;
    public bool playerDisconnected = false;

    public GameObject invitationDialog;
    public ChatClient chatClient;

    public int coinsCount;
    public bool roomOwner = false;
    public float linesLength = 5.0f;
    public int avatarMoveSpeed = 15;
    public bool opponentDisconnected = false;
    public CueController cueController;
    public GameObject friendButtonMenu;
    public GameObject smallMenu;
    public PlayFabManager playfabManager;
    public float messageTime = 0;

    public int tableNumber = 0;
    public AudioSource[] audioSources;
    public int calledPocketID = 0;
    public GameObject coinsTextMenu;
    public GameObject coinsTextShop;
    public int cueIndex = 0;
    public int cuePower = 0;
    public int cueAim = 0;
    public int cueTime = 0;
    public IAPController IAPControl;
    public GameObject cueObject;
    public List<string[]> friendsStatuses = new List<string[]>();
    public int opponentCueIndex = 0;
    public int opponentCueTime = 0;
    public ControlAvatars controlAvatars;
    public InterstitialAdsControllerScript interstitialAds;
    public AdMobObjectController adsScript;
    public ConnectionLostController connectionLost;
    public bool opponentActive = true;
    // Game settings

    // 50, 100, 500, 2500, 10 000, 50 000, 100 000, 250 000, 500 000, 2 500 000, 5 000 000, 10 000 000, 15 000 000
    public int payoutCoins = 15000000;


    public void resetAllData() {
        opponentActive = true;
        readyToAnimateCoins = false;
        opponentDisconnected = false;
        offlinePlayerTurn = 1;
        offlinePlayer1OwnSolid = true;
        offlineMode = false;
        solidPoted = 0;
        stripedPoted = 0;
        messageTime = 0.0f;
        stopTimer = false;
        ownSolids = false;
        playersHaveTypes = false;
        firstBallTouched = false;
        wasFault = false;
        validPot = false;
        validPotsCount = 0;
        faultMessage = "";
        hasCueInHand = false;
        ballsStriked = false;
        ballTouchBeforeStrike = new List<String>();




        ballTouchedBand = 0;
        receivedInitPositions = false;
    }




    private GameManager() { }

    public static GameManager Instance {
        get {
            if (instance == null) {
                instance = new GameManager();
            }
            return instance;
        }
    }

    public void resetTurnVariables() {
        stopTimer = false;
    }

}
