using UnityEngine;
using UnityEngine.Rendering.Universal; // Required to access Light2D

[RequireComponent(typeof(Light2D))]
public class FireLight2D : MonoBehaviour
{
    private Light2D fireLight;

    [Header("Intensity Settings")]
    [SerializeField] private float minIntensity = 0.8f;
    [SerializeField] private float maxIntensity = 1.2f;

    [Header("Radius Settings")]
    [Tooltip("Check your light's current Outer Radius and set these slightly above and below it.")]
    [SerializeField] private float minOuterRadius = 4.8f;
    [SerializeField] private float maxOuterRadius = 5.2f;

    [Header("Animation Settings")]
    [SerializeField] private float flickerSpeed = 2.5f;

    // A random offset so that if you have multiple torches, they don't all flicker perfectly in sync
    private float randomOffset;

    private void Awake()
    {
        fireLight = GetComponent<Light2D>();

        // Pick a random starting point on the noise map
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        // Generate a smooth random value between 0.0 and 1.0 based on time
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * flickerSpeed);

        // Smoothly shift the intensity between min and max
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // Smoothly shift the radius so the light literally "breathes" in and out
        fireLight.pointLightOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, noise);
    }
}