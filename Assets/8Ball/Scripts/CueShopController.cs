using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Globalization;

public class CueShopController : MonoBehaviour {

    public int index;


    public int additionalPower = 0;
    public int additionalAim = 0;
    public int additionalTime = 0;
    public int price = 25;



    public GameObject priceText;

    public GameObject[] forceIndicators;
    public GameObject[] aimIndicators;
    public GameObject[] timeIndicators;

    public GameObject button;
    public GameObject buttonText;
    public Sprite blueSprite;
    private bool own = false;
    // Use this for initialization
    void Start() {

        priceText.GetComponent<Text>().text = price.ToString("0,0", CultureInfo.InvariantCulture).Replace(',', ' ');



        for (int i = 0; i < additionalPower; i++) {
            forceIndicators[i].GetComponent<Image>().color = Color.green;
        }

        for (int i = 0; i < additionalAim; i++) {
            aimIndicators[i].GetComponent<Image>().color = Color.green;
        }

        for (int i = 0; i < additionalTime; i++) {
            timeIndicators[i].GetComponent<Image>().color = Color.green;
        }

        if (GameManager.Instance.ownedCues.Contains("'" + index + "'")) {
            button.GetComponent<Image>().sprite = blueSprite;
            buttonText.GetComponent<Text>().text = "Use";
            own = true;
        }

        if (GameManager.Instance.cueIndex == index) {
            GameManager.Instance.usingCueText = buttonText;
            buttonText.GetComponent<Text>().text = "Using";
        }

    }

    public void buyCue() {
        if (own) {
            GameManager.Instance.playfabManager.setUsedCue(this.index, this.additionalPower, this.additionalAim, this.additionalTime);
            GameManager.Instance.usingCueText.GetComponent<Text>().text = "Use";
            GameManager.Instance.usingCueText = buttonText;
            buttonText.GetComponent<Text>().text = "Using";

        } else {
            if (GameManager.Instance.coinsCount >= this.price) {
                GameManager.Instance.playfabManager.addCoinsRequest(-this.price);
                GameManager.Instance.playfabManager.updateBoughtCues(this.index);
                button.GetComponent<Image>().sprite = blueSprite;
                buttonText.GetComponent<Text>().text = "Use";
                own = true;
            } else {
                GameManager.Instance.dialog.SetActive(true);
            }
        }
    }
}


