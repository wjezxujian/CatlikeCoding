using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
	struct ShadowedDirectionalLight
	{
		public int visibleLightIndex;
	}


	const string bufferName = "Shadows";

	CommandBuffer buffer = new CommandBuffer { name = bufferName };

	ScriptableRenderContext context;

	CullingResults cullingResults;

	ShadowSettings shadowSettings;

	const int maxShadowedDirectionalLightCount = 1;

	ShadowedDirectionalLight[] shadowedDriectionalLights =
		new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

	int shadowedDirectionalLightCount;

	static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");



	public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
		this.context = context;
		this.cullingResults = cullingResults;
		this.shadowSettings = shadowSettings;

		shadowedDirectionalLightCount = 0;
    }

	public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
		if( shadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
			light.shadows != LightShadows.None && light.shadowStrength > 0f &&
			cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b) )
        {
			shadowedDriectionalLights[shadowedDirectionalLightCount++] =
				new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
        }
    }

	public void Render()
    {
		if (shadowedDirectionalLightCount > 0)
        {
			RenderDirectionalShadows();
        }
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
		ExecuteBuffer();

	}

	private void ExecuteBuffer()
    {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
    }
	
}