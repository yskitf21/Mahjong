using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;



public class ResultScene : MonoBehaviour
{
    public Pai PaiPrefab;
    public GameObject Hand;
    public Text[] Points;
    public Text Winner;
    Coroutine _gameLoopCoroutine;
    public GameObject NextButton;
    public GameObject RankingButton;

    void Start()
    {
        RankingButton.SetActive(false);
        Winner.text = GameScene.winningtext;
        DisplayPoints();
        if(!GameScene.isDrawned)
        {
            DisplayHands();
        }
        else
        {
            DestroyHands();
        }
        if(GameScene.isOver)
        {
            NextButton.SetActive(false);
            RankingButton.SetActive(true);
        }
    }

    List<Pai.Data> Readwinnerhand()
    {
        try
        {
            return GameScene.winnerhand;
        }
        catch (System.Exception)
        {
            Debug.Log("winnerhandはありません");
            return new List<Pai.Data>();
            throw;
        }
    }

    void DisplayPoints()
    {
        var list1 = GameScene.pastpoints;
        var list2 = GameScene.points;
        for(var i = 0; i < 4; i++)
        {
            Points[i].text = "プレイヤー" + i + "　　" + list1[i] + "　→　" + list2[i];
        }
    }

    void DisplayHands()
    {
        List<Pai.Data> list = Readwinnerhand();
        for(var i = 0; i < list.Count; i++)
        {
            Transform childTransform = Hand.transform.GetChild(i);
            Pai oldpai = childTransform.GetComponent<Pai>();
            Pai.Data newpaiData = list[i];
            var status = (Pai.Status)0;
            oldpai.SetPai(newpaiData.Number, newpaiData.Mark, status, false);
        }
    }

    void DestroyHands()
    {
        foreach (Transform childTransform in Hand.transform)
        {
            Object.Destroy(childTransform.gameObject);
        }
    }

    public void GotoGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void GotoEndScene()
    {
        SceneManager.LoadScene("EndScene");
    }
}