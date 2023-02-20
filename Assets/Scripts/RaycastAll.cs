using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class RaycastAll : MonoBehaviour
{
    // using UnityEngine.EventSystem を追加すること
    PointerEventData pointer;
    void Start()
    {
        // ポインタ（マウス/タッチ）イベントに関連するイベントの情報
        pointer = new PointerEventData(EventSystem.current);
    }
    void Update()
    {
        // クリックしたら
        if (Input.GetMouseButtonDown(0))
        {
            List<RaycastResult> results = new List<RaycastResult>();
            // マウスポインタの位置にレイ飛ばし、ヒットしたものを保存
            pointer.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointer, results);
            // ヒットしたUIの名前
            foreach (RaycastResult target in results)
            {
                if(target.gameObject.name == "Pai")
                {
                    Object.Destroy(target.gameObject);
                }
                Debug.Log(target.gameObject.name);
            }
        }

    }
}