using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<IResettable> resettables = new List<IResettable>();

    public void Awake()
    {
        instance = this;
    }

    public void RegisterResettable(IResettable obj)
    {
        if (!resettables.Contains(obj))
        {
            resettables.Add(obj);
            Debug.Log("Registered: " + obj);
        }    
    }

    public void ResetAll()
    {
        foreach (IResettable obj in resettables)
        {
            obj.ResetState();
        }
    }
}