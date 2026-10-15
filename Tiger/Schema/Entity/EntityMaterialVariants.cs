using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

public class EntityMaterialVariants
{
    public List<SExternalMaterialMapEntry> MaterialMap;
    public List<short> ConditionParts;
    public List<S80806D98> Conditions;
    public List<Material?> Materials;
    public int OverrideIndex = -1;

    public int Max => MaterialMap.Count == 0 ? 0 : MaterialMap.Max(x => x.MaterialCount);

    public EntityMaterialVariants(TigerReader reader, S80806D8F resource) // normal model
    {
        MaterialMap = resource.ExternalMaterialsMap.Enumerate(reader).ToList();
        ConditionParts = resource.MaterialConditionParts.Enumerate(reader).Select(x => x.Value).ToList();
        Conditions = resource.MaterialConditions.Enumerate(reader).ToList();
        Materials = resource.ExternalMaterials.Enumerate(reader).Select(x => (Material?)x.Material).ToList();
    }

    public EntityMaterialVariants(TigerReader reader, S80806D6C resource) // physics model resource
    {
        MaterialMap = resource.ExternalMaterialsMap.Enumerate(reader).ToList();
        ConditionParts = resource.MaterialConditionParts.Enumerate(reader).Select(x => x.Value).ToList();
        Conditions = resource.MaterialConditions.Enumerate(reader).ToList();
        Materials = resource.ExternalMaterials.Enumerate(reader).Select(x => (Material?)x.Material).ToList();
    }

    public Material? GetMaterial(int groupIndex, Func<int, bool>? isEnabled)
    {
        if (groupIndex < 0 || groupIndex >= MaterialMap.Count)
            return null;

        var entry = MaterialMap[groupIndex];
        if (entry.MaterialCount <= 0 || entry.MaterialStartIndex < 0 || entry.MaterialStartIndex + entry.MaterialCount > Materials.Count)
            return null;

        if (OverrideIndex != -1)
            return Materials[entry.MaterialStartIndex + (OverrideIndex % entry.MaterialCount)];

        for (int i = 0; i < entry.MaterialCount; i++)
        {
            var material = Materials[entry.MaterialStartIndex + i];
            if (material is null || !material.Hash.IsValid())
                continue;

            int conditionIndex = entry.FirstCondition + i;
            if (conditionIndex < 0 || conditionIndex >= Conditions.Count)
                continue;

            var condition = Conditions[conditionIndex];
            if (condition.RequiredCount <= 0 && condition.ExcludedCount <= 0)
                continue;

            if (TestParts(condition.RequiredCount, condition.RequiredIndex, true, isEnabled) && TestParts(condition.ExcludedCount, condition.ExcludedIndex, false, isEnabled))
                return material;
        }

        return Materials[entry.MaterialStartIndex + (0 % entry.MaterialCount)];
    }

    private bool TestParts(int count, int index, bool enabled, Func<int, bool>? isEnabled)
    {
        if (count <= 0)
            return true;

        if (count > 1 && (index < 0 || index + count > ConditionParts.Count))
            return false;

        for (int i = 0; i < count; i++)
        {
            var part = GetPart(count, index, i);
            bool partEnabled = isEnabled == null || isEnabled(part);
            if (partEnabled != enabled)
                return false;
        }

        return true;
    }

    private int GetPart(int count, int index, int i)
    {
        return count == 1 ? index : ConditionParts[index + i];
    }
}
