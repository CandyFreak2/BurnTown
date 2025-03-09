using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool _Pool;
    [SerializeField] private bool repool = true;
    [SerializeField] private float repoolDelay = 5.0f;

    private void OnEnable()
    {
        BeginHide();
    }

    private void BeginHide()
    {
        if (repool)
        {
            Invoke("Repool", repoolDelay);
        }
    }

    public  void Repool()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        transform.parent = _Pool.transform;
        gameObject.SetActive(false);
    }

    public void SetState(bool _repool, float _repoolDelay)
    {
        repool = _repool;
        repoolDelay = _repoolDelay;
    }
}
