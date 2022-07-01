using System.Runtime.InteropServices;
using Field.General;
using Field.Textures;

namespace Field.Models;

[StructLayout(LayoutKind.Sequential, Size = 0x0C)]
public struct Vector2
{
    public float X;
    public float Y;
        
    public Vector2(int x, int y)
    {
        X = x / 32_767.0f;
        Y = y / 32_767.0f;
    }
        
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }
}
    
[StructLayout(LayoutKind.Sequential, Size = 0x0C)]
public struct Vector3
{
    public float X;
    public float Y;
    public float Z;
        
    public Vector3(int x, int y, int z)
    {
        X = x / 32_767.0f;
        Y = y / 32_767.0f;
        Z = z / 32_767.0f;
    }
        
    public Vector3(uint x, uint y, uint z)
    {
        X = x / 65_535.0f;
        Y = y / 65_535.0f;
        Z = z / 65_535.0f;
    }
    
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 Zero {
        get
        {
            Vector3 vec3 = new Vector3();
            vec3.X = 0;
            vec3.Y = 0;
            vec3.Z = 0;
            return vec3;
        }
    }

    public static Vector3 One {
        get
        {
            Vector3 vec3 = new Vector3();
            vec3.X = 1;
            vec3.Y = 1;
            vec3.Z = 1;
            return vec3;
        }
    }
    
    public static Vector3 operator -(Vector3 x, Vector3 y)
    {
        return new Vector3(x.X - y.X, x.Y - y.Y, x.Z - y.Z);
    }
}

public struct IntVector3
{
    public int X;
    public int Y;
    public int Z;
        
    public IntVector3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
    
public struct UIntVector3
{
    public uint X;
    public uint Y;
    public uint Z;
        
    public UIntVector3(uint x, uint y, uint z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
    
[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct Vector4
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
        W = 0;
    }
        
    public Vector4(int x, int y, int z)
    {
        X = x / 32_767.0f;
        Y = y / 32_767.0f;
        Z = z / 32_767.0f;
        W = 0;
    }
        
    public Vector4(int x, int y, int z, int w, bool bIsVector3 = false)
    {
        if (bIsVector3)
        {
            X = x / 32_767.0f;
            Y = y / 32_767.0f;
            Z = z / 32_767.0f;
            W = w;
        }
        else
        {
            X = x / 32_767.0f;
            Y = y / 32_767.0f;
            Z = z / 32_767.0f;
            W = w / 32_767.0f;  
        }
    }

    public Vector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
        
    public Vector4(sbyte x, sbyte y, sbyte z, sbyte w)
    {
        X = x / 255.0f;
        Y = y / 255.0f;
        Z = z / 255.0f;
        W = w / 255.0f;
    }

    public static Vector4 Zero {
        get
        {
            Vector4 vec4 = new Vector4();
            vec4.X = 0;
            vec4.Y = 0;
            vec4.Z = 0;
            vec4.W = 0;
            return vec4;
        }
    }

    public void SetW(int w)
    {
        W = w / 32_767.0f; 
    }
        
    public static Vector4 Quaternion {
        get
        {
            Vector4 vec4 = new Vector4();
            vec4.X = 0;
            vec4.Y = 0;
            vec4.Z = 0;
            vec4.W = 1;
            return vec4;
        }
    }
        
    public double Magnitude
    {
        get
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }
    }

    public Vector3 ToVec3()
    {
        return new Vector3(X, Y, Z);
    }
    
    public Vector3 NormalToEuler()
    {
        Vector3 res = new Vector3();
        if (Math.Abs(Magnitude - 1) < 0.01)  // Quaternion
        {
            var quat = new SharpDX.Quaternion(X, Y, Z, W);
            var a = new SharpDX.Vector3(1, 0, 0);
            var result = SharpDX.Vector3.Transform(a, quat);
            res.X = result.X;
            res.Y = result.Y;
            res.Z = result.Z;
        }
        else
        {
            throw new Exception("Quaternion invalid, not normalised");
        }
        return res;
    }
    
    public float this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return X;
                case 1:
                    return Y;
                case 2:
                    return Z;
                case 3:
                    return W;
            }

            throw new IndexOutOfRangeException();
        }
        set
        {
            switch (index)
            {
                case 0:
                    X = value;
                    return;
                case 1:
                    Y = value;
                    return;
                case 2:
                    Z = value;
                    return;
                case 3:
                    W = value;
                    return;
            }
            throw new IndexOutOfRangeException();
        }
    }
}

public struct IntVector4
{
    public int X;
    public int Y;
    public int Z;
    public int W;

    public IntVector4(int x, int y, int z, int w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }
    
    public int this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return X;
                case 1:
                    return Y;
                case 2:
                    return Z;
                case 3:
                    return W;
            }

            throw new IndexOutOfRangeException();
        }
        set
        {
            switch (index)
            {
                case 0:
                    X = value;
                    return;
                case 1:
                    Y = value;
                    return;
                case 2:
                    Z = value;
                    return;
                case 3:
                    W = value;
                    return;
            }
            throw new IndexOutOfRangeException();
        }
    }
}

public enum EPrimitiveType  // name comes from bungie
{
    Triangles = 3,
    TriangleStrip = 5,
}

public enum ELOD : sbyte
{
    MostDetail,
    LeastDetail,
    All,
}

public class Part
{
    public uint IndexOffset;
    public uint IndexCount;
    public EPrimitiveType PrimitiveType;
    public int DetailLevel;
    public List<UIntVector3> Indices = new List<UIntVector3>();
    public List<uint> VertexIndices = new List<uint>();
    public List<Vector4> VertexPositions = new List<Vector4>();
    public List<Vector2> VertexTexcoords = new List<Vector2>();
    public List<Vector4> VertexNormals = new List<Vector4>();
    public List<Vector4> VertexColours = new List<Vector4>();
    public Material Material;

    public Part()
    {
    }
}
    


[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_IndexHeader
{
    public sbyte Unk00;
    public bool Is32Bit;
    public short Unk02;
    public int Zeros04;
    public long DataSize;
    public int Deadbeef;
    public int Zeros14;
}

[StructLayout(LayoutKind.Sequential, Size = 0xC)]
public struct D2Class_VertexHeader
{
    public uint DataSize;
    public short Stride;
    public short Type;
    public int Deadbeef;
        
}