using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleRenderer : MonoBehaviour
{
    protected void Awake()
    {
        ToggleVisibility();
    }
    public void ToggleVisibility()
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
        if (rend.enabled)
        {
            rend.enabled = false;
        }

        else
        {
            rend.enabled = true;
        }

    }
}
