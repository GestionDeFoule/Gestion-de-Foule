﻿struct SVertexOutput
{
	float4 vertex				: SV_POSITION;

	float4 ProjPos				: TEXCOORD0;

#	if	PK_HAS_COLOR
		float4 Color				: COLOR;
#	endif

#	if	PK_HAS_LIGHTING
		float4 Normal				: NORMAL;
#	endif

#	if	PK_HAS_RIBBON_COMPLEX
		//------------------------------------------
		float2 UV0					: TEXCOORD1;
		float4 UVScaleAndOffset		: TEXCOORD2;
		float4 UVFactors			: TEXCOORD3;

#		if		PK_HAS_ALPHA_REMAP
			float  AlphaCursor : TEXCOORD4;
#		endif
		//------------------------------------------
#	elif	PK_HAS_ANIM_BLEND
		//------------------------------------------
		float2 UV0					: TEXCOORD1;
		float2 UV1					: TEXCOORD2;
		float  FrameLerp : TEXCOORD3;

#		if		PK_HAS_ALPHA_REMAP
			float  AlphaCursor : TEXCOORD4;
#		endif
		//------------------------------------------
#	else
		//------------------------------------------
		float2 UV0					: TEXCOORD1;

#		if		PK_HAS_ALPHA_REMAP
			float  AlphaCursor : TEXCOORD2;
#		endif
	//------------------------------------------
#	endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
};
