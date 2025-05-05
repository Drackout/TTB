using System;
using UnityEngine;

public class GridMouseOver : MonoBehaviour
{
    [SerializeField]
    Color onMouseEnterColor, onMouseExitColor;
    
    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    public void OnMouseEnter() => rend.material.color = Color.Lerp(onMouseExitColor, onMouseEnterColor, 1);
    public void OnMouseExit() => rend.material.color = Color.Lerp(onMouseEnterColor, onMouseExitColor, 1);

}
