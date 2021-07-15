using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace Crystal {
    /// <summary>
    /// Safe area implementation for notched mobile devices. Usage:
    ///  (1) Add this component to the top level of any GUI panel. 
    ///  (2) If the panel uses a full screen background image, then create an immediate child and put the component on that instead, with all other elements childed below it.
    ///      This will allow the background image to stretch to the full extents of the screen behind the notch, which looks nicer.
    ///  (3) For other cases that use a mixture of full horizontal and vertical background stripes, use the Conform X & Y controls on separate elements as needed.
    /// </summary>
    // https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour {
        public RectTransform rectTransform;

        // 因为导航栏可能被隐藏，所以动态设置导航栏，也就是此时的offset的height
        // 表示上下左右各自以safearea为标准，进行调整一个finalSafeArea。 + 表示向内缩，-表示向外扩
        // x,y,z,w分别表示 左右上下.
        // 为了navigationBar的设置
        public Rect offset = new Rect(0f, 0f, 0f, 0f);

        // landleft based
        public bool ignoreLX = false;
        public bool ignoreRX = false;

        // Portrait based
        public bool ignoreUY = false;
        public bool ignoreDY = false;

        private Rect LastSafeArea = new Rect(0, 0, 0, 0);

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null) {
                TryApplySafeArea();
            }
        }

        private void OnEnable() {
            if (rectTransform != null) {
                TryApplySafeArea();
            }
        }

        private void TryApplySafeArea() {
            Rect safeArea = GetSafeArea();
            if (safeArea != LastSafeArea) {
                ApplySafeArea(safeArea, offset);
            }
        }

        public void TryApplySafeArea(ref Rect safeArea) {
            if (safeArea != LastSafeArea)
                ApplySafeArea(safeArea, offset);
        }
        
        // 某些设备获取不到SafeArea,这些特殊处理
        private static Dictionary<string, Rect> areas = new Dictionary<string, Rect>() {
            {"OPPO A3", new Rect(80f, 0f, 1440f, 720f)},
        };

        private Rect GetSafeArea() {
            string deviceModel = SystemInfo.deviceModel;
            deviceModel = "OPPO A3";
            if (!areas.TryGetValue(deviceModel, out Rect safeArea)) {
                safeArea = Screen.safeArea;
            }
            // 后序这里可能做一些safeArea的包装
            return safeArea;
        }

        // 旋转的时候，需要监听事件调用一次
        private void ApplySafeArea(Rect safeArea, Rect offset) {
            LastSafeArea = safeArea;
            if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
                if (ignoreRX) {
                    safeArea.width = Screen.width - safeArea.x;
                }

                if (ignoreLX) {
                    safeArea.width += safeArea.x;
                    safeArea.x = 0;
                }
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight) {
                if (ignoreLX) {
                    safeArea.width = Screen.width - safeArea.x;
                }

                if (ignoreRX) {
                    safeArea.width += safeArea.x;
                    safeArea.x = 0;
                }
            }
            else if (Screen.orientation == ScreenOrientation.Portrait) {
                if (ignoreUY) {
                    safeArea.height = Screen.height - safeArea.y;
                }

                if (ignoreDY) {
                    safeArea.y = 0;
                    safeArea.height += safeArea.y;
                }
            }
            else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
                if (ignoreDY) {
                    safeArea.height = Screen.height - safeArea.y;
                }

                if (ignoreUY) {
                    safeArea.y = 0;
                    safeArea.height += safeArea.y;
                }
            }

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin;
            Vector2 anchorMax;

            var offsetLeft = offset.x;
            var offsetRight = offset.y;
            var offsetUp = offset.width;
            var offsetDown = offset.height;

            // 这个个计算的依据必须是：当前recttransform的anchor为min(0, 0)  max(1, 1) pivot为(0.5, 0.5) 否则必然出错
            // 而且拼prefab必须在失配分辨率下拼
            anchorMin.x = safeArea.x - offsetLeft;
            anchorMin.y = safeArea.y + offsetDown;

            anchorMax.x = safeArea.x + safeArea.width - offsetRight;
            anchorMax.y = safeArea.y + safeArea.height - offsetUp;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
                name, safeArea.x, safeArea.y, safeArea.width, safeArea.height, Screen.width, Screen.height);
        }
    }
}