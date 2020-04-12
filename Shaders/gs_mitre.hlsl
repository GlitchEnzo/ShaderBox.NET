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

	float2 prevStart = input[0].pos.xy;
	float2 p0 = input[1].pos.xy;   // end of previous line, start of current line
	float2 p1 = input[2].pos.xy;   // end of current line, start of next line
	float2 nextEnd = input[3].pos.xy;

	float2 prevVector = p0 - prevStart;
	//float2 prevNormal = normalize(float2(-prevVector.y, prevVector.x));

	float2 lineVector = p1 - p0;
	float2 lineNormal = normalize(float2(-lineVector.y, lineVector.x));

	float2 nextVector = nextEnd - p1;
	//float2 nextNormal = normalize(float2(-nextVector.y, nextVector.x));

	float2 startTangent = normalize(normalize(prevVector) + normalize(lineVector));
	float2 endTangent = normalize(normalize(lineVector) + normalize(nextVector));

	float2 startMiter = float2(-startTangent.y, startTangent.x);
	float2 startMiterThickness = thickness / dot(startMiter, lineNormal);

	float2 endMiter = float2(-endTangent.y, endTangent.x);
	float2 endMiterThickness = thickness / dot(endMiter, lineNormal);

	if (length(prevVector) < 0.001)
	{
		startMiter = lineNormal;
		startMiterThickness = thickness;
	}

	if (length(nextVector) < 0.001)
	{
		endMiter = lineNormal;
		endMiterThickness = thickness;
	}

	float2 a = p0 - startMiterThickness * startMiter;
	float2 b = p0 + startMiterThickness * startMiter;
	float2 c = p1 - endMiterThickness * endMiter;
	float2 d = p1 + endMiterThickness * endMiter;

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
}