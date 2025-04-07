using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreditsController : MonoBehaviour
{
    [SerializeField] GameObject mainCanvas;
    [SerializeField] GameObject mainCanvasButton;
    [SerializeField] GameObject returnButton;
    private void OnEnable()
    {
        mainCanvas.SetActive(false);
        EventSystem.current.SetSelectedGameObject(returnButton);
        
        transform.localScale = Vector3.one;
        //transform.DOScale(Vector3.one, 0.3f);
        transform.DOPunchScale(Vector3.one * 0.4f, 0.6f, 5);
    }

    private void OnDisable()
    {
        
    }

    public void HideCanvas()
    {
        mainCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mainCanvasButton);
        transform.DOScale(Vector3.zero, 0.6f).OnComplete(() => gameObject.SetActive(false));
    }

    void HideDelay()
    {

    }
}
