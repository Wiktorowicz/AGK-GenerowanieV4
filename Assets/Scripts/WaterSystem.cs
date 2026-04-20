using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RiverGenerator : MonoBehaviour
{
    ProceduralTerrainSmooth terrain;
    LineRenderer line;

    public Material waterMaterial;
    public int riverLength = 800;
    public float riverWidth = 3f;

    void Awake()
    {
        terrain = GetComponent<ProceduralTerrainSmooth>();
        line = GetComponent<LineRenderer>();
    }

    IEnumerator Start()
    {
        yield return null;
        GenerateRiver();
    }

    void GenerateRiver()
    {
        float[] heights = terrain.GetHeights();
        int width = terrain.GetWidth();
        int depth = terrain.GetDepth();
        float hMul = terrain.GetHeightMultiplier();

        List<Vector3> points = new List<Vector3>();
        HashSet<int> visited = new HashSet<int>();

        int x = 0, z = 0;
        float maxH = float.MinValue;

        for (int dz = 0; dz < depth; dz++)
            for (int dx = 0; dx < width; dx++)
            {
                float h = heights[dz * width + dx];
                if (h > maxH)
                {
                    maxH = h;
                    x = dx;
                    z = dz;
                }
            }

        for (int i = 0; i < riverLength; i++)
        {
            int idx = z * width + x;
            float h = heights[idx];
            float y = h * hMul;

            points.Add(new Vector3(x, y + 0.3f, z));
            visited.Add(idx);

            Vector2Int next = GetLowestNeighbor(x, z, heights, width, depth, visited);

            if (next.x == x && next.y == z)
            {
                next = GetLowestInRadius(x, z, heights, width, depth, visited, 5);

                if (next.x == x && next.y == z)
                {
                    x += Random.Range(-1, 2);
                    z += Random.Range(-1, 2);

                    x = Mathf.Clamp(x, 0, width - 1);
                    z = Mathf.Clamp(z, 0, depth - 1);

                    continue;
                }
            }

            x = next.x;
            z = next.y;
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());

        line.startWidth = riverWidth;
        line.endWidth = riverWidth * 1.5f;

        if (waterMaterial != null)
            line.material = waterMaterial;

        line.textureMode = LineTextureMode.Tile;
    }

    Vector2Int GetLowestNeighbor(int x, int z, float[] heights, int width, int depth, HashSet<int> visited)
    {
        float lowest = heights[z * width + x];
        Vector2Int best = new Vector2Int(x, z);

        for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;

                int idx = nz * width + nx;
                if (visited.Contains(idx)) continue;

                float h = heights[idx];

                if (h < lowest)
                {
                    lowest = h;
                    best = new Vector2Int(nx, nz);
                }
            }

        return best;
    }

    Vector2Int GetLowestInRadius(int x, int z, float[] heights, int width, int depth, HashSet<int> visited, int radius)
    {
        float lowest = heights[z * width + x];
        Vector2Int best = new Vector2Int(x, z);

        for (int dz = -radius; dz <= radius; dz++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;

                int idx = nz * width + nx;
                if (visited.Contains(idx)) continue;

                float h = heights[idx];

                if (h < lowest)
                {
                    lowest = h;
                    best = new Vector2Int(nx, nz);
                }
            }

        return best;
    }
}