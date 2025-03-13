using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour {
    [Header("Host")]


    [Header("Join")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text charLength;
    [SerializeField] private Button confirmButton;
    private int charCount;

    //[Header("Collective")]

    private void Start() {
        charCount = LobbyUI.GetCodeLength;
        codeInput.characterLimit = charCount;
    }

    private void Update() {
        if (codeInput.text.Length == charCount) {
            confirmButton.interactable = true;
        }
        confirmButton.interactable = false;
    }

    public void UpdateCharLength() {
        charLength.text = (charCount - codeInput.text.Length).ToString();
    }
}
