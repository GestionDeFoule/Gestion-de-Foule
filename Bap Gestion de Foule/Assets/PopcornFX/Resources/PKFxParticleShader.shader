﻿// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "PopcornFX/PKFxParticleShader"
{
	Properties
	{
		 _MainTex("Sprite Texture", 2D) = "white" {}
		_AlphaMap("Alpha Remap Texture", 2D) = "white" {}
		_SrcBlendMode("Src Blend Mode", Int) = 0
		_DstBlendMode("Dst Blend Mode", Int) = 0
		_ZTestMode("ZTest Mode", Int) = 0
		_UniformFlags("Uniform Flags", Int) = 0
		_InvSoftnessDistance("Inverse Softness Distance", Float) = 1
//		Only for debug:
//		_MaterialFlags("Material Flags", Int) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [_ZTestMode]
		Blend [_SrcBlendMode] [_DstBlendMode]

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ PK_HAS_COLOR
			#pragma multi_compile _ PK_HAS_ALPHA_REMAP
			#pragma multi_compile _ PK_HAS_SOFT
			#pragma multi_compile _ PK_HAS_LIGHTING PK_HAS_DISTORTION
			#pragma multi_compile _ PK_HAS_ANIM_BLEND PK_HAS_RIBBON_COMPLEX

			#define	USE_HDRP	0

			//------------------------------------------------------------------------------------
			// Particle shader
			//------------------------------------------------------------------------------------
			#include "PKFxShaderCode/ParticleShader.inc"
			
			ENDCG
		}
	}




}