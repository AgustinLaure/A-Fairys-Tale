using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatManager : MonoSingleton<CheatManager>
{
    [SerializeField] private bool activeCheats = true;

    [SerializeField] private List<Cheat> cheatList = new List<Cheat>();

    void Start()
    {
        int counter = 0;
        foreach (Cheat cheat in cheatList)
        {
            if (cheat == null)
            {
                Debug.LogError("Null cheat found!");
                break;
            }

            if (cheat.active)
            {
                cheat.Initialize(this);
            }
            else
            {
                counter++;
            }
        }
        if(counter != 0)
        {
            Debug.LogWarning(counter + " non active cheats");
        }
    }

    void Update()
    {
        if (!activeCheats) return;

        foreach (Cheat cheat in cheatList)
        {
            cheat.LogicUpdate();
        }
    }

    public void AddCheat(Cheat cheat)
    {
        cheatList.Add(cheat);
    }
}
