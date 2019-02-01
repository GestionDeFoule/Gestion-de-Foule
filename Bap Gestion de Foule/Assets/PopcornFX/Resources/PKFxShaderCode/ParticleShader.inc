
#if		USE_HDRP
#	include "CoreRP/ShaderLibrary/Common.hlsl"
#	include "CoreRP/ShaderLibrary/Wind.hlsl"

#	include "HDRP/ShaderVariables.hlsl"
#	include "HDRP/ShaderPass/FragInputs.hlsl"
#	include "HDRP/ShaderPass/ShaderPass.cs.hlsl"
#	include "HDRP/shaderPass/VaryingMesh.hlsl"
#	include "HDRP/shaderPass/VertMesh.hlsl"
#	include "HDRP/Material/Material.hlsl"
#else
#	include "UnityCG.cginc"
#endif

// Vertex inputs:
#include "VertexInput.inc"

// Vertex outputs:
#include "VertexOutput.inc"

//------------------------------------------------------------------------------------
// Vertex shader
//------------------------------------------------------------------------------------
#include "VertexShader.inc"

//------------------------------------------------------------------------------------
// Fragment shader
//------------------------------------------------------------------------------------
#include "FragmentShader.inc"

