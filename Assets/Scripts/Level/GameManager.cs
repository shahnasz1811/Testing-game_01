using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private List<IResettable> resettables = new List<IResettable>();

    private void Awake()
    {
        instance = this;
    }

    public void RegisterResettable(IResettable obj)
    {
        resettables.Add(obj);
    }

    public void ResetAll()
    {
        foreach (IResettable obj in resettables)
        {
            obj.ResetState();
        }
    }
}