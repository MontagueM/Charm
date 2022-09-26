namespace Field.Utils;

public static class DirectX {

    public static string DisassembleDXBC(byte[] input) {
        return new SharpDX.D3DCompiler.ShaderBytecode(input).Disassemble();
    }
}