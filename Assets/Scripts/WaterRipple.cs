using UnityEngine;

public class WaterRipple : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;

    public float expansionSpeed = 2f;
    public float fadeSpeed = 2f;
    public Vector3 maxScale = new Vector3(3f, 3f, 1f);

    private Color startColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        startColor = spriteRenderer.color;
    }

    void OnEnable()
    {
        transform.localScale = initialScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = startColor;
        }
    }

    void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, maxScale, expansionSpeed * Time.deltaTime);

        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = Mathf.MoveTowards(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
            spriteRenderer.color = currentColor;

            if (currentColor.a <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
