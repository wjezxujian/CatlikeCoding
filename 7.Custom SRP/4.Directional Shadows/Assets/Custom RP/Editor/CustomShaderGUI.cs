﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    enum ShadowMode
    {
        On, Clip, Dither, Off
    };

    MaterialEditor editor;
    Object[] materials;
    MaterialProperty[] properties;
    bool showPresets;

    bool Clipping { set => SetProperty("_Clipping", "_CLIPPING", value); }

    bool PremultipAlpha { set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value); }

    BlendMode SrcBlend { set => SetProperty("_SrcBlend", (float)value); }

    BlendMode DstBlend { set => SetProperty("_DstBlend", (float)value); }

    bool ZWrite { set => SetProperty("_ZWrite", value ? 1f : 0f);  }

    RenderQueue RenderQueue { set { foreach(Material m in materials) { m.renderQueue = (int)value; } } }

    bool HasProperty(string name) => FindProperty(name, properties, false) != null;

    bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");

    ShadowMode Shadows
    { 
        set
        {
            if (SetProperty("_Shadows", (float)value))
            {
                SetKeyword("_SHADOWS_CLIP", value == ShadowMode.Clip);
                SetKeyword("_SHADOWS_DITHER", value == ShadowMode.Dither);
            }

        }
    }


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;

        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
        if (showPresets)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }

    private bool PresetButton(string name)
    {
        if(GUILayout.Button(name))
        {
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    private bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, properties, false);
        if(property != null)
        {
            property.floatValue = value;
            return true;
        }

        return false;
    }

    private void SetProperty(string name, string keyword, bool value)
    { 
        if(SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }        
    }

    private void SetKeyword(string keyword, bool enabled)
    {
        foreach (Material m in materials)
        {
            if (enabled)
            {
                m.EnableKeyword(keyword);
            }
            else
            {
                m.DisableKeyword(keyword);
            }
        }
    }

    private void OpaquePreset()
    {
        if(PresetButton("Opaque"))
        {
            Clipping = false;
            Shadows = ShadowMode.On;
            PremultipAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    private void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping = true;
            Shadows = ShadowMode.Clip;
            PremultipAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    private void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping = true;
            Shadows = ShadowMode.Dither;
            PremultipAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    private void TransparentPreset()
    {
        if (HasPremultiplyAlpha && PresetButton("Transparent"))
        {
            Clipping = false;
            Shadows = ShadowMode.Dither;
            PremultipAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    private void SetShadowCasterPass()
    {
        MaterialProperty shadows = FindProperty("_Shadows", properties, false);
        if (shadows == null || shadows.hasMixedValue)
        {
            return;
        }

        bool enabled = shadows.floatValue < (float)ShadowMode.Off;
        foreach(Material m in materials)
        {
            m.SetShaderPassEnabled("ShadowCaster", enabled);
        }
    }


}
