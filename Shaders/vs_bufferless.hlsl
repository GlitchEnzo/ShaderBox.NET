struct VSIn
{
	uint vertexId : SV_VertexID;
};

struct VSOut
{
	float4 pos : SV_Position;
	float4 color : color;
};

//    0
//   / \
//  2---1
static float3 ScreenspaceTriangleVertices[3] =
{
	float3(0.0, 0.5, 0.5),
	float3(0.5, -0.5, 0.5),
	float3(-0.5, -0.5, 0.5),
};

//  3     0---1  
//  | \     \ |
//  5---4     2
static float3 ScreenspaceFullscreenQuadVertices[6] =
{
	float3(-1.0,  1.0, 0.5),
	float3( 1.0,  1.0, 0.5),
	float3( 1.0, -1.0, 0.5),

	float3(-1.0,  1.0, 0.5),
	float3( 1.0, -1.0, 0.5),
	float3(-1.0, -1.0, 0.5),
};

//   0---1
//       |
//   3---2
static float3 ScreenspaceTestLineVertices[4] =
{
	float3(-0.5, 0.5, 0.5),
	float3(0.5, 0.5, 0.5),
	float3(0.5, -0.5, 0.5),
	float3(-0.5, -0.5, 0.5),
};

// index buffer for the line
static uint ScreenspaceTestLineIndices[6] =
{
	0, 1,
	1, 2,
	2, 3
};

// index buffer for the line with additional adjacency information
static uint ScreenspaceTestLineIndicesAdj[12] =
{
	0, 0, 1, 2, // 0 0 since it is the first and has no adj line at the start
	0, 1, 2, 3,
	1, 2, 3, 3 // 3 3 since it is the last and has no adj line at the end
};

static float3 CubeVertices[8] =
{
	// front
	float3(-0.5, -0.5, 0.5),
	float3( 0.5, -0.5, 0.5),
	float3( 0.5,  0.5, 0.5),
	float3(-0.5,  0.5, 0.5),

	// back
	float3(-0.5, -0.5, -0.5),
	float3( 0.5, -0.5, -0.5),
	float3( 0.5,  0.5, -0.5),
	float3(-0.5,  0.5, -0.5),
};

static uint CubeIndices[36] =
{
	// front
	0, 1, 2,
	2, 3, 0,
	// right
	1, 5, 6,
	6, 2, 1,
	// back
	7, 6, 5,
	5, 4, 7,
	// left
	4, 0, 3,
	3, 7, 4,
	// bottom
	4, 5, 1,
	1, 0, 4,
	// top
	3, 2, 6,
	6, 7, 3
};

struct Message
{
	uint data[20];
};

//void Print(Message message)
void Print(uint message[20]) // this works, but passed in array size must match
{
	// add characters to a RWByteAddress append buffer
}

void Print(string message)
{
	// is there anything you can do with strings in HLSL?
	//uint chr0 = message[0];  // cannot index into a string
	//uint chr0 = message. // there appear to be no functions or methods for strings
	//string str2 = message + "\n"; // there is no string concatenation
}

VSOut main(VSIn input)
{
	VSOut output;

	//uint chr = 0x61; //'a'
	//uint chr2 = 'a';

	//uint data[20] = { 'H','e','l','l','o',' ','W','o','r','l','d',' ','I','D','=', input.vertexId, '\0','\0','\0','\0' }; // this works, but size of array and init must match
	//uint data[] = { 'H','e','l','l','o' }; // this works and doesn't require a size

	//Message message;
	//message.data = { 'H','e','l','l','o',' ','W','o','r','l','d',' ','I','D','=', input.vertexId, '\0','\0','\0','\0' };
	//message.data = data;

	//Print(message);
	//Print(str);

	//output.pos = float4(ScreenspaceTriangleVertices[input.vertexId], 1);
	//output.pos = float4(ScreenspaceFullscreenQuadVertices[input.vertexId], 1);
	//output.pos = float4(ScreenspaceTestLineVertices[input.vertexId], 1);
	//output.pos = float4(ScreenspaceTestLineVertices[ScreenspaceTestLineIndices[input.vertexId]], 1);
	//output.pos = float4(ScreenspaceTestLineVertices[ScreenspaceTestLineIndicesAdj[input.vertexId]], 1);
	output.pos = float4(CubeVertices[CubeIndices[input.vertexId]], 1);

	output.color = clamp(output.pos, 0, 1);
	//output.color = float4(1, 1, 1, 1);

	return output;
}