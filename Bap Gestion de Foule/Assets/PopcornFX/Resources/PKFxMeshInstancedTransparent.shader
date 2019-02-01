// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "PopcornFX/PKFxMeshInstancedTransparent"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_UniformFlags("Uniform Flags", Int) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		LOD 200

		Blend One One
		ZWrite Off
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		// And generate the shadow pass with instancing support
		#pragma surface surf NoLighting
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		// Enable instancing for this shader
		#pragma multi_compile_instancing

		#define	PK_IS_TRANSPARENT	1

		#include "PKFxShaderCode/MeshInstanceSurface.inc"

		ENDCG
	}
	FallBack "Diffuse"
}
