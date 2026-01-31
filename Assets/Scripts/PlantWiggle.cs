using UnityEngine;
using Photon.Pun;

public class PlantWiggle : MonoBehaviourPun
{
    Renderer rend;
    Material mat;

    float currentStrength;
    float targetStrength;

    const float ON_VALUE = 2f;
    const float OFF_VALUE = 0f;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material; // instance per plant
    }

    void Update()
    {
        float speed = targetStrength > currentStrength ? 10f : 3f;

        currentStrength = Mathf.Lerp(currentStrength, targetStrength, Time.deltaTime * speed);
        mat.SetFloat("_moveStrenght", currentStrength);
    }

    [PunRPC]
    public void SetWiggle(bool active)
    {
        targetStrength = active ? ON_VALUE : OFF_VALUE;
    }
}
