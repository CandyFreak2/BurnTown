using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirtualKeyboadUI : MonoBehaviour {
    [SerializeField] private GameObject keyboardPanel;

    private void Awake() {
        foreach(Button button in keyboardPanel.GetComponentsInChildren<Button>()) {
            button.onClick.AddListener(GetTextFromButton);
        }
    }

    public void GetTextFromButton() {
        string input = GetComponentInChildren<TMP_Text>().text;
        Debug.Log(input);
    }
}
