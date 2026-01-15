using UnityEngine;

public class BPItemHighlight : MonoBehaviour
{
    [SerializeField] private Renderer itemRenderer;

    private Material edgeMaterial;
    private Color originalColor;
    private static readonly Color highlightColor = Color.white;

    private void Awake()
    {
        if (itemRenderer == null)
            itemRenderer = GetComponentInChildren<Renderer>();

        if (itemRenderer == null) return;

        // Edge material is always last
        edgeMaterial = itemRenderer.materials[itemRenderer.materials.Length - 1];
        originalColor = edgeMaterial.color;
    }

    public void SetHighlight(bool highlighted)
    {
        if (edgeMaterial == null) return;
        edgeMaterial.color = highlighted ? highlightColor : originalColor;
    }
}
