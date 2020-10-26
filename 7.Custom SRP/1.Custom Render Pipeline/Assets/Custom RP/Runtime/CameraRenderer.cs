﻿using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    ScriptableRenderContext context;

    Camera camera;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        DrawVisibleGeometry();
        Submit();
    }

    private void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera);
    }

    private void Submit()
    {
        context.Submit();
    }
}
