struct VSIn
{
	//uint vertexId : SV_VertexID;
	float2 pos : POSITION;
	//float3 normal : NORMAL0;
	float2 uv : TEXCOORD0;
	float4 color : COLOR0;
};

struct VSOut
{
	float4 pos : SV_Position;
	float2 uv : TEXCOORD0;
	float4 color : COLOR0;
};

cbuffer ShaderBoxConstants : register(b0)
{
	float4x4 Model;
	float4x4 View;
	float4x4 Projection;
	float4x4 ModelViewProjection;
};

VSOut main(VSIn input)
{
	VSOut output;

	float4 pos = float4(input.pos.xy, 0.0, 1.0);
	pos = mul(Model, pos);
	pos = mul(View, pos);
	pos = mul(Projection, pos);
	output.pos = pos;

	output.uv = input.uv;
	//output.color = float4(input.uv, 1, 1);
	output.color = input.color;

	return output;
}