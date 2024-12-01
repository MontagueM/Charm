using System.Runtime.InteropServices;

namespace Tiger.Schema;

public class DirectXSampler : TigerReferenceFile<SSamplerHeader>
{
    public D3D11_SAMPLER_DESC Sampler => GetSampler();

    public DirectXSampler(FileHash hash) : base(hash)
    {
    }

    private D3D11_SAMPLER_DESC GetSampler()
    {
        using TigerReader reader = GetReferenceReader();
        return reader.ReadType<D3D11_SAMPLER_DESC>();
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x34)]
    public struct D3D11_SAMPLER_DESC
    {
        public D3D11_FILTER Filter;
        public D3D11_TEXTURE_ADDRESS_MODE AddressU;
        public D3D11_TEXTURE_ADDRESS_MODE AddressV;
        public D3D11_TEXTURE_ADDRESS_MODE AddressW;
        public float MipLODBias;
        public uint MaxAnisotropy;
        public D3D11_COMPARISON_FUNC ComparisonFunc;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] BorderColor;
        public float MinLOD;
        public float MaxLOD;
    }

    public enum D3D11_FILTER
    {
        MIN_MAG_MIP_POINT = 0x0,
        MIN_MAG_POINT_MIP_LINEAR = 0x1,
        MIN_POINT_MAG_LINEAR_MIP_POINT = 0x4,
        MIN_POINT_MAG_MIP_LINEAR = 0x5,
        MIN_LINEAR_MAG_MIP_POINT = 0x10,
        MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x11,
        MIN_MAG_LINEAR_MIP_POINT = 0x14,
        MIN_MAG_MIP_LINEAR = 0x15,
        ANISOTROPIC = 0x55,
        COMPARISON_MIN_MAG_MIP_POINT = 0x80,
        COMPARISON_MIN_MAG_POINT_MIP_LINEAR = 0x81,
        COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x84,
        COMPARISON_MIN_POINT_MAG_MIP_LINEAR = 0x85,
        COMPARISON_MIN_LINEAR_MAG_MIP_POINT = 0x90,
        COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x91,
        COMPARISON_MIN_MAG_LINEAR_MIP_POINT = 0x94,
        COMPARISON_MIN_MAG_MIP_LINEAR = 0x95,
        COMPARISON_ANISOTROPIC = 0xd5,
        MINIMUM_MIN_MAG_MIP_POINT = 0x100,
        MINIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x101,
        MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x104,
        MINIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x105,
        MINIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x110,
        MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x111,
        MINIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x114,
        MINIMUM_MIN_MAG_MIP_LINEAR = 0x115,
        MINIMUM_ANISOTROPIC = 0x155,
        MAXIMUM_MIN_MAG_MIP_POINT = 0x180,
        MAXIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x181,
        MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x184,
        MAXIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x185,
        MAXIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x190,
        MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x191,
        MAXIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x194,
        MAXIMUM_MIN_MAG_MIP_LINEAR = 0x195,
        MAXIMUM_ANISOTROPIC = 0x1d5
    }

    public enum D3D11_TEXTURE_ADDRESS_MODE
    {
        WRAP = 0x1,
        MIRROR = 0x2,
        CLAMP = 0x3,
        BORDER = 0x4,
        MIRROR_ONCE = 0x5
    }

    public enum D3D11_COMPARISON_FUNC
    {
        NEVER = 0x1,
        LESS = 0x2,
        EQUAL = 0x3,
        LESS_EQUAL = 0x4,
        GREATER = 0x5,
        NOT_EQUAL = 0x6,
        GREATER_EQUAL = 0x7,
        ALWAYS = 0x8
    }
}

[NonSchemaStruct(0x08)]
public struct SSamplerHeader //Header
{
    public ulong Unk00; //Nothing
}
