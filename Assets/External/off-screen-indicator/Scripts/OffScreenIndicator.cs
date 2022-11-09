using PixelPlay.OffScreenIndicator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PixelPlay.OffScreenIndicator
{

/// <summary>
/// Attach the script to the off screen indicator panel.
/// </summary>
[DefaultExecutionOrder(-1)]
public class OffScreenIndicator : MonoBehaviour
{
    [Range(0.5f, 1f)]
    [Tooltip("Distance offset of the indicators from the centre of the screen")]
    [SerializeField] private float screenBoundOffset = 0.9f;

    [Range(0, 400f)]
    [Tooltip("Distance of the centered indicators from the centre of the screen")]
    [SerializeField] private float centeredDistance = 100f;

    private Camera mainCamera;
    private Vector3 screenCentre;
    private Vector3 screenBounds;

    private List<Target> targets = new List<Target>();

    public static Action<Target, bool> TargetStateChanged;

    void Awake()
    {
        mainCamera = Camera.main;
        screenCentre = new Vector3(Screen.width, Screen.height, 0) / 2;
        screenBounds = screenCentre * screenBoundOffset;
        TargetStateChanged += HandleTargetStateChanged;
    }

    void LateUpdate()
    {
        DrawIndicators();
    }

    /// <summary>
    /// Draw the indicators on the screen and set thier position and rotation and other properties.
    /// </summary>
    void DrawIndicators()
    {
        foreach(Target target in targets)
        {
            Vector3 screenPosition = OffScreenIndicatorCore.GetScreenPosition(mainCamera, target.transform.position);
            bool isTargetVisible = OffScreenIndicatorCore.IsTargetVisible(screenPosition);
            float distanceFromCamera = target.GetDistanceFromCamera(mainCamera.transform.position);// Gets the target distance from the camera.
            Indicator indicator = null;

            if(target.BoxIndicator.enabled && isTargetVisible)
            {
                screenPosition.z = 0;
                indicator = GetIndicator(ref target.indicator, IndicatorType.BOX); // Gets the box indicator from the pool.
            }
            else if(target.ArrowIndicator.enabled && !isTargetVisible && target.UseCenteredIndicator)
            {
                float angle = float.MinValue;
                OffScreenIndicatorCore.GetCenteredIndicatorPositionAndAngle(ref screenPosition, ref angle, screenCentre, centeredDistance);
                indicator = GetIndicator(ref target.indicator, IndicatorType.CENTERED); // Gets the arrow indicator from the pool.
                indicator.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg); // Sets the rotation for the arrow indicator.
            }
            else if(target.ArrowIndicator.enabled && !isTargetVisible)
            {
                float angle = float.MinValue;
                OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref screenPosition, ref angle, screenCentre, screenBounds);
                indicator = GetIndicator(ref target.indicator, IndicatorType.ARROW); // Gets the arrow indicator from the pool.
                indicator.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg); // Sets the rotation for the arrow indicator.
            }
            else
            {
                target.indicator?.Activate(false);
                target.indicator = null;
            }
            if(indicator)
            {
                bool isBox = indicator.Type == IndicatorType.BOX;
                IndicatorInfo indicatorInfo = isBox ? target.BoxIndicator : target.ArrowIndicator;
                IndicatorInfo textInfo = target.TextIndicator;

                indicator.transform.position = screenPosition; //Sets the position of the indicator on the screen.
                indicator.SetImageColor(indicatorInfo.color);// Sets the image color of the indicator.
                indicator.SetIndicatorScale(OffScreenIndicatorCore.GetScale(indicatorInfo, distanceFromCamera));
                indicator.SetDistanceText(distanceFromCamera);
                indicator.SetTextScale(OffScreenIndicatorCore.GetScale(textInfo, distanceFromCamera));
                indicator.SetTextRotation(Quaternion.identity); // Sets the rotation of the distance text of the indicator.
            }
        }
    }

    /// <summary>
    /// 1. Add the target to targets list if <paramref name="active"/> is true.
    /// 2. If <paramref name="active"/> is false deactivate the targets indicator, 
    ///     set its reference null and remove it from the targets list.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="active"></param>
    private void HandleTargetStateChanged(Target target, bool active)
    {
        if(active)
        {
            targets.Add(target);
        }
        else
        {
            target.indicator?.Activate(false);
            target.indicator = null;
            targets.Remove(target);
        }
    }

    /// <summary>
    /// Get the indicator for the target.
    /// 1. If its not null and of the same required <paramref name="type"/> 
    ///     then return the same indicator;
    /// 2. If its not null but is of different type from <paramref name="type"/> 
    ///     then deactivate the old reference so that it returns to the pool 
    ///     and request one of another type from pool.
    /// 3. If its null then request one from the pool of <paramref name="type"/>.
    /// </summary>
    /// <param name="indicator"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private Indicator GetIndicator(ref Indicator indicator, IndicatorType type)
    {
        if(indicator != null && indicator.Type != type) {
             // Sets the indicator as inactive
            indicator.Activate(false);
            indicator = null;
        }

        if (indicator == null)
        {
            switch (type) {
                case IndicatorType.ARROW: indicator = GetComponent<ArrowObjectPool>().GetPooledObject(); break;
                case IndicatorType.BOX: indicator = GetComponent<BoxObjectPool>().GetPooledObject(); break;
                case IndicatorType.CENTERED: indicator = GetComponent<CenteredObjectPool>().GetPooledObject(); break;
            }
            indicator.Activate(true); // Sets the indicator as active.
        }
        return indicator;
    }

    private void OnDestroy()
    {
        TargetStateChanged -= HandleTargetStateChanged;
    }
}

}
