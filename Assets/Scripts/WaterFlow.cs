using UnityEngine;

public class WaterFlow : MonoBehaviour
{
    public float speed = 1.5f;

    Renderer rend;
    Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset += new Vector2(0.5f, 1f) * Time.deltaTime * speed;
        rend.material.mainTextureOffset = offset;
    }
}