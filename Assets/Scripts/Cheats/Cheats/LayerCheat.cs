using Unity.VisualScripting;
using UnityEngine;

public class LayerCheat : Cheat
{
    override protected void Start()
    {
        Debug.Log("Stated Layer cheat!");
    }

    override public void CheatLogic()
    {
        Debug.Log("layer cheat logic applied " + this.active);


        base.CheatLogic();
    }
}
