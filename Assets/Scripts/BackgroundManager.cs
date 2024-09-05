using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : Singleton<BackgroundManager>
{
    [SerializeField] private Sprite[] backgroundSprites;

    private static int index
    { 
        get => PlayerPrefs.GetInt("BackgroundChanger", 0);
        set => PlayerPrefs.SetInt("BackgroundChanger", value);
    }
    
    [SerializeField] private Image bgImage;

    private void Start()
    {
        bgImage.sprite = backgroundSprites[index];
        // var aspectRatio = (float) Screen.resolutions[0].height / Screen.resolutions[0].width;
        // var rectTransformRect = bgImage.rectTransform.rect;
        //
        // Debug.Log($"{99} - {Screen.safeArea.width} - {Screen.safeArea.height}");
        //
        // Debug.Log($"{aspectRatio > 16 / 9f} - {aspectRatio} - {rectTransformRect.width} - {rectTransformRect.height} - {Screen.resolutions[0].width} - {Screen.resolutions[0].height}");
        // var width = aspectRatio > 16 / 9f ? Screen.resolutions[0].height / (16 / 9f) : Screen.resolutions[0].width;
        // var height = aspectRatio > 16 / 9f ? Screen.resolutions[0].height : Screen.resolutions[0].width / (9 / 16f);
        // bgImage.rectTransform.sizeDelta = new(width, height);
        // Debug.Log($"{aspectRatio} - {width} - {height}");
     
    }
    
    public void ChangeBackground()
    {
        index = (index + 1) % backgroundSprites.Length;
        bgImage.sprite = backgroundSprites[index];
    }
}

