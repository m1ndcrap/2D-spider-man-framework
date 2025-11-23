using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera cam;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    void Update()
    {
        spriteRenderer.enabled = IsVisible(cam, spriteRenderer, 1.5f);
    }

    bool IsVisible(Camera cam, SpriteRenderer sr, float factor)
    {
        Bounds bounds = sr.bounds;

        float extra = factor - 1f;
        Vector3 expansion = bounds.size * extra;
        bounds.Expand(expansion);

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
}