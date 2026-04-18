using UnityEngine;

public class SpeedBoostTile : MonoBehaviour
{
    [SerializeField] private float zTilingMultiplier = 1f;
    [SerializeField] private float scrollSpeed = 0.1f;
    private float currentOffset;

    private Renderer rend;
    private Material runtimeMat;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) return;
        runtimeMat = rend.material;

        UpdateTiling();
    }

    private void Update()
    {
        if (runtimeMat == null) return;
        //float offset = -Time.time * scrollSpeed;
        //runtimeMat.mainTextureOffset = new Vector2(0, offset);
        currentOffset += scrollSpeed * -Time.deltaTime;
        runtimeMat.mainTextureOffset = new Vector2(0, currentOffset);
    }

    private void UpdateTiling()
    {
        Vector3 scale = transform.lossyScale;

        runtimeMat.mainTextureScale = new Vector2(
            1f,
            scale.z * zTilingMultiplier
        );
    }
}