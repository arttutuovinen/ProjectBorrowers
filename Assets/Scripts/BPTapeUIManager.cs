using UnityEngine;

public class BPTapeUIManager : MonoBehaviour
{
    void Start()
    {
        SetChildrenActive(false);
    }

    public void ShowTapeUI()
    {
        SetChildrenActive(true);
    }

    public void HideTapeUI()
    {
        SetChildrenActive(false);
    }

    private void SetChildrenActive(bool state)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(state);
        }
    }
}
