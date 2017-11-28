using PdfiumViewer;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class OpenPDF : MonoBehaviour
{
    [Range(5,10)]
    [Tooltip("限制頁數數量，取決於你panel的寬度")]
    public int pageLimit = 7;
    public int pageCount;
    public string pdfPath;
    public string pdfName;
    int convertPages;

    public int _nowPage = 1;
    public static Action<int> OnPageChanged;
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
    public bool isOpen;

    [Tooltip("物件在Prefabs資料夾內，看你是用NGUI還UGUI")]
    public EnhanceItem itemPrefab;
    public EnhanceScrollView scrollView;

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
        ConvertToImage();
    }

    public void ConvertToImage(int initialIndex = 0)
    {
        if (isOpen)
            DisposePDF();

        using (var document = PdfDocument.Load(pdfPath))
        {
            pageCount = document.PageCount;
            convertPages = pageCount;
            if (pageCount > pageLimit)
                convertPages = pageLimit;
            InitImage(document, convertPages, initialIndex);
            scrollView.Init();
            isOpen = true;
        }
    }

    void InitImage(PdfDocument doc, int pages, int initialIndex)
    {
        var image = doc.Render(initialIndex, 300, 300, true);
        image.Save(Application.dataPath + "\\temp.png", ImageFormat.Png);
        InitItem(pages);
    }

    void InitItem(int counts)
    {
        for (int index = 0; index < counts; index++)
        {
            EnhanceItem item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, scrollView.transform);
            scrollView.listEnhanceItems.Add(item);
        }
        if (scrollView.inputType == EnhanceScrollView.InputSystemType.NGUIAndWorldInput)
        {
            UITexture itemUI = scrollView.listEnhanceItems[0].GetComponent<UITexture>();
            itemUI.mainTexture = LoadPNG(Application.dataPath + "\\temp.png");
        }
        else
        {
            RawImage itemUI = scrollView.listEnhanceItems[0].GetComponent<RawImage>();
            itemUI.texture = LoadPNG(Application.dataPath + "\\temp.png");
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
