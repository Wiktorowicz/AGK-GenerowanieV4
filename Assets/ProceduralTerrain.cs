using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralTerrain : MonoBehaviour
{
    public int width = 200;
    public int depth = 200;

    public float heightMultiplier = 6f;
    public float hillFrequency = 0.03f;

    public float waterLevel = 1.5f;
    public float textureTiling = 25f;

    public Material sandMat;
    public Material grassMat;
    public Material rockMat;
    public Material snowMat;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[width * depth];
        Vector2[] uv = new Vector2[vertices.Length];

        int[] sandTris = new int[(width - 1) * (depth - 1) * 6];
        int[] grassTris = new int[(width - 1) * (depth - 1) * 6];
        int[] rockTris = new int[(width - 1) * (depth - 1) * 6];
        int[] snowTris = new int[(width - 1) * (depth - 1) * 6];

        int s = 0, g = 0, r = 0, sn = 0;

        float[,] heightMap = new float[width, depth];

        // 🔹 HEIGHTMAP (z zagłębieniami)
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Mathf.PerlinNoise(x * hillFrequency, z * hillFrequency);

                // 🔥 łagodny teren + trochę wyższe górki
                noise = Mathf.Pow(noise, 2.2f);

                float y = noise * heightMultiplier;

                // 🔥 DODATKOWY NOISE NA DOŁKI
                float holeNoise = Mathf.PerlinNoise(x * 0.08f, z * 0.08f);
                if (holeNoise < 0.3f)
                {
                    y -= 1.2f; // robi zagłębienia
                }

                heightMap[x, z] = y;
            }
        }

        int vertIndex = 0;

        // 🔹 MESH + BIOMY
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float y = heightMap[x, z];
                vertices[vertIndex] = new Vector3(x, y, z);

                uv[vertIndex] = new Vector2(
                    (float)x / width * textureTiling,
                    (float)z / depth * textureTiling
                );

                if (x < width - 1 && z < depth - 1)
                {
                    int a = vertIndex;
                    int b = vertIndex + width;
                    int c = vertIndex + 1;
                    int d = vertIndex + width + 1;

                    float avgHeight =
                        (heightMap[x, z] +
                         heightMap[x + 1, z] +
                         heightMap[x, z + 1] +
                         heightMap[x + 1, z + 1]) / 4f;

                    // 🔥 BIOMY

                    if (avgHeight < waterLevel)
                    {
                        // 🔹 zagłębienia / jeziora
                        sandTris[s++] = a; sandTris[s++] = b; sandTris[s++] = c;
                        sandTris[s++] = c; sandTris[s++] = b; sandTris[s++] = d;
                    }
                    else if (avgHeight < 3.5f)
                    {
                        // 🔹 dużo trawy
                        grassTris[g++] = a; grassTris[g++] = b; grassTris[g++] = c;
                        grassTris[g++] = c; grassTris[g++] = b; grassTris[g++] = d;
                    }
                    else if (avgHeight < 5.5f)
                    {
                        // 🔹 skały
                        rockTris[r++] = a; rockTris[r++] = b; rockTris[r++] = c;
                        rockTris[r++] = c; rockTris[r++] = b; rockTris[r++] = d;
                    }
                    else
                    {
                        // 🔹 śnieg na topach
                        snowTris[sn++] = a; snowTris[sn++] = b; snowTris[sn++] = c;
                        snowTris[sn++] = c; snowTris[sn++] = b; snowTris[sn++] = d;
                    }
                }

                vertIndex++;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.subMeshCount = 4;
        mesh.SetTriangles(sandTris, 0);
        mesh.SetTriangles(grassTris, 1);
        mesh.SetTriangles(rockTris, 2);
        mesh.SetTriangles(snowTris, 3);

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        GetComponent<MeshRenderer>().materials =
            new Material[] { sandMat, grassMat, rockMat, snowMat };
    }
}