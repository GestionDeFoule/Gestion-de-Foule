int			_UniformFlags;

#if	SHADER_API_D3D9 || SHADER_API_D3D11_9X
	#define	HANDLE_BIT_OPERATIONS	0
#else
	#define	HANDLE_BIT_OPERATIONS	1
#endif

#define Is_AdditiveAlphaBlend	1
#define Is_Additive				2
#define Is_AdditiveNoAlpha		4
#define Is_RotateTexture		8

#if	HANDLE_BIT_OPERATIONS

	bool		IsAlphaBlendAdditive() { return (_UniformFlags & Is_AdditiveAlphaBlend) != 0; }
	bool		IsAdditive() { return (_UniformFlags & Is_Additive) != 0; }
	bool		IsAdditiveNoAlpha() { return (_UniformFlags & Is_AdditiveNoAlpha) != 0; }
	bool		IsRotateTexture() { return (_UniformFlags & Is_RotateTexture) != 0; }

#else

	bool		IsAlphaBlendAdditive() { return (_UniformFlags / Is_AdditiveAlphaBlend) % 2 == 1; }
	bool		IsAdditive() { return (_UniformFlags / Is_Additive) % 2 == 1; }
	bool		IsAdditiveNoAlpha() { return (_UniformFlags / Is_AdditiveNoAlpha) % 2 == 0; }
	bool		IsRotateTexture() { return (_UniformFlags / Is_RotateTexture) % 2 == 1; }

#endif