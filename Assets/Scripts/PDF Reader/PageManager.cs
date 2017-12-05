using System;
using UnityEngine;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    public Text page;
    public InputField input;
    OpenPDF openPDF;

    void Start()
    {
        openPDF = OpenPDF.instance;
        OpenPDF.OnPageChanged += OnChange;
    }

    void OnDestroy()
    {
        OpenPDF.OnPageChanged -= OnChange;
    }

    void OnChange(int nowPage)
    {
        page.text = "Page." + nowPage.ToString();
    }

    public void ChangePage()
    {
        int index = Convert.ToInt32(input.text);
        if (openPDF.isOpen && index <= openPDF.pageCount && index > 0)
        {
            openPDF.nowPage = index;
            page.text = "Page." + openPDF.nowPage.ToString();
            openPDF.ConvertToImageDragOrInput();
        }
    }
}
