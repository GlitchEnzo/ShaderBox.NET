struct GSIn
{
	float4 pos : SV_Position;
	float4 color : color;
};

struct GSOut
{
	float4 pos : SV_Position;
	float4 color : color;
};

[maxvertexcount(3)]
void main(triangle GSIn input[3], inout TriangleStream<GSOut> outStream)
{
	GSOut output;
	[unroll(3)]
	for (int i = 0; i < 3; ++i)
	{
		output.pos = input[i].pos;
		output.color = input[i].color;
		outStream.Append(output);
	}
}