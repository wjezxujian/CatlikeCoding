﻿using UnityEngine;

public class Explosion : WarEntity
{
    [SerializeField, Range(0f, 1f)]
    float duration = 0.5f;

    [SerializeField]
    AnimationCurve opacityCurve = default;

    [SerializeField]
    AnimationCurve scaleCurve = default;

    static int colorPropertyId = Shader.PropertyToID("_Color");

    static MaterialPropertyBlock propertyBlock;

    float age;

    float scale;

    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(meshRenderer != null, "Explosion without renderer!");
    }

    public void Initialize(Vector3 position, float blastRadius, float damage = 0f)
    {
        if(damage > 0f)
        {
            TargetPoint.FillBuffer(position, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++)
            {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
        }

        transform.localPosition = position;
        //transform.localScale = Vector3.one * (2f * blastRadius);

        scale = 2f * blastRadius;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;
        if(age >= duration)
        {
            OriginFactory.Reclaim(this);
            return false;
        }

        if(propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        float t = age / duration;
        Color c = Color.clear;
        c.a = opacityCurve.Evaluate(t);
        propertyBlock.SetColor(colorPropertyId, c);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));

        return true;
    }
}
