using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tiger.Schema;

public struct VertexWeight
{
    public IntVector4 WeightValues;
    public IntVector4 WeightIndices;
}

public struct Transform
{
    public Transform() { }

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; }
    public Vector4 Quaternion { get; set; } = Vector4.Quaternion;
    public Vector3 Scale { get; set; } = Vector3.One;
    public float Order { get; set; }
}

/// <summary>
/// A Map Transfrom
/// </summary>
/// <returns>Vector4 Rotation, Vector4 Translation</returns>
[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct MapTransform
{
    public Vector4 Rotation;
    public Vector4 Translation;

    public override string ToString()
    {
        return $"Rotation: {Rotation.ToString()}\nTranslation: {Translation.ToString()}";
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct Matrix4x4
{
    public Vector4 X_Axis;
    public Vector4 Y_Axis;
    public Vector4 Z_Axis;
    public Vector4 W_Axis;

    /// <summary>
    /// Converts a Tiger Matrix4x4 to System.Numerics.Matrix4x4
    /// </summary>
    /// <returns>System.Numerics.Matrix4x4</returns>
    public System.Numerics.Matrix4x4 ToSys()
    {
        return new System.Numerics.Matrix4x4(
        X_Axis.X, X_Axis.Y, X_Axis.Z, X_Axis.W,
        Y_Axis.X, Y_Axis.Y, Y_Axis.Z, Y_Axis.W,
        Z_Axis.X, Z_Axis.Y, Z_Axis.Z, Z_Axis.W,
        W_Axis.X, W_Axis.Y, W_Axis.Z, W_Axis.W);
    }

    public static Vector3 Multiply(Matrix4x4 matrix, Vector3 vector)
    {
        float x = matrix.X_Axis.X * vector.X + matrix.X_Axis.Y * vector.Y + matrix.X_Axis.Z * vector.Z + matrix.X_Axis.W * 1.0f;
        float y = matrix.Y_Axis.X * vector.X + matrix.Y_Axis.Y * vector.Y + matrix.Y_Axis.Z * vector.Z + matrix.Y_Axis.W * 1.0f;
        float z = matrix.Z_Axis.X * vector.X + matrix.Z_Axis.Y * vector.Y + matrix.Z_Axis.Z * vector.Z + matrix.Z_Axis.W * 1.0f;
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Decompose this Matrix into Translation, Rotation and Scale.
    /// </summary>
    /// <returns>Vector4 Translation, Vector4 Quaternion, Vector3 Scale</returns>
    public void Decompose(out Vector4 trans, out Vector4 quat, out Vector3 scale)
    {
        System.Numerics.Matrix4x4 matrix = ToSys();

        System.Numerics.Vector3 scale_sys = new();
        System.Numerics.Vector3 trans_sys = new();
        Quaternion quat_sys = new();
        System.Numerics.Matrix4x4.Decompose(matrix, out scale_sys, out quat_sys, out trans_sys);

        trans = new Tiger.Schema.Vector4(trans_sys.X, trans_sys.Y, trans_sys.Z, 1.0f);
        quat = new Tiger.Schema.Vector4(quat_sys.X, quat_sys.Y, quat_sys.Z, quat_sys.W);
        scale = new Tiger.Schema.Vector3(scale_sys.X, scale_sys.Y, scale_sys.Z);
    }

    public override string ToString() =>
        $"[{X_Axis.ToString()}\n" +
        $"{Y_Axis.ToString()}\n" +
        $"{Z_Axis.ToString()}\n" +
        $"{W_Axis.ToString()}]";
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct AABB
{
    public Vector4 Min;
    public Vector4 Max;
}

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

    public Vector2(Half x, Half y)
    {
        X = (float)x;
        Y = (float)y;
    }

    public Vector2(double x, double y)
    {
        X = (float)x;
        Y = (float)y;
    }

    public static Vector2 operator *(Vector2 x, float y)
    {
        return new Vector2(x.X * y, x.Y * y);
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

    public Vector3(float x)
    {
        X = x;
        Y = x;
        Z = x;
    }

    public static Vector3 Zero
    {
        get
        {
            Vector3 vec3 = new();
            vec3.X = 0;
            vec3.Y = 0;
            vec3.Z = 0;
            return vec3;
        }
    }

    public static Vector3 One
    {
        get
        {
            Vector3 vec3 = new();
            vec3.X = 1;
            vec3.Y = 1;
            vec3.Z = 1;
            return vec3;
        }
    }

    public static Vector3 operator +(Vector3 x, Vector3 y)
    {
        return new Vector3(x.X + y.X, x.Y + y.Y, x.Z + y.Z);
    }

    public static Vector3 operator -(Vector3 x, Vector3 y)
    {
        return new Vector3(x.X - y.X, x.Y - y.Y, x.Z - y.Z);
    }

    public static Vector3 operator *(Vector3 x, Vector3 y)
    {
        return new Vector3(x.X * y.X, x.Y * y.Y, x.Z * y.Z);
    }

    public static Vector3 operator *(Vector3 x, float y)
    {
        return new Vector3(x.X * y, x.Y * y, x.Z * y);
    }

    public static bool operator ==(Vector3 x, Vector3 y)
    {
        return x.X == y.X &&
        x.Y == y.Y &&
        x.Z == y.Z;
    }

    public static bool operator !=(Vector3 x, Vector3 y)
    {
        return x.X != y.X &&
        x.Y != y.Y &&
        x.Z != y.Z;
    }

    public static Vector3 Transform(Vector4 value, Vector4 rotation)
    {
        return Transform(value.ToVec3(), rotation);
    }

    public static Vector3 Transform(Vector3 value, Vector4 rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Vector3(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2)
        );
    }

    public override string ToString() =>
    $"({FormatFloat(X)}, {FormatFloat(Y)}, {FormatFloat(Z)})";

    private static string FormatFloat(float value)
    {
        if (float.IsInfinity(value) || float.IsNaN(value))
            return value.ToString(); // This will return "Infinity", "-Infinity", or "NaN"

        return Decimal.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture).ToString();
    }

    public System.Numerics.Vector3 ToSys()
    {
        return new System.Numerics.Vector3(X, Y, Z);
    }

    public static implicit operator System.Numerics.Vector3(Vector3 m)
    {
        return Unsafe.As<Vector3, System.Numerics.Vector3>(ref m);
    }

    public static implicit operator Vector3(System.Numerics.Vector3 m)
    {
        return Unsafe.As<System.Numerics.Vector3, Vector3>(ref m);
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

    public Vector4(float x)
    {
        X = x;
        Y = x;
        Z = x;
        W = x;
    }

    public Vector4(Half x, Half y, Half z, Half w)
    {
        X = (float)x;
        Y = (float)y;
        Z = (float)z;
        W = (float)w;
    }

    public Vector4(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
        W = 0;
    }

    public Vector4(double x, double y, double z, double w)
    {
        X = (float)x;
        Y = (float)y;
        Z = (float)z;
        W = (float)w;
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

    /// <summary>
    /// Terrain specific to ease computation.
    /// </summary>
    public Vector4(ushort x, ushort y, short z, ushort w, bool bIsVector3 = false)
    {
        if (bIsVector3)
        {
            X = x / 65_535.0f;
            Y = y / 65_535.0f;
            Z = z / 32_767.0f;
            W = w;
        }
        else
        {
            X = x / 65_535.0f;
            Y = y / 65_535.0f;
            Z = z / 32_767.0f;
            W = w / 65_535.0f;
        }
    }

    public Vector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vector4(byte x, byte y, byte z, byte w, bool UInt = false)
    {
        if (UInt)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        else
        {
            X = x / 255.0f;
            Y = y / 255.0f;
            Z = z / 255.0f;
            W = w / 255.0f;
        }
    }

    public Vector4(Vector3 vec3)
    {
        X = vec3.X;
        Y = vec3.Y;
        Z = vec3.Z;
        W = 1;
    }

    public Vector4(Vector3 vec3, float w)
    {
        X = vec3.X;
        Y = vec3.Y;
        Z = vec3.Z;
        W = w;
    }

    public Vector4(Quaternion quat)
    {
        X = quat.X;
        Y = quat.Y;
        Z = quat.Z;
        W = quat.W;
    }

    public Vector4(Vector2 xy, Vector2 zw)
    {
        X = xy.X;
        Y = xy.Y;
        Z = zw.X;
        W = zw.Y;
    }

    public static Vector4 Zero
    {
        get
        {
            Vector4 vec4 = new();
            vec4.X = 0;
            vec4.Y = 0;
            vec4.Z = 0;
            vec4.W = 0;
            return vec4;
        }
    }

    public static Vector4 One
    {
        get
        {
            Vector4 vec4 = new();
            vec4.X = 1.0f;
            vec4.Y = 1.0f;
            vec4.Z = 1.0f;
            vec4.W = 1.0f;
            return vec4;
        }
    }

    public void SetW(int w)
    {
        W = w / 32_767.0f;
    }

    public Vector4 WithW(float w)
    {
        return new Vector4(X, Y, Z, w);
    }

    public static Vector4 Quaternion
    {
        get
        {
            Vector4 vec4 = new();
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

    public System.Numerics.Vector4 ToSys()
    {
        return new System.Numerics.Vector4(X, Y, Z, W);
    }

    public static implicit operator System.Numerics.Vector4(Vector4 m)
    {
        return Unsafe.As<Vector4, System.Numerics.Vector4>(ref m);
    }

    public static implicit operator Vector4(System.Numerics.Vector4 m)
    {
        return Unsafe.As<System.Numerics.Vector4, Vector4>(ref m);
    }

    public System.Numerics.Quaternion ToQuat()
    {
        return new System.Numerics.Quaternion(X, Y, Z, W);
    }

    public float this[int index]
    {
        get
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new IndexOutOfRangeException(),
            };
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

    public static bool operator ==(Vector4 x, Vector4 y)
    {
        return x.X == y.X &&
        x.Y == y.Y &&
        x.Z == y.Z &&
        x.W == y.W;
    }

    public static bool operator !=(Vector4 x, Vector4 y)
    {
        return x.X != y.X &&
        x.Y != y.Y &&
        x.Z != y.Z &&
        x.W != y.W;
    }

    public static Vector4 operator +(Vector4 x, Vector4 y)
    {
        return new Vector4(x.X + y.X,
            x.Y + y.Y,
            x.Z + y.Z,
            x.W + y.W);
    }

    public static Vector4 operator -(Vector4 x, Vector4 y)
    {
        return new Vector4(x.X - y.X,
            x.Y - y.Y,
            x.Z - y.Z,
            x.W - y.W);
    }

    public static Vector4 operator *(Vector4 x, Vector4 y)
    {
        return new Vector4(x.X * y.X,
            x.Y * y.Y,
            x.Z * y.Z,
            x.W * y.W);
    }

    public static Vector4 operator /(Vector4 x, Vector4 y)
    {
        return new Vector4(x.X / y.X,
            x.Y / y.Y,
            x.Z / y.Z,
            x.W / y.W);
    }

    public static Vector4 operator /(Vector4 x, float y)
    {
        return new Vector4(x.X / y,
            x.Y / y,
            x.Z / y,
            x.W / y);
    }

    public static Vector4 Cross(Vector4 vector1, Vector4 vector2)
    {
        return new Vector4(
            vector1.Y * vector2.Z - vector1.Z * vector2.Y,
            vector1.Z * vector2.X - vector1.X * vector2.Z,
            vector1.X * vector2.Y - vector1.Y * vector2.X,
            vector1.W * vector2.W);
    }

    public static float Dot(Vector4 vector1, Vector4 vector2)
    {
        return (vector1.X * vector2.X)
                 + (vector1.Y * vector2.Y)
                 + (vector1.Z * vector2.Z)
                 + (vector1.W * vector2.W);
    }

    public static Vector4 Transform(Vector4 vector, Matrix4x4 matrix)
    {
        return new Vector4(
            (vector.X * matrix.X_Axis.X) + (vector.Y * matrix.Y_Axis.X) + (vector.Z * matrix.Z_Axis.X) + (vector.W * matrix.W_Axis.X),
            (vector.X * matrix.X_Axis.Y) + (vector.Y * matrix.Y_Axis.Y) + (vector.Z * matrix.Z_Axis.Y) + (vector.W * matrix.W_Axis.Y),
            (vector.X * matrix.X_Axis.Z) + (vector.Y * matrix.Y_Axis.Z) + (vector.Z * matrix.Z_Axis.Z) + (vector.W * matrix.W_Axis.Z),
            (vector.X * matrix.X_Axis.W) + (vector.Y * matrix.Y_Axis.W) + (vector.Z * matrix.Z_Axis.W) + (vector.W * matrix.W_Axis.W)
        );
    }

    public static Vector4 QuaternionMultiply(Vector4 q1, Vector4 q2)
    {
        Vector4 ret = new()
        {
            X = q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y + q1.W * q2.X,
            Y = -q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X + q1.W * q2.Y,
            Z = q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W + q1.W * q2.Z,
            W = -q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z + q1.W * q2.W
        };
        return ret;
    }

    /// euler degrees
    /// From https://github.com/OwlGamingCommunity/V/blob/492d0cb3e89a97112ac39bf88de39da57a3a1fbf/Source/owl_core/Server/MapLoader.cs
    public static Vector3 QuaternionToEulerAngles(Vector4 q)
    {
        Vector3 retVal = new();

        // roll (x-axis rotation)
        double sinr_cosp = +2.0 * (q.W * q.X + q.Y * q.Z);
        double cosr_cosp = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
        retVal.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        double sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
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
        double siny_cosp = +2.0 * (q.W * q.Z + q.X * q.Y);
        double cosy_cosp = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
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

    /// euler in radians
    public static Vector3 ConsiderQuatToEulerConvert(Vector4 v4N)
    {
        // shadowkeep and below don't have quaternion normals
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        {
            return new Vector3(v4N.X, v4N.Y, v4N.Z);
        }
        Vector3 res = new();
        if (Math.Abs(v4N.Magnitude - 1) < 0.01)  // Quaternion
        {
            var quat = new SharpDX.Quaternion(v4N.X, v4N.Y, v4N.Z, v4N.W);
            var a = new SharpDX.Vector3(1, 0, 0);
            var result = SharpDX.Vector3.Transform(a, quat);
            res.X = result.X;
            res.Y = result.Y;
            res.Z = result.Z;
        }
        else
        {
            res.X = v4N.X;
            res.Y = v4N.Y;
            res.Z = v4N.Z;
        }
        return res;
    }

    public override string ToString() =>
    $"({FormatFloat(X)}, {FormatFloat(Y)}, {FormatFloat(Z)}, {FormatFloat(W)})";

    private static string FormatFloat(float value)
    {
        if (float.IsInfinity(value) || float.IsNaN(value))
            return value.ToString(); // This will return "Infinity", "-Infinity", or "NaN"

        return Decimal.Parse(value.ToString(), NumberStyles.Float).ToString();
    }

    public bool IsZero()
    {
        return X == 0 && Y == 0 && Z == 0 && W == 0;
    }

    public float Length()
    {
        return MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
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
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new IndexOutOfRangeException(),
            };
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

//[StructLayout(LayoutKind.Sequential, Size = 6)]
//public struct UShortVector3
//{
//    public ushort X;
//    public ushort Y;
//    public ushort Z;

//    public UShortVector3(ushort x, ushort y, ushort z)
//    {
//        X = x;
//        Y = y;
//        Z = z;
//    }
//}
