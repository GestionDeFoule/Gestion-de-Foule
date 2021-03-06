SVertexOutput	vert(SVertexInput v)
{
	SVertexOutput	o;

	UNITY_SETUP_INSTANCE_ID(v);
 	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

#if		USE_HDRP
	//Transforming vertex with HDRP
	AttributesMesh coreVertexInput;
	coreVertexInput.positionOS = v.vertex;
	VaryingsMeshType outVarying = VertMesh(coreVertexInput);
	o.vertex = float4(outVarying.positionCS);
	o.ProjPos = float4(outVarying.positionCS);
#else
	// Generic outputs for all variations:
	// Vertex position:
	o.vertex = UnityObjectToClipPos(v.vertex);
	// Could only be for Soft particles and distortion:
	o.ProjPos = o.vertex;
#endif

#	if	PK_HAS_COLOR
		//------------------------------------------
		//	Color
		//------------------------------------------

		o.Color = v.Color;
#	endif

#	if	PK_HAS_LIGHTING
		//------------------------------------------
		//	Lighting
		//------------------------------------------

		o.Normal = v.Normal;
#	endif

#	if	PK_HAS_RIBBON_COMPLEX

		o.UVScaleAndOffset = float4(v.UVScale, v.UVOffset);

		uint		id = v.VertexID;

		if (id % 4 == 0)
		{
			o.UV0 = float2(0.0, 0.0);
			o.UVFactors = float4(1.0, 1.0, 1.0, 1.0);
		}
		else if (id % 4 == 1)
		{
			o.UV0 = float2(0.0, 1.0);
			o.UVFactors = float4(v.UVFactors.x, 1.0, 1.0, 1.0 / v.UVFactors.y);
		}
		else if (id % 4 == 2)
		{
			o.UV0 = float2(1.0, 0.0);
			o.UVFactors = float4(1.0, v.UVFactors.y, 1.0 / v.UVFactors.x, 1.0);
		}
		else
		{
			o.UV0 = float2(1.0, 1.0);
			o.UVFactors = float4(1.0, 1.0, 1.0, 1.0);
		}

#		if	PK_HAS_ALPHA_REMAP
			o.AlphaCursor = v.AlphaCursor.x;
#		endif

#	elif	PK_HAS_ANIM_BLEND
		//------------------------------------------
		//	Animation blend
		//------------------------------------------

		o.UV0 = v.UV0;
		o.UV1 = v.UV1;
		o.FrameLerp = frac(v.AtlasIdAlphaCursor.x);

#		if	PK_HAS_ALPHA_REMAP
			o.AlphaCursor = v.AtlasIdAlphaCursor.y;
#		endif
#	else
		//------------------------------------------
		//	No animation blending
		//------------------------------------------

		o.UV0 = v.UV0;

#		if	PK_HAS_ALPHA_REMAP
			o.AlphaCursor = v.AlphaCursor.x;
#		endif
#	endif
	return o;
}
