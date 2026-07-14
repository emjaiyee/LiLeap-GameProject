using UnityEngine;

public class SeamlessWater : MonoBehaviour
{

    private MeshRenderer meshRenderer;
    private Material waterMaterial;

    public Vector2 idleScrollSpeed = new Vector2(0f, -0.02f);

    public Transform cameraTransform;
    public float parallaxFactor = 0.1f;

    private Vector2 currentOffset = Vector2.zero;
    private Vector3 lastCameraPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            waterMaterial = meshRenderer.material;
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        lastCameraPosition = cameraTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (waterMaterial == null) return;

        currentOffset += idleScrollSpeed * Time.deltaTime;

        UnityEngine.Vector3 cameraDelta = cameraTransform.position - lastCameraPosition;

        currentOffset.y += cameraDelta.y * parallaxFactor;

        waterMaterial.mainTextureOffset = currentOffset;

        lastCameraPosition = cameraTransform.position;
    }
}
