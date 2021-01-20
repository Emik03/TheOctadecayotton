using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheOctadecayotton;
using UnityEngine;

public class SphereScript : MonoBehaviour 
{
	public GameObject Sphere, Polygon;
    public Light Light;
    public Renderer SphereRenderer;
    public Shader MainShader, SpecialShader;
    public TheOctadecayottonScript Octadecayotton;

    internal Position pos;
    internal InteractScript Interact { private get; set; }
    internal List<Vector3> vectors = new List<Vector3>();

    private bool _isUsingSpecialShader, _isUpdatingValue;

    private void Start()
    {
        _isUsingSpecialShader = Octadecayotton.Interact.Dimension > 9;
        SphereRenderer.material.shader = _isUsingSpecialShader ? SpecialShader : MainShader;
        if (!_isUsingSpecialShader)
            StartCoroutine(UpdateColor());
    }

    private IEnumerator UpdateColor()
    {
        while (!Octadecayotton.Interact.isSubmitting || Octadecayotton.Interact.isStarting)
        {
            if (!_isUpdatingValue)
                SphereRenderer.material.color = new Color(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            yield return new WaitForFixedUpdate();
        }
    }

    internal IEnumerator UpdateValue()
    {
        _isUpdatingValue = false;
        yield return new WaitForSecondsRealtime(0.05f);
        _isUpdatingValue = true;
        Light.range = 0.875f / Mathf.Pow(Octadecayotton.Interact.Dimension, 2);

        if (pos.InitialPosition.Where((n, i) => n == Octadecayotton.Interact.startingSphere[(Axis)i]).Count() == 0)
        {
            for (float i = 0; i <= 40 && _isUpdatingValue; i++)
            {
                Light.enabled = true;
                SphereRenderer.material.color = new Color(1, i / 40, i / 40);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }

        else
        {
            for (float i = 40; i > 0 && _isUpdatingValue; i--)
            {
                Light.enabled = false;
                SphereRenderer.material.color = new Color(i / 40, i / 40, i / 40);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        _isUpdatingValue = false;
    }
}
