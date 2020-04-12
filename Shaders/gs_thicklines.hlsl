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

//static float thickness = 0.05;

static float pixelWidth = 50;
static float2 thickness = float2(pixelWidth / 1280, pixelWidth / 720);

[maxvertexcount(6)]
void main(lineadj GSIn input[4], inout TriangleStream<GSOut> outStream)
{
	GSOut output;

	// pass through start of the previous line
	//output.pos = input[0].pos;
	//output.color = input[0].color;
	//outStream.Append(output);

	float2 p0 = input[1].pos.xy;   // end of previous line, start of current line
	float2 p1 = input[2].pos.xy;   // end of current line, start of next line

	float2 lineVector = p1 - p0;
	float2 normal = normalize(float2(-lineVector.y, lineVector.x));

	float2 a = p0 - thickness * normal;
	float2 b = p0 + thickness * normal;
	float2 c = p1 - thickness * normal;
	float2 d = p1 + thickness * normal;

	output.pos = float4(a, 0.5, 1);
	output.color = input[1].color;
	outStream.Append(output);

	output.pos = float4(b, 0.5, 1);
	output.color = input[1].color;
	outStream.Append(output);

	output.pos = float4(c, 0.5, 1);
	output.color = input[2].color;
	outStream.Append(output);

	output.pos = float4(d, 0.5, 1);
	output.color = input[2].color;
	outStream.Append(output);

	// pass through end of the next line
	//output.pos = input[3].pos;
	//output.color = input[3].color;
	//outStream.Append(output);
}