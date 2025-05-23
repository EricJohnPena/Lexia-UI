using System;
using System.Collections;
using System.Collections.Generic;
using RadarChart;
using Unity.VisualScripting;
using UnityEngine;

public class RadarDrawer
{
    private CanvasRenderer canvasRenderer;
    private List<RadarItem> radarItems;
    private RadarStyle style;

    public RadarDrawer(CanvasRenderer canvasRenderer, List<RadarItem> radarItems, RadarStyle style)
    {
        this.canvasRenderer = canvasRenderer;
        this.radarItems = radarItems;
        this.style = style;
    }

    public void Draw(Mesh mesh)
    {
        // Do nothing if there are no radar items
        if (radarItems == null || radarItems.Count == 0)
        {
            if (canvasRenderer != null)
            {
                canvasRenderer.SetMesh(null);
            }
            return;
        }

        // Ensure we have exactly 6 items
        if (radarItems.Count != 6)
        {
            Debug.LogWarning($"Expected 6 radar items, got {radarItems.Count}. Skipping draw.");
            if (canvasRenderer != null)
            {
                canvasRenderer.SetMesh(null);
            }
            return;
        }

        // Ensure items are in the correct order
        string[] expectedOrder = new string[] {
            "accuracy",
            "speed",
            "problem_solving_skills",
            "vocabulary_range",
            "consistency",
            "retention"
        };

        for (int i = 0; i < radarItems.Count; i++)
        {
            if (radarItems[i].Name != expectedOrder[i])
            {
                Debug.LogWarning($"Radar item {i} is out of order. Expected {expectedOrder[i]}, got {radarItems[i].Name}");
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetMesh(null);
                }
                return;
            }
        }

        int count = radarItems.Count;
        float radarItemsMaxValue = GetRadarItemsMaxValue();
        float angle = 2f * Mathf.PI / count;

        float minX = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity;

        float minY = Mathf.Infinity;
        float maxY = Mathf.NegativeInfinity;

        Vector3[] vertices = new Vector3[count + 1];
        Vector2[] uvs = new Vector2[count + 1];
        int[] triangles = new int[3 * count];

        //vertices
        vertices[0] = Vector3.zero;

        float startRotRad = style.StartRot * Mathf.Deg2Rad;

        for (int i = 0; i < count; i++)
        {
            float newAngle = angle * i + startRotRad;
            float newRadius =
                style.Radius * (Mathf.Max(0, radarItems[i].Value) / radarItemsMaxValue);

            float x = newRadius * Mathf.Cos(newAngle);
            float y = newRadius * Mathf.Sin(newAngle);

            if (x > maxX)
                maxX = x;
            if (x < minX)
                minX = x;

            if (y > maxY)
                maxY = y;
            if (y < minY)
                minY = y;

            vertices[i + 1] = new Vector3(x, y);
        }

        //triangles
        for (int i = 0; i < count - 1; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        triangles[3 * count - 3] = 0;
        triangles[3 * count - 2] = count;
        triangles[3 * count - 1] = 1;

        float boundsX = Mathf.Abs(minX) + Mathf.Abs(maxX);
        float boundsY = Mathf.Abs(minY) + Mathf.Abs(maxY);

        //UVs
        if (style.IsGradient)
        {
            uvs[0] = Vector2.zero;
            for (int i = 1; i < uvs.Length; i++)
            {
                uvs[i] = Vector2.one;
            }
        }
        else if (style.ScaleBounds)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(
                    vertices[i].x / boundsX * style.TextureTiling.x - (style.TextureOffset.x),
                    vertices[i].y / boundsY * style.TextureTiling.y - (style.TextureOffset.y)
                );
            }
        }
        else
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(
                    vertices[i].x / style.Radius * (style.TextureTiling.x - .5f)
                        - .5f
                        + style.TextureOffset.x,
                    vertices[i].y / style.Radius * (style.TextureTiling.y - .5f)
                        - .5f
                        + style.TextureOffset.y
                );
            }
        }

        // Clear the existing mesh from the canvas renderer first
        if (canvasRenderer != null)
        {
            canvasRenderer.SetMesh(null);
        }

        // Update the mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        // Set the new mesh
        if (canvasRenderer != null)
        {
            canvasRenderer.SetMesh(mesh);
            canvasRenderer.SetMaterial(style.Material, style.Texture);
        }
    }

    private float GetRadarItemsMaxValue()
    {
        return 10f; // Always use 10 as the maximum value
    }
}
