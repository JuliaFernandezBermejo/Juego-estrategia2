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

    [Header("Terrain Preferences")]
    public TerrainType preferredTerrain;
    public TerrainType penalizedTerrain;
    public float preferredTerrainBonus = 0.5f;  // Movement cost multiplier
    public float penalizedTerrainPenalty = 2.0f; // Movement cost multiplier

    public float GetTerrainMovementModifier(TerrainType terrain)
    {
        if (terrain == preferredTerrain)
            return preferredTerrainBonus;
        if (terrain == penalizedTerrain)
            return penalizedTerrainPenalty;
        return 1.0f;
    }

    public int GetTerrainAttackBonus(TerrainType terrain)
    {
        if (terrain == preferredTerrain)
            return 2;
        return 0;
    }
}
