using UnityEngine;

/// <summary>
/// Defines the stats and properties for a unit type.
/// </summary>
[CreateAssetMenu(fileName = "UnitStats", menuName = "Game/Unit Stats")]
public class UnitStats : ScriptableObject
{
    [Header("Basic Info")]
    public UnitType unitType;
    public string unitName;
    public int cost;

    [Header("Combat Stats")]
    public int maxHealth;
    public int attackPower;
    public int defensePower;
    public int attackRange;

    [Header("Movement")]
    public int movementPoints;
}
