using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypingAnimation : MonoBehaviour
{
    private Animator anim;

    public Text text;
    [TextArea(0,3)]
    public string context;
    [SerializeField] float linearTypingSpeed = 0.1f;

   // private Vector2 origSize;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        RectTransform rectT = GetComponent<RectTransform>();
       // origSize = rectT.sizeDelta;
       // rectT.sizeDelta = Vector2.zero;

        textCount = 0;
        currentTextNum = 9999;
    }

    public void InvokeTypingAnimation(float duration) {
        InvokeTypingAnimation();
        //textCount = context.Length;
        //currentTextNum = 0;
        //addTextTime = duration / textCount;
    }
    public void InvokeTypingAnimation()
    {
        textCount = context.Length;
        currentTextNum = 0;
        addTextTime = linearTypingSpeed;//duration / textCount;
    }

    public void PanelOpen(bool isOpen)
    {
        if (isOpen)
        { anim.SetTrigger("Open"); }
        else
        {
            anim.SetTrigger("Close");
        }
    }

    public void MoveRectPosition(Vector2 anchoredPos)
    {
        RectTransform rectT = GetComponent<RectTransform>();
        rectT.anchoredPosition = anchoredPos;
    }
    public void MoveRectPosition(Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rectT = GetComponent<RectTransform>();
        rectT.anchorMin = anchorMin;
        rectT.anchorMax = anchorMax;
        rectT.anchoredPosition = anchoredPos;
    }


    int textCount;
    int currentTextNum;
    float time;
    float addTextTime;

    // Update is called once per frame
    void Update()
    {
        if (context.Length < currentTextNum)
            return;

        time += Time.deltaTime;
        if (addTextTime < time)
        {
            time = 0;
            text.text = context.Substring(0, currentTextNum++);
        }

    }

    public void ClearContext()
    {
       // textCount = 0;
       // currentTextNum = 0;
        text.text = "";
    }
}
