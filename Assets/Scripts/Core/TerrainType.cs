using UnityEngine;

/// <summary>
/// Different terrain types that affect movement.
/// </summary>
public enum TerrainType
{
    Plains,   // Normal movement
    Forest,   // Slower movement
    Mountain, // Very slow movement
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
            TerrainType.Forest => 2.0f,
            TerrainType.Mountain => 3.0f,
            TerrainType.Water => 999f, // Effectively impassable
            _ => 1.0f
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
