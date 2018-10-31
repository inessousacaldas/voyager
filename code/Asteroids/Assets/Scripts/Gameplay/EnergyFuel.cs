using System.Collections;
using UnityEngine;

/// <summary>
/// Class to manage the energy available for the player perform actions
/// </summary>
public class EnergyFuel : MonoBehaviour {

    [SerializeField]
    private float _maxEnergy = 1000;
    [SerializeField]
    private float _standardRechargeRate = 0.1f;
    [SerializeField]
    private float _boostRechargeRate = 1f;
    [SerializeField]
    private float _idleTimeToBoost = 5f;
    [SerializeField]
    private bool _energyPowerUpMode;

    private float _lastActionTime;
    public float _currEnergy;
    public float _currRechargeRate;

    public float CurrEnergy
    {
        get
        {
            return _currEnergy;
        }

        set
        {
            _currEnergy = value;
        }
    }

    void Start () {

        _energyPowerUpMode = false;
        _currEnergy = _maxEnergy;
        _currRechargeRate = _standardRechargeRate;
    }

    private void Update()
    {
        Recharge();

        UIManager.Instance.GetPanel<IngamePanel>(Panel.PanelState.Ingame).SetEnergyPercentage(PercentageEnergy());
    }

    /// <summary>
    /// Checks if can perform an action that costs some amount of energy
    /// </summary>
    /// <param name="energy_cost">Energy cost to perform the action</param>
    /// <returns>True if enough energy to perform the action. False otherwise</returns>
    public bool CanPerformAction(float energy_cost)
    {
        return (_currEnergy > energy_cost) ? true : false;
        
    }

    /// <summary>
    /// Performs an action that costs some amount of energy.
    /// </summary>
    /// <param name="energy_cost">Energy cost to perform the action</param>
    public void PerformAction(float energy_cost)
    {
        _currEnergy -= (energy_cost * (_energyPowerUpMode ? 0f : 1f));
        _currEnergy = Mathf.Clamp(_currEnergy, 0f, _maxEnergy);
        _lastActionTime = Time.time;
    }

    /// <summary>
    /// Current percentage of energy (value between 0 and 1)
    /// </summary>
    /// <returns>Percentage of energy</returns>
    public float PercentageEnergy()
    {
        return _currEnergy / _maxEnergy;
    }

    public void EnergyPowerUp(float rechargeRate, float duration)
    {
        _energyPowerUpMode = true;
        StartCoroutine(RechargeToMax(rechargeRate));
        Invoke("RemoveEnergyPowerUp", duration);
    }

    /// <summary>
    /// Sets the current recharge rate. If the player is idle for _idleTimeToBoost, the recharge rate is boosted.
    /// </summary>
    private void CheckRechargeRate()
    {
        if(_lastActionTime + _idleTimeToBoost < Time.time)
        {
            _currRechargeRate = _boostRechargeRate;
        }
        else
        {
            _currRechargeRate = _standardRechargeRate;
        }
    }

    /// <summary>
    /// Verifies recharge rate and recharges the energy fuel.
    /// </summary>
    private void Recharge()
    {
        CheckRechargeRate();
        _currEnergy += _currRechargeRate * Time.deltaTime;
        _currEnergy = Mathf.Clamp(_currEnergy, 0f, _maxEnergy);
    }

    private IEnumerator RechargeToMax(float rechargeRate)
    {
        while(_currEnergy < _maxEnergy)
        {
            _currEnergy += rechargeRate * Time.deltaTime;
            _currEnergy = Mathf.Clamp(_currEnergy, 0f, _maxEnergy);
            yield return null;
        }
    }

    private void RemoveEnergyPowerUp()
    {
        _energyPowerUpMode = false;
    }

}
