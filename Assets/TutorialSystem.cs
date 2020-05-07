using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSystem : MonoBehaviour
{
    public TypingAnimation typer;

    public Transform player;

    [System.Serializable]
    public struct Tutorial
    {
        public float invokeZPos;

        [TextArea(0, 4)]
        public string context;
    }
    public Tutorial[] tutorials;

    private int currentDialogueNum;


    // Update is called once per frame
    void Update()
    {
        if (currentDialogueNum >= tutorials.Length)
        {
            this.enabled = false;
            return;
        }

        if(tutorials[currentDialogueNum].invokeZPos < player.position.z)
        {
            typer.PanelOpen(true);
            typer.context = tutorials[currentDialogueNum++].context;
        }
    }

    public void InvokeTutorial(int num)
    {
        currentDialogueNum = num;
        typer.PanelOpen(true);
        typer.context = tutorials[currentDialogueNum++].context;
    }
}
