using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour
{
    public Image[] handSpriteRenderers;

    public Canvas parentCanvas;
    [SerializeField] private bool alwaysFollow;


    public void Start()
    {
        parentCanvas ??= GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, Input.mousePosition,
            parentCanvas.worldCamera,
            out var pos);
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            handSpriteRenderers[0].enabled = true;
            handSpriteRenderers[1].enabled = false;
        }

        if (alwaysFollow)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition, parentCanvas.worldCamera,
                out var movePos);

            transform.position = parentCanvas.transform.TransformPoint(movePos);
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    Input.mousePosition, parentCanvas.worldCamera,
                    out var movePos);

                transform.position = parentCanvas.transform.TransformPoint(movePos);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            handSpriteRenderers[0].enabled = false;
            handSpriteRenderers[1].enabled = true;
        }
    }
}