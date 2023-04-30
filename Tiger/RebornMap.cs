﻿// using System.Runtime.InteropServices;
// using Tiger.Attributes;
//
// namespace Tiger.Maps
// {
//     [StructLayout(LayoutKind.Sequential, Size = 0xC0)]
//     public struct D2S_AD938080
//     {
//         public long FileSize;
//         [DestinyOffset(0x18), DestinyField(FieldType.TagHash)]
//         public Tag<D2Class_B1938080> ModelOcclusionBounds;
//         [DestinyOffset(0x40), DestinyField(FieldType.TablePointer)]
//         public List<D2Class_406D8080> Instances;
//         [DestinyField(FieldType.TablePointer)]
//         public List<D2Class_0B008080> Unk50;
//         [DestinyOffset(0x78), DestinyField(FieldType.TablePointer)]
//         public List<D2Class_BD938080> Statics;
//         [DestinyField(FieldType.TablePointer)]
//         public List<D2Class_286D8080> InstanceCounts;
//         [DestinyOffset(0x98)]
//         public DestinyHash Unk98;
//         [DestinyOffset(0xA0)]
//         public Vector4 UnkA0; // likely a bound corner
//         public Vector4 UnkB0; // likely the other bound corner
//     }
//     
//     public class RebornStaticMap : TigerTag<D2S_AD938080>
//     {
//         protected RebornStaticMap() {}
//         ~RebornStaticMap() {}
//         
//     }
// }
//
// namespace Tiger.Maps.Destiny2_Latest
// {
//     public class RebornStaticMap : Maps.RebornStaticMap
//     {
//         
//     }
// }
