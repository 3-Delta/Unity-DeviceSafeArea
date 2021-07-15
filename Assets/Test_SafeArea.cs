using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crystal;

public class Test_SafeArea : MonoBehaviour {
    public Text deviceSafeArea;

    public InputField inputX;
    public InputField inputY;
    public InputField inputWidth;
    public InputField inputHeight;

    public SafeArea safeArea;

    private void Awake() {
        deviceSafeArea.text = Screen.safeArea.ToString();
    }

    public void OnBtnClicked() {
        float.TryParse(inputX.text, out float x);
        float.TryParse(inputY.text, out float y);
        float.TryParse(inputWidth.text, out float w);
        float.TryParse(inputHeight.text, out float h);

        if (w != 0 && h != 0) {
            Rect rect = new Rect(x, y, w, h);
            safeArea.TryApplySafeArea(ref rect);
        }
    }
}