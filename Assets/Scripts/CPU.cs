using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPU : MonoBehaviour
{
    public Pai PaiPrefab;
    List<int> cpuhand;
    List<int> removedtiles;
    int[] discardorder = {1, 9, 2, 8, 3, 7, 4, 6, 5};
    int discardpai;

    // Start is called before the first frame update
    void Start()
    {
        removedtiles = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Main()
    {   
        discardpai = 0;
        //tmpの現在の手札（ソートされる前）を保存
        var tmp = Copyhand();
        //コピーしてソート
        cpuhand = Copyhand();
        cpuhand.Sort();
        //Removeしていき、捨てる牌を決定
        RemoveRun();
        RemoveTriple();
        RemoveRyanmen();
        RemoveDouble();
        RemoveKanchan();
        RemovePenchan();
        RemoveFloat();
        //その牌が左から何番目にあるか検索
        int num = Searchdiscardpai(tmp);
        Debug.Log("捨てるべき牌は" + discardpai);
        Debug.Log("配列の" + num + "番目を捨てる");
        return num;
    }

    void RemoveRun()
    {
        var list = cpuhand;
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < list.Count-2; j++)
            {
                if(list.Find(m => m == list[j] + 1) == list[j] + 1 && list.Find(n => n == list[j] + 2) == list[j] + 2)
                {
                    list.Remove(list[j] + 2);
                    list.Remove(list[j] + 1);
                    list.Remove(list[j]);
                    break;
                }
            }
        }
    }

    void RemoveTriple()
    {
        var list = cpuhand;
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < list.Count-2; j++)
            {
                if(list[j] == list[j+1] && list[j] == list[j+2])
                {
                    for(int k = 0; k < 3; k++)
                    {
                        list.Remove(list[j]);
                    }
                    break;
                }
            }
        }
    }

    void RemoveRyanmen()
    {
        var list = cpuhand;
        for(int i = 0; i < 7; i++)
        {
            for(int j = 0; j < list.Count-1; j++)
            {
                if(list[j] % 10 != 1 && list[j] % 10 != 8)
                {
                    if(list.Contains(list[j] + 1))
                    {
                        Setremovedtiles(list[j], 1);
                        list.Remove(list[j] + 1);
                        list.Remove(list[j]);
                        break;
                    }
                }
            }
        }
        if(list.Count == 0)
        {
            discardpai = ChooseFloat();
        }
    }

    void RemoveDouble()
    {
        var list = cpuhand;
        removedtiles.Clear();
        if(list.Count != 0)
        {
            for(int i = 0; i < 7; i++)
            {
                for(int j = 0; j < list.Count-1; j++)
                {
                    if(list[j] == list[j+1])
                    {
                        Setremovedtiles(list[j], 0);
                        for(int k = 0; k < 2; k++)
                        {
                            list.Remove(list[j]);
                        }
                        break;
                    }
                }
            }
            if(list.Count == 0)
            {
                discardpai = ChooseFloat();
            }
        }
    }


    void RemoveKanchan()
    {
        var list = cpuhand;
        removedtiles.Clear();
        if(list.Count != 0)
        {
            for(int i = 0; i < 7; i++)
            {
                for(int j = 0; j < list.Count-1; j++)
                {
                    //9と11が嵌張で繋がるのを防ぐ,かつ嵌張があるとき
                    if(list[j] % 10 != 9 && list.Contains(list[j] + 2))
                    {
                        Setremovedtiles(list[j], 2);
                        list.Remove(list[j] + 2);
                        list.Remove(list[j]);
                        break;
                    }
                }
            }
            if(list.Count == 0)
            {
                discardpai = ChooseFloat();
            }
        }
    }

    void RemovePenchan()
    {
        var list = cpuhand;
        removedtiles.Clear();
        if(list.Count != 0)
        {
            for(int i = 0; i < 7; i++)
            {
                for(int j = 0; j < list.Count-1; j++)
                {
                    //辺張のときだけ走らせるための条件付け
                    if(list[j] % 10 == 1 || list[j] % 10 == 8)
                    {
                        if(list.Contains(list[j] + 1))
                        {   
                            Setremovedtiles(list[j], 1);
                            list.Remove(list[j] + 1);
                            list.Remove(list[j]);
                            break;
                        }
                    }
                }
            }
            if(list.Count == 0)
            {
                discardpai = ChooseFloat();
            }
        }
    }

    void RemoveFloat()
    {
        var list = cpuhand;
        removedtiles.Clear();
        if(list.Count != 0)
        {
            foreach(int pai in cpuhand)
            {
                removedtiles.Add(pai);
            }
            list.Clear();
            discardpai = ChooseFloat();
        }
    }


    List<int> Copyhand()
    {
        var list = new List<int>(14);
        foreach(int pai in GameScene.usenumbers[GameScene.currentPlayer])
        {
            list.Add(pai);
        }
        return list;
    }

    //両面、嵌張、辺張の弱い方の牌をゴミ箱へ送る
    void Setremovedtiles(int x, int y)
    {
        foreach(int number in discardorder)
        {
            if(x % 10 == number)
            {
                removedtiles.Add(x);
                break;
            }
            if((x + y) % 10 == number)
            {
                removedtiles.Add(x + y);
                break;
            }
        }
    }

    /*
    //余りがxの浮き牌リストを返す
    List<int> SearchFloat(List<int> list, int x)
    {
        List<int> extrapais = new List<int>();
        foreach(int i in list)
        {
            if(i % 10 == x)
            {
                extrapais.Add(i);
            }
        }
        return extrapais;
    }
    */

    //ゴミ箱から捨てる牌を決定して返す
    int ChooseFloat()
    {
        //乱数xで捨てさせる場合の設定
        var random = new System.Random();
        int x = random.Next(removedtiles.Count);


        //Removeしていって0になったらゴミ箱からランダムに1つ返す
        if(cpuhand.Count == 0)
        {
            //乱数ではない処理で捨てさせる場合のreturn
            int pai = 0;
            bool isRemoved = false;
            foreach(int y in discardorder)
            {
                foreach(int z in removedtiles)
                {
                    if(z % 10 == y)
                    {
                        pai = z;
                        isRemoved = true;
                        break;
                    }
                }
                if(isRemoved)
                {
                    break;
                }
            }
            return pai;


            //乱数xで捨てさせる場合のreturn
            //return removedtiles[x];
        }
        else
        {
            return 0;
        }
    }

    int Searchdiscardpai(List<int> list)
    {
        int count = 0;
        foreach(int pai in list)
        {
            if(pai == discardpai)
            {
                break;
            }
            count++;
        }
        return count;
    }

    //リーチ宣言牌のusenumber(num)がリストの何番目にあるのかを返す
    public int Searchrichicallpai(List<int> list, int num)
    {
        int count = 0;
        foreach(int pai in list)
        {
            if(pai == num)
            {
                break;
            }
            count++;
        }
        return count;
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
}
