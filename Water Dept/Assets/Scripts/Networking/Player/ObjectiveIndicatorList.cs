using Fusion;
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

    // Update is called once per frame
    void Update()
    {
        if (isSpawned)
        {
            UpdateIndicator();
        }
    }

    private void UpdateIndicator()
    {
        if (objectives.Count > 0 && currentIndex < objectives.Count)
        {
            indicator.transform.forward = GetDirectionToObj();

            if (objectives[currentIndex].isTriggered)
            {
                currentIndex++;
            }
        }
        else
        {
            indicator.SetActive(false);
        }
    }

    private Vector3 GetDirectionToObj()
    {
        Vector3 objPos = objectives[currentIndex].transform.position;
        Vector3 indicatorPos = indicator.transform.position;

        return new Vector3(objPos.x -  indicatorPos.x, 0.0f, objPos.z - indicatorPos.z).normalized;
    }

    public Vector3 GetLastObjectivePos()
    {
        if (currentIndex < objectives.Count && currentIndex >= 1)
        {
            return objectives[currentIndex - 1].transform.position;
        }
        else
        {
            return new Vector3(-102.72f, 1.0f, -10.97f); // Sry for magic number - Jake:(. this is world spawn for lvl 1
        }
    }
}
