struct PSIn
{
	float4 pos : SV_Position;
	float2 uv : TEXCOORD0;
	float4 color : COLOR0;
};

struct PSOut
{
	float4 color : SV_Target;
};

Texture2D FontTexture : register(t0);
sampler FontSampler : register(s0);

PSOut main(PSIn input)
{
	PSOut output;

	//output.color = input.color;
	//output.color = float4(1, 0, 0, 1);

	// TODO: Fix bug where the channels are swapped - wait, don't need to swap channels. need to turn on alpha blending!
	//float4 fontColor = FontTexture.Sample(FontSampler, input.uv);
	//fontColor.rgb = fontColor.a;
	////output.color = fontColor;
	//output.color = input.color * fontColor;

	output.color = input.color * FontTexture.Sample(FontSampler, input.uv);

	return output;
}