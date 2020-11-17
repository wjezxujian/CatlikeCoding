using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
	struct ShadowedDirectionalLight
	{
		public int visibleLightIndex;
		public float slopeScaleBias;
		public float nearPlaneOffset;
	}


	const string bufferName = "Shadows";

	CommandBuffer buffer = new CommandBuffer { name = bufferName };

	ScriptableRenderContext context;

	CullingResults cullingResults;

	ShadowSettings shadowSettings;

	const int maxShadowedDirectionalLightCount = 4, maxCascades = 4;

	ShadowedDirectionalLight[] shadowedDirectionalLights =
		new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

	int shadowedDirectionalLightCount;

	bool useShadowMask; 

	static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
				dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
				cascadeCountId = Shader.PropertyToID("_CascadeCount"),
				cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
				cascadeDataId = Shader.PropertyToID("_CascadeData"),
				//shadowDistanceId = Shader.PropertyToID("_ShadowDistance"),
				shadowAtlastSizeId = Shader.PropertyToID("_ShadowAtlasSize"),
				shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

	static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];

	static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades],
					 cascadeData = new Vector4[maxCascades];

	static string[] directionalFilterKeywords =
	{
		"_DIRECTIONAL_PCF3",
		"_DIRECTIONAL_PCF5",
		"_DIRECTIONAL_PCF7"
	};

	static string[] cascadeBlendKeywords =
	{
		"_CASCADE_BLEND_SOFT",
		"_CASCADE_BLEND_HARD"
	};

	static string[] shadowMaskKeywords =
	{
		"_SHADOW_MASK_ALWAYS",
		"_SHADOW_MASK_DISTANCE"
	};


	public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
		this.context = context;
		this.cullingResults = cullingResults;
		this.shadowSettings = shadowSettings;

		shadowedDirectionalLightCount = 0;
		useShadowMask = false;
    }

	public Vector4 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
		if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
			light.shadows != LightShadows.None && light.shadowStrength > 0f //&&
			//cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b) 
		)
        {
			float maskChannel = -1;
			LightBakingOutput lightBaking = light.bakingOutput;
			if (lightBaking.lightmapBakeType == LightmapBakeType.Mixed &&
				lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask)
            {
				useShadowMask = true;
				maskChannel = lightBaking.occlusionMaskChannel;
            }

			if(!cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
				return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
            }

			shadowedDirectionalLights[shadowedDirectionalLightCount] =
				new ShadowedDirectionalLight { 
					visibleLightIndex = visibleLightIndex, 
					slopeScaleBias = light.shadowBias,
					nearPlaneOffset = light.shadowNearPlane
				};

			return new Vector4(light.shadowStrength, shadowSettings.directional.cascadeCount * shadowedDirectionalLightCount++, light.shadowNormalBias, maskChannel);
        }

		return new Vector4(0f, 0f, 0f, -1f);
    }

	public void Render()
    {
		if (shadowedDirectionalLightCount > 0)
        {
			RenderDirectionalShadows();
        }

		buffer.BeginSample(bufferName);
		SetKeywords(shadowMaskKeywords, useShadowMask ? QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask ?  0  : 1 : -1);
		buffer.EndSample(bufferName);
		ExecuteBuffer();
    }

	public void Cleanup()
    {
		if(shadowedDirectionalLightCount > 0)
        {
			buffer.ReleaseTemporaryRT(dirShadowAtlasId);
			ExecuteBuffer();
        }
    }

	private void RenderDirectionalShadows()
    {
		int atlasSize = (int)shadowSettings.directional.atlasSize;
		buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, 
			RenderTextureFormat.Shadowmap);

		buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, 
			RenderBufferStoreAction.Store);
		buffer.ClearRenderTarget(true, false, Color.clear);

		buffer.BeginSample(bufferName);
		ExecuteBuffer();

		int tiles = shadowedDirectionalLightCount * shadowSettings.directional.cascadeCount;
		int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
		int tileSize = atlasSize / split;

		for(int i = 0; i < shadowedDirectionalLightCount; ++i)
        {
			RenderDirectionalShadows(i, split, tileSize);
        }

		buffer.SetGlobalInt(cascadeCountId, shadowSettings.directional.cascadeCount);
		buffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
		buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
		buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
		//buffer.SetGlobalFloat(shadowDistanceId, shadowSettings.maxDistance);
		float f = 1f - shadowSettings.directional.cascadeFade;
		buffer.SetGlobalVector(shadowDistanceFadeId, new Vector4(1f / shadowSettings.maxDistance, 1f / shadowSettings.distanceFade, 1f / (1f - f * f)));
		SetKeywords(directionalFilterKeywords, (int)shadowSettings.directional.filter - 1);
		SetKeywords(cascadeBlendKeywords, (int)shadowSettings.directional.cascadeBlend - 1);
		buffer.SetGlobalVector(shadowAtlastSizeId, new Vector4(atlasSize, 1f / atlasSize));
		buffer.EndSample(bufferName);
		ExecuteBuffer();
	}

	private void RenderDirectionalShadows(int index, int split, int tileSize)
    {
		ShadowedDirectionalLight light = shadowedDirectionalLights[index];
		ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);

		int cascadeCount = this.shadowSettings.directional.cascadeCount;
		int tileOffset = index * cascadeCount;
		Vector3 ratios = this.shadowSettings.directional.CascadeRatios;

		float cullingFactor = Mathf.Max(0f, 0.8f - this.shadowSettings.directional.cascadeFade);

		for (int i = 0; i < cascadeCount; ++i)
        {
			cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
			light.visibleLightIndex, i, cascadeCount, ratios, tileSize, light.nearPlaneOffset, out Matrix4x4 viewMatrix,
			out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);
			splitData.shadowCascadeBlendCullingFactor = cullingFactor;
			shadowSettings.splitData = splitData;
			int tileIndex = tileOffset + i;
			if (index == 0)
            {
				//Vector4 cullingSphere = splitData.cullingSphere;
				//cullingSphere.w *= cullingSphere.w;
				//cascadeCullingSpheres[i] = cullingSphere;

				SetCascadeData(i, splitData.cullingSphere, tileSize);
			}
			//SetTileViewport(index, split, tileSize);
			//dirShadowMatrices[index] = projectionMatrix * viewMatrix;
			dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(tileIndex, split, tileSize), split);
			buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
			buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
			ExecuteBuffer();
			context.DrawShadows(ref shadowSettings);
			buffer.SetGlobalDepthBias(0f, 0f);
		}
    }

	private void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
    {
		
		float texelSize = 2f * cullingSphere.w / tileSize;
		cullingSphere.w *= cullingSphere.w;
		cascadeCullingSpheres[index] = cullingSphere;
		//cascadeData[index].x = 1f / cullingSphere.w;
		cascadeData[index] = new Vector4(1f / cullingSphere.w, texelSize * 1.4142136f);

	}

	private Vector2 SetTileViewport(int index, int split, float tileSize)
    {
		Vector2 offset = new Vector2(index % split, index / split);

		buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));

		return offset;
    }

	private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
		if (SystemInfo.usesReversedZBuffer)
		{
			m.m20 = -m.m20;
			m.m21 = -m.m21;
			m.m22 = -m.m22;
			m.m23 = -m.m23;
		}

		float scale = 1f / split;
		m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
		m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
		m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
		m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
		m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
		m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
		m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
		m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
		m.m20 = 0.5f * (m.m20 + m.m30);
		m.m21 = 0.5f * (m.m21 + m.m31);
		m.m22 = 0.5f * (m.m22 + m.m32);
		m.m23 = 0.5f * (m.m23 + m.m33);

		return m;
    }

	private void SetKeywords(string[] keywords, int enableIndex)
    {
		//int enableIndex = (int)shadowSettings.directional.filter - 1;
		for (int i = 0; i < keywords.Length; ++i)
        {
			if (i == enableIndex)
            {
				//buffer.EnableShaderKeyword(directionalFilterKeywords[i]);
				buffer.EnableShaderKeyword(keywords[i]);
            }
            else
            {
				//buffer.DisableShaderKeyword(directionalFilterKeywords[i]);
				buffer.DisableShaderKeyword(keywords[i]);
			}
        }
    }

	private void ExecuteBuffer()
    {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	
}