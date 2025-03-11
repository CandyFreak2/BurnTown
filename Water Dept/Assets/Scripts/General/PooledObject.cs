using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PooledObject : MonoBehaviour
{
    public ObjectPool _Pool;
    private bool repool = true;
    private float repoolDelay = 5.0f;

    [SerializeField] private float onEnableOffset = 0.0f;
    [SerializeField] private float onDisableOffset = 0.1f;
    [SerializeField] UnityEvent onEnable;
    [SerializeField] UnityEvent onDisable;
    private void OnEnable()
    {
        BeginHide();
        Invoke("PostEnableEvents", Mathf.Clamp(onEnableOffset, 0f, repoolDelay - 0.1f));
    }

    private void BeginHide()
    {
        if (repool)
        {
            Invoke("PreHideEvents", Mathf.Clamp(repoolDelay - onDisableOffset, 0f, repoolDelay));
            Invoke("Repool", repoolDelay);
        }
    }

    private void PostEnableEvents()
    {
        onEnable?.Invoke();
    }

    private void PreHideEvents()
    {
        onDisable?.Invoke();
    }

    public void Repool()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        transform.parent = _Pool.objectPool;
        gameObject.SetActive(false);
    }

    public void SetPooling(bool _repool, float _repoolDelay)
    {
        repool = _repool;
        repoolDelay = _repoolDelay;

    }
}
