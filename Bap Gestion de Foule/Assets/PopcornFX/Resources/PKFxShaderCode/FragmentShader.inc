
#if	SHADER_API_D3D11 || SHADER_API_D3D9 || SHADER_API_XBOXONE || SHADER_API_METAL || SHADER_API_PSSL || SHADER_API_SWITCH
#	define	CLIP_Y_REVERSED		1
#else
#	define	CLIP_Y_REVERSED		0
#endif

#if	SHADER_API_D3D11 || SHADER_API_D3D9 || SHADER_API_XBOXONE || SHADER_API_METAL || SHADER_API_PSSL || SHADER_API_SWITCH
#	define	CLIP_Z_BETWEEN_MINUS_ONE_AND_ONE		0
#else
#	define	CLIP_Z_BETWEEN_MINUS_ONE_AND_ONE		1
#endif

//------------------------------------------
// Diffuse texture sampling
//------------------------------------------
sampler2D	_MainTex;

float4	SampleSpriteTexture(float2 uv)
{
	uv.y = 1 - uv.y;
	float4 color = tex2D(_MainTex, uv);
	return color;
}
//------------------------------------------

#if	PK_HAS_ALPHA_REMAP
	//------------------------------------------
	// Alpha remap sampling
	//------------------------------------------
	sampler2D	_AlphaMap;

	float4	SampleAlphaRemapTexture(float2 uv)
	{
		uv.y = 1 - uv.y;
		float4 color = tex2D(_AlphaMap, uv);
		return color;
	}
	//------------------------------------------
#endif

#if	(PK_HAS_DISTORTION || PK_HAS_SOFT)
	//------------------------------------------
	// Depth texture sampling
	//------------------------------------------
	float		_InvSoftnessDistance;

#	if		USE_HDRP
		TEXTURE2D(_CameraDepthTexture);
#		define	LINEARIZE_DEPTH(x)		LinearEyeDepth(x, _ZBufferParams)
#	else
		//sampler2D	_CameraDepthTexture;
		UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture); //Insert
#		define	LINEARIZE_DEPTH(x)		LinearEyeDepth(x)
#	endif

	float		SampleDepthTexture(float2 uv)
	{
		float2		depthTexcoord;

#		if	UNITY_SINGLE_PASS_STEREO
		depthTexcoord = TransformStereoScreenSpaceTex(uv, 1.0);
#		else
		depthTexcoord = uv;
#		endif

#		if	CLIP_Y_REVERSED
		depthTexcoord.y = 1 - depthTexcoord.y;
#		endif

#	if		USE_HDRP
		float depth = LOAD_TEXTURE2D(_CameraDepthTexture, depthTexcoord * _ScreenParams.xy).r;
#	else
		//float depth = tex2D(_CameraDepthTexture, depthTexcoord).r;
		float depth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, depthTexcoord).r; //Insert
#	endif
		return depth;
	}
	//------------------------------------------
#endif

//------------------------------------------

#include "UniformFlags.inc"

float4 frag(SVertexOutput i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	// Non OpenGlES implementation:
	float4	diffuse = float4(1.0, 1.0, 1.0, 1.0);

#	if	PK_HAS_LIGHTING
	// Do something with i.Normal...
#	endif

#	if	PK_HAS_RIBBON_COMPLEX
		float2	uv;

		if (i.UV0.x + i.UV0.y < 1.0f)
			uv = i.UV0.xy / i.UVFactors.xy;
		else
			uv = 1.0f - ((1.0f - i.UV0.xy) / i.UVFactors.zw);
	
			if (IsRotateTexture())
				uv = uv.yx;
	
		uv = uv * i.UVScaleAndOffset.xy + i.UVScaleAndOffset.zw;

#	elif	!PK_HAS_ANIM_BLEND
		float2	uv = i.UV0;
#	endif

#	if		PK_HAS_ANIM_BLEND
		float4	diffuseA = SampleSpriteTexture(i.UV0);
		float4	diffuseB = SampleSpriteTexture(i.UV1);
		diffuse = lerp(diffuseA, diffuseB, i.FrameLerp);
#	else
		diffuse = SampleSpriteTexture(uv);
#	endif

#	if	PK_HAS_ALPHA_REMAP
		float2	alphaCoord = float2(diffuse.a, i.AlphaCursor);
		float	newAlpha = SampleAlphaRemapTexture(alphaCoord).x;
		diffuse = float4(diffuse.xyz, newAlpha);
#	endif

#	if	(PK_HAS_DISTORTION || PK_HAS_SOFT)
		float3	transformedPosition = i.ProjPos.xyz / i.ProjPos.w;

#		if	CLIP_Z_BETWEEN_MINUS_ONE_AND_ONE
			transformedPosition.z = transformedPosition.z * 0.5f + 0.5f;
#		endif

		float2	screenUV = transformedPosition.xy * 0.5f + 0.5f;
		float	sceneDepth = LINEARIZE_DEPTH(SampleDepthTexture(screenUV));
		float	fragDepth = LINEARIZE_DEPTH(transformedPosition.z);
		float	depthfade = saturate(_InvSoftnessDistance * (sceneDepth - fragDepth));

#	else
		float	depthfade = 1.f;
#	endif

#	if	PK_HAS_DISTORTION
#		if	PK_HAS_COLOR
			float4	color = i.Color;
#		else
			float4	color = float4(1, 1, 1, 1);
#		endif

		float	distoInvSoftnessDistance = 0.75; // Was hard coded in the original shader
		float	distoDepthFade = saturate(distoInvSoftnessDistance * (sceneDepth - fragDepth));
//		float	distoDepthFade = depthfade;
		float3	baseDisto = color.xyz * (diffuse.xyz * float3(2, 2, 1.0) - float3(1, 1, 0.0));
		float3	fadedDisto = float3(distoDepthFade, distoDepthFade, 1) * baseDisto / fragDepth;
		diffuse = float4(fadedDisto, 1);
		float	distDepth = LINEARIZE_DEPTH(SampleDepthTexture(screenUV + diffuse.xy * 0.26));

		if (fragDepth > distDepth)
			discard;

#	elif	PK_HAS_COLOR
		diffuse *= i.Color;
#	endif

	if (IsAdditive())
	{
		diffuse = diffuse * diffuse.w * depthfade;
		diffuse.w = 0.0f;
	}
	else if (IsAdditiveNoAlpha())
	{
		diffuse *= depthfade;
		diffuse.w = 0.0f;
	}
	else if (IsAlphaBlendAdditive())
	{
		diffuse *= depthfade;
	}
	else
	{
		diffuse.w *= depthfade;
	}
	return diffuse;
}
