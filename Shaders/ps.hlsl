struct PSIn
{
	float4 pos : SV_Position;
	float2 uv : TEXCOORD0;
	linear float4 color : color;
};

struct PSOut
{
	float4 color : SV_Target;
};

Texture2D Texture : register(t0);
sampler Sampler : register(s0);

PSOut main(PSIn input)
{
	PSOut output;

	//output.color = input.color;
	output.color = Texture.Sample(Sampler, input.uv);

	return output;
}