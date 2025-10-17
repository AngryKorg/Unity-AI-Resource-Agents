using UnityEngine;

public class ProgressBarTest : MonoBehaviour
{
    public GameObject progressBarPrefab;
    private ResourceProgressBar currentProgressBar;
    private float progress = 0f;

    void Start()
    {
        if (progressBarPrefab != null)
        {
            GameObject go = Instantiate(progressBarPrefab);
            currentProgressBar = go.GetComponent<ResourceProgressBar>();
            if (currentProgressBar != null)
            {
                currentProgressBar.SetTarget(transform);
                currentProgressBar.Show();
                currentProgressBar.UpdateProgress(0f);
            }
        }
    }

    void Update()
    {
        if (currentProgressBar != null)
        {
            progress += Time.deltaTime * 0.2f; // медленное заполнение
            if (progress > 1f) progress = 0f;

            currentProgressBar.UpdateProgress(progress);
        }
    }
}
