﻿using System.Text;
using Field.General;
using Field.Models;

namespace Field.Entities;

/// <summary>
/// EntityControlRig stores the control rig (IK) skeletal data.
/// Format:
/// - each item is a node
/// - if a node has a parent to which it inherits, the value at UnkB8.Unk04/Unk0A are the parent node index
/// - if the value is instead its own index, its parent is pedestal (ie no parent)
/// </summary>
public class EntityControlRig : EntityResource
{
    public EntityControlRig(TagHash resource) : base(resource)
    {
        // WriteDebugObj();
    }

    private void WriteDebugObj()
    {
        StringBuilder sb = new StringBuilder();
        D2Class_5F8B8080 skelInfo = (D2Class_5F8B8080)Header.Unk18;
        var ikTransforms = skelInfo.UnkC8;
        var ikDescriptors = skelInfo.UnkB8;
        for (int i = 0; i < ikDescriptors.Count; i++)
        {
            string name = ikDescriptors[i].Unk00.ToString();
            // if (!(name.Contains("thigh") || name.Contains("thigh"))) continue;;
            var rotation = ikTransforms[i].Rotation;
            var translation = ikTransforms[i].Translation.ToVec3();
            // if parent id == 1, we actually set it to == 2
            int parentIndex = ikDescriptors[i].Unk04;
            if (parentIndex == 1) parentIndex = 2;
            if (parentIndex != i && parentIndex != -1) // if it has a parent
            {
                // account for the parent's translation
                // rotate the parent's translation
                Vector3 parentTranslation = ikTransforms[parentIndex].Translation.ToVec3();
                var quat = new SharpDX.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
                var a = new SharpDX.Vector3(parentTranslation.X, parentTranslation.Y, parentTranslation.Z);
                var result = SharpDX.Vector3.Transform(a, quat);
                translation.X += result.X;
                translation.Y += result.Y;
                translation.Z += result.Z;
            }
            sb.AppendLine($"v {translation.X} {translation.Y} {translation.Z}");
        }
        System.IO.File.WriteAllText("C:/T/test_control_rig.obj", sb.ToString());
    }
}
