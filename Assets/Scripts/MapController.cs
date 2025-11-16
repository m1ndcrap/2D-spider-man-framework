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
        spriteRenderer.enabled = IsVisible(cam, spriteRenderer);
    }

    bool IsVisible(Camera cam, SpriteRenderer sr)
    {
        // Get the world-space bounding box of the sprite
        Bounds bounds = sr.bounds;

        // Check if the bounds intersects the camera frustum
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        // If bounds do NOT intersect, it's completely off-screen
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
}
