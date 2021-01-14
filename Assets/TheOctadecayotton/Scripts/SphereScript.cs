﻿using System.Collections;
using TheOctadecayotton;
using UnityEngine;

public class SphereScript : MonoBehaviour 
{
	public GameObject Sphere, Polygon;
    public Renderer SphereRenderer;
    public TheOctadecayottonScript Octadecayotton;
    public Light Light;

    internal Position pos;
    internal InteractScript Interact { private get; set; }
    
    private const int FadeSpeed = 20;
    private bool _isUpdating;

    internal void HandleUpdate()
    {
        if (Octadecayotton.Interact.isRotating || Octadecayotton.Interact.isStarting)
            UpdateColor();

        else if (Octadecayotton.Interact.isSubmitting)
            StartCoroutine(UpdateValue());
    }

    internal void UpdateColor()
    {
        SphereRenderer.material.color = new Color32(
            (byte)(Mathf.Clamp(Sphere.transform.localPosition.x, 0, 1) * 255),
            (byte)(Mathf.Clamp(Sphere.transform.localPosition.y, 0, 1) * 255),
            (byte)(Mathf.Clamp(Sphere.transform.localPosition.z, 0, 1) * 255), 255);
    }

    private IEnumerator UpdateValue()
    {
        _isUpdating = true;
        yield return new WaitForSecondsRealtime(0.02f);
        yield return new WaitForSecondsRealtime(0.02f);
        _isUpdating = false;

        Light.range = transform.localScale.x / 256;
        bool white = true;

        for (int i = 0; i < Interact.Dimension; i++)
        {
            if (pos.InitialPosition[i] != Octadecayotton.Interact.startingSphere[(Axis)i])
            {
                white = false;
                break;
            }
        }

        Light.enabled = white;
        for (int i = 0; i < Mathf.Pow(FadeSpeed, 2) && !_isUpdating && !Octadecayotton.IsSolved; i++)
        {
            yield return new WaitForSecondsRealtime(0.02f);
            SphereRenderer.material.color = SphereRenderer.material.color.Step(white ? Color.white : new Color(0.125f, 0.125f, 0.125f), FadeSpeed);
        }
    }
}
