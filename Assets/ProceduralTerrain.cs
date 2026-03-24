using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ProceduralTerrainSmooth : MonoBehaviour
{
    public int width = 200;
    public int depth = 200;
    public float heightMultiplier = 15f;
    public float hillFrequency = 0.02f;

    [Header("Tekstury")]
    public Texture2D sandTexture;
    public Texture2D grassTexture;
    public Texture2D rockTexture;
    public Texture2D snowTexture;
    public float textureScale = 0.05f; // 🔥 lepsze UV

    [Header("Progi Wysokości")]
    public float waterLevel = 2.0f;
    public float grassLevel = 5.0f;
    public float rockLevel = 9.0f;

    public Material terrainMaterial;
    private Material internalMat;

    void Start()
    {
        if (terrainMaterial != null)
        {
            internalMat = new Material(terrainMaterial);
            GetComponent<MeshRenderer>().material = internalMat;
        }

        GenerateTerrain();
        ApplyTextures();
    }

    void GenerateTerrain()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        Vector3[] vertices = new Vector3[width * depth];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[(width - 1) * (depth - 1) * 6];

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = z * width + x;

                // 🔥 LEPSZY NOISE (2 warstwy)
                float n1 = Mathf.PerlinNoise(x * hillFrequency, z * hillFrequency);
                float n2 = Mathf.PerlinNoise(x * hillFrequency * 2f, z * hillFrequency * 2f) * 0.5f;

                float noise = n1 + n2;

                float y = Mathf.Pow(noise, 1.5f) * heightMultiplier;

                vertices[i] = new Vector3(x, y, z);

                // 🔥 KLUCZOWE – NORMALIZOWANE WAGI
                colors[i] = CalculateWeights(y);
            }
        }

        int tri = 0;
        for (int z = 0; z < depth - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int s = z * width + x;

                triangles[tri++] = s;
                triangles[tri++] = s + width;
                triangles[tri++] = s + 1;

                triangles[tri++] = s + 1;
                triangles[tri++] = s + width;
                triangles[tri++] = s + width + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void ApplyTextures()
    {
        if (internalMat == null) return;

        internalMat.SetTexture("_SandTex", sandTexture);
        internalMat.SetTexture("_GrassTex", grassTexture);
        internalMat.SetTexture("_RockTex", rockTexture);
        internalMat.SetTexture("_SnowTex", snowTexture);

        internalMat.SetFloat("_Tiling", textureScale);
    }

    // 🔥 NAJWAŻNIEJSZA FUNKCJA (NAPRAWIONA)
    Color CalculateWeights(float y)
    {
        // 🔥 wyraźne zakresy wysokości
        float sand = Mathf.Clamp01((waterLevel - y) * 2f);

        float grass = Mathf.Clamp01(1f - Mathf.Abs(y - grassLevel) / 2f);
        float rock = Mathf.Clamp01(1f - Mathf.Abs(y - rockLevel) / 2f);
        float snow = Mathf.Clamp01((y - rockLevel) / 2f);

        // 🔥 bonus: wycinamy słabe wpływy (ważne!)
        sand = sand < 0.01f ? 0 : sand;
        grass = grass < 0.01f ? 0 : grass;
        rock = rock < 0.01f ? 0 : rock;
        snow = snow < 0.01f ? 0 : snow;

        float sum = sand + grass + rock + snow + 0.0001f;

        return new Color(
            sand / sum,
            grass / sum,
            rock / sum,
            snow / sum
        );
    }
}