using System.Numerics;
using System.Runtime;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform froggyTarget;
    public float smoothSpeed = 3f;
    public float yOffset = 1f;

    //private UnityEngine.Vector3 originalPosition;
    private float shakeDuration = 0f;
    private float shakeIntensity = 0f;
    private float decayMultiplier = 1f;
    
    void LateUpdate()
    {
        if (froggyTarget == null) return;

        UnityEngine.Vector3 desiredPosition = new UnityEngine.Vector3(froggyTarget.position.x, froggyTarget.position.y + yOffset, -10f);
        UnityEngine.Vector3 smoothedPosition = UnityEngine.Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        //transform.position = smoothedPosition;

        UnityEngine.Vector3 currentShakeOffset = UnityEngine.Vector3.zero;

        if (shakeDuration > 0)
        {
            UnityEngine.Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity;

            currentShakeOffset = new UnityEngine.Vector3(randomOffset.x, randomOffset.y, 0f);

            shakeDuration -= Time.deltaTime;
            shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, decayMultiplier * Time.deltaTime);
        }
        transform.position = smoothedPosition + currentShakeOffset;
    }

    public void TriggerShake(float intensity, float duration, float decayRate)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
        decayMultiplier = decayRate;

    }
}
