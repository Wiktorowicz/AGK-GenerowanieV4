using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGenerator : MonoBehaviour {
    [SerializeField] private ProceduralTerrainSmooth terrainGenerator;

    [SerializeField] private int villageSize = 30;
    [SerializeField] private float buildingOffset = 6f;

    [Header("Road")]
    [SerializeField] private Material roadMaterial;
    [SerializeField] private float roadWidth = 4f;

    private List<Vector3> roadPoints = new List<Vector3>();
    private List<Vector2Int> villageCenters = new List<Vector2Int>();
    private List<Vector3> occupiedPositions = new List<Vector3>();

    IEnumerator Start() {
        yield return new WaitForSeconds(0.5f);

        int villageCount = Random.Range(1, 5);

        for (int i = 0; i < villageCount; i++) {
            GenerateVillage();
        }
    }

    void GenerateVillage() {
        occupiedPositions.Clear();

        Vector2Int startPoint = FindFlatArea();

        foreach (var center in villageCenters) {
            if (Vector2Int.Distance(center, startPoint) < 50f) return;
        }

        villageCenters.Add(startPoint);

        villageSize = Random.Range(50, 150);

        terrainGenerator.FlattenArea(startPoint.x, startPoint.y, 30);

        GenerateRoad(startPoint);
        GenerateBuildings();
        GenerateDecorations();
    }

    bool IsAreaFree(Vector3 pos, float radius) {
        foreach (var p in occupiedPositions) {
            if (Vector3.Distance(p, pos) < radius)
                return false;
        }
        return true;
    }

    Vector2Int FindFlatArea() {
        List<Vector2Int> validPoints = new List<Vector2Int>();

        for (int i = 0; i < 1000; i++) {
            int x = Random.Range(10, terrainGenerator.GetWidth() - 10);
            int z = Random.Range(10, terrainGenerator.GetDepth() - 10);

            if (IsFlat(x, z) && IsAboveWater(x, z)) {
                validPoints.Add(new Vector2Int(x, z));
            }
        }

        if (validPoints.Count > 0)
            return validPoints[Random.Range(0, validPoints.Count)];

        return new Vector2Int(50, 50);
    }

    bool IsFlat(int x, int z) {
        float baseHeight = terrainGenerator.GetHeightWorld(x, z);
        if (baseHeight > 15f) return false;

        int radius = 10;

        for (int dx = -radius; dx <= radius; dx++) {
            for (int dz = -radius; dz <= radius; dz++) {
                float h = terrainGenerator.GetHeightWorld(x + dx, z + dz);
                if (Mathf.Abs(h - baseHeight) > 2.0f) return false;
            }
        }

        return true;
    }

    bool IsAboveWater(float x, float z) {
        float h = terrainGenerator.GetHeightWorld(x, z);
        return h > terrainGenerator.GetWaterLevel() + 0.2f;
    }

    void GenerateRoad(Vector2Int start) {
        roadPoints.Clear();

        float x = start.x;
        float z = start.y;

        for (int i = 0; i < villageSize; i++) {

            if (x < 5 || z < 5 ||
                x > terrainGenerator.GetWidth() - 5 ||
                z > terrainGenerator.GetDepth() - 5)
                break;

            float y = terrainGenerator.GetHeightWorld(x, z);
            float waterLevel = terrainGenerator.GetWaterLevel();

            if (y <= waterLevel + 0.1f) break;

            Vector3 point = new Vector3(x, y + 0.05f, z);
            roadPoints.Add(point);

            x += 2.5f;
            z += Random.Range(-0.3f, 0.3f);
        }

        for (int i = 0; i < roadPoints.Count - 1; i++) {
            Vector3 a = roadPoints[i];
            Vector3 b = roadPoints[i + 1];

            CreateRoadSegment(a, b);

            if (i % 8 == 0 && Random.value > 0.5f) {
                Vector3 dir = (b - a).normalized;
                GenerateSideRoad(a, dir);
            }
        }
    }

    void GenerateSideRoad(Vector3 startPoint, Vector3 mainDir) {
        List<Vector3> sidePoints = new List<Vector3>();

        Vector3 dir = Vector3.Cross(Vector3.up, mainDir).normalized;
        if (Random.value > 0.5f) dir *= -1f;

        float x = startPoint.x;
        float z = startPoint.z;

        int length = Random.Range(5, 20);

        for (int i = 0; i < length; i++) {

            if (x < 5 || z < 5 ||
                x > terrainGenerator.GetWidth() - 5 ||
                z > terrainGenerator.GetDepth() - 5)
                break;

            float y = terrainGenerator.GetHeightWorld(x, z);
            float waterLevel = terrainGenerator.GetWaterLevel();

            if (y <= waterLevel + 0.1f) break;

            Vector3 point = new Vector3(x, y + 0.05f, z);
            sidePoints.Add(point);

            x += dir.x * 2.5f;
            z += dir.z * 2.5f;
        }

        for (int i = 0; i < sidePoints.Count - 1; i++) {
            CreateRoadSegment(sidePoints[i], sidePoints[i + 1]);
        }
    }

    void CreateRoadSegment(Vector3 a, Vector3 b) {
        Vector3 center = (a + b) * 0.5f;

        if (!IsAreaFree(center, roadWidth))
            return;

        occupiedPositions.Add(center);

        Vector3 dir = (b - a).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, dir) * (roadWidth / 2f);

        Vector3 p1 = a + side;
        Vector3 p2 = a - side;
        Vector3 p3 = b + side;
        Vector3 p4 = b - side;

        p1.y = terrainGenerator.GetHeightWorld(p1.x, p1.z) + 0.1f;
        p2.y = terrainGenerator.GetHeightWorld(p2.x, p2.z) + 0.1f;
        p3.y = terrainGenerator.GetHeightWorld(p3.x, p3.z) + 0.1f;
        p4.y = terrainGenerator.GetHeightWorld(p4.x, p4.z) + 0.1f;

        GameObject segment = new GameObject("RoadSegment");

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { p1, p2, p3, p4 };
        mesh.triangles = new int[] {
            0, 1, 2,
            2, 1, 3
        };
        mesh.RecalculateNormals();

        MeshFilter mf = segment.AddComponent<MeshFilter>();
        MeshRenderer mr = segment.AddComponent<MeshRenderer>();

        mf.mesh = mesh;

        if (roadMaterial != null)
            mr.material = roadMaterial;
        else {
            mr.material = new Material(Shader.Find("Standard"));
            mr.material.color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    void GenerateBuildings() {
        for (int i = 0; i < roadPoints.Count; i++) {
            if (i % 4 != 0) continue;

            Vector3 point = roadPoints[i];

            float offsetL = buildingOffset + Random.Range(-2f, 3f);
            float offsetR = buildingOffset + Random.Range(-2f, 3f);

            Vector3 left = point + Vector3.forward * offsetL;
            Vector3 right = point - Vector3.forward * offsetR;

            left += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            right += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

            left.y = terrainGenerator.GetHeightWorld(left.x, left.z);
            right.y = terrainGenerator.GetHeightWorld(right.x, right.z);

            if (IsAboveWater(left.x, left.z) && Random.value > 0.4f && IsAreaFree(left, 4f))
                SpawnHouse(left);

            if (IsAboveWater(right.x, right.z) && Random.value > 0.4f && IsAreaFree(right, 4f))
                SpawnHouse(right);
        }
    }

    void SpawnHouse(Vector3 pos) {
        occupiedPositions.Add(pos);

        GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);

        float height = Random.Range(3f, 6f);

        house.transform.position = pos + Vector3.up * (height / 2f);
        house.transform.localScale = new Vector3(4f, height, 4f);

        house.GetComponent<Renderer>().material.color = new Color(
            Random.Range(0.6f, 0.9f),
            Random.Range(0.6f, 0.9f),
            Random.Range(0.6f, 0.9f)
        );
    }

    void GenerateDecorations() {
        for (int i = 0; i < roadPoints.Count; i++) {
            if (i % 5 != 0) continue;

            Vector3 point = roadPoints[i];

            Vector3 left = point + Vector3.forward * 5f;
            Vector3 right = point - Vector3.forward * 5f;

            if (IsAboveWater(left.x, left.z) && IsAreaFree(left, 2f))
                SpawnLamp(left);

            if (IsAboveWater(right.x, right.z) && IsAreaFree(right, 2f))
                SpawnLamp(right);
        }
    }

    void SpawnLamp(Vector3 pos) {
        occupiedPositions.Add(pos);

        float y = terrainGenerator.GetHeightWorld(pos.x, pos.z);

        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lamp.transform.position = new Vector3(pos.x, y + 1, pos.z);
        lamp.transform.localScale = new Vector3(0.2f, 2f, 0.2f);

        lamp.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
    }
}