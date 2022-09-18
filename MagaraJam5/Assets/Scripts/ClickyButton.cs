using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ClickyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private Image img;
    [SerializeField] private Sprite _default, _pressed;
    [SerializeField] private AudioClip _clip;
    [SerializeField] private AudioSource audioSource;
    public void OnPointerDown(PointerEventData eventData)
    {
        img.sprite = _pressed;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        img.sprite = _default;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        audioSource.PlayOneShot(_clip);
    }
}
