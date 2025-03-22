using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveIndicatorList : MonoBehaviour
{
    public static ObjectiveIndicatorList Singleton
    {
        get => mSingleton;
        set
        {
            if (value == null)
                mSingleton = null;
            else if (mSingleton == null)
                mSingleton = value;
            else if (mSingleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(ObjectiveIndicatorList)}!");
            }
        }
    }
    private static ObjectiveIndicatorList mSingleton;

    [SerializeField] private GameObject indicator;
    [SerializeField] private List<ObjectiveMarker> objectives;
    private int currentIndex;

    private bool isSpawned = false;

    private void Awake()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentIndex = 0;
    }

    public void SetIndicator(GameObject _indicator)
    {
        if (indicator == null)
        {
            indicator = _indicator;
            isSpawned = true;
        }
    }

    public void SetPlayerParent(GameObject _player) // needs to be called after indicator is set
    {
        indicator.transform.SetParent(_player.transform);
        indicator.transform.localPosition = new Vector3(0.0f, 4.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawned)
        {
            if (objectives[currentIndex].isTriggered)
            {
                currentIndex++;
            }

            if (currentIndex < objectives.Count)
            {
                indicator.transform.forward = (objectives[currentIndex].transform.position - indicator.transform.position).normalized;
            }
            else
            {
                indicator.transform.rotation = Quaternion.identity;
            }
        }
    }
}
