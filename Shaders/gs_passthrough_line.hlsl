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

[maxvertexcount(2)]
void main(line GSIn input[2], inout LineStream<GSOut> outStream)
{
	GSOut output;
	[unroll(2)]
	for (int i = 0; i < 2; ++i)
	{
		output.pos = input[i].pos;
		output.color = input[i].color;
		outStream.Append(output);
	}
}