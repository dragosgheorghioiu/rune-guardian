using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GazeInteractableButtonWidget : MonoBehaviour
{
    public void OnHover()
    {
        var borderGO = transform.Find("SelectionBorder");
        var image = borderGO.GetComponent<Image>();

        image.color = Color.green;
    }

    public void OnUnhover()
    {
        var borderGO = transform.Find("SelectionBorder");
        var image = borderGO.GetComponent<Image>();

        image.color = Color.white;
    }
}
