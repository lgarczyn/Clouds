﻿using UnityEngine;
using Image = UnityEngine.UI.Image;
using Text = TMPro.TextMeshProUGUI;

namespace PixelPlay.OffScreenIndicator
{

/// <summary>
/// Assign this script to the indicator prefabs.
/// </summary>
public class Indicator : MonoBehaviour
{
    [SerializeField] private IndicatorType indicatorType;
    private Image indicatorImage;
    private Sprite originalSprite;
    private Text distanceText;

    /// <summary>
    /// Gets if the game object is active in hierarchy.
    /// </summary>
    public bool Active
    {
        get
        {
            return transform.gameObject.activeInHierarchy;
        }
    }

    /// <summary>
    /// Gets the indicator type
    /// </summary>
    public IndicatorType Type
    {
        get
        {
            return indicatorType;
        }
    }

    void Awake()
    {
        indicatorImage = transform.GetComponent<Image>();
        originalSprite = indicatorImage.sprite;
        distanceText = transform.GetComponentInChildren<Text>();
    }

    /// <summary>
    /// Sets the image color for the indicator.
    /// </summary>
    /// <param name="color"></param>
    public void SetImageColor(Color color)
    {
        indicatorImage.color = color;
    }

    /// <summary>
    /// Sets a sprite override, or reset the sprite by sending null
    public void SetSpriteOverride(Sprite sprite)
    {
        indicatorImage.sprite = sprite != null ? sprite : this.originalSprite;
    }

    /// <summary>
    /// Sets the distance text for the indicator.
    /// </summary>
    /// <param name="value"></param>
    public void SetDistanceText(float value)
    {
        if (value > 10000) distanceText.text = Mathf.Floor(value / 1000) + "km";
        else if (value > 1000) distanceText.text =  Mathf.Floor(value / 100) / 10 + "km";
        else if (value > 0) distanceText.text = Mathf.Floor(value) + "m";
        else distanceText.text = "";
    }

    /// <summary>
    /// Sets the scale of the indicator
    /// </summary>
    /// <param name="value"></param>
    public void SetIndicatorScale(float value)
    {
        indicatorImage.rectTransform.sizeDelta = Vector2.one * 20f * value;
    }

    /// <summary>
    /// Sets the scale of the text
    /// </summary>
    /// <param name="value"></param>
    public void SetTextScale(float value)
    {
        distanceText.rectTransform.localScale = Vector2.one * value;
    }

    /// <summary>
    /// Sets the distance text rotation of the indicator.
    /// </summary>
    /// <param name="rotation"></param>
    public void SetTextRotation(Quaternion rotation)
    {
        distanceText.rectTransform.rotation = rotation;
    }

    /// <summary>
    /// Sets the indicator as active or inactive.
    /// </summary>
    /// <param name="value"></param>
    public void Activate(bool value)
    {
        transform.gameObject.SetActive(value);
    }
}

public enum IndicatorType
{
    BOX,
    ARROW,
    CENTERED,
}

}
