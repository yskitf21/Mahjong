using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;

//CPUを参戦させる前のものをバックアップ
public class GameSceneBackup : MonoBehaviour
{
    public Pai PaiPrefab;
    public GameObject[] Hands;
    public GameObject[] Rivers;
    public GameObject[] RichiButtons;
    public GameObject[] Chips;
    public GameObject Winning;
    public Text[] Points;
    public Text[] Winds;
    public Text Remaining;
    public Text AllWind;
    public Text WinningText;
    public Text Round;
    public List<Pai.Data>[] hands = new List<Pai.Data>[4];
    public List<Pai.Data>[] rivers;
    public static List<int>[] usenumbers;
    public static List<int> points = new List<int>(4);
    public static string[] winds = {"東", "南", "西", "北"};
    int startpoints = 25000;
    static int windcount = 1;
    static int round = 0;
    public static int currentPlayer = 0;
    List<Pai.Data> pile;
    Coroutine _gameLoopCoroutine;
    PointerEventData pointer;
    List<int> headcandidates  = new List<int>(7);
    public enum State
    {
        Init,
        Deal,
        Discard,
        Over,
    }
    public State CurrentState = State.Deal;
    public static bool isOver = false;
    bool winning = false;
    static int parent = 0;
    int[] Jihainumber = {1, 9, 11, 19, 21, 29, 31};
    int runcount = 0;
    int triplecount = 0;
    List<int> tmp1 = new List<int>(14);
    List<int> tmp2 = new List<int>(14);
    int score = 0;
    List<string> yaku = new List<string>();
    bool isReverse = false;
    List<int> allPaiData;
    bool[] isRichi = new bool[4];
    List<int> richicallpais = new List<int>();
    bool richiCalled = false;
    //音声宣言用♪
    AudioSource audioSource;
    public AudioClip deal_sound;
    public AudioClip set_sound;
    public AudioClip richi_sound;
    public AudioClip ron_sound;
    public AudioClip tsumo_sound;
    public static List<Pai.Data> winnerhand = new List<Pai.Data>();
    Pai.Data discardedpai = new Pai.Data();
    public static List<int> pastpoints = new List<int>(4);
    public static bool isStarted = false;
    public static bool isDrawned = false;
    List<int> tempaidplayer;
    public static string winningtext;
    bool isTempai = false;
    bool isNotem;
    int wangpai = 14;
    public GameObject ResultButton;
    public GameObject CPU;

    void Start()
    {

        AllWind.text = "東" + windcount + "局";
        Round.text = round + "本場";
        ResultButton.SetActive(false);
        Winning.SetActive(false);
        winning = false;
        isReverse = true;
        isDrawned = false;
        //Sceneが遷移しても(終局しても)オブジェクトが破壊されないようにする
        //↓シングルトンパターンで使う？
        //DontDestroyOnLoad (this);
        triplecandidates = new List<int>(4);
        triplecombinations = new List<int>[16];
        usenumbers = new List<int>[4];
        rivers = new List<Pai.Data>[4];
        tempaidplayer = new List<int>(4);
        SetWinds();
        //ゲーム開始時の1回のみ走らせる
        if(!isStarted)
        {
            
            for(int i = 0; i < 4; i++)
            {
                points.Add(startpoints);
            }
            /*
            isStarted = true;
            points.Add(26000);
            points.Add(25000);
            points.Add(23000);
            points.Add(27000);
            */
        }
        else
        {
            SetPoints();
        }

        audioSource = GetComponent<AudioSource>();
        pointer = new PointerEventData(EventSystem.current);
        _gameLoopCoroutine = StartCoroutine(GameLoop());
    }

    
    IEnumerator GameLoop()
    {
        InitGame();
        //↓無いとなぜかバグる
        yield return new WaitForSeconds(1);
        DealPais();
        yield return new WaitForSeconds(1);
        audioSource.PlayOneShot(set_sound);
        do
        {
        //ターンプレイヤー以外の牌を隠す
            CurrentState = State.Discard;
            for(int i = 0; i < 4; i++)
            {
                if(i == currentPlayer)
                {
                    isReverse = false;
                    DisplayHands(i);
                }
                else
                {
                    isReverse = true;
                    DisplayHands(i);
                }
            }
            Winds[currentPlayer].color = Color.yellow;
            isReverse = false;
            richiCalled = false;
            Debug.Log("現在のプレイヤーは" + currentPlayer.ToString() + "です");
            DealPai(currentPlayer);
            var hand = hands[currentPlayer];
            Pai.Data drawedpai = hand[13];
            Setusenumber(currentPlayer);
            RemainingUpdate();
            Sort(hands[currentPlayer]);
            JudgeReady(currentPlayer);
            //リーチしてないかつ残り牌が4枚以上ならテンパイ判定
            if(!isRichi[currentPlayer] && (pile.Count - wangpai) >= 4)
            {
                JudgeRichi(currentPlayer);
            }
            int num = CPU.GetComponent<CPU>().Main();
            do
            {
                if(!isRichi[currentPlayer] || richiCalled)
                {
                    yield return new WaitWhile(() => Input.GetMouseButtonDown(0));
                    if (Input.GetMouseButtonDown(0))
                    {
                        List<RaycastResult> results = new List<RaycastResult>();
                        pointer.position = Input.mousePosition;
                        // ヒットしたUIの名前をresultsに格納
                        EventSystem.current.RaycastAll(pointer, results);
                        foreach (RaycastResult target in results)
                        {
                            //targetがPaiなのかResetButtonなのかで分岐させる（メソッドをそれぞれ作る）
                            if(target.gameObject.name == "Pai(Clone)")
                            {
                                Pai clickedPai = target.gameObject.GetComponent<Pai>();
                                Pai.Data clickedPaidata = clickedPai.GetPaidata();
                                //Pai.Data y = new Pai.Data();
                                {
                                    //クリックされた牌がターンプレイヤーの牌だったら
                                    if((int)clickedPai.CurrentStatus == currentPlayer)
                                    {
                                        bool correct = false;
                                        //手牌のリストから一致するものを1つ検索して、それをyとする
                                        foreach(Pai.Data x in hands[currentPlayer])
                                        {
                                            if(x.Number == clickedPaidata.Number && x.Mark == clickedPaidata.Mark)
                                            {
                                                
                                                //リーチ宣言がされていた場合
                                                if(richiCalled)
                                                {
                                                    foreach(int i in richicallpais)
                                                    {
                                                        //宣言牌となりうるものがクリックされたときだけStateを変更
                                                        if(i == clickedPaidata.Number + (int)clickedPaidata.Mark * 10)
                                                        {
                                                            discardedpai = x;
                                                            correct = true;
                                                            Chips[currentPlayer].SetActive(true);
                                                            audioSource.PlayOneShot(richi_sound);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    discardedpai = x;
                                                }
                                            }
                                        }
                                        //非リーチ宣言中か、リーチ宣言中かつ正しい捨て牌が選ばれた時のみターン処理
                                        if(!richiCalled || richiCalled && correct)
                                        {
                                                
    //自分が切った牌でロン判定になるのでいずれ修正
                                            CurrentState = State.Deal;
                                            //オブジェクトを河に捨て、河のリストに追加
                                            rivers[currentPlayer].Add(discardedpai);
                                            Object.Instantiate(target.gameObject, Rivers[currentPlayer].transform);
                                            Transform drawedPai = Hands[currentPlayer].transform.GetChild(13);
                                            Object.Destroy(drawedPai.gameObject);

                                            //手牌のリストから消去
                                            hands[currentPlayer].Remove(discardedpai);

                                            //捨てた後の手牌をソート
                                            Sort(hands[currentPlayer]);
                                            
                                            //usenumberを更新
                                            Setusenumber(currentPlayer);
                                            DisplayHands(currentPlayer);
                                            
                                            RichiButtons[currentPlayer].SetActive(false);
                                            //ここからロン判定
                                            for(int i = 0; i < 4; i++)
                                            {
                                                //捨て牌を配列に加える
                                                hands[i].Add(discardedpai);
                                                Sort(hands[i]);
                                                Setusenumber(i);
                                                JudgeReady(i);
                                                hands[i].Remove(discardedpai);
                                                Setusenumber(i);
                                            }
                                            //ここまでロン判定
                                            //流局判定
                                            JudgeDrawngame();
                                            
                                            //ここに流局時の処理を書く
                                            if(isDrawned)
                                            {
                                                DrawnGame();
                                                yield return new WaitForSeconds(3);
                                            }
                                            else
                                            {
                                            //勝利判定が下りていないなら、次のプレイヤーにターンを渡す
                                                if(!winning)
                                                {
                                                Winds[currentPlayer].color = Color.white;
                                                currentPlayer = (currentPlayer + 1) % 4;
                                                }
                                            }
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //リーチ中
                    CurrentState = State.Deal;
                    yield return new WaitForSeconds(1);
                    AutoDiscardPai();
                    //流局判定
                    JudgeDrawngame();
                    if(isDrawned)
                    {
                        DrawnGame();
                        yield return new WaitForSeconds(3);
                    }
                    else
                    {
                        Winds[currentPlayer].color = Color.white;
                        currentPlayer = (currentPlayer + 1) % 4;
                    }
                }
                if(winning)
                {
                    Winning.SetActive(true);
                    yield return new WaitForSeconds(2);
                    winning = false;
                }
            } while(CurrentState == State.Discard);
        } while(CurrentState != State.Over);
        JudgeGameOver();
        //SceneManager.LoadScene("ResultScene");
        ResultButton.SetActive(true);
    }

    void InitGame()
    {
        foreach(GameObject obj in Hands)
        {
            foreach (Transform childTransform in obj.transform)
            {
                Object.Destroy(childTransform.gameObject);
            }
        }
        foreach(GameObject obj in Rivers)
        {
            foreach (Transform childTransform in obj.transform)
            {
                Object.Destroy(childTransform.gameObject);
            }
        }
        for(var i = 0; i < 4; i++)
        {
            hands[i] = new List<Pai.Data>(14);
            rivers[i] = new List<Pai.Data>();
            usenumbers[i] = new List<int>(14);
            isRichi[i] = false;
            RichiButtons[i].SetActive(false);
            Chips[i].SetActive(false);
            Winds[i].color = Color.white;
        }
        for(int i = 0; i < 16; i++)
        {
            triplecombinations[i] = new List<int>(4);
        }
        GeneratePais();
        ShufflePais();
        RemainingUpdate();
        Sort(hands[currentPlayer]);
        JudgeReady(currentPlayer);
        CurrentState = State.Discard;
    }

    void RemainingUpdate()
    {
        Remaining.text = "残" + (pile.Count - wangpai) + "枚";
    }

    void JudgeGameOver()
    {
        if(windcount >= 5)
        {
            isOver = true;
        }
        for(int i = 0; i < 4; i++)
        {
            if(points[i] < 0)
            isOver = true;
        }
    }

    void GeneratePais()
    {   
        pile = new List<Pai.Data>(34 * 4);
        allPaiData = new List<int>(34);
        var marks = new List<Pai.Mark>() 
        {
            Pai.Mark.Manzu,
            Pai.Mark.Souzu,
            Pai.Mark.Pinzu,
            Pai.Mark.Jihai,
        };

        //牌山の生成
        for(int i = 0; i < 4; i++)
        {
            foreach(var mark in marks)
            {
                //数牌を積む
                if(mark != Pai.Mark.Jihai)
                {
                    for(int num = 1; num < 10; ++num)
                    {
                        var pai = new Pai.Data()
                        {
                            Mark = mark,
                            Number = num,
                        };
                        pile.Add(pai);
                        if(i == 0)
                        {
                            int j = (int)mark * 10 + num;
                            allPaiData.Add(j);
                        }
                    }
                }
                //字牌を積む
                else
                {
                    /*
                    foreach(int num in Jihainumber)
                    {
                        var pai = new Pai.Data()
                        {
                            Mark = mark,
                            Number = num,
                        };
                        pile.Add(pai);
                        if(i == 0)
                        {
                            int j = (int)mark * 10 + num;
                            allPaiData.Add(j);
                        }
                    }
                    */
                }
            }
        }
    }

//使いたい牌山を生成するプログラムを別で書けるとベスト

    void ShufflePais()
    {
        int ShuffleCount = 100;
        //var random = new System.Random();

        // 乱数の初期化
        var randomEx = new RandomEx(0); // 固定のシード値を設定する。

        for(var i=0; i<ShuffleCount; ++i)
        {
            //List.Count　→　現在Listの中にある要素の個数
            //Random.Next　→　指定した値より小さい0以上のランダムな整数値を返す。
            //int index = random.Next(pile.Count);
            //int index2 = random.Next(pile.Count);
            int index = randomEx.Range(0, pile.Count);
            int index2 = randomEx.Range(0, pile.Count);
 
            //配列の中の[index]番目と[index2]番目のカードの位置を入れ替える。
            var tmp = pile[index];
            pile[index] = pile[index2];
            pile[index2] = tmp;
        }
    }

    //配牌
    void DealPais()
    {
        for(var i = 0; i < 4; i++)
        {
            //リストに加える
            for(var j = 1; j <= 13; j++)
            {
                DealPai(i);
            }
            Sort(hands[i]);
            DisplayHands(i);
        }
    }
    
    //手牌のリストをソート
    public void Sort(List<Pai.Data> list)
    {
        list.Sort ((a, b) => a.Number - b.Number);
        list.Sort ((a, b) => a.Mark - b.Mark);
    }

    //プレイヤーxに牌を1枚配る
    void DealPai(int x)
    {
        var pai = pile[0];
        pile.Remove(pai);
        hands[x].Add(pai);
        var paiObj = Object.Instantiate(PaiPrefab, Hands[x].transform);
        var status = (Pai.Status)x;
        DisplayHands(x);
        audioSource.PlayOneShot(deal_sound);
    }

    void AutoDiscardPai()
    {
        Transform drawedPai = Hands[currentPlayer].transform.GetChild(13);
        Pai drawedpai = drawedPai.GetComponent<Pai>();
        var drawedpaiData = drawedpai.GetPaidata();
        Object.Instantiate(drawedPai.gameObject, Rivers[currentPlayer].transform);
        Object.Destroy(drawedPai.gameObject);
        var y = new Pai.Data();
        foreach(Pai.Data x in hands[currentPlayer])
        {
            if(x.Number == drawedpaiData.Number && x.Mark == drawedpaiData.Mark)
            {
                y = x ;
            }
        }
        rivers[currentPlayer].Add(y);
        hands[currentPlayer].Remove(y);
        Sort(hands[currentPlayer]);
        Setusenumber(currentPlayer);
        DisplayHands(currentPlayer);
    }

    void CPUDiscardPai(int num)
    {
        //num番目のPaiコンポーネントを取得
        Transform drawedPai = Hands[currentPlayer].transform.GetChild(num);
        Pai drawedpai = drawedPai.GetComponent<Pai>();
        var drawedpaiData = drawedpai.GetPaidata();
        Object.Instantiate(drawedPai.gameObject, Rivers[currentPlayer].transform);
        Object.Destroy(drawedPai.gameObject);
        var y = new Pai.Data();
        foreach(Pai.Data x in hands[currentPlayer])
        {
            if(x.Number == drawedpaiData.Number && x.Mark == drawedpaiData.Mark)
            {
                y = x ;
            }
        }
        rivers[currentPlayer].Add(y);
        hands[currentPlayer].Remove(y);
        Sort(hands[currentPlayer]);
        Setusenumber(currentPlayer);
        DisplayHands(currentPlayer);
    }

    //hands[x]を使って、xの手牌を表示
    void DisplayHands(int x)
    {
        GameObject Hand = Hands[x];
        for(var i = 0; i < hands[x].Count; i++)
        {
        //i番目の子オブジェクトのPaiコンポーネントを取得
        Transform childTransform = Hand.transform.GetChild(i);
        Pai oldpai = childTransform.GetComponent<Pai>();

        //手牌のi番目のデータを取得
        List<Pai.Data> list = hands[x];
        Pai.Data newpaiData = list[i];

        //書き換えて表示
        var status = (Pai.Status)x;

//正規設定に戻す（ターンプレイヤー以外の手牌を隠す）にはfalseをisReverseに書き換える
        oldpai.SetPai(newpaiData.Number, newpaiData.Mark, status, isReverse);
        }
    }

    //xの手牌のusenumberのリストを作成
    void Setusenumber(int x)
    {   
        usenumbers[x] = new List<int>(14);
        foreach(Pai.Data data in hands[x])
        {
            int usenumber = data.Number + (int)data.Mark * 10;
            usenumbers[x].Add(usenumber);
        }
    }

    //デバッグ用メソッド（リストの中身がわかる）
    public void DebugList(List<int> list)
    {
        string log = "";

        foreach(var x in list)
        {
            log += ( x.ToString() + "," );
        }

    Debug.Log(log);
    }

    void SetPoints()
    {
        for(int i = 0; i < 4; i++)
        {
            Points[i].text = points[i].ToString();
        }
    }

    void Setpastpoints()
    {   pastpoints.Clear();
        foreach(int point in points)
        {
            pastpoints.Add(point);
        }
    }

    public void SetisRichi()
    {
        isRichi[currentPlayer] = true;
        RichiButtons[currentPlayer].SetActive(false);
        richiCalled = true;
    }

    void SetWinds()
    {
        for(int j = 0; j < 4; j++)
        {
            Winds[j].text = winds[j];
        }
    }

    //流局判定
    void JudgeDrawngame()
    {
        //流局する枚数を設定（正しいのは14,30にするとちょうどいい）
        if(pile.Count <= wangpai)
        {
            CurrentState = State.Over;
            isDrawned = true;
        }
    }

    List<int> SetPenalty()
    {
        var list = new List<int>(2);
        switch(tempaidplayer.Count)
        {
            case 1:
                list.Add(3000);
                list.Add(-1000);
                break;
            case 2:
                list.Add(1500);
                list.Add(-1500);
                break;
            case 3:
                list.Add(1000);
                list.Add(-3000);
                break;
            default:
                list.Add(0);
                list.Add(0);
                break;
        }
        return list;
    }

    void SetDrawnedpoints()
    {
        var tmp = new List<int>(4);
        var penalty = SetPenalty();
        foreach(int point in points)
        {
            tmp.Add(point);
        }
        points.Clear();

        for(int i = 0; i < 4; i++)
        {
            if(tempaidplayer.Contains(i))
            {
                points.Add(tmp[i] + penalty[0]);
            }
            else
            {
                points.Add(tmp[i] + penalty[1]);
            }
        }
    }

    void Setparent()
    {
        //親がノーテンのとき
        if(!tempaidplayer.Contains(parent))
        {
            //親を交代
            parent = (parent + 1) % 4;
            //ツモ番交代
            currentPlayer = parent;
            //風の交代
            winds.SetValue("東", parent);
            winds.SetValue("南", (parent + 1) % 4);
            winds.SetValue("西", (parent + 2) % 4);
            winds.SetValue("北", (parent + 3) % 4);
                        
            //局を次に進める
            windcount++;
        }
        round++;
    }

    //流局時の処理
    void DrawnGame()
    {
        isReverse = true;
        DisplayHands(currentPlayer);
        Setpastpoints();
        //各プレイヤーのテンパイ状況を確認
        for(int i = 0; i < 4; i++)
        {
            isNotem = true;
            JudgeNoTempai(i);
            if(!isNotem)
            {
                //聴牌のプレイヤーとして保存する
                tempaidplayer.Add(i);
            }
        }
        isReverse = false;
        //手牌の開示
        foreach(int i in tempaidplayer)
        {
            DisplayHands(i);
        }
        //点数の受け渡し
        SetDrawnedpoints();
        //親の交代
        Setparent();
        //テキストの表示（できてる）
        winningtext = "～流局～";
        WinningText.text = winningtext;
        Winning.SetActive(true);
    }

    public void GotoResultScene()
    {
        SceneManager.LoadScene("ResultScene");
    }

    void Tsumo()
    {
        var tmp = new List<int>();
        int count = 0;
        int y = 6000;
        if(currentPlayer == parent)
        {
            y = y*3/2;
        }
        if(isRichi[currentPlayer])
        {
            y = y + 3000;
        }
        y = y + round * 3;
        foreach(int point in points)
        {
            if(count == currentPlayer)
            {
                int x = point + y;
                tmp.Add(x);
            }
            else
            {
                int x = point - y/3;
                tmp.Add(x);
            }
            count++;
        }
        points.Clear();
        foreach(int point in tmp)
        {
            points.Add(point);
        }
        CurrentState = State.Over;
    }

    void Ron(int wonplayer)
    {
        var tmp = new List<int>();
        int count = 0;
        int y = 5000;
        if(wonplayer == parent)
        {
            y = y*3/2;
        }
        if(isRichi[wonplayer])
        {
            y = y + 3000;
        }
        y = y + round * 300;
        foreach(int point in points)
        {
            if(count == currentPlayer)
            {
                int x = point - y;
                tmp.Add(x);
                count++;
            }
            else 
            {
                if(count == wonplayer)
                {
                    int x = point + y;
                    tmp.Add(x);
                    count++;
                }
                else
                {
                    int x = point;
                    tmp.Add(x);
                    count++;
                }
            }
        }
        points.Clear();
        foreach(int point in tmp)
        {
            points.Add(point);
        }
        CurrentState = State.Over;
    }

    //////////////////ここから役判定メソッド////////////////////////////
    
    //順子を1つ抜き去る
    void RemoveRun(List<int> list)
    {
        for(int i = 0; i < list.Count-2; i++)
        {
            if(list.Find(m => m == list[i] + 1) == list[i] + 1 && list.Find(n => n == list[i] + 2) == list[i] + 2)
            {
                list.Remove(list[i] + 2);
                list.Remove(list[i] + 1);
                list.Remove(list[i]);
                break;
            }
        }
    }
    
    //3枚以上あるものを刻子候補にする
    List<int> triplecandidates;
    void SearchTriplecandidates(List<int> list)
    {   
        //triplecandidatesの中身をリセット
        triplecandidates.Clear();

        //刻子候補の検索
        for(int i = 0; i < list.Count-2; i++)
        {
            if(list[i] == list[i+1] && list[i] == list[i+2])
            {
                triplecandidates.Add(list[i]);
                i = i + 2;
            }
        }

        for(int i = 0; i < 4; i++)
        {
            triplecandidates.Add(0);
        }
        
    }

    void SearchHeadcandidates(List<int> list)
    {   
        //headcandidateの中身をリセット
        headcandidates.Clear();

        //頭候補の検索
        for(int i = 0; i < list.Count-1; i++)
        {
            if(list[i] == list[i+1])
            {
                headcandidates.Add(list[i]);
                i++;
            }
        }
    }
    
    //暗刻の組み合わせを作る
    void SetTriplecombinations()
    {
        //triplecombinationの中身を初期化
        foreach(List<int> x in triplecombinations)
        {
            x.Clear();
        }

//triplecandidatesのnullの箇所に無理やり0をつっこんで解決
        for(int i = 0; i < 8; i++)
        {
            triplecombinations[i].Add(triplecandidates[0]);
        }

        for(int j = 0; j < 4; j++)
        {
            triplecombinations[j].Add(triplecandidates[1]);
            triplecombinations[j+8].Add(triplecandidates[1]);
        }

        for(int k = 0; k < 4; k++)
        {
            triplecombinations[k*4].Add(triplecandidates[2]);
            triplecombinations[k*4 + 1].Add(triplecandidates[2]);
        }

        for(int l = 0; l < 8; l++)
        {
            triplecombinations[l*2].Add(triplecandidates[3]);
        }
    }

    List<int>[] triplecombinations;

    //アガり判定
    void JudgeReady(int player)
    {   
        yaku.Clear();
        var list = usenumbers[player];
        SearchHeadcandidates(list);
        SearchTriplecandidates(list);

        //一時的な保管場所を作成
        //var tmp1 = new List<int>(14);
        
        //全ての雀頭候補について以下を実行
        foreach(int x in headcandidates)
        {
            //tmp1にlistを移す
            tmp1.Clear();
            foreach(int y in list)
            {
                tmp1.Add(y);
            }

            //雀頭xを削除
            for(int i = 0; i < 2; i++)
            {
            tmp1.Remove(x);
            }

            //ここから刻子候補の除去開始
            SetTriplecombinations();
            
            //再び一時的な保管場所を作成
            //var tmp2 = new List<int>(14);

            //全ての暗刻候補の組み合わせについて以下を実行
            foreach(List<int> y in triplecombinations)
            {
                triplecount = 0;
                tmp2.Clear();
                foreach(int a in tmp1)
                {
                    tmp2.Add(a);
                }

                //刻子を削除
                foreach(int z in y)
                {
                    for(int i = 0; i < 3; i++)
                    {
                        tmp2.Remove(z);
                    }
                }

                //残った面子から順子を抜き去る
                runcount = 0;
                for(int i = 0; i < 4; i++)
                {
                    RemoveRun(tmp2);
                    runcount ++;
                    if(tmp2.Count == 0)
                    {
                        Setpastpoints();
                        score = 0;
                        winning = true;
                        triplecount = 4 - runcount;

                        //デバッグ用
                        if(player == currentPlayer)
                        {
                            audioSource.PlayOneShot(tsumo_sound);
                            Debug.Log("ツモ！プレイヤー"+ player + "のアガリ!!");
                            winningtext = "ツモ!プレイヤー" + player + "のアガリ!!";
                            WinningText.text = winningtext;
                        }
                        else
                        {
                            audioSource.PlayOneShot(ron_sound);
                            winningtext = "ロン!プレイヤー" + player + "のアガリ!!";
                            WinningText.text = winningtext;
                            isReverse = false;
                            //DisplayHands(player);
                            winnerhand.Clear();
                            foreach(Pai.Data data in hands[player])
                            {
                                winnerhand.Add(data);
                            }
                            winnerhand.Remove(discardedpai);
                            winnerhand.Add(discardedpai);
                        }

                        JudgeAllRuns();
                        
                        if(player == currentPlayer)
                        {
                            Tsumo();
                        }
                        else
                        {
                            Ron(player);
                        }
                        /*
                        Debug.Log("刻子は" + triplecount + "個、順子は" + runcount + "個です。");
                        Debug.Log( "翻数は" + score + "翻です。");
                        string str = "";
                        foreach(string s in yaku)
                        {
                            str += s;
                        }
                        Debug.Log("役は　" + str + "です。");
                        */
                        break;
                    }
                }

//より高い役で上がるにはここでbreakせずに全てのアガりについて考える必要がある
                if(tmp2.Count == 0)
                {   
                    //親以外のアガりのとき
                    if(player != parent)
                    {
                        //親を交代
                        parent = (parent + 1) % 4;
                        //ツモ番交代
                        currentPlayer = parent;
                        //風の交代
                        winds.SetValue("東", parent);
                        winds.SetValue("南", (parent + 1) % 4);
                        winds.SetValue("西", (parent + 2) % 4);
                        winds.SetValue("北", (parent + 3) % 4);
                        
                        //局を次に進める
                        windcount++;
                        //AllWind.text = "東" + windcount + "局";
                        round = 0;
                    }
                    //親のアガりのときは連荘
                    else
                    {
                        round++;
                        currentPlayer = parent;
                    }
                    //Round.text = round + "本場";
                    break;
                }
            }
        }
    }

    void JudgeRichi(int player)
    {
        richicallpais.Clear();
        var tmp = new List<int>();
        //手牌をtmpに再現
        foreach(int i in usenumbers[player])
        {
            tmp.Add(i);
        }

        foreach(int hand in tmp)
        {
            usenumbers[player].Remove(hand);
//Removeしたやつを保管する場所を後ほど作成
            foreach(int data in allPaiData)
            {
                usenumbers[player].Add(data);
                usenumbers[player].Sort();
                JudgeTempai(player);
                if(isTempai)
                {
                    richicallpais.Add(hand);
                }
                usenumbers[player].Remove(data);
            }
            usenumbers[player].Add(hand);
        }
    }


    //テンパイ判定
    void JudgeTempai(int player)
    {   
        isTempai = false;
        var list = usenumbers[player];
        SearchHeadcandidates(list);
        SearchTriplecandidates(list);
        foreach(int x in headcandidates)
        {
            tmp1.Clear();
            foreach(int y in list)
            {
                tmp1.Add(y);
            }
            for(int i = 0; i < 2; i++)
            {
            tmp1.Remove(x);
            }
            SetTriplecombinations();
            foreach(List<int> y in triplecombinations)
            {
                triplecount = 0;
                tmp2.Clear();
                foreach(int a in tmp1)
                {
                    tmp2.Add(a);
                }
                foreach(int z in y)
                {
                    for(int i = 0; i < 3; i++)
                    {
                        tmp2.Remove(z);
                    }
                }
                runcount = 0;
                for(int i = 0; i < 4; i++)
                {
                    RemoveRun(tmp2);
                    runcount ++;
                    if(tmp2.Count == 0)
                    {
                        isTempai = true;
                        isNotem = false;
                        if(pile.Count - wangpai != 0)
                        {
                            Debug.Log("テンパイ！リーチしますか？");
                            RichiButtons[player].SetActive(true);
                        }
                        break;
                    }
                }
                if(tmp2.Count == 0)
                {
                    break;
                }
            }
            if(tmp2.Count == 0)
            {
                break;
            }
        }
    }

    void JudgeNoTempai(int player)
    {
        var tmp = new List<int>();
        foreach(int i in usenumbers[player])
        {
            tmp.Add(i);
        }
        foreach(int data in allPaiData)
        {
            usenumbers[player].Add(data);
            usenumbers[player].Sort();
            JudgeTempai(player);
            usenumbers[player].Remove(data);
        }
    }

    bool JudgeAllRuns()
    {
        if(runcount == 4)
        {
            yaku.Add("平和　");
            score++;
            return true;
        }
        else
        {
            return false;
        }
    }
}
