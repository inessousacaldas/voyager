using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicinePack : CollectableObject {

    public override void Effect(Player player)
    {
        print("add life");
        player.ReduceLife(-1);
    }
}
