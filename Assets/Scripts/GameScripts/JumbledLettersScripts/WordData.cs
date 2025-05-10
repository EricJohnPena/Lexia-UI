using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordData : MonoBehaviour
{
    [SerializeField]
    private Text charText;

    [HideInInspector]
    public char charValue;

    private Button buttonObj;

    private void Awake()
    {
        buttonObj = GetComponent<Button>();
        if (buttonObj)
        {
            buttonObj.onClick.AddListener(() => CharSelected());
        }
    }

    public void SetChar(char value)
    {
        charText.text = value + "";
        charValue = value;
    }

    public void SetHintStyle(bool isHint)
    {
        if (isHint)
        {
            charText.color = Color.green;
            charText.fontStyle = FontStyle.Bold;
        }
        else
        {
            charText.color = Color.black;
            charText.fontStyle = FontStyle.Normal;
        }
    }

    private void CharSelected()
    {
        JumbledLettersManager.instance.SelectedOption(this);
    }
}
