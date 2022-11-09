using UnityEngine;

namespace PixelPlay.OffScreenIndicator
{
    /// 
    [System.Serializable]
    public enum ScalingType {
        None,
        Log,
        Linear,
    }

    [System.Serializable]
    public struct IndicatorInfo {

        public IndicatorInfo(Color indicatorColor) {
            enabled = true;
            color = indicatorColor;
            minDistance = 1f;
            maxDistance = 100f;
            hideTooClose = true;
            hideTooFar = false;
            baseScale = 1f;
            distanceScaling = ScalingType.None;
            farScale = 0f;
        }
        
        [Tooltip("Select if type of indicator is required for this target")]
        public bool enabled;
        [Tooltip("Change this color to change the indicators color for this target")]
        public Color color;
        
        [Tooltip("The minimum distance, for hiding and/or scaling")]
        public float minDistance;
        
        [Tooltip("The maximum distance, for hiding and/or scaling")]
        public float maxDistance;
        
        [Tooltip("Select if indicator should be hidden when closer than min range")]
        public bool hideTooClose;

        [Tooltip("Select if indicator should be hidden when further than max range")]
        public bool hideTooFar;

        [Tooltip("The scale of the indicator. When distanceScaling, the scale at the min distance.")]
        public float baseScale;

        [Tooltip("The type of distance scaling, if any")]
        public ScalingType distanceScaling;

        [Tooltip("The scale at the max distance, if using distance scaling")]
        public float farScale;
    }

    public class OffScreenIndicatorCore
    {
        /// <summary>
        /// Get the scale of the indicator depending on the given info and distance
        /// </summary>
        /// <param name="info">The display data for this type of indicator</param>
        /// <param name="distance">Target distance</param>
        /// <returns></returns>
        public static float GetScale(IndicatorInfo info, float distance)
        {
            if (distance > info.maxDistance && info.hideTooFar) return 0f;
            if (distance < info.minDistance && info.hideTooClose) return 0f;

            float ratio;

            if (info.distanceScaling == ScalingType.Log)
            {
                ratio = Mathf.InverseLerp(
                    Mathf.Log(info.minDistance),
                    Mathf.Log(info.maxDistance),
                    Mathf.Log(distance)
                );
            } else if (info.distanceScaling == ScalingType.Linear)
            {
                ratio = Mathf.InverseLerp(
                    info.minDistance,
                    info.maxDistance,
                    distance
                );
            } else {
                ratio = 0f;
            }

            return Mathf.Lerp(info.baseScale, info.farScale, ratio);
        }


        /// <summary>
        /// Gets the position of the target mapped to screen cordinates.
        /// </summary>
        /// <param name="mainCamera">Refrence to the main camera</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns></returns>
        public static Vector3 GetScreenPosition(Camera mainCamera, Vector3 targetPosition)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
            return screenPosition;
        }

        /// <summary>
        /// Gets if the target is within the view frustrum.
        /// </summary>
        /// <param name="screenPosition">Position of the target mapped to screen cordinates</param>
        /// <returns></returns>
        public static bool IsTargetVisible(Vector3 screenPosition)
        {
            bool isTargetVisible = screenPosition.z > 0 && screenPosition.x > 0 && screenPosition.x < Screen.width && screenPosition.y > 0 && screenPosition.y < Screen.height;
            return isTargetVisible;
        }

        /// <summary>
        /// Gets the screen position and angle for the arrow indicator. 
        /// </summary>
        /// <param name="screenPosition">Position of the target mapped to screen cordinates</param>
        /// <param name="angle">Angle of the arrow</param>
        /// <param name="screenCentre">The screen  centre</param>
        /// <param name="screenBounds">The screen bounds</param>
        public static void GetArrowIndicatorPositionAndAngle(ref Vector3 screenPosition, ref float angle, Vector3 screenCentre, Vector3 screenBounds)
        {
            // Our screenPosition's origin is screen's bottom-left corner.
            // But we have to get the arrow's screenPosition and rotation with respect to screenCentre.
            screenPosition -= screenCentre;

            // When the targets are behind the camera their projections on the screen (WorldToScreenPoint) are inverted,
            // so just invert them.
            if(screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            // Angle between the x-axis (bottom of screen) and a vector starting at zero(bottom-left corner of screen) and terminating at screenPosition.
            angle = Mathf.Atan2(screenPosition.y, screenPosition.x);
            // Slope of the line starting from zero and terminating at screenPosition.
            float slope = Mathf.Tan(angle);

            // Two point's line's form is (y2 - y1) = m (x2 - x1) + c, 
            // starting point (x1, y1) is screen botton-left (0, 0),
            // ending point (x2, y2) is one of the screenBounds,
            // m is the slope
            // c is y intercept which will be 0, as line is passing through origin.
            // Final equation will be y = mx.
            if(screenPosition.x > 0)
            {
                // Keep the x screen position to the maximum x bounds and
                // find the y screen position using y = mx.
                screenPosition = new Vector3(screenBounds.x, screenBounds.x * slope, 0);
            }
            else
            {
                screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * slope, 0);
            }
            // Incase the y ScreenPosition exceeds the y screenBounds 
            if(screenPosition.y > screenBounds.y)
            {
                // Keep the y screen position to the maximum y bounds and
                // find the x screen position using x = y/m.
                screenPosition = new Vector3(screenBounds.y / slope, screenBounds.y, 0);
            }
            else if(screenPosition.y < -screenBounds.y)
            {
                screenPosition = new Vector3(-screenBounds.y / slope, -screenBounds.y, 0);
            }
            // Bring the ScreenPosition back to its original reference.
            screenPosition += screenCentre;
        }

        /// <summary>
        /// Gets the screen position and angle for the arrow indicator. 
        /// </summary>
        /// <param name="screenPosition">Position of the target mapped to screen cordinates</param>
        /// <param name="angle">Angle of the arrow</param>
        /// <param name="screenCentre">The screen  centre</param>
        /// <param name="screenBounds">The screen bounds</param>
        public static void GetCenteredIndicatorPositionAndAngle(ref Vector3 screenPosition, ref float angle, Vector3 screenCentre, float distanceFromCentre)
        {
            // Our screenPosition's origin is screen's bottom-left corner.
            // But we have to get the arrow's screenPosition and rotation with respect to screenCentre.
            screenPosition -= screenCentre;

            // When the targets are behind the camera their projections on the screen (WorldToScreenPoint) are inverted,
            // so just invert them.
            if(screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            // Angle between the x-axis (bottom of screen) and a vector starting at zero(bottom-left corner of screen) and terminating at screenPosition.
            angle = Mathf.Atan2(screenPosition.y, screenPosition.x);

            screenPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distanceFromCentre;

            // Bring the ScreenPosition back to its original reference.
            screenPosition += screenCentre;
        }
    }
}
