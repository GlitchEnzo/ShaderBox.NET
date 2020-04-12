struct VSIn
{
	//uint vertexId : SV_VertexID;
	float4 pos : POSITION;
	float3 normal : NORMAL0;
	float2 uv : TEXCOORD0;
};

struct VSOut
{
	float4 pos : SV_Position;
	float2 uv : TEXCOORD0;
	float4 color : color;
};

cbuffer ShaderBoxConstants : register(b0)
{
	float4x4 Model;
	float4x4 View;
	float4x4 Projection;
	//float4x4 ModelView;
	//float4x4 ViewProjection;
	float4x4 ModelViewProjection;
};

VSOut main(VSIn input)
{
	// https://www.geeks3d.com/20111026/how-to-compute-position-and-normal-vertex-shader-opengl-glsl-direct3d-hlsl-matrix/
	VSOut output;

	// WORKS!
	float4 pos = float4(input.pos.xyz, 1.0);
	pos = mul(Model, pos);
	pos = mul(View, pos);
	pos = mul(Projection, pos);
	output.pos = pos;

	// WORKS!
	//float4x4 modelView = mul(View, Model);
	//float4x4 modelViewProjection = mul(Projection, modelView);
	//output.pos = mul(float4(input.pos.xyz, 1.0), transpose(modelViewProjection));

	// WORKS!
	//float4x4 viewProjection = mul(Projection, View); // why are these transposed???
	// Answer: https://stackoverflow.com/questions/32037617/why-is-this-transpose-required-in-my-worldviewproj-matrix
	//output.pos = mul(viewProjection, input.pos); // reverse multiplication to prevent transpose

	// WORKS!
	//output.pos = mul(input.pos, ModelViewProjection);

	output.uv = input.uv;

	//output.color = clamp(output.pos, 0, 1);
	//output.color = float4(1, 0, 0, 1);
	//output.color = float4(input.normal, 1);
	output.color = float4(input.uv, 1, 1);

	return output;
}