using UnityEngine;
using UnityEngine.UI;

public class ResourceProgressBar : MonoBehaviour
{
    public Slider slider;
    public Vector3 offset = new Vector3(0, 1, 0);

    private Transform target;

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void Show()
    {
        if (slider != null)
            slider.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (slider != null)
            slider.gameObject.SetActive(false);
    }

    public void UpdateProgress(float value)
    {
        if (slider != null)
            slider.value = value;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = Quaternion.identity;
        }
    }
}
