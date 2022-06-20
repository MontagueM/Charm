using Field.General;

namespace Field.Models;

public class VertexBuffer : Tag
{
    private D2Class_VertexHeader header;

    public VertexBuffer(string hash, VertexHeader parent) : base(hash)
    {
	    header = parent.Header;
    }

    /// <summary>
    /// Parses a vertex buffer from a tag, and returns a list of vertices, only parsing the vertices that are actually used.
    /// </summary>
    /// <param name="dynamicPart">The parent part to set the changes to.</param>
    /// <param name="uniqueVertexIndices">All the vertex indices that will be acquired.</param>
    public void ParseBuffer(Part part, HashSet<uint> uniqueVertexIndices)
    {
	    GetHandle();
	    foreach (var vertexIndex in uniqueVertexIndices)
	    {
		    ReadVertexData(part, vertexIndex);
	    }
	    CloseHandle();
    }

    private void ReadVertexData(Part part, uint vertexIndex)
    {
	    Handle.BaseStream.Seek(vertexIndex * header.Stride, SeekOrigin.Begin);
	    bool status = false;
	    switch (header.Type)
	    {
		    case 0:
			    status = ReadVertexDataType0(part);
			    break;
		    case 1:
			    status = ReadVertexDataType1(part);
			    break;
		    case 6:
			    status = ReadVertexDataType6(part);
			    break;
		    default:
			    throw new NotImplementedException($"Vertex type {header.Type} is not implemented.");
			    break;
	    }
	    if (!status)
	    {
		    throw new NotImplementedException($"Vertex stride {header.Stride} for type {header.Type} is not implemented.");
	    }
    }
    
    private bool ReadVertexDataType0(Part part)
    {
	    switch (header.Stride)
	    {
		    case 0x4:
			    part.VertexUVs.Add(new Vector2(Handle.ReadInt16(), Handle.ReadInt16()));
			    break;
		    case 0x10:
			    part.VertexPositions.Add(new Vector4(Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), true));
			    // Quaternion normal
			    part.VertexNormals.Add(new Vector4(Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16()));
			    break;
		    case 0x18:
			    part.VertexPositions.Add(new Vector4(Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), true));
			    part.VertexNormals.Add(new Vector4(Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), true));
			    // Tangent
			    break;
		    default:
			    return false;
	    }
	    return true;
    }
    
    private bool ReadVertexDataType1(Part part)
	{
		switch (header.Stride)
		{
			case 0x4:
				part.VertexUVs.Add(new Vector2(Handle.ReadInt16(), Handle.ReadInt16()));
				break;
			// 0x8 at least old weights 8e2dab80
			case 0x18:
				part.VertexPositions.Add(new Vector4(Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), true));
				part.VertexNormals.Add(new Vector4(Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), Handle.ReadInt16(), true));
				// Tangent
				break;
			case 0x30: // physics
				part.VertexPositions.Add(new Vector4(Handle.ReadSingle(), Handle.ReadSingle(), Handle.ReadSingle(), Handle.ReadSingle()));
				part.VertexNormals.Add(new Vector4(Handle.ReadSingle(), Handle.ReadSingle(), Handle.ReadSingle(), Handle.ReadSingle()));
				// Tangent
				break;
			default:
				return false;
		}
		return true;
	}
    private bool ReadVertexDataType6(Part part)
    {
	    switch (header.Stride)
	    {
		    case 0x4:
			    // TODO implement single pass vertex buffer
			    return false;
			    break;
		    default:
			    return false;
	    }
	    return true;
	}
}