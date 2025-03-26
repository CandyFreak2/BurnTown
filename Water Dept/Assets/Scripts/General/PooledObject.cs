using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PooledObject : MonoBehaviour
{
    public ObjectPool _Pool;
    private bool repool = true;
    private float repoolDelay = 5.0f;

    //[SerializeField] private float onEnableOffset = 0.0f;
    [SerializeField] UnityEvent onEnable;
    [SerializeField] UnityEvent onDisable;
    private void OnEnable()
    {
        //BeginHide();
        //Invoke("PostEnableEvents", Mathf.Clamp(onEnableOffset, 0f, repoolDelay - 0.1f));
    }

    public void EnableEvents()
    {
        BeginHide();
        onEnable?.Invoke();
    }

    public void HideEvents()
    {
        onDisable?.Invoke();
    }
    private void BeginHide()
    {
        if (repool)
        {
            Invoke("Repool", repoolDelay);
        }
    }

    public void Repool()
    {
        CancelInvoke();
        HideEvents();

        transform.parent = _Pool.objectPool;
        gameObject.SetActive(false);
    }

    public void SetPooling(bool _repool, float _repoolDelay)
    {
        repool = _repool;
        repoolDelay = _repoolDelay;

    }
}
