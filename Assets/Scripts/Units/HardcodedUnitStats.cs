using UnityEngine;

/// <summary>
/// Simple hardcoded unit stats (no ScriptableObjects needed).
/// </summary>
[System.Serializable]
public class HardcodedUnitStats
{
    public string unitName;
    public UnitType unitType;
    public int cost;
    public int maxHealth;
    public int attackPower;
    public int defensePower;
    public int attackRange;
    public int movementPoints;
    public TerrainType preferredTerrain;
    public TerrainType penalizedTerrain;

    public HardcodedUnitStats(string name, UnitType type, int cost, int hp, int attack, int defense, int range, int movement, TerrainType preferred, TerrainType penalized)
    {
        this.unitName = name;
        this.unitType = type;
        this.cost = cost;
        this.maxHealth = hp;
        this.attackPower = attack;
        this.defensePower = defense;
        this.attackRange = range;
        this.movementPoints = movement;
        this.preferredTerrain = preferred;
        this.penalizedTerrain = penalized;
    }

    public float GetTerrainMovementModifier(TerrainType terrain)
    {
        if (terrain == preferredTerrain)
            return 0.7f; // 30% faster
        if (terrain == penalizedTerrain)
            return 1.5f; // 50% slower
        return 1.0f;
    }

    public int GetTerrainAttackBonus(TerrainType terrain)
    {
        if (terrain == preferredTerrain)
            return 2;
        return 0;
    }

    // Predefined unit stats
    public static HardcodedUnitStats Infantry => new HardcodedUnitStats(
        "Infantry", UnitType.Infantry,
        cost: 20,
        hp: 100,
        attack: 10,
        defense: 5,
        range: 1,
        movement: 3,
        preferred: TerrainType.Forest,
        penalized: TerrainType.Water
    );

    public static HardcodedUnitStats Cavalry => new HardcodedUnitStats(
        "Cavalry", UnitType.Cavalry,
        cost: 30,
        hp: 80,
        attack: 12,
        defense: 3,
        range: 2,
        movement: 5,
        preferred: TerrainType.Plains,
        penalized: TerrainType.Mountain
    );

    public static HardcodedUnitStats Artillery => new HardcodedUnitStats(
        "Artillery", UnitType.Artillery,
        cost: 40,
        hp: 60,
        attack: 15,
        defense: 2,
        range: 4,
        movement: 2,
        preferred: TerrainType.Plains,
        penalized: TerrainType.Mountain
    );
}
