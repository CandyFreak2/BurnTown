using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirtualKeyboardUI : MonoBehaviour {
    [SerializeField] private GameObject keyboardPanel;

    private void Awake() {
        foreach (Button button in keyboardPanel.GetComponentsInChildren<Button>()) {
            button.onClick.AddListener(() => GetTextFromButton(button));
        }
    }

    public void GetTextFromButton(Button button) {
        EventManager.KeyboardPress(button.GetComponentInChildren<TMP_Text>().text);
    }

    public void Clear() {
        EventManager.VMClear();
    }

    public void Delete() {
        EventManager.VMDelete();
    }
}
