using UnityEngine;

/// <summary>
/// Different terrain types that affect movement and combat.
/// </summary>
public enum TerrainType
{
    Plains,   // Normal movement
    Forest,   // Slower movement, defense bonus
    Mountain, // Very slow movement, high defense
    Water     // Impassable for most units
}

/// <summary>
/// Extension methods and data for terrain types.
/// </summary>
public static class TerrainTypeExtensions
{
    // Movement cost multiplier (higher = slower)
    public static float GetMovementCost(this TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Plains => 1.0f,
            TerrainType.Forest => 1.5f,
            TerrainType.Mountain => 2.0f,
            TerrainType.Water => 999f, // Effectively impassable
            _ => 1.0f
        };
    }

    // Defense bonus when unit is on this terrain
    public static int GetDefenseBonus(this TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Plains => 0,
            TerrainType.Forest => 2,
            TerrainType.Mountain => 3,
            TerrainType.Water => 0,
            _ => 0
        };
    }

    // Visual color for terrain
    public static Color GetTerrainColor(this TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Plains => new Color(0.8f, 0.9f, 0.6f),    // Light green
            TerrainType.Forest => new Color(0.2f, 0.6f, 0.2f),    // Dark green
            TerrainType.Mountain => new Color(0.5f, 0.5f, 0.5f),  // Gray
            TerrainType.Water => new Color(0.3f, 0.5f, 0.9f),     // Blue
            _ => Color.white
        };
    }
}
