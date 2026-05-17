using UnityEngine;

public class SmokeBombTransparency : MonoBehaviour
{
    [Range(0f, 1f)]
    public float spDensityMultiplier = 0.35f;

    void OnEnable()
    {
        ApplySPVisualReduction();
    }

    void ApplySPVisualReduction()
    {
        if (!PlayerRole.IsSmallPlayerClient)
            return;

        var particleSystems = GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            var emission = ps.emission;

            // 1. Reduce particle lifetime slightly (less overlap buildup)
            main.startLifetimeMultiplier *= spDensityMultiplier;

            // 2. Reduce particle count cap (prevents fog stacking)
            main.maxParticles = Mathf.RoundToInt(main.maxParticles * spDensityMultiplier);

            // 3. Reduce emission rate (LESS particles spawned per second)
            var rate = emission.rateOverTime;
            rate.constant *= spDensityMultiplier;
            emission.rateOverTime = rate;

            // Optional: small speed reduction to avoid clustering
            main.startSpeedMultiplier *= 0.8f;

            Debug.Log($"[SP Smoke] Reduced density on {ps.name}");
        }
    }
}
