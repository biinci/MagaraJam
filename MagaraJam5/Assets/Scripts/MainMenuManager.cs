using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private float transitionSpeed;
    [SerializeField] private RectTransform buttonsBackground;

    private RectTransform currentObject;
    Vector3 firstPosition;
    IEnumerator OpenTabCoroutine(RectTransform openingObject)
    {
        if (currentObject == openingObject)
        {
            StartCoroutine(CloseTabCoroutine());
            yield break;
        }
        if (currentObject != null)
        {
            yield return StartCoroutine(CloseTabCoroutine());
        }
        currentObject = openingObject;
        firstPosition = currentObject.position;
        while (openingObject.position != new Vector3(buttonsBackground.rect.width, Screen.height / 2))
        {
            currentObject.position = Vector2.MoveTowards(
                currentObject.position,
                new(buttonsBackground.rect.width, Screen.height / 2),
                transitionSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
    IEnumerator CloseTabCoroutine()
    {
        while (currentObject.position != firstPosition)
        {
            currentObject.position = Vector2.MoveTowards(
                currentObject.position,
                firstPosition,
                transitionSpeed * Time.deltaTime
            );
            yield return null;
        }
        currentObject = null;
    }
    public void OnClick_SelectObject(RectTransform selectedObject)
    {
        StartCoroutine(OpenTabCoroutine(selectedObject));
    }
    public void OnClick_Play() => UnityEngine.SceneManagement.SceneManager.LoadScene(1);
}
