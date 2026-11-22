using UnityEngine;

/// <summary>Fait dériver un nuage horizontalement et le fait reboucler.</summary>
public class CloudDrift : MonoBehaviour
{
    public float speed = 0.3f, minX = -20f, maxX = 20f;

    void Update()
    {
        var p = transform.position;
        p.x += speed * Time.deltaTime;
        if (p.x > maxX) p.x = minX;
        transform.position = p;
    }
}
