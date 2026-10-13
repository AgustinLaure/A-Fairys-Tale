using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatManager : MonoSingleton<CheatManager>
{
    [SerializeField] private List<Cheat> cheatList = new List<Cheat>();

    void Start()
    {

    }

    void Update()
    {
        foreach (Cheat cheat in cheatList)
        {
            if (cheat.action.action.WasPerformedThisFrame())
            {
                cheat.CheatLogic();
            }
        }
    }

    public void AddCheat(Cheat cheat)
    {
        cheatList.Add(cheat);
    }
}
