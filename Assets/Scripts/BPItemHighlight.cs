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

        foreach (Material mat in itemRenderer.materials)
        {
            if (mat.name.Contains("EdgeLine"))
            {
                edgeMaterial = mat;
                originalColor = mat.color;
                break;
            }
        }
    }

    public void SetHighlight(bool highlighted)
    {
        if (edgeMaterial == null) return;
        edgeMaterial.color = highlighted ? highlightColor : originalColor;
    }
}
