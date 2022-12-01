using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleResolutions : MonoBehaviour
{
  public List<FullScreenMode> modes = new List<FullScreenMode>(){
    FullScreenMode.Windowed,
    FullScreenMode.FullScreenWindow,
    FullScreenMode.ExclusiveFullScreen};
  public List<Resolution> resolutions;

  public FullScreenMode currentMode = FullScreenMode.Windowed;
  public Resolution? currentResolution = null;

  void Start()
  {
    resolutions = Screen.resolutions.ToList();
    currentResolution = resolutions[0];
    Screen.fullScreen = true;
    Screen.fullScreenMode = currentMode;
    Screen.SetResolution(1280, 720, currentMode);
  }

  public void OnSelectResolution(InputAction.CallbackContext context)
  {
    int index = resolutions.FindIndex((v) => v.ToString() == currentResolution.ToString());
    index = (index + 1) % resolutions.Count;

    currentResolution = resolutions[index];

    Screen.SetResolution(currentResolution?.width ?? 1080, currentResolution?.height ?? 1000, currentMode);
  }

  public void OnSelectFullScreenMode(InputAction.CallbackContext context)
  {

    int index = modes.FindIndex((v) => v == currentMode);
    index = (index + 1) % modes.Count;

    Screen.fullScreenMode = currentMode = modes[index];
  }

  public void OnToggleFullScreen(InputAction.CallbackContext context)
  {
    Screen.fullScreen = !Screen.fullScreen;
  }
}
