using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBoost : CollectableObject {

    [SerializeField]
    private float _rechargeRate;
    [SerializeField]
    private float _duration;


    public override void Effect(Player player)
    {
        player.EnergyBoost(_rechargeRate, _duration);
    }

}
