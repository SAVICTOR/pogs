using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SelectedTabluButtonClicked : MonoBehaviour {

    public int tableNumber;
    public int fee;

    // Use this for initialization

    void Start() {
        Debug.Log("start");
        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        gameObject.GetComponent<Button>().onClick.AddListener(startGame);
    }

    // Update is called once per frame
    void Update() {

    }


    public void startGame() {


        Debug.Log("Fee: " + fee + "  Coins: " + GameManager.Instance.coinsCount);
        if (GameManager.Instance.coinsCount >= fee) {

            if (GameManager.Instance.inviteFriendActivated) {
                GameManager.Instance.tableNumber = tableNumber;
                GameManager.Instance.payoutCoins = fee;
                GameManager.Instance.initMenuScript.backToMenuFromTableSelect();
                GameManager.Instance.playfabManager.challengeFriend(GameManager.Instance.challengedFriendID, "" + fee + ";" + tableNumber);

            } else {
                GameManager.Instance.tableNumber = tableNumber;
                GameManager.Instance.payoutCoins = fee;
                GameManager.Instance.facebookManager.startRandomGame();
            }

        } else {
            GameManager.Instance.dialog.SetActive(true);
        }

    }


}
