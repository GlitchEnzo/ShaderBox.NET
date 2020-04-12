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

[maxvertexcount(4)]
void main(lineadj GSIn input[4], inout LineStream<GSOut> outStream)
{
	GSOut output;
	[unroll(4)]
	for (int i = 0; i < 4; ++i)
	{
		output.pos = input[i].pos;
		output.color = input[i].color;
		outStream.Append(output);
	}
}