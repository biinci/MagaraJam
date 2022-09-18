using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ClickyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image img;
    [SerializeField] private Sprite _default, _pressed;
    [SerializeField] private AudioClip _compressClip, _uncompressClip;
    [SerializeField] private AudioSource audioSource;
    public void OnPointerDown(PointerEventData eventData)
    {
        img.sprite = _pressed;
        audioSource.PlayOneShot(_compressClip);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        img.sprite = default;
        audioSource.PlayOneShot(_uncompressClip);
    }

}
