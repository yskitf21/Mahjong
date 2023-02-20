using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Pai : MonoBehaviour
{
    public GameObject PaiPrefab;
    public bool IsReverse = false;
    public enum Mark {
        Manzu,
        Souzu,
        Pinzu,
        Jihai,
    }
    public Mark CurrentMark = Mark.Manzu;
    public int CurrentNumber = 1;
    public enum Status
    {
        Player = 0,
        Right = 1,
        Opposite = 2,
        Left = 3,
    }
    public Status CurrentStatus = Status.Player;
    
    public class Data
    {
        public Mark Mark;
        public int Number;
    }
    //構造体、エラーが出る ←未定義のものがあるから（名刺と携帯連絡先の話）
    /*
    public struct Data
    {
        public int Number;
        public Mark Mark;
        public static bool operator == (Data x, Data y){return (x.Number == y.Number && x.Mark == y.Mark);}
        public static bool operator != (Data x, Data y){return (x.Number != y.Number || x.Mark != y.Mark);}
    }
    */

    public Data GetPaidata()
    {
        Data pai = new Data()
        {
            Mark = CurrentMark,
            Number = CurrentNumber,
        };
        return pai;
    }

    private Sprite[] paiSprite;
    public int usenumber;
    public int Getusenumber()
    {
        usenumber = (int)CurrentMark * 10 + CurrentNumber;
        return usenumber;
    }

    public void SetPai(int number, Mark mark, Status status, bool isReverse)
    {
        //(範囲内に収めたい値) = Mathf.Clamp(範囲内に指定したい値,最小値,最大値);　超えるとエラーが出る
        CurrentNumber = Mathf.Clamp(number, 1, 9);
        CurrentMark = mark;
        CurrentNumber = number;
        CurrentStatus = status;
        paiSprite = Resources.LoadAll<Sprite>("sheet2");
        IsReverse = isReverse;
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 scale = transform.localScale;

        //オモテ表示
        if (!IsReverse)
        {
            int num = number + (int)mark * 10;
            /*
            if(GameScene.richiCalled == true)
            {
                num = num + 70;
            }
            */
            PaiPrefab.GetComponent<Image>().sprite = paiSprite[num];
            scale.x = 1;
            scale.y = 1;
            transform.localScale = scale;
        }
        //ウラ表示
        else
        {
            switch(status)
            {
                case Status.Player:
                PaiPrefab.GetComponent<Image>().sprite = paiSprite[62];
                scale.x = 1;
                scale.y = -1;
                transform.localScale = scale;
                break;
                case Status.Left:
                PaiPrefab.GetComponent<Image>().sprite = paiSprite[70];
                scale.x = -1;
                scale.y = 1;
                transform.localScale = scale;
                break;
                case Status.Opposite:
                PaiPrefab.GetComponent<Image>().sprite = paiSprite[62];
                scale.x = 1;
                scale.y = 1;
                transform.localScale = scale;
                break;
                case Status.Right:
                PaiPrefab.GetComponent<Image>().sprite = paiSprite[70];
                scale.x = 1;
                scale.y = 1;
                transform.localScale = scale;
                break;
            }
        }
    }

    //Inspectorから入力した時でも、プレハブに入力内容が適応されるようにするメソッド
    
    public void OnValidate()
    {
        SetPai(CurrentNumber,  CurrentMark, CurrentStatus, IsReverse);
    }
}