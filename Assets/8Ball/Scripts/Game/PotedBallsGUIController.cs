using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PotedBallsGUIController : MonoBehaviour {

    public GameObject prefab;
    private GameObject parent;
    public Sprite[] sprite;
    private float addPos = 2.3f;
    private GameObject[] balls;
    [HideInInspector]
    private bool ownSolid = false;
    private int potedBallNumber;
    public GameObject ballsTypeMessage;

    public GameObject spotPrefab;

    private List<int> removedBallsSolid = new List<int>();
    private List<int> removedBallsStriped = new List<int>();
    private int cc = 0;
    private int cc1 = 0;
    public bool potedBallsVisible = false;
    // Use this for initialization
    void Start() {

        parent = GameObject.Find("PotedBallsGUI");


    }



    public void showPotedBalls(bool ownSolid) {

        this.ownSolid = ownSolid;

        if (!GameManager.Instance.offlineMode)
            GameManager.Instance.ownSolids = ownSolid;
        potedBallsVisible = true;
        balls = new GameObject[15];

        ballsTypeMessage.SetActive(true);

        if (!GameManager.Instance.offlineMode) {
            if (ownSolid)
                ballsTypeMessage.GetComponent<ChangeBallsMessageText>().changeSprite();
        } else {
            if (GameManager.Instance.offlinePlayerTurn == 1) {
                if (GameManager.Instance.offlinePlayer1OwnSolid) {
                    ballsTypeMessage.GetComponent<ChangeBallsMessageText>().changeSprite();
                }
            } else {
                if (!GameManager.Instance.offlinePlayer1OwnSolid) {
                    ballsTypeMessage.GetComponent<ChangeBallsMessageText>().changeSprite();
                }
            }
        }

        ballsTypeMessage.GetComponent<Animator>().Play("BallsMessageAnimation");

        //if (ownSolid) {
        cc = removedBallsSolid.Count;
        //        } else {
        //            cc = removedBallsStriped.Count;
        //        }



        for (int i = 0; i < 7; i++) {
            addBall(false, i);
        }

        //        if (ownSolid) {
        //            cc = removedBallsStriped.Count;  
        //} else {
        cc = 0;
        //}
        for (int i = 0; i < 7; i++) {
            addBall(true, i);
        }
    }




    private void addBall(bool isRight, int i) {
        GameObject ball = Instantiate(prefab);

        int cont = -1;

        if (isRight) {
            cont = 14 - i;
        } else {
            cont = i;
        }

        if (removedBallsSolid.Contains(cont) || removedBallsStriped.Contains(cont)) {
            if (isRight) {
                balls[14 - i] = ball;
            } else {
                balls[i] = ball;
            }

            ball.SetActive(false);
        } else {

            if (isRight) {
                ball.GetComponent<SpriteRenderer>().sprite = sprite[sprite.Length - 1 - i];
                balls[14 - i] = ball;
            } else {
                balls[i] = ball;
                ball.GetComponent<SpriteRenderer>().sprite = sprite[i];
            }
            ball.transform.parent = parent.transform;
            ball.transform.position = parent.transform.position;
            Vector3 pos = ball.transform.position;


            if (ownSolid) {
                if (isRight) {
                    pos.x = addPos + pos.x + cc * 0.5f;
                } else {
                    pos.x = -addPos - 3f + pos.x + cc * 0.5f;
                }
            } else {
                if (isRight) {
                    pos.x = -addPos + pos.x - cc * 0.5f;
                } else {
                    pos.x = addPos + 3f + pos.x - cc * 0.5f;
                }
            }

            cc++;
            ball.transform.position = pos;

        }
    }


    // Update is called once per frame
    void Update() {

    }

    public void setOwnSolid(bool solid) {
        ownSolid = solid;
    }

    public void hidePotedBall(int i) {
        potedBallNumber = i;
        Debug.Log("REMOVE_REMOVE: " + i);
        if (balls == null) {
            Debug.Log("REMOVE: " + i);
            if (i < 7) {
                removedBallsSolid.Add(i);
            } else if (i > 7) {
                removedBallsStriped.Add(i);
            }

        } else {
            Debug.Log("REMOVE_REMOVE: " + i);
            balls[i].GetComponent<Animator>().Play("PotedBallsGUIAnimation");
        }
    }

    public void moveOtherBalls() {
        int i = potedBallNumber;
        if (ownSolid) {
            if (i > 6) {
                for (int j = i - 1; j >= 8; j--) {
                    Vector3 pos = balls[j].transform.position;
                    pos.x -= 0.5f;
                    balls[j].transform.position = pos;
                }
            } else {
                for (int j = i - 1; j >= 0; j--) {
                    Vector3 pos = balls[j].transform.position;
                    pos.x += 0.5f;
                    balls[j].transform.position = pos;
                }
            }
        } else {
            if (i > 6) {
                for (int j = i - 1; j >= 8; j--) {
                    Vector3 pos = balls[j].transform.position;
                    pos.x += 0.5f;
                    balls[j].transform.position = pos;
                }
            } else {
                for (int j = i - 1; j >= 0; j--) {
                    Vector3 pos = balls[j].transform.position;
                    pos.x -= 0.5f;
                    balls[j].transform.position = pos;
                }
            }
        }
    }


}
