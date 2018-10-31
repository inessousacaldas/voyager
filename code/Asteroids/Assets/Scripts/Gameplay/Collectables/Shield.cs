using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : CollectableObject {

    [SerializeField]
    private float _shieldDuration = 5f;

    public override void Effect(Player player)
    {
        player.ActivateShield(_shieldDuration);
    }
}
