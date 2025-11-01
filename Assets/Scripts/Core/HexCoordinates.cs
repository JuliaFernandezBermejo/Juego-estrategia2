using UnityEngine;

/// <summary>
/// Represents hexagonal grid coordinates using axial coordinate system (q, r).
/// Can convert to/from cube coordinates for calculations.
/// </summary>
[System.Serializable]
public struct HexCoordinates
{
    public int q; // Column (diagonal axis)
    public int r; // Row (horizontal axis)

    public HexCoordinates(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    // Cube coordinates (for easier calculations)
    public int X => q;
    public int Y => -q - r;
    public int Z => r;

    // Neighbor directions in axial coordinates (6 directions for hexagons)
    private static readonly HexCoordinates[] directions = new HexCoordinates[]
    {
        new HexCoordinates(1, 0),   // East
        new HexCoordinates(1, -1),  // Northeast
        new HexCoordinates(0, -1),  // Northwest
        new HexCoordinates(-1, 0),  // West
        new HexCoordinates(-1, 1),  // Southwest
        new HexCoordinates(0, 1)    // Southeast
    };

    public static HexCoordinates GetNeighbor(HexCoordinates coord, int direction)
    {
        HexCoordinates d = directions[direction];
        return new HexCoordinates(coord.q + d.q, coord.r + d.r);
    }

    public static int Distance(HexCoordinates a, HexCoordinates b)
    {
        // Using cube coordinates for distance calculation
        return (Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) + Mathf.Abs(a.Z - b.Z)) / 2;
    }

    // Convert axial to world position (for rendering)
    // Using pointy-top hexagon orientation (points on left/right, flat sides on top/bottom)
    public Vector3 ToWorldPosition(float hexSize)
    {
        // Pointy-top hexagon tiling
        // Horizontal spacing = sqrt(3) * radius, vertical spacing = 1.5 * radius
        float xSpacing = Mathf.Sqrt(3f) * hexSize;
        float zSpacing = 1.5f * hexSize;

        // Offset odd rows by half the horizontal spacing
        float x = q * xSpacing + (r % 2) * (xSpacing / 2f);
        float z = r * zSpacing;

        return new Vector3(x, 0, z);
    }

    public override string ToString()
    {
        return $"({q}, {r})";
    }

    public override bool Equals(object obj)
    {
        if (obj is HexCoordinates other)
            return q == other.q && r == other.r;
        return false;
    }

    public override int GetHashCode()
    {
        return (q, r).GetHashCode();
    }

    public static bool operator ==(HexCoordinates a, HexCoordinates b)
    {
        return a.q == b.q && a.r == b.r;
    }

    public static bool operator !=(HexCoordinates a, HexCoordinates b)
    {
        return !(a == b);
    }
}
