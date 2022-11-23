// Copyright 2017 by Astral Byte Ltd. 
// Source: http://www.astralbyte.co.nz/code/AutoHideUILayer.cs
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
// Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEditor;

namespace AstralByte.Editor
{
    /// <summary>
    /// Automatically hides the UI layer inside the Editor so it doesn't get in the way of 3D objects in scene view.
    ///<para></para>
    /// Installation: put in any folder named "Editor". Requires Unity 5.2 or later (for Selection.selectionChanged).
    ///<para></para>
    /// When any object is selected that is on the UI layer, the layer will be shown and the camera changed to 2D orthographic and zoomed to the current selection.
    ///<para></para>
    /// When any object on another layer is selected, the UI layer will be hidden and the camera changed back to the previous state.
    /// </summary>
    [InitializeOnLoad]
    internal static class AutoHideUILayer
    {
        /************************************************************************************************************************/

        private const string
            EnabledToggleMenuPath = "Edit/Auto Hide UI",
            Previous2DModePref = EnabledToggleMenuPath + "_Previous2dMode",
            PreviousOrthographicModePref = EnabledToggleMenuPath + "_PreviousOrthographicMode",
            PreviousPivotPrefX = EnabledToggleMenuPath + "_PreviousPivot.x",
            PreviousPivotPrefY = EnabledToggleMenuPath + "_PreviousPivot.y",
            PreviousPivotPrefZ = EnabledToggleMenuPath + "_PreviousPivot.z",
            PreviousRotationPrefX = EnabledToggleMenuPath + "_PreviousRotation.x",
            PreviousRotationPrefY = EnabledToggleMenuPath + "_PreviousRotation.y",
            PreviousRotationPrefZ = EnabledToggleMenuPath + "_PreviousRotation.z",
            PreviousRotationPrefW = EnabledToggleMenuPath + "_PreviousRotation.w",
            PreviousSizePref = EnabledToggleMenuPath + "_PreviousSize";

        private const int UiLayer = 5;

        private static bool _IsEnabled;
        private static bool _IsShowingUI;
        private static bool _Previous2dMode;
        private static bool _PreviousOrthographicMode;
        private static Vector3 _PreviousPivot;
        private static Quaternion _PreviousRotation;
        private static float _PreviousSize;

        /************************************************************************************************************************/

        static AutoHideUILayer()
        {
            EditorApplication.delayCall += () =>
            {
                _IsEnabled = EditorPrefs.GetBool(EnabledToggleMenuPath, true);

                if (_IsEnabled)
                {
                    if (SceneView.lastActiveSceneView != null)
                        StoreCurrentSceneViewState();

                    _Previous2dMode = EditorPrefs.GetBool(Previous2DModePref, _Previous2dMode);
                    _PreviousOrthographicMode = EditorPrefs.GetBool(PreviousOrthographicModePref, _PreviousOrthographicMode);
                    _PreviousPivot.x = EditorPrefs.GetFloat(PreviousPivotPrefX, _PreviousPivot.x);
                    _PreviousPivot.y = EditorPrefs.GetFloat(PreviousPivotPrefY, _PreviousPivot.y);
                    _PreviousPivot.z = EditorPrefs.GetFloat(PreviousPivotPrefZ, _PreviousPivot.z);
                    _PreviousRotation.x = EditorPrefs.GetFloat(PreviousRotationPrefX, _PreviousRotation.x);
                    _PreviousRotation.y = EditorPrefs.GetFloat(PreviousRotationPrefY, _PreviousRotation.y);
                    _PreviousRotation.z = EditorPrefs.GetFloat(PreviousRotationPrefZ, _PreviousRotation.z);
                    _PreviousRotation.w = EditorPrefs.GetFloat(PreviousRotationPrefW, _PreviousRotation.w);
                    _PreviousSize = EditorPrefs.GetFloat(PreviousSizePref, _PreviousSize);

                    var activeGameObject = Selection.activeGameObject;
                    _IsShowingUI = activeGameObject != null && activeGameObject.layer == UiLayer;

                    Selection.selectionChanged += OnSelectionChanged;
                    EditorApplication.playModeStateChanged += OnSelectionChanged;
                }
            };
        }

        /************************************************************************************************************************/

        private static void StoreCurrentSceneViewState()
        {
            var sceneView = SceneView.lastActiveSceneView;

            _Previous2dMode = sceneView.in2DMode;
            _PreviousOrthographicMode = sceneView.orthographic;
            _PreviousPivot = sceneView.pivot;
            _PreviousRotation = sceneView.rotation;
            _PreviousSize = sceneView.size;
        }

        /************************************************************************************************************************/

        [MenuItem(EnabledToggleMenuPath, validate = true)]
        private static bool ValidateToggleEnabled()
        {
            Menu.SetChecked(EnabledToggleMenuPath, _IsEnabled);
            return true;
        }

        [MenuItem(EnabledToggleMenuPath, priority = 200)]
        private static void ToggleEnabled()
        {
            _IsEnabled = !_IsEnabled;
            EditorPrefs.SetBool(EnabledToggleMenuPath, _IsEnabled);

            if (_IsEnabled)
            {
                Selection.selectionChanged += OnSelectionChanged;
                EditorApplication.playModeStateChanged += OnSelectionChanged;
                OnSelectionChanged();
            }
            else
            {
                Selection.selectionChanged -= OnSelectionChanged;
                EditorApplication.playModeStateChanged -= OnSelectionChanged;
                HideUI();
            }
        }

        /************************************************************************************************************************/

        private static void OnSelectionChanged()
        {
            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject != null && activeGameObject.layer == UiLayer)
                ShowUI();
            else
                HideUI();
        }

        private static void OnSelectionChanged(PlayModeStateChange sc)
        {
            OnSelectionChanged();
        }

        /************************************************************************************************************************/

        private static void ShowUI()
        {
            if (_IsShowingUI)
                return;

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
                return;

            StoreCurrentSceneViewState();
            SetCurrentSceneViewStatePrefs();

            // Apply UI mode and show the UI layer.
            sceneView.in2DMode = true;
            sceneView.orthographic = true;
            sceneView.FrameSelected();
            Tools.visibleLayers |= 1 << UiLayer;
            _IsShowingUI = true;
        }

        /************************************************************************************************************************/

        private static void SetCurrentSceneViewStatePrefs()
        {
            EditorPrefs.SetBool(Previous2DModePref, _Previous2dMode);
            EditorPrefs.SetBool(PreviousOrthographicModePref, _PreviousOrthographicMode);
            EditorPrefs.SetFloat(PreviousPivotPrefX, _PreviousPivot.x);
            EditorPrefs.SetFloat(PreviousPivotPrefY, _PreviousPivot.y);
            EditorPrefs.SetFloat(PreviousPivotPrefZ, _PreviousPivot.z);
            EditorPrefs.SetFloat(PreviousRotationPrefX, _PreviousRotation.x);
            EditorPrefs.SetFloat(PreviousRotationPrefY, _PreviousRotation.y);
            EditorPrefs.SetFloat(PreviousRotationPrefZ, _PreviousRotation.z);
            EditorPrefs.SetFloat(PreviousRotationPrefW, _PreviousRotation.w);
            EditorPrefs.SetFloat(PreviousSizePref, _PreviousSize);
        }

        /************************************************************************************************************************/

        private static void HideUI()
        {
            if (!_IsShowingUI)
                return;

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
                return;

            // Return to the stored scene view state and hide the UI layer.
            sceneView.in2DMode = _Previous2dMode;
            sceneView.LookAt(_PreviousPivot, _PreviousRotation, _PreviousSize, _PreviousOrthographicMode);
            Tools.visibleLayers &= ~(1 << UiLayer);
            _IsShowingUI = false;
        }

        /************************************************************************************************************************/
    }
}