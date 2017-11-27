using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangePage : MonoBehaviour
{
    public InputField input;
    public OpenPDF openPDF;

    void Start()
    {
        openPDF = OpenPDF.instance;
    }

    public void Change()
    {
        int index = Convert.ToInt32(input.text);
        if (openPDF.isOpen && index <= openPDF.pageCount && index > 0)
        {
            openPDF.ConvertToImage(index - 1);
            openPDF.nowPage = index;
        }
    }
}
