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

static float pixelWidth = 10;
static float2 thickness = float2(pixelWidth / 1280, pixelWidth / 720);

[maxvertexcount(7)]
void main(lineadj GSIn vertices[4], inout TriangleStream<GSOut> triStream)
{
	float2 p0 = vertices[0].pos.xy; // start of previous line
	float2 p1 = vertices[1].pos.xy; // end of previous line, start of current line
	float2 p2 = vertices[2].pos.xy; // end of current line, start of next line
	float2 p3 = vertices[3].pos.xy; // end of next line

	float2 v0 = normalize(p1 - p0); // normalized vector for previous line
	float2 v1 = normalize(p2 - p1); // normalized vector for current line
	float2 v2 = normalize(p3 - p2); // normalized vector for next line

	// determine the normal of each of the 3 lines (previous, current, next)
	float2 n0 = { -v0.y, v0.x };
	float2 n1 = { -v1.y, v1.x };
	float2 n2 = { -v2.y, v2.x };

	// determine miter lines by averaging the normals of the 2 lines
	float2 miter_a = normalize(n0 + n1); // miter at start of current line
	float2 miter_b = normalize(n1 + n2); // miter at end of current line

	// determine the length of the miter by projecting it onto normal and then inverse it
	float length_a = 1 / dot(miter_a, n1);
	float length_b = 1 / dot(miter_b, n1);

	GSOut v;
	float2 temp;

	// generate the triangle strip
	temp = (p1 + length_a * miter_a);
	v.pos = float4(temp, 0.5, 1.0);
	v.color = float4(1, 0, 0, 1);
	triStream.Append(v);

	temp = (p1 - length_a * miter_a);
	v.pos = float4(temp, 0.5, 1.0);
	v.color = float4(0, 1, 0, 1);
	triStream.Append(v);

	temp = (p2 + length_b * miter_b);
	v.pos = float4(temp, 0.5, 1.0);
	v.color = float4(0, 0, 1, 1);
	triStream.Append(v);

	temp = (p2 - length_b * miter_b);
	v.pos = float4(temp, 0.5, 1.0);
	v.color = float4(1, 1, 1, 1);
	triStream.Append(v);
}