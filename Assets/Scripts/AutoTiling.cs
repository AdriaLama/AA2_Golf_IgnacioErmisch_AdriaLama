using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AutoTiling : MonoBehaviour
{
    public float textureScale = 1f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        Vector3 scale = transform.localScale;
        rend.material.SetTextureScale("_BaseMap",
            new Vector2(scale.x * textureScale, scale.z * textureScale));
    }
}