using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MyUGUIEnhanceItem : EnhanceItem
{
    private Button uButton;
    private RawImage rawImage;

    protected override void OnStart()
    {
        rawImage = GetComponent<RawImage>();
        uButton = GetComponent<Button>();
        uButton.onClick.AddListener(OnClickUGUIButton);
    }

    private void OnClickUGUIButton()
    {
        OnClickEnhanceItem();
    }

    // Set the item "depth" 2d or 3d
    protected override void SetItemDepth(float depthCurveValue, int depthFactor, float itemCount)
    {
        int newDepth = (int)(depthCurveValue * itemCount);
        this.transform.SetSiblingIndex(newDepth);
    }

    public override void SetSelectState(bool isCenter)
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();
        Color colorGray = new Color();
        colorGray = Color.gray;
        colorGray.a = 0.5f;
        rawImage.color = isCenter ? Color.white : colorGray;
    }

    public override void ClearImage()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.texture = null;
    }
}
