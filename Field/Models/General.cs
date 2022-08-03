using System.Runtime.InteropServices;
using Field.General;
using Field.Textures;
using Internal.Fbx;

namespace Field.Models;

[StructLayout(LayoutKind.Sequential, Size = 8)]
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

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct IntVector2
{
    public int X;
    public int Y;
        
    public IntVector2(int x, int y)
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
        
    public Vector3(int x, int y, int z, bool bAsInt=false)
    {
        if (bAsInt)
        {
            X = x;
            Y = y;
            Z = z;
        }
        else
        {
            X = x / 32_767.0f;
            Y = y / 32_767.0f;
            Z = z / 32_767.0f;  
        }
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
            }
            throw new IndexOutOfRangeException();
        }
    }

    public FbxVector4 ToFbxVector4()
    {
        return new FbxVector4(X, Y, Z);
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
        
    public Vector4(byte x, byte y, byte z, byte w)
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
    
    // From https://github.com/OwlGamingCommunity/V/blob/492d0cb3e89a97112ac39bf88de39da57a3a1fbf/Source/owl_core/Server/MapLoader.cs
    public Vector3 QuaternionToEulerAngles()
    {
        Vector3 retVal = new Vector3();

        // roll (x-axis rotation)
        double sinr_cosp = +2.0 * (W * X + Y * Z);
        double cosr_cosp = +1.0 - 2.0 * (X * X + Y * Y);
        retVal.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        double sinp = +2.0 * (W * Y - Z * X);
        double absSinP = Math.Abs(sinp);
        bool bSinPOutOfRage = absSinP >= 1.0;
        if (bSinPOutOfRage)
        {
            retVal.Y = 90.0f; // use 90 degrees if out of range
        }
        else
        {
            retVal.Y = (float)Math.Asin(sinp);
        }

        // yaw (z-axis rotation)
        double siny_cosp = +2.0 * (W * Z + X * Y);
        double cosy_cosp = +1.0 - 2.0 * (Y * Y + Z * Z);
        retVal.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

        // Rad to Deg
        retVal.X *= (float)(180.0f / Math.PI);

        if (!bSinPOutOfRage) // only mult if within range
        {
            retVal.Y *= (float)(180.0f / Math.PI);
        }
        retVal.Z *= (float)(180.0f / Math.PI);
        
        return retVal;
    }

    public FbxQuaternion ToFbxQuaternion()
    {
        return new FbxQuaternion(X, Y, Z, W);
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
    public List<Vector4> VertexTangents = new List<Vector4>();
    public List<Vector4> VertexColours = new List<Vector4>();
    public Material Material;
    public int GroupIndex = 0;

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