using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndScene : MonoBehaviour
{
    List<int> ranking= new List<int>(4);
    List<int> tmp = new List<int>(4);
    public Text[] Ranking;

    void Start()
    {
        Settmp();
        tmp.Sort();
        tmp.Reverse();
        DebugList(tmp);
        Setranking();
        DisplayRanking();
    }

    void Setranking()
    {
        foreach(int x in tmp)
        {
            //xが何番目に格納されているかチェックし、rankingに格納
            var index = GameScene.points.IndexOf(x);
            ranking.Add(index);

            GameScene.points[index] = -200000;
        }
        DebugList(ranking);
    }

    void Settmp()
    {
        foreach(int point in GameScene.points)
        {
            tmp.Add(point);
        }
    }
    
    void DebugList(List<int> list)
    {
        string log = "";

        foreach(var x in list)
        {
            log += ( x.ToString() + "," );
        }

    Debug.Log(log);
    }

    void DisplayRanking()
    {
        for(int i = 0; i < 4; i++)
        {
            Ranking[i].text = (i+1) + "位　プレイヤー" + ranking[i] + "　" + tmp[i] + "点";  ;
        }
    }

    void Update()
    {
        
    }
    
}
