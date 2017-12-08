using UnityEngine;
using UnityEngine.UI;

public class PageView : MonoBehaviour
{
    public GameObject PDFViewCanvas;
    public GameObject choosePage;
    Animator anim;
    Vector3 localScale { get { return choosePage.transform.localScale; } }
    Vector3 position { get { return choosePage.transform.position; } }

    void Start()
    {
        anim = choosePage.GetComponent<Animator>();
        EnhanceScrollView.OnPageViewed += OnView;
    }

    void OnDestroy()
    {
        EnhanceScrollView.OnPageViewed -= OnView;
    }

    void OnView(EnhanceItem item)
    {
        PDFViewCanvas.SetActive(true);
        anim.enabled = true;
        RawImage itemImage = item.GetComponent<RawImage>();
        RawImage image = PDFViewCanvas.GetComponentInChildren<RawImage>();
        image.texture = itemImage.texture;
    }

    public void Back()
    {
        PDFViewCanvas.SetActive(false);
        choosePage.transform.localScale = new Vector3(0.9f, 0.8f);
        choosePage.transform.localPosition = Vector3.zero;
    }

    public void ZoomIn()
    {
        choosePage.transform.localScale = new Vector3(localScale.x + 0.18f, localScale.y + 0.16f, localScale.z);
    }

    public void ZoomOut()
    {
        choosePage.transform.localScale = new Vector3(localScale.x - 0.18f, localScale.y - 0.16f, localScale.z);
    }

    public void Up()
    {
        choosePage.transform.position = new Vector3(position.x, position.y + 25);
    }

    public void Down()
    {
        choosePage.transform.position = new Vector3(position.x, position.y - 25);
    }

    public void Left()
    {
        choosePage.transform.position = new Vector3(position.x - 25, position.y);
    }

    public void Right()
    {
        choosePage.transform.position = new Vector3(position.x + 25, position.y);
    }
}
