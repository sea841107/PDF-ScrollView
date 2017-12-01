using PdfiumViewer;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class OpenPDF : MonoBehaviour
{
    [Range(5,20)]
    [Tooltip("限制頁數數量，取決於Panel、CellWidth、Prefab三者的大小")]
    public int pageLimit = 7;
    int _halfLimit;
    public int halfLimit
    {
        get { return  _halfLimit; }
        set { _halfLimit = value; }
    }
    public int pageCount;
    public string pdfPath;
    public string pdfName;
    public bool isOpen;
    public bool ExceedLimit { get { return pageCount > pageLimit; } }

    public static Action<int> OnPageChanged;
    int _nowPage = 1;
    public int nowPage
    {
        get { return _nowPage; }
        set
        {
            _nowPage = value;
            if (OnPageChanged != null)
                OnPageChanged(value);
        }
    }

    [Tooltip("物件在Prefabs資料夾內，看你是用NGUI還UGUI")]
    public EnhanceItem itemPrefab;
    EnhanceScrollView scrollView;
    List<System.Drawing.Image> imageList;


    static OpenPDF _instance;
    public static OpenPDF instance
    {
        get { return _instance; }
        set { _instance = value; }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        scrollView = EnhanceScrollView.GetInstance;
    }

    public void Open()
    {
        OpenFile file = new OpenFile();
        file.structSize = Marshal.SizeOf(file);
        file.filter = "PDF Files\0*.pdf*";
        file.file = new string(new char[256]);
        file.maxFile = file.file.Length;
        file.fileTitle = new string(new char[64]);
        file.maxFileTitle = file.fileTitle.Length;
        file.initialDir = Application.dataPath;
        file.title = "Open PDF";
        file.defExt = "pdf";
        //file_EXPLORER | file_FILEMUSTEXIST | file_PATHMUSTEXIST | file_ALLOWMULTISELECT | file_NOCHANGEDIR
        file.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (OpenFile.GetOpenFileName(file))
        {
            StartCoroutine(WaitLoad(file.file));
            Debug.Log("File path:" + file.file);
        }
    }

    IEnumerator WaitLoad(string fileName)
    {
        yield return null;
        nowPage = 1;
        pdfPath = fileName;
        pdfName = Path.GetFileNameWithoutExtension(fileName);
        ConvertToImageBegin();
    }

    void ConvertToImageBegin()
    {
        if (isOpen)
        {
            DisposePDF();
            imageList.Clear();
        }

        using (var document = PdfDocument.Load(pdfPath))
        {
            pageCount = document.PageCount;
            SaveImageList(document, pageCount);

            if (pageCount < pageLimit)
                InitImageEven(pageCount);
            else
            {
                halfLimit = Mathf.CeilToInt((float)pageLimit / 2);
                //右半邊圖片
                InitImageEven(halfLimit);
                //左半邊圖片
                InitImageUnEven(pageLimit - halfLimit, pageCount);
            }
            scrollView.Init();
            isOpen = true;
        }
    }

    public void ConvertToImageRuntime()
    {
        if (pageCount <= pageLimit)
            return;

        if (isOpen)
            DisposePDF();

        if (nowPage >= halfLimit)
        {
            for (int i = 1; i <= halfLimit; i++)
            {
                if (i == 1)
                {
                    if ((pageCount - nowPage) >= (halfLimit - i))
                    {
                        InitImageEven(halfLimit, nowPage - 1);
                        InitImageUnEven(pageLimit - halfLimit, nowPage - 1);
                        break;
                    }
                }
                else if ((pageCount - nowPage) == (halfLimit - i))
                {
                    InitImageEven(halfLimit, pageCount - (halfLimit - i + 1));
                    InitImageEven(pageLimit - halfLimit, nowPage - halfLimit);
                    break;
                }
                else if (i == halfLimit)
                {
                    if ((pageCount - nowPage) == (halfLimit - i))
                    {
                        InitImageEven(halfLimit, pageCount - 1);
                        InitImageEven(pageLimit - halfLimit, nowPage - halfLimit);
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i <= (pageLimit - halfLimit); i++)
            {
                if ((halfLimit - nowPage == i))
                {
                    InitImageEven(halfLimit, nowPage - 1);
                    InitImageEven(pageLimit - halfLimit, pageCount - i);
                    break;
                }
            }
        }
        scrollView.Init();
    }

    void SaveImageList(PdfDocument doc, int page)
    {
        imageList = new List<System.Drawing.Image>();
        for (int index = 0; index < page; index++)
        {
            var image = doc.Render(index, 300, 300, true);
            imageList.Add(image);
        }
    }

    void InitImageEven(int page, int initialIndex = 0)
    {
        for (int index = 0; index < page; index++)
        {
            int finalIndex = index + initialIndex;
            if (finalIndex >= pageCount)
                finalIndex -= pageCount;
            imageList[finalIndex].Save(Application.dataPath + "\\temp.png", ImageFormat.Png);
            InitItem();
        }
    }

    void InitImageUnEven(int page, int lastIndex)
    {
        for (int index = page; index > 0; index--)
        {
            int finalIndex = lastIndex - index;
            if (finalIndex < 0)
                finalIndex += pageCount;
            imageList[finalIndex].Save(Application.dataPath + "\\temp.png", ImageFormat.Png);
            InitItem();
        }
    }

    void InitItem()
    {
        EnhanceItem item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, scrollView.transform);
        scrollView.listEnhanceItems.Add(item);
        if (scrollView.inputType == EnhanceScrollView.InputSystemType.UGUIInput)
        {
            UnityEngine.UI.RawImage itemUI = item.GetComponent<UnityEngine.UI.RawImage>();
            itemUI.texture = LoadPNG(Application.dataPath + "\\temp.png");
        }
        else
        {
            UITexture itemUI = item.GetComponent<UITexture>();
            itemUI.mainTexture = LoadPNG(Application.dataPath + "\\temp.png");
        }
    }

    Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            File.Delete(filePath);
        }
        return tex;
    }

    void DisposePDF()
    {
        for (int i = 0; i < scrollView.listEnhanceItems.Count; i++)
        {
            Destroy(scrollView.listEnhanceItems[i].gameObject);
        }
        scrollView.listEnhanceItems.Clear();
    }
}
