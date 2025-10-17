using UnityEngine;
using UnityEngine.EventSystems;

public class SoundButtonClick : MonoBehaviour, IPointerClickHandler
{
    public AudioClip clickSound;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(clickSound);
        }
    }
}
