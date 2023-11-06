import bpy
import os

RIP_LOCATION = None
#Detail Texture Position and Scale
armordetaildiffuseposition = (DiffTrans1.Z, DiffTrans1.W, 0.000)
armordetaildiffusescale = (DiffTrans1.X, DiffTrans1.Y, 0.000)
armordetailnormalposition = (NormTrans1.Z, NormTrans1.W, 0.000)
armordetailnormalscale = (NormTrans1.X, NormTrans1.Y, 0.000)
clothdetaildiffuseposition = (DiffTrans2.Z, DiffTrans2.W, 0.000)
clothdetaildiffusescale = (DiffTrans2.X, DiffTrans2.Y, 0.000)
clothdetailnormalposition = (NormTrans2.Z, NormTrans2.W, 0.000)
clothdetailnormalscale = (NormTrans2.X, NormTrans2.Y, 0.000)
suitdetaildiffuseposition = (DiffTrans3.Z, DiffTrans3.W, 0.000)
suitdetaildiffusescale = (DiffTrans3.X, DiffTrans3.Y, 0.000)
suitdetailnormalposition = (NormTrans3.Z, NormTrans3.W, 0.000)
suitdetailnormalscale = (NormTrans3.X, NormTrans3.Y, 0.000)

#Armor Primary Slot
armorprimarydyecolor = (CPrime1.X, CPrime1.Y, CPrime1.Z, 1.0)
armorprimaryroughnessremapX = PrimeRoughMap1.X
armorprimaryroughnessremapY = PrimeRoughMap1.Y
armorprimaryroughnessremapZ = PrimeRoughMap1.Z
armorprimaryroughnessremapW = PrimeRoughMap1.W
armorprimarywearremapX = PrimeWearMap1.X
armorprimarywearremapY = PrimeWearMap1.Y
armorprimarywearremapZ = PrimeWearMap1.Z
armorprimarywearremapW = PrimeWearMap1.W
armorprimarydetaildiffuseblend = PrimeMatParams1.X
armorprimarydetailnormalblend = PrimeMatParams1.Y
armorprimarydetailroughnessblend = PrimeMatParams1.Z
armorprimarymetalness = PrimeMatParams1.W
armorprimaryiridescence = PrimeAdvMatParams1.X
armorprimaryfuzz = PrimeAdvMatParams1.Y
armorprimarytransmission = PrimeAdvMatParams1.Z
armorprimaryemissioncolor =  (CPrimeEmit1.X, CPrimeEmit1.Y, CPrimeEmit1.Z, 1.0)
#Worn Armor Primary Slot
wornarmorprimarydyecolor = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z, 1.0)
wornarmorprimaryroughnessremapX = PrimeWornRoughMap1.X
wornarmorprimaryroughnessremapY = PrimeWornRoughMap1.Y
wornarmorprimaryroughnessremapZ = PrimeWornRoughMap1.Z
wornarmorprimaryroughnessremapW = PrimeWornRoughMap1.W
wornarmorprimarydetaildiffuseblend = PrimeWornMatParams1.X
wornarmorprimarydetailnormalblend = PrimeWornMatParams1.Y
wornarmorprimarydetailroughnessblend = PrimeWornMatParams1.Z
wornarmorprimarymetalness = PrimeWornMatParams1.W

#Armor Secondary Slot
armorsecondarydyecolor = (CSecon1.X, CSecon1.Y, CSecon1.Z, 1.0)
armorsecondaryroughnessremapX = SeconRoughMap1.X
armorsecondaryroughnessremapY = SeconRoughMap1.Y
armorsecondaryroughnessremapZ = SeconRoughMap1.Z
armorsecondaryroughnessremapW = SeconRoughMap1.W
armorsecondarywearremapX = SeconWearMap1.X
armorsecondarywearremapY = SeconWearMap1.Y
armorsecondarywearremapZ = SeconWearMap1.Z
armorsecondarywearremapW = SeconWearMap1.W
armorsecondarydetaildiffuseblend = SeconMatParams1.X
armorsecondarydetailnormalblend = SeconMatParams1.Y
armorsecondarydetailroughnessblend = SeconMatParams1.Z
armorsecondarymetalness = SeconMatParams1.W
armorsecondaryiridescence = SeconAdvMatParams1.X
armorsecondaryfuzz = SeconAdvMatParams1.Y
armorsecondarytransmission = SeconAdvMatParams1.Z
armorsecondaryemissioncolor = (CSeconEmit1.X, CSeconEmit1.Y, CSeconEmit1.Z, 1.0)
#Worn Armor Secondary Slot
wornarmorsecondarydyecolor = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z, 1.0)
wornarmorsecondaryroughnessremapX = SeconWornRoughMap1.X
wornarmorsecondaryroughnessremapY = SeconWornRoughMap1.Y
wornarmorsecondaryroughnessremapZ = SeconWornRoughMap1.Z
wornarmorsecondaryroughnessremapW = SeconWornRoughMap1.W
wornarmorsecondarydetaildiffuseblend = SeconWornMatParams1.X
wornarmorsecondarydetailnormalblend = SeconWornMatParams1.Y
wornarmorsecondarydetailroughnessblend = SeconWornMatParams1.Z
wornarmorsecondarymetalness = SeconWornMatParams1.W

#Cloth Primary Slot
clothprimarydyecolor = (CPrime2.X, CPrime2.Y, CPrime2.Z, 1.0)
clothprimaryroughnessremapX = PrimeRoughMap2.X
clothprimaryroughnessremapY = PrimeRoughMap2.Y
clothprimaryroughnessremapZ = PrimeRoughMap2.Z
clothprimaryroughnessremapW = PrimeRoughMap2.W
clothprimarywearremapX = PrimeWearMap2.X
clothprimarywearremapY = PrimeWearMap2.Y
clothprimarywearremapZ = PrimeWearMap2.Z
clothprimarywearremapW = PrimeWearMap2.W
clothprimarydetaildiffuseblend = PrimeMatParams2.X
clothprimarydetailnormalblend = PrimeMatParams2.Y
clothprimarydetailroughnessblend = PrimeMatParams2.Z
clothprimarymetalness = PrimeMatParams2.W
clothprimaryiridescence = PrimeAdvMatParams2.X
clothprimaryfuzz = PrimeAdvMatParams2.Y
clothprimarytransmission = PrimeAdvMatParams2.Z
clothprimaryemissioncolor = (CPrimeEmit2.X, CPrimeEmit2.Y, CPrimeEmit2.Z, 1.0)
#Worn Cloth Primary Slot
wornclothprimarydyecolor = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z, 1.0)
wornclothprimaryroughnessremapX = PrimeWornRoughMap2.X
wornclothprimaryroughnessremapY = PrimeWornRoughMap2.Y
wornclothprimaryroughnessremapZ = PrimeWornRoughMap2.Z
wornclothprimaryroughnessremapW = PrimeWornRoughMap2.W
wornclothprimarydetaildiffuseblend = PrimeWornMatParams1.X
wornclothprimarydetailnormalblend = PrimeWornMatParams1.Y
wornclothprimarydetailroughnessblend = PrimeWornMatParams1.Z
wornclothprimarymetalness = PrimeWornMatParams1.W

#Cloth secondary Slot
clothsecondarydyecolor = (CSecon2.X, CSecon2.Y, CSecon2.Z, 1.0)
clothsecondaryroughnessremapX = SeconRoughMap2.X
clothsecondaryroughnessremapY = SeconRoughMap2.Y
clothsecondaryroughnessremapZ = SeconRoughMap2.Z
clothsecondaryroughnessremapW = SeconRoughMap2.W
clothsecondarywearremapX = SeconWearMap2.X
clothsecondarywearremapY = SeconWearMap2.Y
clothsecondarywearremapZ = SeconWearMap2.Z
clothsecondarywearremapW = SeconWearMap2.W
clothsecondarydetaildiffuseblend = SeconMatParams2.X
clothsecondarydetailnormalblend = SeconMatParams2.Y
clothsecondarydetailroughnessblend = SeconMatParams2.Z
clothsecondarymetalness = SeconMatParams2.W
clothsecondaryiridescence = SeconAdvMatParams2.X
clothsecondaryfuzz = SeconAdvMatParams2.Y
clothsecondarytransmission = SeconAdvMatParams2.Z
clothsecondaryemissioncolor = (CSeconEmit2.X, CSeconEmit2.Y, CSeconEmit2.Z, 1.0)
#Worn Cloth secondary Slot
wornclothsecondarydyecolor = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z, 1.0)
wornclothsecondaryroughnessremapX = SeconWornRoughMap2.X
wornclothsecondaryroughnessremapY = SeconWornRoughMap2.Y
wornclothsecondaryroughnessremapZ = SeconWornRoughMap2.Z
wornclothsecondaryroughnessremapW = SeconWornRoughMap2.W
wornclothsecondarydetaildiffuseblend = SeconWornMatParams2.X
wornclothsecondarydetailnormalblend = SeconWornMatParams2.Y
wornclothsecondarydetailroughnessblend = SeconWornMatParams2.Z
wornclothsecondarymetalness = SeconWornMatParams2.W

#Suit Primary Slot
suitprimarydyecolor = (CPrime3.X, CPrime3.Y, CPrime3.Z, 1.0)
suitprimaryroughnessremapX = PrimeRoughMap3.X
suitprimaryroughnessremapY = PrimeRoughMap3.Y
suitprimaryroughnessremapZ = PrimeRoughMap3.Z
suitprimaryroughnessremapW = PrimeRoughMap3.W
suitprimarywearremapX = PrimeWearMap3.X
suitprimarywearremapY = PrimeWearMap3.Y
suitprimarywearremapZ = PrimeWearMap3.Z
suitprimarywearremapW = PrimeWearMap3.W
suitprimarydetaildiffuseblend = PrimeMatParams3.X
suitprimarydetailnormalblend = PrimeMatParams3.Y
suitprimarydetailroughnessblend = PrimeMatParams3.Z
suitprimarymetalness = PrimeMatParams3.W
suitprimaryiridescence = PrimeAdvMatParams3.X
suitprimaryfuzz = PrimeAdvMatParams3.Y
suitprimarytransmission = PrimeAdvMatParams3.Z
suitprimaryemissioncolor =  (CPrimeEmit3.X, CPrimeEmit3.Y, CPrimeEmit3.Z, 1.0)
#Worn Suit Primary Slot
wornsuitprimarydyecolor = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z, 1.0)
wornsuitprimaryroughnessremapX = PrimeWornRoughMap3.X
wornsuitprimaryroughnessremapY = PrimeWornRoughMap3.Y
wornsuitprimaryroughnessremapZ = PrimeWornRoughMap3.Z
wornsuitprimaryroughnessremapW = PrimeWornRoughMap3.W
wornsuitprimarydetaildiffuseblend = PrimeWornMatParams3.X
wornsuitprimarydetailnormalblend = PrimeWornMatParams3.Y
wornsuitprimarydetailroughnessblend = PrimeWornMatParams3.Z
wornsuitprimarymetalness = PrimeWornMatParams3.W

#Suit Secondary Slot
suitsecondarydyecolor = (CSecon3.X, CSecon3.Y, CSecon3.Z, 1.0)
suitsecondaryroughnessremapX = SeconRoughMap3.X
suitsecondaryroughnessremapY = SeconRoughMap3.Y
suitsecondaryroughnessremapZ = SeconRoughMap3.Z
suitsecondaryroughnessremapW = SeconRoughMap3.W
suitsecondarywearremapX = SeconWearMap3.X
suitsecondarywearremapY = SeconWearMap3.Y
suitsecondarywearremapZ = SeconWearMap3.Z
suitsecondarywearremapW = SeconWearMap3.W
suitsecondarydetaildiffuseblend = SeconMatParams3.X
suitsecondarydetailnormalblend = SeconMatParams3.Y
suitsecondarydetailroughnessblend = SeconMatParams3.Z
suitsecondarymetalness = SeconMatParams3.W
suitsecondaryiridescence = SeconAdvMatParams3.X
suitsecondaryfuzz = SeconAdvMatParams3.Y
suitsecondarytransmission = SeconAdvMatParams3.Z
suitsecondaryemissioncolor = (CSeconEmit3.X, CSeconEmit3.Y, CSeconEmit3.Z, 1.0)
#Worn Suit Secondary Slot
wornsuitsecondarydyecolor = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z, 1.0)
wornsuitsecondaryroughnessremapX = SeconWornRoughMap3.X
wornsuitsecondaryroughnessremapY = SeconWornRoughMap3.Y
wornsuitsecondaryroughnessremapZ = SeconWornRoughMap3.Z
wornsuitsecondaryroughnessremapW = SeconWornRoughMap3.W
wornsuitsecondarydetaildiffuseblend = SeconWornMatParams3.X
wornsuitsecondarydetailnormalblend = SeconWornMatParams3.Y
wornsuitsecondarydetailroughnessblend = SeconWornMatParams3.Z
wornsuitsecondarymetalness = SeconWornMatParams3.W
#########################################################

class MAINPANEL(bpy.types.Panel):
    bl_label = "D2 Shader Preset"
    bl_idname = "MAINPANEL"
    bl_space_type = 'NODE_EDITOR'
    bl_region_type = 'UI'
    bl_category = 'D2 Shader'

    def draw(self, context):
        layout = self.layout

        row = layout.row()
        row.operator('node.test_operator')

def create_Shader_Preset(context, operator, group_name, riplocation):

    bpy.context.scene.use_nodes = True
    
    Shader_Preset = bpy.data.node_groups.new(group_name, 'ShaderNodeTree')
#Nodegroup Inputs
    Shader_Preset.inputs.new('NodeSocketColor', 'Dyemap Color')
    Shader_Preset.inputs.new('NodeSocketFloat', 'Dyemap Alpha')
    Shader_Preset.inputs.new('NodeSocketFloat', 'Slot Override [1-6]')
#Nodegroup Outputs
    Shader_Preset.outputs.new('NodeSocketColor', 'Dye Color A')
    Shader_Preset.outputs.new('NodeSocketColor', 'Dye Color B')
    Shader_Preset.outputs.new('NodeSocketColor', 'Wear Remap_A')
    Shader_Preset.outputs.new('NodeSocketColor', 'Wear Remap_B')
    Shader_Preset.outputs.new('NodeSocketColor', 'Roughness Remap_A')
    Shader_Preset.outputs.new('NodeSocketColor', 'Roughness Remap_B')
    Shader_Preset.outputs.new('NodeSocketColor', 'Roughness Remap_C')
    Shader_Preset.outputs.new('NodeSocketColor', 'Detail Diffuse')
    Shader_Preset.outputs.new('NodeSocketColor', 'Detail Normal')
    Shader_Preset.outputs.new('NodeSocketColor', 'Detail Blends')
    Shader_Preset.outputs.new('NodeSocketColor', 'Worn Detail Blends')
    Shader_Preset.outputs.new('NodeSocketColor', 'Iridescence, Fuzz, Transmission')
    Shader_Preset.outputs.new('NodeSocketColor', 'Emission')
#Frames
    ArmorPrimaryFrame = Shader_Preset.nodes.new("NodeFrame")
    ArmorPrimaryFrame.label = "Armor Primary"
    ArmorPrimaryFrame.use_custom_color = True
    ArmorPrimaryFrame.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)

    WornArmorPrimaryFrame = Shader_Preset.nodes.new("NodeFrame")
    WornArmorPrimaryFrame.label = "Worn Armor Primary"
    WornArmorPrimaryFrame.use_custom_color = True
    WornArmorPrimaryFrame.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)

    ArmorSecondaryFrame = Shader_Preset.nodes.new("NodeFrame")
    ArmorSecondaryFrame.label = "Armor Secondary"
    ArmorSecondaryFrame.use_custom_color = True
    ArmorSecondaryFrame.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)

    WornArmorSecondaryFrame = Shader_Preset.nodes.new("NodeFrame")
    WornArmorSecondaryFrame.label = "Worn Armor Secondary"
    WornArmorSecondaryFrame.use_custom_color = True
    WornArmorSecondaryFrame.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    
    ClothPrimaryFrame = Shader_Preset.nodes.new("NodeFrame")
    ClothPrimaryFrame.label = "Cloth Primary"
    ClothPrimaryFrame.use_custom_color = True
    ClothPrimaryFrame.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)

    WornClothPrimaryFrame = Shader_Preset.nodes.new("NodeFrame")
    WornClothPrimaryFrame.label = "Worn Cloth Primary"
    WornClothPrimaryFrame.use_custom_color = True
    WornClothPrimaryFrame.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    
    ClothSecondaryFrame = Shader_Preset.nodes.new("NodeFrame")
    ClothSecondaryFrame.label = "Cloth Secondary"
    ClothSecondaryFrame.use_custom_color = True
    ClothSecondaryFrame.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    
    WornClothSecondaryFrame = Shader_Preset.nodes.new("NodeFrame")
    WornClothSecondaryFrame.label = "Worn Cloth Secondary"
    WornClothSecondaryFrame.use_custom_color = True
    WornClothSecondaryFrame.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    
    SuitPrimaryFrame = Shader_Preset.nodes.new("NodeFrame")
    SuitPrimaryFrame.label = "Suit Primary"
    SuitPrimaryFrame.use_custom_color = True
    SuitPrimaryFrame.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    
    WornSuitPrimaryFrame = Shader_Preset.nodes.new("NodeFrame")
    WornSuitPrimaryFrame.label = "Worn Suit Primary"
    WornSuitPrimaryFrame.use_custom_color = True
    WornSuitPrimaryFrame.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    
    SuitSecondaryFrame = Shader_Preset.nodes.new("NodeFrame")
    SuitSecondaryFrame.label = "Suit Secondary"
    SuitSecondaryFrame.use_custom_color = True
    SuitSecondaryFrame.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)

    WornSuitSecondaryFrame = Shader_Preset.nodes.new("NodeFrame")
    WornSuitSecondaryFrame.label = "Worn Suit Secondary"
    WornSuitSecondaryFrame.use_custom_color = True
    WornSuitSecondaryFrame.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)

    DoNotTouchFrame = Shader_Preset.nodes.new("NodeFrame")
    DoNotTouchFrame.label = "DO NOT TOUCH!"
    DoNotTouchFrame.use_custom_color = True
    DoNotTouchFrame.color = (0.0, 0.0, 0.0)
#Texture nodes
    armor_primary_detail_diffuse_map = Shader_Preset.nodes.new("ShaderNodeTexImage")
    armor_primary_detail_diffuse_map.label = "Detail Diffuse Map"
    armor_primary_detail_diffuse_map.use_custom_color = True
    armor_primary_detail_diffuse_map.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_detail_diffuse_map.interpolation = 'Linear'
    armor_primary_detail_diffuse_map.projection = 'FLAT'
    armor_primary_detail_diffuse_map.extension = 'REPEAT'
    armor_primary_detail_diffuse_map.hide = True

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/DiffMap1"), check_existing=False)
    DetailDiffuse01 = bpy.data.images.get("DiffMap1")
    DetailDiffuse01.colorspace_settings.name = "sRGB"
    DetailDiffuse01.alpha_mode = "CHANNEL_PACKED"
    armor_primary_detail_diffuse_map.image = DetailDiffuse01

    armor_primary_detail_normal_map = Shader_Preset.nodes.new("ShaderNodeTexImage")
    armor_primary_detail_normal_map.label = "Detail Normal Map"
    armor_primary_detail_normal_map.use_custom_color = True
    armor_primary_detail_normal_map.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_detail_normal_map.interpolation = 'Linear'
    armor_primary_detail_normal_map.projection = 'FLAT'
    armor_primary_detail_normal_map.extension = 'REPEAT'
    armor_primary_detail_normal_map.hide = True
    
    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/NormMap1"), check_existing=False)
    DetailNormal01 = bpy.data.images.get("NormMap1")
    DetailNormal01.colorspace_settings.name = "Non-Color"
    DetailNormal01.alpha_mode = "CHANNEL_PACKED"
    armor_primary_detail_normal_map.image = DetailNormal01

    cloth_primary_detail_diffuse_map = Shader_Preset.nodes.new("ShaderNodeTexImage")
    cloth_primary_detail_diffuse_map.label = "Detail Diffuse Map"
    cloth_primary_detail_diffuse_map.use_custom_color = True
    cloth_primary_detail_diffuse_map.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_detail_diffuse_map.interpolation = 'Linear'
    cloth_primary_detail_diffuse_map.projection = 'FLAT'
    cloth_primary_detail_diffuse_map.extension = 'REPEAT'
    cloth_primary_detail_diffuse_map.hide = True

    bpy.data.images.load(os.path.join(riplocation,"OUTPUTPATH/DiffMap2"), check_existing=False)
    DetailDiffuse02 = bpy.data.images.get("DiffMap2")
    DetailDiffuse02.colorspace_settings.name = "sRGB"
    DetailDiffuse02.alpha_mode = "CHANNEL_PACKED"
    cloth_primary_detail_diffuse_map.image = DetailDiffuse02

    cloth_primary_detail_normal_map = Shader_Preset.nodes.new("ShaderNodeTexImage")
    cloth_primary_detail_normal_map.label = "Detail Normal Map"
    cloth_primary_detail_normal_map.use_custom_color = True
    cloth_primary_detail_normal_map.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_detail_normal_map.interpolation = 'Linear'
    cloth_primary_detail_normal_map.projection = 'FLAT'
    cloth_primary_detail_normal_map.extension = 'REPEAT'
    cloth_primary_detail_normal_map.hide = True

    bpy.data.images.load(os.path.join(riplocation,"OUTPUTPATH/NormMap2"), check_existing=False)
    DetailNormal02 = bpy.data.images.get("NormMap2")
    DetailNormal02.colorspace_settings.name = "Non-Color"
    DetailNormal02.alpha_mode = "CHANNEL_PACKED"
    cloth_primary_detail_normal_map.image = DetailNormal02
    
    suit_primary_detail_diffuse_map = Shader_Preset.nodes.new("ShaderNodeTexImage")
    suit_primary_detail_diffuse_map.label = "Detail Diffuse Map"
    suit_primary_detail_diffuse_map.use_custom_color = True
    suit_primary_detail_diffuse_map.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_detail_diffuse_map.interpolation = 'Linear'
    suit_primary_detail_diffuse_map.projection = 'FLAT'
    suit_primary_detail_diffuse_map.extension = 'REPEAT'
    suit_primary_detail_diffuse_map.hide = True

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/DiffMap3"), check_existing=False)
    DetailDiffuse03 = bpy.data.images.get("DiffMap3")
    DetailDiffuse03.colorspace_settings.name = "sRGB"
    DetailDiffuse03.alpha_mode = "CHANNEL_PACKED"
    suit_primary_detail_diffuse_map.image = DetailDiffuse03

    suit_primary_detail_normal_map = Shader_Preset.nodes.new("ShaderNodeTexImage")
    suit_primary_detail_normal_map.label = "Detail Normal Map"
    suit_primary_detail_normal_map.use_custom_color = True
    suit_primary_detail_normal_map.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_detail_normal_map.interpolation = 'Linear'
    suit_primary_detail_normal_map.projection = 'FLAT'
    suit_primary_detail_normal_map.extension = 'REPEAT'
    suit_primary_detail_normal_map.hide = True

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/NormMap3"), check_existing=False)
    DetailNormal03 = bpy.data.images.get("NormMap3")
    DetailNormal03.colorspace_settings.name = "Non-Color"
    DetailNormal03.alpha_mode = "CHANNEL_PACKED"
    suit_primary_detail_normal_map.image = DetailNormal03

#nodes
    worn_suit_secondary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    worn_suit_secondary_detail_diffuse_blend.use_custom_color = True
    worn_suit_secondary_detail_diffuse_blend.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_detail_diffuse_blend.hide = True
    worn_suit_secondary_detail_diffuse_blend.outputs[0].default_value = wornsuitsecondarydetaildiffuseblend

    math_017 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_017.operation = 'MULTIPLY_ADD'
    math_017.use_clamp = True

    math_017.inputs[1].default_value = 100000.0

    math_017.inputs[2].default_value = -99999.0

    suit_detail_diffuse_transform = Shader_Preset.nodes.new("ShaderNodeMapping")
    suit_detail_diffuse_transform.label = "Detail Diffuse Transform"
    suit_detail_diffuse_transform.use_custom_color = True
    suit_detail_diffuse_transform.color = (0.2352934032678604, 0.2352934032678604, 0.5137253999710083)
    suit_detail_diffuse_transform.vector_type = 'POINT'
    suit_detail_diffuse_transform.inputs[2].hide = True
    suit_detail_diffuse_transform.inputs[1].default_value = suitdetaildiffuseposition
    suit_detail_diffuse_transform.inputs[2].default_value = (0.0, 0.0, 0.0)
    suit_detail_diffuse_transform.inputs[3].default_value = suitdetaildiffusescale

    reroute_004 = Shader_Preset.nodes.new("NodeReroute")
    reroute_057 = Shader_Preset.nodes.new("NodeReroute")

    mix_047 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_primary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_roughness_remap_w.label = "Roughness Remap W"
    suit_primary_roughness_remap_w.use_custom_color = True
    suit_primary_roughness_remap_w.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_roughness_remap_w.hide = True
    suit_primary_roughness_remap_w.outputs[0].default_value = suitprimaryroughnessremapW

    reroute_003 = Shader_Preset.nodes.new("NodeReroute")

    armor_secondary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_detail_roughness_blend.label = "Detail Roughness Blend"
    armor_secondary_detail_roughness_blend.use_custom_color = True
    armor_secondary_detail_roughness_blend.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_detail_roughness_blend.hide = True
    armor_secondary_detail_roughness_blend.outputs[0].default_value = armorsecondarydetailroughnessblend

    reroute_152 = Shader_Preset.nodes.new("NodeReroute")

    reroute_053 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_primary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_roughness_remap_z.label = "Roughness Remap Z"
    worn_suit_primary_roughness_remap_z.use_custom_color = True
    worn_suit_primary_roughness_remap_z.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_roughness_remap_z.hide = True
    worn_suit_primary_roughness_remap_z.outputs[0].default_value = wornsuitprimaryroughnessremapZ

    math_008 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_008.operation = 'MULTIPLY_ADD'

    math_008.inputs[1].default_value = 1000.0

    math_008.inputs[2].default_value = -666.0

    reroute_190 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_016 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_032 = Shader_Preset.nodes.new("NodeReroute")

    mix_023 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    #node Suit Primary Wear Remap Z
    suit_primary_wear_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_wear_remap_z.label = "Wear Remap Z"
    suit_primary_wear_remap_z.use_custom_color = True
    suit_primary_wear_remap_z.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_wear_remap_z.hide = True
    suit_primary_wear_remap_z.outputs[0].default_value = suitprimarywearremapZ

    worn_armor_secondary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_roughness_remap_w.label = "Roughness Remap W"
    worn_armor_secondary_roughness_remap_w.use_custom_color = True
    worn_armor_secondary_roughness_remap_w.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_roughness_remap_w.hide = True
    worn_armor_secondary_roughness_remap_w.outputs[0].default_value = wornarmorsecondaryroughnessremapW

    reroute_162 = Shader_Preset.nodes.new("NodeReroute")

    worn_cloth_primary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    worn_cloth_primary_dye_color.label = "Dye Color"
    worn_cloth_primary_dye_color.use_custom_color = True
    worn_cloth_primary_dye_color.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_dye_color.hide = True
    worn_cloth_primary_dye_color.outputs[0].default_value = wornclothprimarydyecolor

    group_input = Shader_Preset.nodes.new("NodeGroupInput")

    mix_065 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_065.inputs[2].default_value = (4.0, 4.0, 4.0, 1.0)

    reroute_023 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_017 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_suit_secondary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_roughness_remap_w.label = "Roughness Remap W"
    worn_suit_secondary_roughness_remap_w.use_custom_color = True
    worn_suit_secondary_roughness_remap_w.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_roughness_remap_w.hide = True
    worn_suit_secondary_roughness_remap_w.outputs[0].default_value = wornsuitsecondaryroughnessremapW

    reroute_158 = Shader_Preset.nodes.new("NodeReroute")

    suit_primary_iridescence = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_iridescence.label = "Iridescence"
    suit_primary_iridescence.use_custom_color = True
    suit_primary_iridescence.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_iridescence.hide = True
    suit_primary_iridescence.outputs[0].default_value = suitprimaryiridescence

    mix_005 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    math_010 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_010.operation = 'MULTIPLY'
    math_010.inputs[2].default_value = -500.0

    reroute_196 = Shader_Preset.nodes.new("NodeReroute")

    reroute_026 = Shader_Preset.nodes.new("NodeReroute")

    mix_049 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    math_024 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_024.operation = 'MULTIPLY'
    math_024.inputs[1].default_value = 0.3330000042915344
    math_024.inputs[2].default_value = 0.5

    reroute_168 = Shader_Preset.nodes.new("NodeReroute")

    mix_028 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    math_015 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_015.operation = 'MULTIPLY_ADD'
    math_015.use_clamp = True

    math_015.inputs[1].default_value = 1000000.0

    math_015.inputs[2].default_value = -500000.0

    reroute_192 = Shader_Preset.nodes.new("NodeReroute")

    reroute_076 = Shader_Preset.nodes.new("NodeReroute")

    cloth_primary_fuzz = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_fuzz.label = "Fuzz"
    cloth_primary_fuzz.use_custom_color = True
    cloth_primary_fuzz.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_fuzz.hide = True
    cloth_primary_fuzz.outputs[0].default_value = clothprimaryfuzz

    armor_primary_wear_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_wear_remap_w.label = "Wear Remap W"
    armor_primary_wear_remap_w.use_custom_color = True
    armor_primary_wear_remap_w.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_wear_remap_w.hide = True
    armor_primary_wear_remap_w.outputs[0].default_value = armorprimarywearremapW

    mix = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_187 = Shader_Preset.nodes.new("NodeReroute")

    reroute_169 = Shader_Preset.nodes.new("NodeReroute")

    reroute_138 = Shader_Preset.nodes.new("NodeReroute")

    reroute_071 = Shader_Preset.nodes.new("NodeReroute")

    reroute_033 = Shader_Preset.nodes.new("NodeReroute")

    reroute_157 = Shader_Preset.nodes.new("NodeReroute")

    reroute_054 = Shader_Preset.nodes.new("NodeReroute")

    mix_059 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    worn_armor_secondary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_roughness_remap_z.label = "Roughness Remap Z"
    worn_armor_secondary_roughness_remap_z.use_custom_color = True
    worn_armor_secondary_roughness_remap_z.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_roughness_remap_z.hide = True
    worn_armor_secondary_roughness_remap_z.outputs[0].default_value = wornarmorsecondaryroughnessremapZ

    armor_secondary_transmission = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_transmission.label = "Transmission"
    armor_secondary_transmission.use_custom_color = True
    armor_secondary_transmission.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_transmission.hide = True
    armor_secondary_transmission.outputs[0].default_value = armorsecondarytransmission

    reroute_141 = Shader_Preset.nodes.new("NodeReroute")

    reroute_142 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_043 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    math_001 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_001.operation = 'MULTIPLY_ADD'
    math_001.use_clamp = True

    math_001.inputs[1].default_value = 1000.0

    math_001.inputs[2].default_value = -666.0

    cloth_secondary_fuzz = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_fuzz.label = "Fuzz"
    cloth_secondary_fuzz.use_custom_color = True
    cloth_secondary_fuzz.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_fuzz.hide = True
    cloth_secondary_fuzz.outputs[0].default_value = clothsecondaryfuzz

    reroute_107 = Shader_Preset.nodes.new("NodeReroute")

    reroute_365 = Shader_Preset.nodes.new("NodeReroute")

    armor_detail_normal_transform = Shader_Preset.nodes.new("ShaderNodeMapping")
    armor_detail_normal_transform.label = "Detail Normal Transform"
    armor_detail_normal_transform.use_custom_color = True
    armor_detail_normal_transform.color = (0.2352934032678604, 0.2352934032678604, 0.5137253999710083)
    armor_detail_normal_transform.vector_type = 'POINT'
    armor_detail_normal_transform.inputs[2].hide = True
    armor_detail_normal_transform.inputs[1].default_value = armordetailnormalposition
    armor_detail_normal_transform.inputs[2].default_value = (0.0, 0.0, 0.0)
    armor_detail_normal_transform.inputs[3].default_value = armordetailnormalscale

    reroute_181 = Shader_Preset.nodes.new("NodeReroute")

    reroute_137 = Shader_Preset.nodes.new("NodeReroute")

    reroute_020 = Shader_Preset.nodes.new("NodeReroute")

    mix_056 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_102 = Shader_Preset.nodes.new("NodeReroute")

    cloth_secondary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_metalness.label = "Metalness"
    cloth_secondary_metalness.use_custom_color = True
    cloth_secondary_metalness.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_metalness.hide = True
    cloth_secondary_metalness.outputs[0].default_value = clothsecondarymetalness

    worn_armor_secondary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_metalness.label = "Metalness"
    worn_armor_secondary_metalness.use_custom_color = True
    worn_armor_secondary_metalness.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_metalness.hide = True
    worn_armor_secondary_metalness.outputs[0].default_value = wornarmorsecondarymetalness

    math_012 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_012.operation = 'MULTIPLY'
    math_012.inputs[2].default_value = -500.0

    separate_xyz_002 = Shader_Preset.nodes.new("ShaderNodeSeparateXYZ")

    math_003 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_003.operation = 'MULTIPLY'
    math_003.inputs[2].default_value = 0.5

    suit_secondary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_detail_roughness_blend.label = "Detail Roughness Blend"
    suit_secondary_detail_roughness_blend.use_custom_color = True
    suit_secondary_detail_roughness_blend.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_detail_roughness_blend.hide = True
    suit_secondary_detail_roughness_blend.outputs[0].default_value = suitsecondarydetailroughnessblend

    cloth_secondary_wear_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_wear_remap_z.label = "Wear Remap Z"
    cloth_secondary_wear_remap_z.use_custom_color = True
    cloth_secondary_wear_remap_z.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_wear_remap_z.hide = True
    cloth_secondary_wear_remap_z.outputs[0].default_value = clothsecondarywearremapZ

    armor_primary_fuzz = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_fuzz.label = "Fuzz"
    armor_primary_fuzz.use_custom_color = True
    armor_primary_fuzz.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_fuzz.hide = True
    armor_primary_fuzz.outputs[0].default_value = armorprimaryfuzz

    reroute_056 = Shader_Preset.nodes.new("NodeReroute")

    reroute_115 = Shader_Preset.nodes.new("NodeReroute")

    reroute_049 = Shader_Preset.nodes.new("NodeReroute")

    reroute_096 = Shader_Preset.nodes.new("NodeReroute")

    worn_armor_primary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_roughness_remap_x.label = "Roughness Remap X"
    worn_armor_primary_roughness_remap_x.use_custom_color = True
    worn_armor_primary_roughness_remap_x.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_roughness_remap_x.hide = True
    worn_armor_primary_roughness_remap_x.outputs[0].default_value = wornarmorprimaryroughnessremapX

    reroute_025 = Shader_Preset.nodes.new("NodeReroute")

    reroute_055 = Shader_Preset.nodes.new("NodeReroute")

    cloth_secondary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_roughness_remap_z.label = "Roughness Remap Z"
    cloth_secondary_roughness_remap_z.use_custom_color = True
    cloth_secondary_roughness_remap_z.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_roughness_remap_z.hide = True
    cloth_secondary_roughness_remap_z.outputs[0].default_value = clothsecondaryroughnessremapZ

    worn_cloth_primary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_roughness_remap_x.label = "Roughness Remap X"
    worn_cloth_primary_roughness_remap_x.use_custom_color = True
    worn_cloth_primary_roughness_remap_x.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_roughness_remap_x.hide = True
    worn_cloth_primary_roughness_remap_x.outputs[0].default_value = wornclothprimaryroughnessremapX

    mix_022 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_primary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_roughness_remap_z.label = "Roughness Remap Z"
    suit_primary_roughness_remap_z.use_custom_color = True
    suit_primary_roughness_remap_z.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_roughness_remap_z.hide = True
    suit_primary_roughness_remap_z.outputs[0].default_value = suitprimaryroughnessremapZ

    reroute_081 = Shader_Preset.nodes.new("NodeReroute")

    worn_cloth_secondary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_metalness.label = "Metalness"
    worn_cloth_secondary_metalness.use_custom_color = True
    worn_cloth_secondary_metalness.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_metalness.hide = True
    worn_cloth_secondary_metalness.outputs[0].default_value = wornclothsecondarymetalness

    math_019 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_019.operation = 'MULTIPLY_ADD'
    math_019.use_clamp = True
    math_019.inputs[1].default_value = 100000.0
    math_019.inputs[2].default_value = -99999.0

    combine_xyz_038 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    suit_secondary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_roughness_remap_w.label = "Roughness Remap W"
    suit_secondary_roughness_remap_w.use_custom_color = True
    suit_secondary_roughness_remap_w.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_roughness_remap_w.hide = True
    suit_secondary_roughness_remap_w.outputs[0].default_value = suitsecondaryroughnessremapW

    math_007 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_007.label = "Suit_Primary_Slot"
    math_007.operation = 'MULTIPLY_ADD'
    math_007.inputs[1].default_value = 1000.0
    math_007.inputs[2].default_value = -333.0

    reroute_095 = Shader_Preset.nodes.new("NodeReroute")

    reroute_160 = Shader_Preset.nodes.new("NodeReroute")

    reroute_195 = Shader_Preset.nodes.new("NodeReroute")

    reroute_140 = Shader_Preset.nodes.new("NodeReroute")

    reroute_021 = Shader_Preset.nodes.new("NodeReroute")

    mix_048 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    cloth_secondary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_detail_roughness_blend.label = "Detail Roughness Blend"
    cloth_secondary_detail_roughness_blend.use_custom_color = True
    cloth_secondary_detail_roughness_blend.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_detail_roughness_blend.hide = True
    cloth_secondary_detail_roughness_blend.outputs[0].default_value = clothsecondarydetailroughnessblend

    reroute_082 = Shader_Preset.nodes.new("NodeReroute")

    armor_secondary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_detail_normal_blend.label = "Detail Normal Blend"
    armor_secondary_detail_normal_blend.use_custom_color = True
    armor_secondary_detail_normal_blend.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_detail_normal_blend.hide = True
    armor_secondary_detail_normal_blend.outputs[0].default_value = armorsecondarydetailnormalblend

    worn_cloth_primary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_roughness_remap_z.label = "Roughness Remap Z"
    worn_cloth_primary_roughness_remap_z.use_custom_color = True
    worn_cloth_primary_roughness_remap_z.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_roughness_remap_z.hide = True
    worn_cloth_primary_roughness_remap_z.outputs[0].default_value = wornclothprimaryroughnessremapZ

    combine_xyz_036 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    math_002 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_002.operation = 'MULTIPLY_ADD'
    math_002.use_clamp = True
    math_002.inputs[1].default_value = 1000000.0
    math_002.inputs[2].default_value = -500000.0

    worn_cloth_secondary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    worn_cloth_secondary_detail_diffuse_blend.use_custom_color = True
    worn_cloth_secondary_detail_diffuse_blend.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_detail_diffuse_blend.hide = True
    worn_cloth_secondary_detail_diffuse_blend.outputs[0].default_value = wornclothsecondarydetaildiffuseblend

    reroute_068 = Shader_Preset.nodes.new("NodeReroute")

    math_016 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_016.operation = 'MULTIPLY_ADD'
    math_016.use_clamp = True
    math_016.inputs[1].default_value = 1000000.0
    math_016.inputs[2].default_value = -500000.0

    reroute_041 = Shader_Preset.nodes.new("NodeReroute")
    
    worn_suit_primary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    worn_suit_primary_detail_diffuse_blend.use_custom_color = True
    worn_suit_primary_detail_diffuse_blend.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_detail_diffuse_blend.hide = True
    worn_suit_primary_detail_diffuse_blend.outputs[0].default_value = wornsuitprimarydetaildiffuseblend

    suit_secondary_wear_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_wear_remap_x.label = "Wear Remap X"
    suit_secondary_wear_remap_x.use_custom_color = True
    suit_secondary_wear_remap_x.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_wear_remap_x.hide = True
    suit_secondary_wear_remap_x.outputs[0].default_value = suitsecondarywearremapX

    armor_primary_transmission = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_transmission.label = "Transmission"
    armor_primary_transmission.use_custom_color = True
    armor_primary_transmission.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_transmission.hide = True
    armor_primary_transmission.outputs[0].default_value = armorprimarytransmission

    worn_armor_primary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_roughness_remap_y.label = "Roughness Remap Y"
    worn_armor_primary_roughness_remap_y.use_custom_color = True
    worn_armor_primary_roughness_remap_y.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_roughness_remap_y.hide = True
    worn_armor_primary_roughness_remap_y.outputs[0].default_value = wornarmorprimaryroughnessremapY

    reroute_109 = Shader_Preset.nodes.new("NodeReroute")

    reroute_129 = Shader_Preset.nodes.new("NodeReroute")

    reroute_177 = Shader_Preset.nodes.new("NodeReroute")

    reroute_014 = Shader_Preset.nodes.new("NodeReroute")

    reroute_130 = Shader_Preset.nodes.new("NodeReroute")

    cloth_secondary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_roughness_remap_y.label = "Roughness Remap Y"
    cloth_secondary_roughness_remap_y.use_custom_color = True
    cloth_secondary_roughness_remap_y.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_roughness_remap_y.hide = True
    cloth_secondary_roughness_remap_y.outputs[0].default_value = clothsecondaryroughnessremapY

    cloth_secondary_transmission = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_transmission.label = "Transmission"
    cloth_secondary_transmission.use_custom_color = True
    cloth_secondary_transmission.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_transmission.hide = True
    cloth_secondary_transmission.outputs[0].default_value = clothsecondarytransmission

    mix_011 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    cloth_primary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_detail_normal_blend.label = "Detail Normal Blend"
    cloth_primary_detail_normal_blend.use_custom_color = True
    cloth_primary_detail_normal_blend.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_detail_normal_blend.hide = True
    cloth_primary_detail_normal_blend.outputs[0].default_value = clothprimarydetailnormalblend

    reroute_171 = Shader_Preset.nodes.new("NodeReroute")

    armor_secondary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    armor_secondary_detail_diffuse_blend.use_custom_color = True
    armor_secondary_detail_diffuse_blend.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_detail_diffuse_blend.hide = True
    armor_secondary_detail_diffuse_blend.outputs[0].default_value = armorsecondarydetaildiffuseblend

    worn_cloth_secondary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_detail_roughness_blend.label = "Detail Roughness Blend"
    worn_cloth_secondary_detail_roughness_blend.use_custom_color = True
    worn_cloth_secondary_detail_roughness_blend.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_detail_roughness_blend.hide = True
    worn_cloth_secondary_detail_roughness_blend.outputs[0].default_value = wornclothsecondarydetailroughnessblend

    mix_066 = Shader_Preset.nodes.new("ShaderNodeMixRGB")
    mix_066.inputs[2].default_value = (6.0, 6.0, 6.0, 1.0)

    combine_xyz_003 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    armor_primary_emission_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    armor_primary_emission_color.label = "Emission Color"
    armor_primary_emission_color.use_custom_color = True
    armor_primary_emission_color.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_emission_color.hide = True
    armor_primary_emission_color.outputs[0].default_value = armorprimaryemissioncolor

    suit_secondary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_roughness_remap_z.label = "Roughness Remap Z"
    suit_secondary_roughness_remap_z.use_custom_color = True
    suit_secondary_roughness_remap_z.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_roughness_remap_z.hide = True
    suit_secondary_roughness_remap_z.outputs[0].default_value = (0.000)

    suit_primary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_detail_normal_blend.label = "Detail Normal Blend"
    suit_primary_detail_normal_blend.use_custom_color = True
    suit_primary_detail_normal_blend.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_detail_normal_blend.hide = True
    suit_primary_detail_normal_blend.outputs[0].default_value = suitprimarydetailnormalblend

    reroute_174 = Shader_Preset.nodes.new("NodeReroute")

    reroute_191 = Shader_Preset.nodes.new("NodeReroute")

    reroute_016 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_secondary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    worn_suit_secondary_dye_color.label = "Dye Color"
    worn_suit_secondary_dye_color.use_custom_color = True
    worn_suit_secondary_dye_color.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_dye_color.hide = True
    worn_suit_secondary_dye_color.outputs[0].default_value = wornsuitsecondarydyecolor

    reroute_094 = Shader_Preset.nodes.new("NodeReroute")
    
    reroute_131 = Shader_Preset.nodes.new("NodeReroute")

    mix_027 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_primary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_detail_roughness_blend.label = "Detail Roughness Blend"
    suit_primary_detail_roughness_blend.use_custom_color = True
    suit_primary_detail_roughness_blend.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_detail_roughness_blend.hide = True
    suit_primary_detail_roughness_blend.outputs[0].default_value = suitprimarydetailroughnessblend

    cloth_secondary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_detail_normal_blend.label = "Detail Normal Blend"
    cloth_secondary_detail_normal_blend.use_custom_color = True
    cloth_secondary_detail_normal_blend.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_detail_normal_blend.hide = True
    cloth_secondary_detail_normal_blend.outputs[0].default_value = clothsecondarydetailnormalblend

    reroute_077 = Shader_Preset.nodes.new("NodeReroute")

    cloth_detail_diffuse_transform = Shader_Preset.nodes.new("ShaderNodeMapping")
    cloth_detail_diffuse_transform.label = "Detail Diffuse Transform"
    cloth_detail_diffuse_transform.use_custom_color = True
    cloth_detail_diffuse_transform.color = (0.2352934032678604, 0.2352934032678604, 0.5137253999710083)
    cloth_detail_diffuse_transform.vector_type = 'POINT'
    cloth_detail_diffuse_transform.inputs[2].hide = True
    cloth_detail_diffuse_transform.inputs[1].default_value = clothdetaildiffuseposition
    cloth_detail_diffuse_transform.inputs[2].default_value = (0.0, 0.0, 0.0)
    cloth_detail_diffuse_transform.inputs[3].default_value = clothdetaildiffusescale

    suit_primary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_roughness_remap_x.label = "Roughness Remap X"
    suit_primary_roughness_remap_x.use_custom_color = True
    suit_primary_roughness_remap_x.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_roughness_remap_x.hide = True
    suit_primary_roughness_remap_x.outputs[0].default_value = suitprimaryroughnessremapX

    cloth_primary_emission_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    cloth_primary_emission_color.label = "Emission Color"
    cloth_primary_emission_color.use_custom_color = True
    cloth_primary_emission_color.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_emission_color.hide = True
    cloth_primary_emission_color.outputs[0].default_value = clothprimaryemissioncolor

    worn_cloth_primary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_roughness_remap_y.label = "Roughness Remap Y"
    worn_cloth_primary_roughness_remap_y.use_custom_color = True
    worn_cloth_primary_roughness_remap_y.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_roughness_remap_y.hide = True
    worn_cloth_primary_roughness_remap_y.outputs[0].default_value = wornclothprimaryroughnessremapY

    reroute_146 = Shader_Preset.nodes.new("NodeReroute")

    reroute_127 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_primary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_detail_roughness_blend.label = "Detail Roughness Blend"
    worn_suit_primary_detail_roughness_blend.use_custom_color = True
    worn_suit_primary_detail_roughness_blend.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_detail_roughness_blend.hide = True
    worn_suit_primary_detail_roughness_blend.outputs[0].default_value = wornsuitprimarydetailroughnessblend

    mix_006 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_secondary_wear_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_wear_remap_w.label = "Wear Remap W"
    suit_secondary_wear_remap_w.use_custom_color = True
    suit_secondary_wear_remap_w.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_wear_remap_w.hide = True
    suit_secondary_wear_remap_w.outputs[0].default_value = suitsecondarywearremapW

    reroute_063 = Shader_Preset.nodes.new("NodeReroute")

    reroute_126 = Shader_Preset.nodes.new("NodeReroute")

    reroute_018 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_secondary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_detail_normal_blend.label = "Detail Normal Blend"
    worn_suit_secondary_detail_normal_blend.use_custom_color = True
    worn_suit_secondary_detail_normal_blend.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_detail_normal_blend.hide = True
    worn_suit_secondary_detail_normal_blend.outputs[0].default_value = wornsuitsecondarydetailnormalblend

    reroute_135 = Shader_Preset.nodes.new("NodeReroute")

    worn_cloth_secondary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_roughness_remap_y.label = "Roughness Remap Y"
    worn_cloth_secondary_roughness_remap_y.use_custom_color = True
    worn_cloth_secondary_roughness_remap_y.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_roughness_remap_y.hide = True
    worn_cloth_secondary_roughness_remap_y.outputs[0].default_value = wornclothsecondaryroughnessremapY

    reroute_182 = Shader_Preset.nodes.new("NodeReroute")

    mix_002 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_012 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    cloth_secondary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    cloth_secondary_detail_diffuse_blend.use_custom_color = True
    cloth_secondary_detail_diffuse_blend.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_detail_diffuse_blend.hide = True
    cloth_secondary_detail_diffuse_blend.outputs[0].default_value = clothsecondarydetaildiffuseblend

    armor_secondary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    armor_secondary_dye_color.label = "Dye Color"
    armor_secondary_dye_color.use_custom_color = True
    armor_secondary_dye_color.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_dye_color.hide = True
    armor_secondary_dye_color.outputs[0].default_value = armorsecondarydyecolor

    reroute_132 = Shader_Preset.nodes.new("NodeReroute")

    worn_cloth_secondary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_detail_normal_blend.label = "Detail Normal Blend"
    worn_cloth_secondary_detail_normal_blend.use_custom_color = True
    worn_cloth_secondary_detail_normal_blend.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_detail_normal_blend.hide = True
    worn_cloth_secondary_detail_normal_blend.outputs[0].default_value = wornclothsecondarydetailnormalblend

    reroute_027 = Shader_Preset.nodes.new("NodeReroute")

    reroute_154 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_020 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_099 = Shader_Preset.nodes.new("NodeReroute")

    mix_074 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    armor_primary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_roughness_remap_x.label = "Roughness Remap_X"
    armor_primary_roughness_remap_x.use_custom_color = True
    armor_primary_roughness_remap_x.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_roughness_remap_x.hide = True
    armor_primary_roughness_remap_x.outputs[0].default_value = armorprimaryroughnessremapX

    armor_secondary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_metalness.label = "Metalness"
    armor_secondary_metalness.use_custom_color = True
    armor_secondary_metalness.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_metalness.hide = True
    armor_secondary_metalness.outputs[0].default_value = armorsecondarymetalness

    reroute_179 = Shader_Preset.nodes.new("NodeReroute")

    cloth_detail_normal_transform = Shader_Preset.nodes.new("ShaderNodeMapping")
    cloth_detail_normal_transform.label = "Detail Normal Transform"
    cloth_detail_normal_transform.use_custom_color = True
    cloth_detail_normal_transform.color = (0.2352934032678604, 0.2352934032678604, 0.5137253999710083)
    cloth_detail_normal_transform.vector_type = 'POINT'
    cloth_detail_normal_transform.inputs[2].hide = True
    cloth_detail_normal_transform.inputs[1].default_value = clothdetailnormalposition
    cloth_detail_normal_transform.inputs[2].default_value = (0.0, 0.0, 0.0)
    cloth_detail_normal_transform.inputs[3].default_value = clothdetailnormalscale

    reroute_106 = Shader_Preset.nodes.new("NodeReroute")

    reroute_134 = Shader_Preset.nodes.new("NodeReroute")

    mix_013 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    armor_primary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_roughness_remap_w.label = "Roughness Remap_W"
    armor_primary_roughness_remap_w.use_custom_color = True
    armor_primary_roughness_remap_w.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_roughness_remap_w.hide = True
    armor_primary_roughness_remap_w.outputs[0].default_value = armorprimaryroughnessremapW

    cloth_secondary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_roughness_remap_x.label = "Roughness Remap X"
    cloth_secondary_roughness_remap_x.use_custom_color = True
    cloth_secondary_roughness_remap_x.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_roughness_remap_x.hide = True
    cloth_secondary_roughness_remap_x.outputs[0].default_value = clothsecondaryroughnessremapX

    reroute_178 = Shader_Preset.nodes.new("NodeReroute")

    armor_secondary_wear_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_wear_remap_z.label = "Wear Remap Z"
    armor_secondary_wear_remap_z.use_custom_color = True
    armor_secondary_wear_remap_z.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_wear_remap_z.hide = True
    armor_secondary_wear_remap_z.outputs[0].default_value = armorsecondarywearremapZ

    mix_069 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_069.inputs[1].default_value = (1.0, 1.0, 1.0, 1.0)

    mix_069.inputs[2].default_value = (2.0, 2.0, 2.0, 1.0)

    cloth_primary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_roughness_remap_x.label = "Roughness Remap X"
    cloth_primary_roughness_remap_x.use_custom_color = True
    cloth_primary_roughness_remap_x.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_roughness_remap_x.hide = True
    cloth_primary_roughness_remap_x.outputs[0].default_value = clothprimaryroughnessremapX

    reroute_114 = Shader_Preset.nodes.new("NodeReroute")

    armor_primary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    armor_primary_dye_color.label = "Dye Color"
    armor_primary_dye_color.use_custom_color = True
    armor_primary_dye_color.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_dye_color.hide = True
    armor_primary_dye_color.outputs[0].default_value = armorprimarydyecolor
    
    reroute_019 = Shader_Preset.nodes.new("NodeReroute")

    mix_054 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    worn_suit_primary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_detail_normal_blend.label = "Detail Normal Blend"
    worn_suit_primary_detail_normal_blend.use_custom_color = True
    worn_suit_primary_detail_normal_blend.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_detail_normal_blend.hide = True
    worn_suit_primary_detail_normal_blend.outputs[0].default_value = wornsuitprimarydetailnormalblend

    mix_007 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_secondary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_metalness.label = "Metalness"
    suit_secondary_metalness.use_custom_color = True
    suit_secondary_metalness.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_metalness.hide = True
    suit_secondary_metalness.outputs[0].default_value = suitsecondarymetalness

    reroute_058 = Shader_Preset.nodes.new("NodeReroute")

    worn_armor_primary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_roughness_remap_z.label = "Roughness Remap Z"
    worn_armor_primary_roughness_remap_z.use_custom_color = True
    worn_armor_primary_roughness_remap_z.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_roughness_remap_z.hide = True
    worn_armor_primary_roughness_remap_z.outputs[0].default_value = wornarmorprimaryroughnessremapZ

    reroute_110 = Shader_Preset.nodes.new("NodeReroute")

    suit_primary_wear_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_wear_remap_w.label = "Wear Remap W"
    suit_primary_wear_remap_w.use_custom_color = True
    suit_primary_wear_remap_w.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_wear_remap_w.hide = True
    suit_primary_wear_remap_w.outputs[0].default_value = suitprimarywearremapW

    worn_armor_primary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_roughness_remap_w.label = "Roughness Remap W"
    worn_armor_primary_roughness_remap_w.use_custom_color = True
    worn_armor_primary_roughness_remap_w.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_roughness_remap_w.hide = True
    worn_armor_primary_roughness_remap_w.outputs[0].default_value = wornarmorprimaryroughnessremapW

    reroute_120 = Shader_Preset.nodes.new("NodeReroute")

    reroute_091 = Shader_Preset.nodes.new("NodeReroute")

    cloth_secondary_emission_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    cloth_secondary_emission_color.label = "Emission Color"
    cloth_secondary_emission_color.use_custom_color = True
    cloth_secondary_emission_color.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_emission_color.hide = True
    cloth_secondary_emission_color.outputs[0].default_value = clothsecondaryemissioncolor

    reroute_185 = Shader_Preset.nodes.new("NodeReroute")

    mix_031 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_189 = Shader_Preset.nodes.new("NodeReroute")

    reroute_128 = Shader_Preset.nodes.new("NodeReroute")

    reroute_111 = Shader_Preset.nodes.new("NodeReroute")

    reroute_084 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_048 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    combine_xyz_048.inputs[2].default_value = 0.0

    armor_primary_wear_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_wear_remap_x.label = "Wear Remap X"
    armor_primary_wear_remap_x.use_custom_color = True
    armor_primary_wear_remap_x.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_wear_remap_x.hide = True
    armor_primary_wear_remap_x.outputs[0].default_value = armorprimarywearremapX

    combine_xyz_033 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_079 = Shader_Preset.nodes.new("NodeReroute")

    suit_secondary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_roughness_remap_y.label = "Roughness Remap Y"
    suit_secondary_roughness_remap_y.use_custom_color = True
    suit_secondary_roughness_remap_y.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_roughness_remap_y.hide = True
    suit_secondary_roughness_remap_y.outputs[0].default_value = suitsecondaryroughnessremapY

    reroute_113 = Shader_Preset.nodes.new("NodeReroute")

    armor_primary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_roughness_remap_y.label = "Roughness Remap_Y"
    armor_primary_roughness_remap_y.use_custom_color = True
    armor_primary_roughness_remap_y.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_roughness_remap_y.hide = True
    armor_primary_roughness_remap_y.outputs[0].default_value = armorprimaryroughnessremapY

    combine_xyz_029 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_armor_primary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    worn_armor_primary_detail_diffuse_blend.use_custom_color = True
    worn_armor_primary_detail_diffuse_blend.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_detail_diffuse_blend.hide = True
    worn_armor_primary_detail_diffuse_blend.outputs[0].default_value = wornarmorprimarydetaildiffuseblend

    reroute_103 = Shader_Preset.nodes.new("NodeReroute")

    reroute_005 = Shader_Preset.nodes.new("NodeReroute")

    mix_015 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_145 = Shader_Preset.nodes.new("NodeReroute")
    
    reroute_183 = Shader_Preset.nodes.new("NodeReroute")

    mix_045 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_029 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    node = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_065 = Shader_Preset.nodes.new("NodeReroute")

    cloth_primary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_roughness_remap_y.label = "Roughness Remap Y"
    cloth_primary_roughness_remap_y.use_custom_color = True
    cloth_primary_roughness_remap_y.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_roughness_remap_y.hide = True
    cloth_primary_roughness_remap_y.outputs[0].default_value = clothprimaryroughnessremapY

    mix_070 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_070.inputs[2].default_value = (3.0, 3.0, 3.0, 1.0)

    attribute = Shader_Preset.nodes.new("ShaderNodeAttribute")
    attribute.attribute_type = 'GEOMETRY'
    attribute.attribute_name = 'slots'

    mix_053 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    combine_xyz_006 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    suit_secondary_wear_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_wear_remap_y.label = "Wear Remap Y"
    suit_secondary_wear_remap_y.use_custom_color = True
    suit_secondary_wear_remap_y.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_wear_remap_y.hide = True
    suit_secondary_wear_remap_y.outputs[0].default_value = suitsecondarywearremapY

    combine_xyz_025 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_097 = Shader_Preset.nodes.new("NodeReroute")

    worn_armor_primary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_detail_roughness_blend.label = "Detail Roughness Blend"
    worn_armor_primary_detail_roughness_blend.use_custom_color = True
    worn_armor_primary_detail_roughness_blend.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_detail_roughness_blend.hide = True
    worn_armor_primary_detail_roughness_blend.outputs[0].default_value = wornarmorprimarydetailroughnessblend

    reroute_117 = Shader_Preset.nodes.new("NodeReroute")

    reroute_125 = Shader_Preset.nodes.new("NodeReroute")

    mix_058 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_073 = Shader_Preset.nodes.new("NodeReroute")

    suit_primary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_metalness.label = "Metalness"
    suit_primary_metalness.use_custom_color = True
    suit_primary_metalness.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_metalness.hide = True
    suit_primary_metalness.outputs[0].default_value = suitprimarymetalness

    cloth_secondary_wear_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_wear_remap_w.label = "Wear Remap W"
    cloth_secondary_wear_remap_w.use_custom_color = True
    cloth_secondary_wear_remap_w.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_wear_remap_w.hide = True
    cloth_secondary_wear_remap_w.outputs[0].default_value = clothsecondarywearremapW

    cloth_secondary_iridescence = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_iridescence.label = "Iridescence"
    cloth_secondary_iridescence.use_custom_color = True
    cloth_secondary_iridescence.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_iridescence.hide = True
    cloth_secondary_iridescence.outputs[0].default_value = clothsecondaryiridescence

    reroute_139 = Shader_Preset.nodes.new("NodeReroute")

    armor_secondary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_roughness_remap_y.label = "Roughness Remap Y"
    armor_secondary_roughness_remap_y.use_custom_color = True
    armor_secondary_roughness_remap_y.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_roughness_remap_y.hide = True
    armor_secondary_roughness_remap_y.outputs[0].default_value = armorsecondaryroughnessremapY

    reroute_175 = Shader_Preset.nodes.new("NodeReroute")

    suit_secondary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_roughness_remap_x.label = "Roughness Remap X"
    suit_secondary_roughness_remap_x.use_custom_color = True
    suit_secondary_roughness_remap_x.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_roughness_remap_x.hide = True
    suit_secondary_roughness_remap_x.outputs[0].default_value = suitsecondaryroughnessremapX

    combine_xyz_041 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    mix_044 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    cloth_secondary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_roughness_remap_w.label = "Roughness Remap W"
    cloth_secondary_roughness_remap_w.use_custom_color = True
    cloth_secondary_roughness_remap_w.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_roughness_remap_w.hide = True
    cloth_secondary_roughness_remap_w.outputs[0].default_value = clothsecondaryroughnessremapW

    group_output = Shader_Preset.nodes.new("NodeGroupOutput")

    armor_primary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_roughness_remap_z.label = "Roughness Remap_Z"
    armor_primary_roughness_remap_z.use_custom_color = True
    armor_primary_roughness_remap_z.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_roughness_remap_z.hide = True
    armor_primary_roughness_remap_z.outputs[0].default_value = armorprimaryroughnessremapZ

    combine_xyz_027 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_188 = Shader_Preset.nodes.new("NodeReroute")

    reroute_122 = Shader_Preset.nodes.new("NodeReroute")

    reroute_002 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_secondary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_roughness_remap_x.label = "Roughness Remap X"
    worn_suit_secondary_roughness_remap_x.use_custom_color = True
    worn_suit_secondary_roughness_remap_x.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_roughness_remap_x.hide = True
    worn_suit_secondary_roughness_remap_x.outputs[0].default_value = wornsuitsecondaryroughnessremapX

    suit_primary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    suit_primary_detail_diffuse_blend.use_custom_color = True
    suit_primary_detail_diffuse_blend.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_detail_diffuse_blend.hide = True
    suit_primary_detail_diffuse_blend.outputs[0].default_value = suitprimarydetaildiffuseblend

    cloth_primary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_roughness_remap_z.label = "Roughness Remap Z"
    cloth_primary_roughness_remap_z.use_custom_color = True
    cloth_primary_roughness_remap_z.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_roughness_remap_z.hide = True
    cloth_primary_roughness_remap_z.outputs[0].default_value = clothprimaryroughnessremapZ

    combine_xyz_019 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    armor_secondary_fuzz = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_fuzz.label = "Fuzz"
    armor_secondary_fuzz.use_custom_color = True
    armor_secondary_fuzz.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_fuzz.hide = True
    armor_secondary_fuzz.outputs[0].default_value = armorsecondaryfuzz

    reroute_069 = Shader_Preset.nodes.new("NodeReroute")

    reroute_030 = Shader_Preset.nodes.new("NodeReroute")

    reroute_116 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_046 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_194 = Shader_Preset.nodes.new("NodeReroute")

    worn_armor_secondary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_roughness_remap_y.label = "Roughness Remap Y"
    worn_armor_secondary_roughness_remap_y.use_custom_color = True
    worn_armor_secondary_roughness_remap_y.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_roughness_remap_y.hide = True
    worn_armor_secondary_roughness_remap_y.outputs[0].default_value = wornarmorsecondaryroughnessremapY

    cloth_primary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    cloth_primary_dye_color.label = "Dye Color"
    cloth_primary_dye_color.use_custom_color = True
    cloth_primary_dye_color.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_dye_color.hide = True
    cloth_primary_dye_color.outputs[0].default_value = clothprimarydyecolor

    mix_071 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_197 = Shader_Preset.nodes.new("NodeReroute")

    armor_primary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    armor_primary_detail_diffuse_blend.use_custom_color = True
    armor_primary_detail_diffuse_blend.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_detail_diffuse_blend.hide = True
    armor_primary_detail_diffuse_blend.outputs[0].default_value = armorprimarydetaildiffuseblend

    combine_xyz_002 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    suit_secondary_iridescence = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_iridescence.label = "Iridescence"
    suit_secondary_iridescence.use_custom_color = True
    suit_secondary_iridescence.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_iridescence.hide = True
    suit_secondary_iridescence.outputs[0].default_value = suitsecondaryiridescence

    reroute_161 = Shader_Preset.nodes.new("NodeReroute")

    armor_primary_wear_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_wear_remap_y.label = "Wear Remap Y"
    armor_primary_wear_remap_y.use_custom_color = True
    armor_primary_wear_remap_y.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_wear_remap_y.hide = True
    armor_primary_wear_remap_y.outputs[0].default_value = armorprimarywearremapY

    reroute_155 = Shader_Preset.nodes.new("NodeReroute")

    reroute_193 = Shader_Preset.nodes.new("NodeReroute")

    mix_033 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_080 = Shader_Preset.nodes.new("NodeReroute")
    
    suit_secondary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    suit_secondary_detail_diffuse_blend.use_custom_color = True
    suit_secondary_detail_diffuse_blend.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_detail_diffuse_blend.hide = True
    suit_secondary_detail_diffuse_blend.outputs[0].default_value = suitsecondarydetaildiffuseblend

    combine_xyz_042 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    math_018 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_018.operation = 'MULTIPLY_ADD'
    math_018.use_clamp = True
    math_018.inputs[1].default_value = -100000.0
    math_018.inputs[2].default_value = 1.0

    combine_xyz_018 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_166 = Shader_Preset.nodes.new("NodeReroute")
    
    reroute_070 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_primary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_roughness_remap_y.label = "Roughness Remap Y"
    worn_suit_primary_roughness_remap_y.use_custom_color = True
    worn_suit_primary_roughness_remap_y.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_roughness_remap_y.hide = True
    worn_suit_primary_roughness_remap_y.outputs[0].default_value = wornsuitprimaryroughnessremapY

    combine_xyz_008 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_armor_primary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_metalness.label = "Metalness"
    worn_armor_primary_metalness.use_custom_color = True
    worn_armor_primary_metalness.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_metalness.hide = True
    worn_armor_primary_metalness.outputs[0].default_value = wornarmorprimarymetalness

    reroute_029 = Shader_Preset.nodes.new("NodeReroute")

    reroute_119 = Shader_Preset.nodes.new("NodeReroute")

    reroute_024 = Shader_Preset.nodes.new("NodeReroute")

    worn_cloth_secondary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_roughness_remap_x.label = "Roughness Remap X"
    worn_cloth_secondary_roughness_remap_x.use_custom_color = True
    worn_cloth_secondary_roughness_remap_x.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_roughness_remap_x.hide = True
    worn_cloth_secondary_roughness_remap_x.outputs[0].default_value = wornclothsecondaryroughnessremapX

    mix_050 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_primary_emission_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    suit_primary_emission_color.label = "Emission Color"
    suit_primary_emission_color.use_custom_color = True
    suit_primary_emission_color.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_emission_color.hide = True
    suit_primary_emission_color.outputs[0].default_value = suitprimaryemissioncolor

    cloth_primary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_metalness.label = "Metalness"
    cloth_primary_metalness.use_custom_color = True
    cloth_primary_metalness.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_metalness.hide = True
    cloth_primary_metalness.outputs[0].default_value = clothprimarymetalness

    worn_cloth_primary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    worn_cloth_primary_detail_diffuse_blend.use_custom_color = True
    worn_cloth_primary_detail_diffuse_blend.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_detail_diffuse_blend.hide = True
    worn_cloth_primary_detail_diffuse_blend.outputs[0].default_value = wornclothprimarydetaildiffuseblend

    mix_068 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_068.inputs[2].default_value = (5.0, 5.0, 5.0, 1.0)

    mix_026 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    combine_xyz = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    armor_primary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_metalness.label = "Metalness"
    armor_primary_metalness.use_custom_color = True
    armor_primary_metalness.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_metalness.hide = True
    armor_primary_metalness.outputs[0].default_value = armorprimarymetalness

    combine_xyz_021 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    suit_secondary_fuzz = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_fuzz.label = "Fuzz"
    suit_secondary_fuzz.use_custom_color = True
    suit_secondary_fuzz.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_fuzz.hide = True
    suit_secondary_fuzz.outputs[0].default_value = suitsecondaryfuzz

    mix_003 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_176 = Shader_Preset.nodes.new("NodeReroute")

    reroute_156 = Shader_Preset.nodes.new("NodeReroute")

    cloth_primary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_roughness_remap_w.label = "Roughness Remap W"
    cloth_primary_roughness_remap_w.use_custom_color = True
    cloth_primary_roughness_remap_w.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_roughness_remap_w.hide = True
    cloth_primary_roughness_remap_w.outputs[0].default_value = clothprimaryroughnessremapW

    reroute_098 = Shader_Preset.nodes.new("NodeReroute")

    reroute_136 = Shader_Preset.nodes.new("NodeReroute")

    mix_040 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_186 = Shader_Preset.nodes.new("NodeReroute")

    cloth_primary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    cloth_primary_detail_diffuse_blend.use_custom_color = True
    cloth_primary_detail_diffuse_blend.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_detail_diffuse_blend.hide = True
    cloth_primary_detail_diffuse_blend.outputs[0].default_value = clothprimarydetaildiffuseblend

    combine_xyz_044 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    math = Shader_Preset.nodes.new("ShaderNodeMath")
    math.operation = 'MULTIPLY_ADD'
    math.use_clamp = True
    math.inputs[1].default_value = 1000.0
    math.inputs[2].default_value = -333.0

    worn_suit_primary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_roughness_remap_x.label = "Roughness Remap X"
    worn_suit_primary_roughness_remap_x.use_custom_color = True
    worn_suit_primary_roughness_remap_x.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_roughness_remap_x.hide = True
    worn_suit_primary_roughness_remap_x.outputs[0].default_value = wornsuitprimaryroughnessremapX

    combine_xyz_009 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_170 = Shader_Preset.nodes.new("NodeReroute")

    worn_cloth_secondary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_roughness_remap_z.label = "Roughness Remap Z"
    worn_cloth_secondary_roughness_remap_z.use_custom_color = True
    worn_cloth_secondary_roughness_remap_z.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_roughness_remap_z.hide = True
    worn_cloth_secondary_roughness_remap_z.outputs[0].default_value = wornclothsecondaryroughnessremapZ

    reroute_017 = Shader_Preset.nodes.new("NodeReroute")

    reroute_118 = Shader_Preset.nodes.new("NodeReroute")

    mix_057 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_004 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_051 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_primary_transmission = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_transmission.label = "Transmission"
    suit_primary_transmission.use_custom_color = True
    suit_primary_transmission.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_transmission.hide = True
    suit_primary_transmission.outputs[0].default_value = suitprimarytransmission

    reroute_147 = Shader_Preset.nodes.new("NodeReroute")

    armor_secondary_emission_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    armor_secondary_emission_color.label = "Emission Color"
    armor_secondary_emission_color.use_custom_color = True
    armor_secondary_emission_color.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_emission_color.hide = True
    armor_secondary_emission_color.outputs[0].default_value = armorsecondaryemissioncolor

    reroute_151 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_030 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    combine_xyz_007 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    suit_secondary_transmission = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_transmission.label = "Transmission"
    suit_secondary_transmission.use_custom_color = True
    suit_secondary_transmission.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_transmission.hide = True
    suit_secondary_transmission.outputs[0].default_value = suitsecondarytransmission

    mix_017 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_015 = Shader_Preset.nodes.new("NodeReroute")

    reroute_104 = Shader_Preset.nodes.new("NodeReroute")

    reroute_048 = Shader_Preset.nodes.new("NodeReroute")

    mix_020 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_078 = Shader_Preset.nodes.new("NodeReroute")

    mix_055 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    armor_secondary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_roughness_remap_x.label = "Roughness Remap X"
    armor_secondary_roughness_remap_x.use_custom_color = True
    armor_secondary_roughness_remap_x.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_roughness_remap_x.hide = True
    armor_secondary_roughness_remap_x.outputs[0].default_value = armorsecondaryroughnessremapX

    cloth_primary_iridescence = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_iridescence.label = "Iridescence"
    cloth_primary_iridescence.use_custom_color = True
    cloth_primary_iridescence.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_iridescence.hide = True
    cloth_primary_iridescence.outputs[0].default_value = clothprimaryiridescence

    reroute_059 = Shader_Preset.nodes.new("NodeReroute")

    cloth_primary_wear_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_wear_remap_x.label = "Wear Remap X"
    cloth_primary_wear_remap_x.use_custom_color = True
    cloth_primary_wear_remap_x.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_wear_remap_x.hide = True
    cloth_primary_wear_remap_x.outputs[0].default_value = clothprimarywearremapX

    mix_021 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_022 = Shader_Preset.nodes.new("NodeReroute")

    mix_019 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    combine_xyz_039 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_armor_secondary_roughness_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_roughness_remap_x.label = "Roughness Remap X"
    worn_armor_secondary_roughness_remap_x.use_custom_color = True
    worn_armor_secondary_roughness_remap_x.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_roughness_remap_x.hide = True
    worn_armor_secondary_roughness_remap_x.outputs[0].default_value = wornarmorsecondaryroughnessremapX

    mix_073 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    worn_suit_primary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    worn_suit_primary_dye_color.label = "Dye Color"
    worn_suit_primary_dye_color.use_custom_color = True
    worn_suit_primary_dye_color.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_dye_color.hide = True
    worn_suit_primary_dye_color.outputs[0].default_value = wornsuitprimarydyecolor

    combine_xyz_024 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_172 = Shader_Preset.nodes.new("NodeReroute")

    armor_detail_diffuse_transform = Shader_Preset.nodes.new("ShaderNodeMapping")
    armor_detail_diffuse_transform.label = "Detail Diffuse Transform"
    armor_detail_diffuse_transform.use_custom_color = True
    armor_detail_diffuse_transform.color = (0.2352934032678604, 0.2352934032678604, 0.5137253999710083)
    armor_detail_diffuse_transform.vector_type = 'POINT'
    armor_detail_diffuse_transform.inputs[2].hide = True
    armor_detail_diffuse_transform.inputs[1].default_value = armordetaildiffuseposition
    armor_detail_diffuse_transform.inputs[2].default_value = (0.0, 0.0, 0.0)
    armor_detail_diffuse_transform.inputs[3].default_value = armordetaildiffusescale

    reroute = Shader_Preset.nodes.new("NodeReroute")

    reroute_105 = Shader_Preset.nodes.new("NodeReroute")

    mix_052 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    cloth_secondary_wear_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_wear_remap_x.label = "Wear Remap X"
    cloth_secondary_wear_remap_x.use_custom_color = True
    cloth_secondary_wear_remap_x.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_wear_remap_x.hide = True
    cloth_secondary_wear_remap_x.outputs[0].default_value = clothsecondarywearremapX

    armor_secondary_wear_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_wear_remap_y.label = "Wear Remap Y"
    armor_secondary_wear_remap_y.use_custom_color = True
    armor_secondary_wear_remap_y.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_wear_remap_y.hide = True
    armor_secondary_wear_remap_y.outputs[0].default_value = armorsecondarywearremapY

    worn_cloth_primary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_detail_roughness_blend.label = "Detail Roughness Blend"
    worn_cloth_primary_detail_roughness_blend.use_custom_color = True
    worn_cloth_primary_detail_roughness_blend.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_detail_roughness_blend.hide = True
    worn_cloth_primary_detail_roughness_blend.outputs[0].default_value = wornclothprimarydetailroughnessblend

    math_021 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_021.operation = 'MULTIPLY_ADD'
    math_021.use_clamp = True

    math_021.inputs[1].default_value = -100000.0

    math_021.inputs[2].default_value = 1.0

    math_004 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_004.operation = 'MULTIPLY'
    math_004.use_clamp = True

    math_004.inputs[1].default_value = 1000.0

    math_004.inputs[2].default_value = 0.0

    combine_xyz_031 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    combine_xyz_023 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    armor_secondary_iridescence = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_iridescence.label = "Iridescence"
    armor_secondary_iridescence.use_custom_color = True
    armor_secondary_iridescence.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_iridescence.hide = True
    armor_secondary_iridescence.outputs[0].default_value = armorsecondaryiridescence

    reroute_047 = Shader_Preset.nodes.new("NodeReroute")

    cloth_primary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_detail_roughness_blend.label = "Detail Roughness Blend"
    cloth_primary_detail_roughness_blend.use_custom_color = True
    cloth_primary_detail_roughness_blend.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_detail_roughness_blend.hide = True
    cloth_primary_detail_roughness_blend.outputs[0].default_value = clothprimarydetailroughnessblend

    combine_xyz_004 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_159 = Shader_Preset.nodes.new("NodeReroute")

    worn_armor_primary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    worn_armor_primary_dye_color.label = "Dye Color"
    worn_armor_primary_dye_color.use_custom_color = True
    worn_armor_primary_dye_color.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_dye_color.hide = True
    worn_armor_primary_dye_color.outputs[0].default_value = wornarmorprimarydyecolor

    reroute_173 = Shader_Preset.nodes.new("NodeReroute")

    reroute_052 = Shader_Preset.nodes.new("NodeReroute")

    suit_detail_normal_transform = Shader_Preset.nodes.new("ShaderNodeMapping")
    suit_detail_normal_transform.label = "Detail Normal Transform"
    suit_detail_normal_transform.use_custom_color = True
    suit_detail_normal_transform.color = (0.2352934032678604, 0.2352934032678604, 0.5137253999710083)
    suit_detail_normal_transform.vector_type = 'POINT'
    suit_detail_normal_transform.inputs[2].hide = True
    suit_detail_normal_transform.inputs[1].default_value = suitdetailnormalposition
    suit_detail_normal_transform.inputs[2].default_value = (0.0, 0.0, 0.0)
    suit_detail_normal_transform.inputs[3].default_value = suitdetailnormalscale

    mix_008 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_primary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_roughness_remap_y.label = "Roughness Remap Y"
    suit_primary_roughness_remap_y.use_custom_color = True
    suit_primary_roughness_remap_y.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_roughness_remap_y.hide = True
    suit_primary_roughness_remap_y.outputs[0].default_value = suitprimaryroughnessremapY

    mix_042 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    combine_xyz_026 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_123 = Shader_Preset.nodes.new("NodeReroute")

    reroute_148 = Shader_Preset.nodes.new("NodeReroute")

    worn_armor_secondary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_detail_roughness_blend.label = "Detail Roughness Blend"
    worn_armor_secondary_detail_roughness_blend.use_custom_color = True
    worn_armor_secondary_detail_roughness_blend.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_detail_roughness_blend.hide = True
    worn_armor_secondary_detail_roughness_blend.outputs[0].default_value = wornarmorsecondarydetailroughnessblend

    reroute_066 = Shader_Preset.nodes.new("NodeReroute")

    cloth_primary_wear_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_wear_remap_y.label = "Wear Remap Y"
    cloth_primary_wear_remap_y.use_custom_color = True
    cloth_primary_wear_remap_y.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_wear_remap_y.hide = True
    cloth_primary_wear_remap_y.outputs[0].default_value = clothprimarywearremapY

    combine_xyz_034 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_064 = Shader_Preset.nodes.new("NodeReroute")

    armor_primary_wear_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_wear_remap_z.label = "Wear Remap Z"
    armor_primary_wear_remap_z.use_custom_color = True
    armor_primary_wear_remap_z.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_wear_remap_z.hide = True
    armor_primary_wear_remap_z.outputs[0].default_value = armorprimarywearremapZ

    suit_secondary_emission_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    suit_secondary_emission_color.label = "Emission Color"
    suit_secondary_emission_color.use_custom_color = True
    suit_secondary_emission_color.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_emission_color.hide = True
    suit_secondary_emission_color.outputs[0].default_value = suitsecondaryemissioncolor

    reroute_028 = Shader_Preset.nodes.new("NodeReroute")

    reroute_092 = Shader_Preset.nodes.new("NodeReroute")

    reroute_009 = Shader_Preset.nodes.new("NodeReroute")

    reroute_067 = Shader_Preset.nodes.new("NodeReroute")

    reroute_150 = Shader_Preset.nodes.new("NodeReroute")

    reroute_083 = Shader_Preset.nodes.new("NodeReroute")

    mix_041 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_010 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    suit_primary_fuzz = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_fuzz.label = "Fuzz"
    suit_primary_fuzz.use_custom_color = True
    suit_primary_fuzz.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_fuzz.hide = True
    suit_primary_fuzz.outputs[0].default_value = suitprimaryfuzz

    mix_032 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    worn_cloth_primary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_detail_normal_blend.label = "Detail Normal Blend"
    worn_cloth_primary_detail_normal_blend.use_custom_color = True
    worn_cloth_primary_detail_normal_blend.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_detail_normal_blend.hide = True
    worn_cloth_primary_detail_normal_blend.outputs[0].default_value = wornclothprimarydetailnormalblend

    reroute_045 = Shader_Preset.nodes.new("NodeReroute")

    mix_072 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    armor_secondary_wear_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_wear_remap_w.label = "Wear Remap W"
    armor_secondary_wear_remap_w.use_custom_color = True
    armor_secondary_wear_remap_w.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_wear_remap_w.hide = True
    armor_secondary_wear_remap_w.outputs[0].default_value = armorsecondarywearremapW

    worn_armor_secondary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    worn_armor_secondary_dye_color.label = "Dye Color"
    worn_armor_secondary_dye_color.use_custom_color = True
    worn_armor_secondary_dye_color.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_dye_color.hide = True
    worn_armor_secondary_dye_color.outputs[0].default_value = wornarmorsecondarydyecolor

    combine_xyz_035 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    math_013 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_013.operation = 'ADD'
    math_013.use_clamp = True
    math_013.inputs[2].default_value = -500.0

    combine_xyz_001 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    suit_secondary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    suit_secondary_dye_color.label = "Dye Color"
    suit_secondary_dye_color.use_custom_color = True
    suit_secondary_dye_color.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_dye_color.hide = True
    suit_secondary_dye_color.outputs[0].default_value = suitsecondarydyecolor

    armor_primary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_detail_normal_blend.label = "Detail Normal Blend"
    armor_primary_detail_normal_blend.use_custom_color = True
    armor_primary_detail_normal_blend.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_detail_normal_blend.hide = True
    armor_primary_detail_normal_blend.outputs[0].default_value = armorprimarydetailnormalblend

    cloth_secondary_wear_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_secondary_wear_remap_y.label = "Wear Remap Y"
    cloth_secondary_wear_remap_y.use_custom_color = True
    cloth_secondary_wear_remap_y.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_wear_remap_y.hide = True
    cloth_secondary_wear_remap_y.outputs[0].default_value = clothsecondarywearremapY

    combine_xyz_045 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_167 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_011 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_149 = Shader_Preset.nodes.new("NodeReroute")

    detail_uvs = Shader_Preset.nodes.new("ShaderNodeUVMap")
    detail_uvs.label = "Detail UVs"
    detail_uvs.uv_map = 'uv1'
    detail_uvs.hide = True

    reroute_121 = Shader_Preset.nodes.new("NodeReroute")

    reroute_044 = Shader_Preset.nodes.new("NodeReroute")

    mix_018 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    worn_cloth_secondary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    worn_cloth_secondary_dye_color.label = "Dye Color"
    worn_cloth_secondary_dye_color.use_custom_color = True
    worn_cloth_secondary_dye_color.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_dye_color.hide = True
    worn_cloth_secondary_dye_color.outputs[0].default_value = wornclothsecondarydyecolor

    reroute_087 = Shader_Preset.nodes.new("NodeReroute")

    mix_043 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    worn_cloth_primary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_metalness.label = "Metalness"
    worn_cloth_primary_metalness.use_custom_color = True
    worn_cloth_primary_metalness.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_metalness.hide = True
    worn_cloth_primary_metalness.outputs[0].default_value = wornclothprimarymetalness

    armor_secondary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_roughness_remap_z.label = "Roughness Remap Z"
    armor_secondary_roughness_remap_z.use_custom_color = True
    armor_secondary_roughness_remap_z.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_roughness_remap_z.hide = True
    armor_secondary_roughness_remap_z.outputs[0].default_value = armorsecondaryroughnessremapZ

    cloth_primary_wear_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_wear_remap_z.label = "Wear Remap Z"
    cloth_primary_wear_remap_z.use_custom_color = True
    cloth_primary_wear_remap_z.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_wear_remap_z.hide = True
    cloth_primary_wear_remap_z.outputs[0].default_value = clothprimarywearremapZ

    reroute_008 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_032 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    combine_xyz_028 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    math_023 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_023.operation = 'SUBTRACT'
    math_023.inputs[1].default_value = 3.0
    math_023.inputs[2].default_value = 0.5

    mix_016 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    worn_suit_secondary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_metalness.label = "Metalness"
    worn_suit_secondary_metalness.use_custom_color = True
    worn_suit_secondary_metalness.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_metalness.hide = True
    worn_suit_secondary_metalness.outputs[0].default_value = wornsuitsecondarymetalness

    mix_014 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_100 = Shader_Preset.nodes.new("NodeReroute")

    reroute_060 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_secondary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_detail_roughness_blend.label = "Detail Roughness Blend"
    worn_suit_secondary_detail_roughness_blend.use_custom_color = True
    worn_suit_secondary_detail_roughness_blend.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_detail_roughness_blend.hide = True
    worn_suit_secondary_detail_roughness_blend.outputs[0].default_value = wornsuitsecondarydetailroughnessblend

    combine_xyz_040 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_001 = Shader_Preset.nodes.new("NodeReroute")

    reroute_090 = Shader_Preset.nodes.new("NodeReroute")

    mix_025 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_124 = Shader_Preset.nodes.new("NodeReroute")

    cloth_secondary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    cloth_secondary_dye_color.label = "Dye Color"
    cloth_secondary_dye_color.use_custom_color = True
    cloth_secondary_dye_color.color = (CSecon2.X, CSecon2.Y, CSecon2.Z)
    cloth_secondary_dye_color.hide = True
    cloth_secondary_dye_color.outputs[0].default_value = clothsecondarydyecolor

    worn_cloth_primary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_primary_roughness_remap_w.label = "Roughness Remap W"
    worn_cloth_primary_roughness_remap_w.use_custom_color = True
    worn_cloth_primary_roughness_remap_w.color = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z)
    worn_cloth_primary_roughness_remap_w.hide = True
    worn_cloth_primary_roughness_remap_w.outputs[0].default_value = wornclothprimaryroughnessremapW

    combine_xyz_010 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_armor_primary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_primary_detail_normal_blend.label = "Detail Normal Blend"
    worn_armor_primary_detail_normal_blend.use_custom_color = True
    worn_armor_primary_detail_normal_blend.color = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z)
    worn_armor_primary_detail_normal_blend.hide = True
    worn_armor_primary_detail_normal_blend.outputs[0].default_value = wornarmorprimarydetailnormalblend

    suit_secondary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_detail_normal_blend.label = "Detail Normal Blend"
    suit_secondary_detail_normal_blend.use_custom_color = True
    suit_secondary_detail_normal_blend.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_detail_normal_blend.hide = True
    suit_secondary_detail_normal_blend.outputs[0].default_value = suitsecondarydetailnormalblend

    combine_xyz_015 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_suit_secondary_roughness_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_roughness_remap_y.label = "Roughness Remap Y"
    worn_suit_secondary_roughness_remap_y.use_custom_color = True
    worn_suit_secondary_roughness_remap_y.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_roughness_remap_y.hide = True
    worn_suit_secondary_roughness_remap_y.outputs[0].default_value = wornsuitsecondaryroughnessremapY

    reroute_012 = Shader_Preset.nodes.new("NodeReroute")

    reroute_013 = Shader_Preset.nodes.new("NodeReroute")

    armor_primary_iridescence = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_iridescence.label = "Iridescence"
    armor_primary_iridescence.use_custom_color = True
    armor_primary_iridescence.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_iridescence.hide = True
    armor_primary_iridescence.outputs[0].default_value = armorprimaryiridescence

    reroute_089 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_012 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_062 = Shader_Preset.nodes.new("NodeReroute")

    reroute_101 = Shader_Preset.nodes.new("NodeReroute")

    reroute_043 = Shader_Preset.nodes.new("NodeReroute")

    reroute_086 = Shader_Preset.nodes.new("NodeReroute")

    reroute_031 = Shader_Preset.nodes.new("NodeReroute")

    mix_030 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_050 = Shader_Preset.nodes.new("NodeReroute")

    suit_primary_wear_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_wear_remap_x.label = "Wear Remap X"
    suit_primary_wear_remap_x.use_custom_color = True
    suit_primary_wear_remap_x.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_wear_remap_x.hide = True
    suit_primary_wear_remap_x.outputs[0].default_value = suitprimarywearremapX

    armor_secondary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_roughness_remap_w.label = "Roughness Remap W"
    armor_secondary_roughness_remap_w.use_custom_color = True
    armor_secondary_roughness_remap_w.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_roughness_remap_w.hide = True
    armor_secondary_roughness_remap_w.outputs[0].default_value = armorsecondaryroughnessremapW

    combine_xyz_037 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    cloth_primary_wear_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_wear_remap_w.label = "Wear Remap W"
    cloth_primary_wear_remap_w.use_custom_color = True
    cloth_primary_wear_remap_w.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_wear_remap_w.hide = True
    cloth_primary_wear_remap_w.outputs[0].default_value = clothprimarywearremapW

    reroute_007 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_005 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_112 = Shader_Preset.nodes.new("NodeReroute")

    reroute_006 = Shader_Preset.nodes.new("NodeReroute")

    armor_primary_detail_roughness_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_primary_detail_roughness_blend.label = "Detail Roughness Blend"
    armor_primary_detail_roughness_blend.use_custom_color = True
    armor_primary_detail_roughness_blend.color = (CPrime1.X, CPrime1.Y, CPrime1.Z)
    armor_primary_detail_roughness_blend.hide = True
    armor_primary_detail_roughness_blend.outputs[0].default_value = armorprimarydetailroughnessblend
    
    reroute_010 = Shader_Preset.nodes.new("NodeReroute")

    reroute_184 = Shader_Preset.nodes.new("NodeReroute")

    suit_secondary_wear_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_secondary_wear_remap_z.label = "Wear Remap Z"
    suit_secondary_wear_remap_z.use_custom_color = True
    suit_secondary_wear_remap_z.color = (CSecon3.X, CSecon3.Y, CSecon3.Z)
    suit_secondary_wear_remap_z.hide = True
    suit_secondary_wear_remap_z.outputs[0].default_value = suitsecondarywearremapZ

    separate_xyz_001 = Shader_Preset.nodes.new("ShaderNodeSeparateXYZ")

    reroute_093 = Shader_Preset.nodes.new("NodeReroute")

    reroute_061 = Shader_Preset.nodes.new("NodeReroute")

    worn_armor_secondary_detail_normal_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_detail_normal_blend.label = "Detail Normal Blend"
    worn_armor_secondary_detail_normal_blend.use_custom_color = True
    worn_armor_secondary_detail_normal_blend.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_detail_normal_blend.hide = True
    worn_armor_secondary_detail_normal_blend.outputs[0].default_value = wornarmorsecondarydetailnormalblend

    reroute_051 = Shader_Preset.nodes.new("NodeReroute")

    mix_034 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    math_022 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_022.operation = 'MULTIPLY'
    math_022.inputs[1].default_value = 0.3330000042915344
    math_022.inputs[2].default_value = 0.5

    reroute_072 = Shader_Preset.nodes.new("NodeReroute")

    worn_suit_primary_metalness = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_metalness.label = "Metalness"
    worn_suit_primary_metalness.use_custom_color = True
    worn_suit_primary_metalness.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_metalness.hide = True
    worn_suit_primary_metalness.outputs[0].default_value = wornsuitprimarymetalness

    cloth_primary_transmission = Shader_Preset.nodes.new("ShaderNodeValue")
    cloth_primary_transmission.label = "Transmission"
    cloth_primary_transmission.use_custom_color = True
    cloth_primary_transmission.color = (CPrime2.X, CPrime2.Y, CPrime2.Z)
    cloth_primary_transmission.hide = True
    cloth_primary_transmission.outputs[0].default_value = clothprimarytransmission

    suit_primary_dye_color = Shader_Preset.nodes.new("ShaderNodeRGB")
    suit_primary_dye_color.label = "Dye Color"
    suit_primary_dye_color.use_custom_color = True
    suit_primary_dye_color.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_dye_color.hide = True
    suit_primary_dye_color.outputs[0].default_value = suitprimarydyecolor

    mix_076 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_067 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_067.inputs[2].default_value = (0.0, 0.0, 0.0, 0.0)

    worn_cloth_secondary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_cloth_secondary_roughness_remap_w.label = "Roughness Remap W"
    worn_cloth_secondary_roughness_remap_w.use_custom_color = True
    worn_cloth_secondary_roughness_remap_w.color = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z)
    worn_cloth_secondary_roughness_remap_w.hide = True
    worn_cloth_secondary_roughness_remap_w.outputs[0].default_value = wornclothsecondaryroughnessremapW

    combine_xyz_013 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_011 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_014 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_suit_primary_roughness_remap_w = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_primary_roughness_remap_w.label = "Roughness Remap W"
    worn_suit_primary_roughness_remap_w.use_custom_color = True
    worn_suit_primary_roughness_remap_w.color = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z)
    worn_suit_primary_roughness_remap_w.hide = True
    worn_suit_primary_roughness_remap_w.outputs[0].default_value = wornsuitprimaryroughnessremapW

    worn_suit_secondary_roughness_remap_z = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_suit_secondary_roughness_remap_z.label = "Roughness Remap Z"
    worn_suit_secondary_roughness_remap_z.use_custom_color = True
    worn_suit_secondary_roughness_remap_z.color = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z)
    worn_suit_secondary_roughness_remap_z.hide = True
    worn_suit_secondary_roughness_remap_z.outputs[0].default_value = wornsuitsecondaryroughnessremapZ

    reroute_165 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_047 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    reroute_108 = Shader_Preset.nodes.new("NodeReroute")

    reroute_042 = Shader_Preset.nodes.new("NodeReroute")

    mix_024 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    reroute_046 = Shader_Preset.nodes.new("NodeReroute")

    reroute_180 = Shader_Preset.nodes.new("NodeReroute")

    suit_primary_wear_remap_y = Shader_Preset.nodes.new("ShaderNodeValue")
    suit_primary_wear_remap_y.label = "Wear Remap Y"
    suit_primary_wear_remap_y.use_custom_color = True
    suit_primary_wear_remap_y.color = (CPrime3.X, CPrime3.Y, CPrime3.Z)
    suit_primary_wear_remap_y.hide = True
    suit_primary_wear_remap_y.outputs[0].default_value = suitprimarywearremapY

    mix_001 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_009 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    mix_046 = Shader_Preset.nodes.new("ShaderNodeMixRGB")

    armor_secondary_wear_remap_x = Shader_Preset.nodes.new("ShaderNodeValue")
    armor_secondary_wear_remap_x.label = "Wear Remap X"
    armor_secondary_wear_remap_x.use_custom_color = True
    armor_secondary_wear_remap_x.color = (CSecon1.X, CSecon1.Y, CSecon1.Z)
    armor_secondary_wear_remap_x.hide = True
    armor_secondary_wear_remap_x.outputs[0].default_value = armorsecondarywearremapX

    worn_armor_secondary_detail_diffuse_blend = Shader_Preset.nodes.new("ShaderNodeValue")
    worn_armor_secondary_detail_diffuse_blend.label = "Detail Diffuse Blend"
    worn_armor_secondary_detail_diffuse_blend.use_custom_color = True
    worn_armor_secondary_detail_diffuse_blend.color = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z)
    worn_armor_secondary_detail_diffuse_blend.hide = True
    worn_armor_secondary_detail_diffuse_blend.outputs[0].default_value = wornarmorsecondarydetaildiffuseblend

    math_005 = Shader_Preset.nodes.new("ShaderNodeMath")
    math_005.operation = 'MULTIPLY'
    math_005.inputs[2].default_value = 0.5

    reroute_088 = Shader_Preset.nodes.new("NodeReroute")

    combine_xyz_022 = Shader_Preset.nodes.new("ShaderNodeCombineXYZ")

    worn_suit_secondary_detail_diffuse_blend.parent = WornSuitSecondaryFrame
    math_017.parent = DoNotTouchFrame
    mix_047.parent = DoNotTouchFrame
    suit_primary_roughness_remap_w.parent = SuitPrimaryFrame
    armor_secondary_detail_roughness_blend.parent = ArmorSecondaryFrame
    worn_suit_primary_roughness_remap_z.parent = WornSuitPrimaryFrame
    math_008.parent = DoNotTouchFrame
    combine_xyz_016.parent = DoNotTouchFrame
    mix_023.parent = DoNotTouchFrame
    suit_primary_wear_remap_z.parent = SuitPrimaryFrame
    worn_armor_secondary_roughness_remap_w.parent = WornArmorSecondaryFrame
    worn_cloth_primary_dye_color.parent = WornClothPrimaryFrame
    group_input.parent = DoNotTouchFrame
    mix_065.parent = DoNotTouchFrame
    combine_xyz_017.parent = DoNotTouchFrame
    worn_suit_secondary_roughness_remap_w.parent = WornSuitSecondaryFrame
    suit_primary_iridescence.parent = SuitPrimaryFrame
    mix_005.parent = DoNotTouchFrame
    math_010.parent = DoNotTouchFrame
    mix_049.parent = DoNotTouchFrame
    math_024.parent = DoNotTouchFrame
    mix_028.parent = DoNotTouchFrame
    math_015.parent = DoNotTouchFrame
    cloth_primary_fuzz.parent = ClothPrimaryFrame
    armor_primary_wear_remap_w.parent = ArmorPrimaryFrame
    mix.parent = DoNotTouchFrame
    mix_059.parent = DoNotTouchFrame
    worn_armor_secondary_roughness_remap_z.parent = WornArmorSecondaryFrame
    armor_secondary_transmission.parent = ArmorSecondaryFrame
    combine_xyz_043.parent = DoNotTouchFrame
    math_001.parent = DoNotTouchFrame
    cloth_secondary_fuzz.parent = ClothSecondaryFrame
    mix_056.parent = DoNotTouchFrame
    cloth_secondary_metalness.parent = ClothSecondaryFrame
    worn_armor_secondary_metalness.parent = WornArmorSecondaryFrame
    math_012.parent = DoNotTouchFrame
    separate_xyz_002.parent = DoNotTouchFrame
    math_003.parent = DoNotTouchFrame
    suit_secondary_detail_roughness_blend.parent = SuitSecondaryFrame
    cloth_secondary_wear_remap_z.parent = ClothSecondaryFrame
    armor_primary_fuzz.parent = ArmorPrimaryFrame
    worn_armor_primary_roughness_remap_x.parent = WornArmorPrimaryFrame
    cloth_secondary_roughness_remap_z.parent = ClothSecondaryFrame
    worn_cloth_primary_roughness_remap_x.parent = WornClothPrimaryFrame
    mix_022.parent = DoNotTouchFrame
    suit_primary_roughness_remap_z.parent = SuitPrimaryFrame
    worn_cloth_secondary_metalness.parent = WornClothSecondaryFrame
    math_019.parent = DoNotTouchFrame
    combine_xyz_038.parent = DoNotTouchFrame
    suit_secondary_roughness_remap_w.parent = SuitSecondaryFrame
    math_007.parent = DoNotTouchFrame
    mix_048.parent = DoNotTouchFrame
    cloth_secondary_detail_roughness_blend.parent = ClothSecondaryFrame
    armor_secondary_detail_normal_blend.parent = ArmorSecondaryFrame
    worn_cloth_primary_roughness_remap_z.parent = WornClothPrimaryFrame
    combine_xyz_036.parent = DoNotTouchFrame
    math_002.parent = DoNotTouchFrame
    worn_cloth_secondary_detail_diffuse_blend.parent = WornClothSecondaryFrame
    math_016.parent = DoNotTouchFrame
    worn_suit_primary_detail_diffuse_blend.parent = WornSuitPrimaryFrame
    suit_secondary_wear_remap_x.parent = SuitSecondaryFrame
    armor_primary_transmission.parent = ArmorPrimaryFrame
    worn_armor_primary_roughness_remap_y.parent = WornArmorPrimaryFrame
    cloth_secondary_roughness_remap_y.parent = ClothSecondaryFrame
    cloth_secondary_transmission.parent = ClothSecondaryFrame
    mix_011.parent = DoNotTouchFrame
    cloth_primary_detail_normal_blend.parent = ClothPrimaryFrame
    armor_secondary_detail_diffuse_blend.parent = ArmorSecondaryFrame
    worn_cloth_secondary_detail_roughness_blend.parent = WornClothSecondaryFrame
    mix_066.parent = DoNotTouchFrame
    combine_xyz_003.parent = DoNotTouchFrame
    armor_primary_emission_color.parent = ArmorPrimaryFrame
    suit_secondary_roughness_remap_z.parent = SuitSecondaryFrame
    suit_primary_detail_normal_blend.parent = SuitPrimaryFrame
    worn_suit_secondary_dye_color.parent = WornSuitSecondaryFrame
    mix_027.parent = DoNotTouchFrame
    suit_primary_detail_roughness_blend.parent = SuitPrimaryFrame
    cloth_secondary_detail_normal_blend.parent = ClothSecondaryFrame
    suit_primary_roughness_remap_x.parent = SuitPrimaryFrame
    cloth_primary_emission_color.parent = ClothPrimaryFrame
    worn_cloth_primary_roughness_remap_y.parent = WornClothPrimaryFrame
    worn_suit_primary_detail_roughness_blend.parent = WornSuitPrimaryFrame
    mix_006.parent = DoNotTouchFrame
    suit_secondary_wear_remap_w.parent = SuitSecondaryFrame
    worn_suit_secondary_detail_normal_blend.parent = WornSuitSecondaryFrame
    worn_cloth_secondary_roughness_remap_y.parent = WornClothSecondaryFrame
    mix_002.parent = DoNotTouchFrame
    mix_012.parent = DoNotTouchFrame
    cloth_secondary_detail_diffuse_blend.parent = ClothSecondaryFrame
    armor_secondary_dye_color.parent = ArmorSecondaryFrame
    suit_primary_detail_diffuse_map.parent = SuitPrimaryFrame
    worn_cloth_secondary_detail_normal_blend.parent = WornClothSecondaryFrame
    combine_xyz_020.parent = DoNotTouchFrame
    mix_074.parent = DoNotTouchFrame
    armor_primary_roughness_remap_x.parent = ArmorPrimaryFrame
    armor_secondary_metalness.parent = ArmorSecondaryFrame
    mix_013.parent = DoNotTouchFrame
    armor_primary_roughness_remap_w.parent = ArmorPrimaryFrame
    cloth_secondary_roughness_remap_x.parent = ClothSecondaryFrame
    armor_secondary_wear_remap_z.parent = ArmorSecondaryFrame
    mix_069.parent = DoNotTouchFrame
    cloth_primary_roughness_remap_x.parent = ClothPrimaryFrame
    armor_primary_dye_color.parent = ArmorPrimaryFrame
    mix_054.parent = DoNotTouchFrame
    worn_suit_primary_detail_normal_blend.parent = WornSuitPrimaryFrame
    mix_007.parent = DoNotTouchFrame
    suit_secondary_metalness.parent = SuitSecondaryFrame
    worn_armor_primary_roughness_remap_z.parent = WornArmorPrimaryFrame
    suit_primary_wear_remap_w.parent = SuitPrimaryFrame
    worn_armor_primary_roughness_remap_w.parent = WornArmorPrimaryFrame
    cloth_secondary_emission_color.parent = ClothSecondaryFrame
    mix_031.parent = DoNotTouchFrame
    combine_xyz_048.parent = DoNotTouchFrame
    armor_primary_wear_remap_x.parent = ArmorPrimaryFrame
    combine_xyz_033.parent = DoNotTouchFrame
    suit_secondary_roughness_remap_y.parent = SuitSecondaryFrame
    armor_primary_roughness_remap_y.parent = ArmorPrimaryFrame
    combine_xyz_029.parent = DoNotTouchFrame
    worn_armor_primary_detail_diffuse_blend.parent = WornArmorPrimaryFrame
    mix_015.parent = DoNotTouchFrame
    suit_primary_detail_normal_map.parent = SuitPrimaryFrame
    cloth_primary_detail_normal_map.parent = ClothPrimaryFrame
    mix_045.parent = DoNotTouchFrame
    mix_029.parent = DoNotTouchFrame
    node.parent = DoNotTouchFrame
    cloth_primary_roughness_remap_y.parent = ClothPrimaryFrame
    mix_070.parent = DoNotTouchFrame
    attribute.parent = DoNotTouchFrame
    mix_053.parent = DoNotTouchFrame
    combine_xyz_006.parent = DoNotTouchFrame
    suit_secondary_wear_remap_y.parent = SuitSecondaryFrame
    combine_xyz_025.parent = DoNotTouchFrame
    worn_armor_primary_detail_roughness_blend.parent = WornArmorPrimaryFrame
    mix_058.parent = DoNotTouchFrame
    suit_primary_metalness.parent = SuitPrimaryFrame
    cloth_secondary_wear_remap_w.parent = ClothSecondaryFrame
    cloth_secondary_iridescence.parent = ClothSecondaryFrame
    armor_secondary_roughness_remap_y.parent = ArmorSecondaryFrame
    suit_secondary_roughness_remap_x.parent = SuitSecondaryFrame
    combine_xyz_041.parent = DoNotTouchFrame
    mix_044.parent = DoNotTouchFrame
    cloth_secondary_roughness_remap_w.parent = ClothSecondaryFrame
    group_output.parent = DoNotTouchFrame
    armor_primary_roughness_remap_z.parent = ArmorPrimaryFrame
    combine_xyz_027.parent = DoNotTouchFrame
    worn_suit_secondary_roughness_remap_x.parent = WornSuitSecondaryFrame
    suit_primary_detail_diffuse_blend.parent = SuitPrimaryFrame
    cloth_primary_roughness_remap_z.parent = ClothPrimaryFrame
    combine_xyz_019.parent = DoNotTouchFrame
    armor_secondary_fuzz.parent = ArmorSecondaryFrame
    combine_xyz_046.parent = DoNotTouchFrame
    worn_armor_secondary_roughness_remap_y.parent = WornArmorSecondaryFrame
    cloth_primary_dye_color.parent = ClothPrimaryFrame
    mix_071.parent = DoNotTouchFrame
    armor_primary_detail_diffuse_blend.parent = ArmorPrimaryFrame
    combine_xyz_002.parent = DoNotTouchFrame
    suit_secondary_iridescence.parent = SuitSecondaryFrame
    armor_primary_wear_remap_y.parent = ArmorPrimaryFrame
    mix_033.parent = DoNotTouchFrame
    suit_secondary_detail_diffuse_blend.parent = SuitSecondaryFrame
    combine_xyz_042.parent = DoNotTouchFrame
    math_018.parent = DoNotTouchFrame
    combine_xyz_018.parent = DoNotTouchFrame
    worn_suit_primary_roughness_remap_y.parent = WornSuitPrimaryFrame
    combine_xyz_008.parent = DoNotTouchFrame
    worn_armor_primary_metalness.parent = WornArmorPrimaryFrame
    worn_cloth_secondary_roughness_remap_x.parent = WornClothSecondaryFrame
    mix_050.parent = DoNotTouchFrame
    suit_primary_emission_color.parent = SuitPrimaryFrame
    cloth_primary_metalness.parent = ClothPrimaryFrame
    worn_cloth_primary_detail_diffuse_blend.parent = WornClothPrimaryFrame
    mix_068.parent = DoNotTouchFrame
    mix_026.parent = DoNotTouchFrame
    combine_xyz.parent = DoNotTouchFrame
    armor_primary_metalness.parent = ArmorPrimaryFrame
    combine_xyz_021.parent = DoNotTouchFrame
    suit_secondary_fuzz.parent = SuitSecondaryFrame
    mix_003.parent = DoNotTouchFrame
    cloth_primary_roughness_remap_w.parent = ClothPrimaryFrame
    mix_040.parent = DoNotTouchFrame
    cloth_primary_detail_diffuse_blend.parent = ClothPrimaryFrame
    cloth_primary_detail_diffuse_map.parent = ClothPrimaryFrame
    combine_xyz_044.parent = DoNotTouchFrame
    math.parent = DoNotTouchFrame
    armor_primary_detail_normal_map.parent = ArmorPrimaryFrame
    worn_suit_primary_roughness_remap_x.parent = WornSuitPrimaryFrame
    combine_xyz_009.parent = DoNotTouchFrame
    worn_cloth_secondary_roughness_remap_z.parent = WornClothSecondaryFrame
    mix_057.parent = DoNotTouchFrame
    mix_004.parent = DoNotTouchFrame
    mix_051.parent = DoNotTouchFrame
    suit_primary_transmission.parent = SuitPrimaryFrame
    armor_secondary_emission_color.parent = ArmorSecondaryFrame
    combine_xyz_030.parent = DoNotTouchFrame
    combine_xyz_007.parent = DoNotTouchFrame
    suit_secondary_transmission.parent = SuitSecondaryFrame
    mix_017.parent = DoNotTouchFrame
    armor_primary_detail_diffuse_map.parent = ArmorPrimaryFrame
    mix_020.parent = DoNotTouchFrame
    mix_055.parent = DoNotTouchFrame
    armor_secondary_roughness_remap_x.parent = ArmorSecondaryFrame
    cloth_primary_iridescence.parent = ClothPrimaryFrame
    cloth_primary_wear_remap_x.parent = ClothPrimaryFrame
    mix_021.parent = DoNotTouchFrame
    mix_019.parent = DoNotTouchFrame
    combine_xyz_039.parent = DoNotTouchFrame
    worn_armor_secondary_roughness_remap_x.parent = WornArmorSecondaryFrame
    mix_073.parent = DoNotTouchFrame
    worn_suit_primary_dye_color.parent = WornSuitPrimaryFrame
    combine_xyz_024.parent = DoNotTouchFrame
    mix_052.parent = DoNotTouchFrame
    cloth_secondary_wear_remap_x.parent = ClothSecondaryFrame
    armor_secondary_wear_remap_y.parent = ArmorSecondaryFrame
    worn_cloth_primary_detail_roughness_blend.parent = WornClothPrimaryFrame
    math_021.parent = DoNotTouchFrame
    math_004.parent = DoNotTouchFrame
    combine_xyz_031.parent = DoNotTouchFrame
    combine_xyz_023.parent = DoNotTouchFrame
    armor_secondary_iridescence.parent = ArmorSecondaryFrame
    cloth_primary_detail_roughness_blend.parent = ClothPrimaryFrame
    combine_xyz_004.parent = DoNotTouchFrame
    worn_armor_primary_dye_color.parent = WornArmorPrimaryFrame
    mix_008.parent = DoNotTouchFrame
    suit_primary_roughness_remap_y.parent = SuitPrimaryFrame
    mix_042.parent = DoNotTouchFrame
    combine_xyz_026.parent = DoNotTouchFrame
    worn_armor_secondary_detail_roughness_blend.parent = WornArmorSecondaryFrame
    cloth_primary_wear_remap_y.parent = ClothPrimaryFrame
    combine_xyz_034.parent = DoNotTouchFrame
    armor_primary_wear_remap_z.parent = ArmorPrimaryFrame
    suit_secondary_emission_color.parent = SuitSecondaryFrame
    mix_041.parent = DoNotTouchFrame
    mix_010.parent = DoNotTouchFrame
    suit_primary_fuzz.parent = SuitPrimaryFrame
    mix_032.parent = DoNotTouchFrame
    worn_cloth_primary_detail_normal_blend.parent = WornClothPrimaryFrame
    mix_072.parent = DoNotTouchFrame
    armor_secondary_wear_remap_w.parent = ArmorSecondaryFrame
    worn_armor_secondary_dye_color.parent = WornArmorSecondaryFrame
    combine_xyz_035.parent = DoNotTouchFrame
    math_013.parent = DoNotTouchFrame
    combine_xyz_001.parent = DoNotTouchFrame
    suit_secondary_dye_color.parent = SuitSecondaryFrame
    armor_primary_detail_normal_blend.parent = ArmorPrimaryFrame
    cloth_secondary_wear_remap_y.parent = ClothSecondaryFrame
    combine_xyz_045.parent = DoNotTouchFrame
    combine_xyz_011.parent = DoNotTouchFrame
    mix_018.parent = DoNotTouchFrame
    worn_cloth_secondary_dye_color.parent = WornClothSecondaryFrame
    mix_043.parent = DoNotTouchFrame
    worn_cloth_primary_metalness.parent = WornClothPrimaryFrame
    armor_secondary_roughness_remap_z.parent = ArmorSecondaryFrame
    cloth_primary_wear_remap_z.parent = ClothPrimaryFrame
    combine_xyz_032.parent = DoNotTouchFrame
    combine_xyz_028.parent = DoNotTouchFrame
    math_023.parent = DoNotTouchFrame
    mix_016.parent = DoNotTouchFrame
    worn_suit_secondary_metalness.parent = WornSuitSecondaryFrame
    mix_014.parent = DoNotTouchFrame
    worn_suit_secondary_detail_roughness_blend.parent = WornSuitSecondaryFrame
    combine_xyz_040.parent = DoNotTouchFrame
    mix_025.parent = DoNotTouchFrame
    cloth_secondary_dye_color.parent = ClothSecondaryFrame
    worn_cloth_primary_roughness_remap_w.parent = WornClothPrimaryFrame
    combine_xyz_010.parent = DoNotTouchFrame
    worn_armor_primary_detail_normal_blend.parent = WornArmorPrimaryFrame
    suit_secondary_detail_normal_blend.parent = SuitSecondaryFrame
    combine_xyz_015.parent = DoNotTouchFrame
    worn_suit_secondary_roughness_remap_y.parent = WornSuitSecondaryFrame
    armor_primary_iridescence.parent = ArmorPrimaryFrame
    combine_xyz_012.parent = DoNotTouchFrame
    mix_030.parent = DoNotTouchFrame
    suit_primary_wear_remap_x.parent = SuitPrimaryFrame
    armor_secondary_roughness_remap_w.parent = ArmorSecondaryFrame
    combine_xyz_037.parent = DoNotTouchFrame
    cloth_primary_wear_remap_w.parent = ClothPrimaryFrame
    combine_xyz_005.parent = DoNotTouchFrame
    armor_primary_detail_roughness_blend.parent = ArmorPrimaryFrame
    suit_secondary_wear_remap_z.parent = SuitSecondaryFrame
    separate_xyz_001.parent = DoNotTouchFrame
    worn_armor_secondary_detail_normal_blend.parent = WornArmorSecondaryFrame
    mix_034.parent = DoNotTouchFrame
    math_022.parent = DoNotTouchFrame
    worn_suit_primary_metalness.parent = WornSuitPrimaryFrame
    cloth_primary_transmission.parent = ClothPrimaryFrame
    suit_primary_dye_color.parent = SuitPrimaryFrame
    mix_076.parent = DoNotTouchFrame
    mix_067.parent = DoNotTouchFrame
    worn_cloth_secondary_roughness_remap_w.parent = WornClothSecondaryFrame
    combine_xyz_013.parent = DoNotTouchFrame
    combine_xyz_014.parent = DoNotTouchFrame
    worn_suit_primary_roughness_remap_w.parent = WornSuitPrimaryFrame
    worn_suit_secondary_roughness_remap_z.parent = WornSuitSecondaryFrame
    combine_xyz_047.parent = DoNotTouchFrame
    mix_024.parent = DoNotTouchFrame
    suit_primary_wear_remap_y.parent = SuitPrimaryFrame
    mix_001.parent = DoNotTouchFrame
    mix_009.parent = DoNotTouchFrame
    mix_046.parent = DoNotTouchFrame
    armor_secondary_wear_remap_x.parent = ArmorSecondaryFrame
    worn_armor_secondary_detail_diffuse_blend.parent = WornArmorSecondaryFrame
    math_005.parent = DoNotTouchFrame
    combine_xyz_022.parent = DoNotTouchFrame

    #Set locations
    ArmorPrimaryFrame.location = (-660.0, 720.0)
    SuitSecondaryFrame.location = (-700.0, -3837.078125)
    DoNotTouchFrame.location = (1503.5009765625, -1193.9627685546875)
    WornSuitPrimaryFrame.location = (-680.0, -3487.9482421875)
    WornClothSecondaryFrame.location = (-680.0, -2490.54150390625)
    ArmorSecondaryFrame.location = (-680.0, 102.05084991455078)
    SuitPrimaryFrame.location = (-680.0, -2838.179443359375)
    WornClothPrimaryFrame.location = (-680.0, -1619.5882568359375)
    WornArmorSecondaryFrame.location = (920.0, -941.9125366210938)
    ClothSecondaryFrame.location = (-679.4005737304688, -1887.1016845703125)
    ClothPrimaryFrame.location = (-680.0, -891.457275390625)
    WornSuitSecondaryFrame.location = (-700.0, -4440.0595703125)
    WornArmorPrimaryFrame.location = (-660.0, 450.19281005859375)
    worn_suit_secondary_detail_diffuse_blend.location = (40.0, -200.126953125)
    math_017.location = (-1882.6395263671875, -3699.33740234375)
    suit_detail_diffuse_transform.location = (-913.1685180664062, -2939.849365234375)
    reroute_004.location = (-377.1822509765625, -3036.572998046875)
    mix_047.location = (-1880.0882568359375, -3704.32177734375)
    suit_primary_roughness_remap_w.location = (20.0, -147.927978515625)
    reroute_003.location = (-377.1822509765625, -2964.310546875)
    armor_secondary_detail_roughness_blend.location = (20.0, -392.22430419921875)
    reroute_152.location = (-377.1822509765625, -3950.9873046875)
    reroute_053.location = (-377.1822509765625, -3348.07177734375)
    worn_suit_primary_roughness_remap_z.location = (20.0, -136.16259765625)
    math_008.location = (-1880.5616455078125, -3701.11279296875)
    reroute_190.location = (-377.1822509765625, -1349.905029296875)
    combine_xyz_016.location = (-1881.3702392578125, -3702.39404296875)
    reroute_032.location = (-717.01171875, -3028.1875)
    mix_023.location = (-1880.0921630859375, -3702.02197265625)
    suit_primary_wear_remap_z.location = (20.0, -275.856689453125)
    worn_armor_secondary_roughness_remap_w.location = (-1580.0, 271.220947265625)
    reroute_162.location = (-377.1822509765625, -4238.8603515625)
    worn_cloth_primary_dye_color.location = (20.0, 60.0)
    group_input.location = (-2072.258544921875, -3667.01513671875)
    mix_065.location = (-1882.0794677734375, -3698.79736328125)
    reroute_023.location = (-717.01171875, -1008.372314453125)
    combine_xyz_017.location = (-1881.3702392578125, -3702.51416015625)
    worn_suit_secondary_roughness_remap_w.location = (40.0, -167.92822265625)
    reroute_158.location = (-377.1822509765625, -4143.43115234375)
    suit_primary_iridescence.location = (20.0, -499.947509765625)
    mix_005.location = (-1880.0882568359375, -3701.37353515625)
    math_010.location = (-1882.1995849609375, -3699.0576171875)
    reroute_196.location = (-377.1822509765625, -1446.5072021484375)
    reroute_026.location = (-717.01171875, -3246.12939453125)
    mix_049.location = (-1880.0882568359375, -3704.38232421875)
    math_024.location = (-1880.9088134765625, -3699.49365234375)
    reroute_168.location = (-377.1822509765625, -4399.40869140625)
    mix_028.location = (-1880.0921630859375, -3702.17626953125)
    math_015.location = (-1882.9595947265625, -3698.69775390625)
    reroute_192.location = (-377.1822509765625, -1286.6318359375)
    reroute_076.location = (-377.1822509765625, -300.3265380859375)
    cloth_primary_fuzz.location = (20.0, -512.628173828125)
    armor_primary_wear_remap_w.location = (0.888427734375, 71.297119140625)
    mix.location = (-1880.0882568359375, -3701.19384765625)
    reroute_187.location = (-377.1822509765625, -1126.6097412109375)
    reroute_169.location = (-377.1822509765625, 1070.6392822265625)
    reroute_138.location = (-377.1822509765625, -2173.7353515625)
    reroute_071.location = (-377.1822509765625, -428.3582763671875)
    reroute_033.location = (-717.01171875, -3188.9267578125)
    reroute_157.location = (-377.1822509765625, -4110.927734375)
    reroute_054.location = (-377.1822509765625, -3380.574951171875)
    mix_059.location = (-1880.0870361328125, -3705.96728515625)
    worn_armor_secondary_roughness_remap_z.location = (-1580.0, 303.6099853515625)
    armor_secondary_transmission.location = (20.0, -520.1524047851562)
    reroute_141.location = (-377.1822509765625, -2237.310546875)
    reroute_142.location = (-377.1822509765625, -2269.076171875)
    combine_xyz_043.location = (-1883.5101318359375, -3702.13427734375)
    math_001.location = (-1880.5616455078125, -3700.99267578125)
    cloth_secondary_fuzz.location = (20.0, -468.169189453125)
    reroute_107.location = (-377.1822509765625, 589.4473876953125)
    reroute_365.location = (-377.1822509765625, -1414.7418212890625)
    armor_detail_normal_transform.location = (-913.1685180664062, 727.1367797851562)
    reroute_181.location = (-377.1822509765625, -1030.2899169921875)
    reroute_137.location = (-377.1822509765625, -2141.53662109375)
    reroute_020.location = (-717.01171875, -1222.9573974609375)
    mix_056.location = (-1880.0870361328125, -3705.87744140625)
    reroute_102.location = (-377.1822509765625, 685.6102294921875)
    cloth_secondary_metalness.location = (20.0, -404.205322265625)
    worn_armor_secondary_metalness.location = (-1580.0, 143.1917724609375)
    math_012.location = (-1882.3995361328125, -3698.99755859375)
    separate_xyz_002.location = (-1883.2196044921875, -3699.0576171875)
    math_003.location = (-1882.6995849609375, -3698.53759765625)
    suit_secondary_detail_roughness_blend.location = (40.0, -391.36962890625)
    cloth_secondary_wear_remap_z.location = (20.0, -243.600341796875)
    armor_primary_fuzz.location = (0.0, -153.38067626953125)
    reroute_056.location = (-377.1822509765625, -1794.3809814453125)
    reroute_115.location = (-377.1822509765625, -3538.58544921875)
    reroute_049.location = (-377.1822509765625, -3251.967529296875)
    reroute_096.location = (-377.1822509765625, 1038.5089111328125)
    worn_armor_primary_roughness_remap_x.location = (0.0, -72.19877624511719)
    reroute_025.location = (-939.7109985351562, -1334.8509521484375)
    reroute_055.location = (-377.1822509765625, -3412.60986328125)
    cloth_secondary_roughness_remap_z.location = (20.0, -115.881591796875)
    worn_cloth_primary_roughness_remap_x.location = (20.0, 27.474609375)
    mix_022.location = (-1880.0921630859375, -3701.99169921875)
    reroute_057.location = (-377.1822509765625, -1666.8238525390625)
    suit_primary_roughness_remap_z.location = (20.0, -116.16259765625)
    reroute_081.location = (-377.1822509765625, -172.86279296875)
    worn_cloth_secondary_metalness.location = (20.0, -276.723388671875)
    math_019.location = (-1882.6395263671875, -3698.9775390625)
    combine_xyz_038.location = (-1883.1702880859375, -3701.7138671875)
    suit_secondary_roughness_remap_w.location = (40.0, -167.49462890625)
    math_007.location = (-1880.5616455078125, -3701.07275390625)
    reroute_095.location = (-377.1822509765625, 1006.5672607421875)
    reroute_160.location = (-377.1822509765625, -4175.72802734375)
    reroute_195.location = (-377.1822509765625, -1382.4083251953125)
    reroute_140.location = (-377.1822509765625, -2205.11181640625)
    reroute_021.location = (-717.01171875, -1062.218017578125)
    mix_048.location = (-1880.0882568359375, -3704.35205078125)
    cloth_secondary_detail_roughness_blend.location = (20.0, -372.006591796875)
    reroute_082.location = (-377.1822509765625, -139.966796875)
    armor_secondary_detail_normal_blend.location = (20.0, -360.458984375)
    worn_cloth_primary_roughness_remap_z.location = (20.0, -36.42626953125)
    combine_xyz_036.location = (-1882.6102294921875, -3705.25390625)
    math_002.location = (-1882.9595947265625, -3698.51708984375)
    worn_cloth_secondary_detail_diffuse_blend.location = (20.0, -180.560546875)
    reroute_068.location = (-377.1822509765625, -4650.48876953125)
    math_016.location = (-1882.9595947265625, -3698.87744140625)
    reroute_041.location = (-377.1822509765625, -3060.040283203125)
    worn_suit_primary_detail_diffuse_blend.location = (20.0, -200.12646484375)
    suit_secondary_wear_remap_x.location = (40.0, -199.3896484375)
    armor_primary_transmission.location = (0.0, -185.14605712890625)
    worn_armor_primary_roughness_remap_y.location = (0.0, -103.96417236328125)
    reroute_109.location = (-377.1822509765625, 525.4830322265625)
    reroute_129.location = (-377.1822509765625, -1981.0106201171875)
    reroute_177.location = (-377.1822509765625, 176.5667724609375)
    reroute_014.location = (-717.01171875, 964.4620361328125)
    reroute_130.location = (-377.1822509765625, -2045.8648681640625)
    cloth_secondary_roughness_remap_y.location = (20.0, -84.07275390625)
    cloth_secondary_transmission.location = (20.0, -499.9345703125)
    mix_011.location = (-1880.0931396484375, -3701.6025390625)
    cloth_primary_detail_normal_blend.location = (20.0, -384.5181884765625)
    reroute_171.location = (-377.1822509765625, 336.7640380859375)
    armor_secondary_detail_diffuse_blend.location = (20.0, -327.8268127441406)
    worn_cloth_secondary_detail_roughness_blend.location = (20.0, -244.091064453125)
    mix_066.location = (-1881.8795166015625, -3698.79736328125)
    combine_xyz_003.location = (-1882.1102294921875, -3701.49365234375)
    armor_primary_emission_color.location = (0.0, -217.34481811523438)
    suit_secondary_roughness_remap_z.location = (40.0, -136.1630859375)
    suit_primary_detail_normal_blend.location = (20.0, -404.21826171875)
    reroute_174.location = (-377.1822509765625, 400.8836669921875)
    reroute_191.location = (-377.1822509765625, -1254.7452392578125)
    reroute_016.location = (-717.01171875, 692.674560546875)
    worn_suit_secondary_dye_color.location = (40.0, -40.0)
    reroute_094.location = (-377.1822509765625, 973.5015869140625)
    reroute_131.location = (-377.1822509765625, -1948.9759521484375)
    mix_027.location = (-1880.0921630859375, -3702.14697265625)
    suit_primary_detail_roughness_blend.location = (20.0, -435.983642578125)
    cloth_secondary_detail_normal_blend.location = (20.0, -340.2412109375)
    reroute_077.location = (-377.1822509765625, -268.6146240234375)
    cloth_detail_diffuse_transform.location = (-913.1685180664062, -973.8799438476562)
    suit_primary_roughness_remap_x.location = (20.0, -52.19873046875)
    cloth_primary_emission_color.location = (20.0, -576.59228515625)
    worn_cloth_primary_roughness_remap_y.location = (20.0, -4.475830078125)
    reroute_146.location = (-377.1822509765625, -2365.671875)
    reroute_127.location = (-377.1822509765625, -745.2362060546875)
    worn_suit_primary_detail_roughness_blend.location = (20.0, -263.656982421875)
    mix_006.location = (-1880.0882568359375, -3701.40380859375)
    suit_secondary_wear_remap_w.location = (40.0, -295.552734375)
    reroute_063.location = (-377.1822509765625, -1730.8682861328125)
    reroute_126.location = (-377.1822509765625, -712.7672119140625)
    reroute_018.location = (-939.7109985351562, 909.7139282226562)
    worn_suit_secondary_detail_normal_blend.location = (40.0, -231.892578125)
    reroute_135.location = (-377.1822509765625, -2077.57275390625)
    worn_cloth_secondary_roughness_remap_y.location = (20.0, -84.3974609375)
    reroute_182.location = (-377.1822509765625, -934.41748046875)
    mix_002.location = (-1880.0882568359375, -3701.25390625)
    mix_012.location = (-1880.0931396484375, -3701.63232421875)
    cloth_secondary_detail_diffuse_blend.location = (20.0, -308.04248046875)
    armor_secondary_dye_color.location = (20.0, -40.0)
    suit_primary_detail_diffuse_map.location = (20.0, -180.127197265625)
    reroute_132.location = (-377.1822509765625, -2013.0452880859375)
    worn_cloth_secondary_detail_normal_blend.location = (20.0, -212.32568359375)
    reroute_027.location = (-377.1822509765625, -2521.78955078125)
    reroute_154.location = (-377.1822509765625, -3887.38623046875)
    combine_xyz_020.location = (-1881.3702392578125, -3700.15380859375)
    reroute_099.location = (-377.1822509765625, 845.1400146484375)
    mix_074.location = (-1880.0889892578125, -3702.98974609375)
    armor_primary_roughness_remap_x.location = (0.0, 327.80126953125)
    armor_secondary_metalness.location = (20.0, -423.98968505859375)
    reroute_179.location = (-377.1822509765625, -1215.3074951171875)
    cloth_detail_normal_transform.location = (-913.1685180664062, -1245.697509765625)
    reroute_106.location = (-377.1822509765625, 622.0794677734375)
    reroute_134.location = (-377.1822509765625, -1916.9410400390625)
    mix_013.location = (-1880.0931396484375, -3701.66259765625)
    armor_primary_roughness_remap_w.location = (0.0, 231.2049560546875)
    cloth_secondary_roughness_remap_x.location = (20.0, -52.263916015625)
    reroute_178.location = (-377.1822509765625, 143.9346923828125)
    armor_secondary_wear_remap_z.location = (20.0, -264.27203369140625)
    mix_069.location = (-1882.6395263671875, -3698.79736328125)
    cloth_primary_roughness_remap_x.location = (20.0, -32.303955078125)
    reroute_114.location = (-377.1822509765625, -3667.18017578125)
    armor_primary_dye_color.location = (0.0, 360.0)
    reroute_019.location = (-939.7109985351562, 637.9833984375)
    mix_054.location = (-1880.0882568359375, -3707.41943359375)
    worn_suit_primary_detail_normal_blend.location = (20.43377685546875, -231.458251953125)
    mix_007.location = (-1880.0882568359375, -3701.43408203125)
    suit_secondary_metalness.location = (40.0, -423.568359375)
    reroute_058.location = (-377.1822509765625, -1698.3143310546875)
    worn_armor_primary_roughness_remap_z.location = (0.0, -136.16294860839844)
    reroute_110.location = (-377.1822509765625, 493.2843017578125)
    suit_primary_wear_remap_w.location = (20.0, -307.6220703125)
    worn_armor_primary_roughness_remap_w.location = (0.0, -168.3617401123047)
    reroute_120.location = (-377.1822509765625, -585.2867431640625)
    reroute_091.location = (-377.1822509765625, -3444.64453125)
    cloth_secondary_emission_color.location = (20.0, -532.13330078125)
    reroute_185.location = (-377.1822509765625, -902.113525390625)
    mix_031.location = (-1880.0882568359375, -3702.27197265625)
    reroute_189.location = (-377.1822509765625, -1190.625732421875)
    reroute_128.location = (-377.1822509765625, -777.0906982421875)
    reroute_111.location = (-377.1822509765625, -3570.987548828125)
    reroute_084.location = (-377.1822509765625, 51.98681640625)
    combine_xyz_048.location = (-1880.6287841796875, -3699.41357421875)
    armor_primary_wear_remap_x.location = (0.888427734375, 166.59326171875)
    combine_xyz_033.location = (-1882.5701904296875, -3701.53369140625)
    reroute_079.location = (-377.1822509765625, -332.0384521484375)
    suit_secondary_roughness_remap_y.location = (40.0, -103.96435546875)
    reroute_113.location = (-377.1822509765625, -3634.271728515625)
    armor_primary_roughness_remap_y.location = (0.0, 295.60247802734375)
    combine_xyz_029.location = (-1881.3702392578125, -3706.23388671875)
    worn_armor_primary_detail_diffuse_blend.location = (0.0, -199.70541381835938)
    reroute_103.location = (-377.1822509765625, 653.4114990234375)
    reroute_005.location = (-377.1822509765625, -2868.5810546875)
    mix_015.location = (-1880.0931396484375, -3701.72900390625)
    reroute_145.location = (-377.1822509765625, -2333.4736328125)
    reroute_183.location = (-377.1822509765625, -998.50390625)
    suit_primary_detail_normal_map.location = (20.0, -339.820556640625)
    cloth_primary_detail_normal_map.location = (20.0, -320.298095703125)
    mix_045.location = (-1880.0882568359375, -3704.26220703125)
    mix_029.location = (-1880.0921630859375, -3702.20654296875)
    node.location = (-1880.0958251953125, -3703.15576171875)
    reroute_065.location = (-377.1822509765625, -2681.93701171875)
    cloth_primary_roughness_remap_y.location = (20.0, -64.001220703125)
    mix_070.location = (-1882.4595947265625, -3698.79736328125)
    attribute.location = (-1880.6160888671875, -3699.580078125)
    mix_053.location = (-1880.0882568359375, -3707.3896484375)
    combine_xyz_006.location = (-1882.1102294921875, -3703.97412109375)
    suit_secondary_wear_remap_y.location = (40.0, -232.02197265625)
    combine_xyz_025.location = (-1881.3702392578125, -3704.99365234375)
    reroute_097.location = (-377.1822509765625, 902.1060791015625)
    worn_armor_primary_detail_roughness_blend.location = (0.0, -264.2435302734375)
    reroute_117.location = (-377.1822509765625, -3698.0625)
    reroute_125.location = (-377.1822509765625, -809.1253662109375)
    mix_058.location = (-1880.0870361328125, -3705.93701171875)
    reroute_073.location = (-377.1822509765625, -364.9344482421875)
    suit_primary_metalness.location = (20.0, -468.18212890625)
    cloth_secondary_wear_remap_w.location = (20.0, -275.798828125)
    cloth_secondary_iridescence.location = (20.0, -435.537109375)
    reroute_139.location = (-377.1822509765625, -2301.27490234375)
    armor_secondary_roughness_remap_y.location = (20.0, -104.39756774902344)
    reroute_175.location = (-377.1822509765625, 240.9642333984375)
    suit_secondary_roughness_remap_x.location = (40.0, -71.33203125)
    combine_xyz_041.location = (-1883.1702880859375, -3705.43408203125)
    mix_044.location = (-1880.0882568359375, -3703.49658203125)
    cloth_secondary_roughness_remap_w.location = (20.0, -147.92822265625)
    group_output.location = (-1632.9449462890625, -3703.83984375)
    armor_primary_roughness_remap_z.location = (0.0, 262.97027587890625)
    combine_xyz_027.location = (-1881.3702392578125, -3706.35400390625)
    reroute_188.location = (-377.1822509765625, -1158.3216552734375)
    reroute_122.location = (-377.1822509765625, -649.3729248046875)
    reroute_002.location = (-377.1822509765625, -2900.77978515625)
    worn_suit_secondary_roughness_remap_x.location = (40.0, -71.76513671875)
    suit_primary_detail_diffuse_blend.location = (20.0, -372.01953125)
    cloth_primary_roughness_remap_z.location = (20.0, -96.390380859375)
    combine_xyz_019.location = (-1881.3702392578125, -3703.73388671875)
    armor_secondary_fuzz.location = (20.0, -488.38714599609375)
    reroute_069.location = (-377.1822509765625, -4682.27490234375)
    reroute_030.location = (-939.7109985351562, -3029.08984375)
    reroute_116.location = (-377.1822509765625, -3794.76171875)
    combine_xyz_046.location = (-1883.5101318359375, -3705.85400390625)
    reroute_194.location = (-377.1822509765625, -1055.48974609375)
    worn_armor_secondary_roughness_remap_y.location = (-1580.0, 335.30712890625)
    cloth_primary_dye_color.location = (20.0, 0.0)
    mix_071.location = (-1880.2335205078125, -3699.36474609375)
    reroute_197.location = (-377.1822509765625, -1478.7059326171875)
    armor_primary_detail_diffuse_blend.location = (0.0, 7.17987060546875)
    combine_xyz_002.location = (-1882.1102294921875, -3701.37353515625)
    suit_secondary_iridescence.location = (40.0, -455.76708984375)
    reroute_161.location = (-377.1822509765625, -4207.7626953125)
    armor_primary_wear_remap_y.location = (0.888427734375, 134.827880859375)
    reroute_155.location = (-377.1822509765625, -4046.8583984375)
    reroute_193.location = (-377.1822509765625, -1318.6666259765625)
    mix_033.location = (-1880.0882568359375, -3702.33154296875)
    reroute_080.location = (-377.1822509765625, -204.57470703125)
    suit_secondary_detail_diffuse_blend.location = (40.0, -327.4052734375)
    combine_xyz_042.location = (-1883.1702880859375, -3706.673828125)
    math_018.location = (-1882.6395263671875, -3699.15771484375)
    combine_xyz_018.location = (-1881.3702392578125, -3703.61376953125)
    reroute_166.location = (-377.1822509765625, -4335.44482421875)
    reroute_070.location = (-377.1822509765625, -4714.06103515625)
    worn_suit_primary_roughness_remap_y.location = (20.0, -103.9638671875)
    combine_xyz_008.location = (-1882.1102294921875, -3705.09375)
    worn_armor_primary_metalness.location = (0.0, -295.80975341796875)
    reroute_029.location = (-717.01171875, -2974.341796875)
    reroute_119.location = (-377.1822509765625, -3763.373046875)
    reroute_024.location = (-939.7109985351562, -1063.120361328125)
    worn_cloth_secondary_roughness_remap_x.location = (20.0, -52.632080078125)
    mix_050.location = (-1880.0882568359375, -3707.29931640625)
    suit_primary_emission_color.location = (20.0, -596.1103515625)
    cloth_primary_metalness.location = (20.0, -447.7913818359375)
    worn_cloth_primary_detail_diffuse_blend.location = (20.0, -100.111328125)
    mix_068.location = (-1882.2794189453125, -3698.79736328125)
    mix_026.location = (-1880.0921630859375, -3702.11669921875)
    combine_xyz.location = (-1882.1102294921875, -3700.13427734375)
    armor_primary_metalness.location = (0.0, -88.5496826171875)
    combine_xyz_021.location = (-1881.3702392578125, -3701.39404296875)
    suit_secondary_fuzz.location = (40.0, -487.9658203125)
    mix_003.location = (-1880.0882568359375, -3701.28369140625)
    reroute_176.location = (-377.1822509765625, 208.3321533203125)
    reroute_156.location = (-377.1822509765625, -4079.361328125)
    cloth_primary_roughness_remap_w.location = (20.0, -128.1763916015625)
    reroute_098.location = (-377.1822509765625, 876.9053955078125)
    reroute_136.location = (-377.1822509765625, -2108.90478515625)
    mix_040.location = (-1880.0882568359375, -3703.37646484375)
    reroute_186.location = (-377.1822509765625, -1094.3057861328125)
    cloth_primary_detail_diffuse_blend.location = (20.0, -352.631591796875)
    cloth_primary_detail_diffuse_map.location = (20.0, -160.4803466796875)
    combine_xyz_044.location = (-1883.5101318359375, -3703.37353515625)
    math.location = (-1880.5616455078125, -3700.95263671875)
    armor_primary_detail_normal_map.location = (0.0, 38.5118408203125)
    worn_suit_primary_roughness_remap_x.location = (20.0, -72.19873046875)
    combine_xyz_009.location = (-1882.1102294921875, -3705.2138671875)
    reroute_170.location = (-377.1822509765625, 303.8829345703125)
    worn_cloth_secondary_roughness_remap_z.location = (20.0, -116.59619140625)
    reroute_017.location = (-717.01171875, 749.876953125)
    reroute_118.location = (-377.1822509765625, -3729.451416015625)
    mix_057.location = (-1880.0870361328125, -3705.9072265625)
    mix_004.location = (-1880.0894775390625, -3701.31396484375)
    mix_051.location = (-1880.0882568359375, -3707.32958984375)
    suit_primary_transmission.location = (20.0, -563.91162109375)
    reroute_147.location = (-377.1822509765625, -2397.00390625)
    armor_secondary_emission_color.location = (20.0, -552.3511352539062)
    reroute_151.location = (-377.1822509765625, -3919.4208984375)
    combine_xyz_030.location = (-1882.5701904296875, -3700.2939453125)
    combine_xyz_007.location = (-1882.1102294921875, -3703.85400390625)
    suit_secondary_transmission.location = (40.0, -520.16455078125)
    mix_017.location = (-1880.0931396484375, -3701.78857421875)
    armor_primary_detail_diffuse_map.location = (0.0, 199.07232666015625)
    reroute_015.location = (-717.01171875, 910.6163330078125)
    reroute_104.location = (-377.1822509765625, 916.9053955078125)
    reroute_048.location = (-377.1822509765625, -3220.167236328125)
    mix_020.location = (-1880.0921630859375, -3701.93212890625)
    reroute_078.location = (-377.1822509765625, -236.3106689453125)
    mix_055.location = (-1880.0870361328125, -3705.84716796875)
    armor_secondary_roughness_remap_x.location = (20.0, -72.19877624511719)
    cloth_primary_iridescence.location = (20.0, -480.294677734375)
    reroute_059.location = (-377.1822509765625, -1634.8016357421875)
    cloth_primary_wear_remap_x.location = (20.0, -192.1922607421875)
    mix_021.location = (-1880.0921630859375, -3701.9619140625)
    reroute_022.location = (-717.01171875, -1280.1597900390625)
    mix_019.location = (-1880.0931396484375, -3701.8486328125)
    combine_xyz_039.location = (-1883.1702880859375, -3702.95361328125)
    worn_armor_secondary_roughness_remap_x.location = (-1580.0, 367.6961669921875)
    mix_073.location = (-1880.0904541015625, -3702.95849609375)
    worn_suit_primary_dye_color.location = (20.0, -40.0)
    combine_xyz_024.location = (-1881.3702392578125, -3704.87353515625)
    reroute_172.location = (-377.1822509765625, 369.0975341796875)
    armor_detail_diffuse_transform.location = (-913.1685180664062, 998.9544067382812)
    reroute.location = (-377.1822509765625, -2932.545166015625)
    reroute_105.location = (-377.1822509765625, 758.5118408203125)
    mix_052.location = (-1880.0882568359375, -3707.35986328125)
    cloth_secondary_wear_remap_x.location = (20.0, -180.069580078125)
    armor_secondary_wear_remap_y.location = (20.0, -231.63986206054688)
    worn_cloth_primary_detail_roughness_blend.location = (20.0, -164.687744140625)
    math_021.location = (-1882.6395263671875, -3699.517578125)
    math_004.location = (-1880.5616455078125, -3701.03271484375)
    combine_xyz_031.location = (-1883.1702880859375, -3700.47412109375)
    combine_xyz_023.location = (-1881.3702392578125, -3703.85400390625)
    armor_secondary_iridescence.location = (20.0, -456.1883544921875)
    reroute_047.location = (-377.1822509765625, -4746.94189453125)
    cloth_primary_detail_roughness_blend.location = (20.0, -416.552978515625)
    combine_xyz_004.location = (-1882.1102294921875, -3702.73388671875)
    reroute_159.location = (-377.1822509765625, -4271.36376953125)
    worn_armor_primary_dye_color.location = (0.0, -40.0)
    reroute_173.location = (-377.1822509765625, 272.6444091796875)
    reroute_052.location = (-377.1822509765625, -3181.467529296875)
    suit_detail_normal_transform.location = (-913.1685180664062, -3211.6669921875)
    mix_008.location = (-1880.0882568359375, -3701.4638671875)
    suit_primary_roughness_remap_y.location = (20.0, -83.9638671875)
    mix_042.location = (-1880.0882568359375, -3703.43603515625)
    combine_xyz_026.location = (-1881.3702392578125, -3705.11376953125)
    reroute_123.location = (-377.1822509765625, -681.7620849609375)
    reroute_148.location = (-377.1822509765625, -2429.20263671875)
    worn_armor_secondary_detail_roughness_blend.location = (-1580.0, 175.2264404296875)
    reroute_066.location = (-377.1822509765625, -2713.26904296875)
    cloth_primary_wear_remap_y.location = (20.0, -224.4962158203125)
    combine_xyz_034.location = (-1882.5701904296875, -3702.77392578125)
    reroute_064.location = (-377.1822509765625, -1762.3587646484375)
    armor_primary_wear_remap_z.location = (0.888427734375, 103.49591064453125)
    suit_secondary_emission_color.location = (40.0, -551.49658203125)
    reroute_028.location = (-377.1822509765625, -4490.12353515625)
    reroute_092.location = (-377.1822509765625, 781.6092529296875)
    reroute_009.location = (-377.1822509765625, -2649.73828125)
    reroute_067.location = (-377.1822509765625, -2745.034423828125)
    reroute_150.location = (-377.1822509765625, -3983.490234375)
    reroute_083.location = (-377.1822509765625, -108.25487518310547)
    mix_041.location = (-1880.0882568359375, -3703.40576171875)
    mix_010.location = (-1880.0931396484375, -3701.57275390625)
    suit_primary_fuzz.location = (20.0, -532.146240234375)
    mix_032.location = (-1880.0882568359375, -3702.3017578125)
    worn_cloth_primary_detail_normal_blend.location = (20.0, -132.66552734375)
    reroute_045.location = (-377.1822509765625, -2777.66650390625)
    mix_072.location = (-1881.4156494140625, -3699.21630859375)
    armor_secondary_wear_remap_w.location = (20.0, -296.4707946777344)
    worn_armor_secondary_dye_color.location = (-1580.0, 400.0)
    combine_xyz_035.location = (-1882.5902099609375, -3704.01416015625)
    math_013.location = (-1881.9395751953125, -3699.11767578125)
    combine_xyz_001.location = (-1882.1102294921875, -3700.25390625)
    suit_secondary_dye_color.location = (40.0, -40.0)
    armor_primary_detail_normal_blend.location = (0.0, -25.0189208984375)
    cloth_secondary_wear_remap_y.location = (20.0, -211.8349609375)
    combine_xyz_045.location = (-1883.5101318359375, -3704.61376953125)
    reroute_167.location = (-377.1822509765625, -4367.2099609375)
    combine_xyz_011.location = (-1882.1102294921875, -3706.45361328125)
    reroute_149.location = (-377.1822509765625, -4015.056640625)
    detail_uvs.location = (-1107.0074462890625, 919.2714233398438)
    reroute_121.location = (-377.1822509765625, -616.9840087890625)
    reroute_044.location = (-377.1822509765625, -3156.203125)
    mix_018.location = (-1880.0931396484375, -3701.81884765625)
    worn_cloth_secondary_dye_color.location = (20.0, -20.0)
    reroute_087.location = (-377.1822509765625, -44.92504119873047)
    mix_043.location = (-1880.0882568359375, -3703.46630859375)
    worn_cloth_primary_metalness.location = (20.0, -196.7099609375)
    armor_secondary_roughness_remap_z.location = (20.0, -136.16293334960938)
    cloth_primary_wear_remap_z.location = (20.0, -256.2081298828125)
    reroute_008.location = (-377.1822509765625, -2617.10595703125)
    combine_xyz_032.location = (-1883.5101318359375, -3700.89404296875)
    combine_xyz_028.location = (-1881.3702392578125, -3706.11376953125)
    math_023.location = (-1881.0887451171875, -3699.49365234375)
    mix_016.location = (-1880.0931396484375, -3701.75830078125)
    worn_suit_secondary_metalness.location = (40.0, -296.2900390625)
    mix_014.location = (-1880.0931396484375, -3701.69287109375)
    reroute_100.location = (-377.1822509765625, 813.8079833984375)
    reroute_060.location = (-377.1822509765625, -1602.2474365234375)
    worn_suit_secondary_detail_roughness_blend.location = (40.0, -264.09130859375)
    combine_xyz_040.location = (-1883.1702880859375, -3704.19384765625)
    reroute_001.location = (-377.1822509765625, -2996.50927734375)
    reroute_090.location = (-377.1822509765625, -460.662109375)
    mix_025.location = (-1880.0921630859375, -3702.08642578125)
    reroute_124.location = (-377.1822509765625, -552.2911376953125)
    cloth_secondary_dye_color.location = (20.0, -20.0)
    worn_cloth_primary_roughness_remap_w.location = (20.0, -68.620849609375)
    combine_xyz_010.location = (-1882.1102294921875, -3706.333984375)
    worn_armor_primary_detail_normal_blend.location = (0.0, -231.7401885986328)
    suit_secondary_detail_normal_blend.location = (40.0, -360.03759765625)
    combine_xyz_015.location = (-1881.3702392578125, -3701.27392578125)
    worn_suit_secondary_roughness_remap_y.location = (40.0, -103.96435546875)
    reroute_012.location = (-377.1822509765625, -4586.443359375)
    reroute_013.location = (-377.1822509765625, -4618.703125)
    armor_primary_iridescence.location = (0.0, -121.181884765625)
    reroute_089.location = (-377.1822509765625, 19.682861328125)
    combine_xyz_012.location = (-1881.3702392578125, -3699.91357421875)
    reroute_062.location = (-377.1822509765625, -1826.4031982421875)
    reroute_101.location = (-377.1822509765625, 717.8089599609375)
    reroute_043.location = (-377.1822509765625, -3124.437744140625)
    reroute_086.location = (-377.1822509765625, -76.63695526123047)
    reroute_031.location = (-939.7109985351562, -3300.8203125)
    mix_030.location = (-1880.0882568359375, -3702.24169921875)
    reroute_050.location = (-377.1822509765625, -3284.470947265625)
    suit_primary_wear_remap_x.location = (20.0, -211.892578125)
    armor_secondary_roughness_remap_w.location = (20.0, -168.3616943359375)
    combine_xyz_037.location = (-1882.6102294921875, -3706.49365234375)
    cloth_primary_wear_remap_w.location = (20.0, -288.5120849609375)
    reroute_007.location = (-377.1822509765625, -2585.774169921875)
    combine_xyz_005.location = (-1882.1102294921875, -3702.61376953125)
    reroute_112.location = (-377.1822509765625, -3602.8828125)
    reroute_006.location = (-377.1822509765625, -2553.5751953125)
    armor_primary_detail_roughness_blend.location = (0.0, -57.21771240234375)
    reroute_010.location = (-377.1822509765625, -4554.1396484375)
    reroute_184.location = (-377.1822509765625, -1070.7535400390625)
    suit_secondary_wear_remap_z.location = (40.0, -264.220703125)
    separate_xyz_001.location = (-1880.8416748046875, -3700.99267578125)
    reroute_093.location = (-377.1822509765625, 940.8695068359375)
    reroute_061.location = (-377.1822509765625, -1570.7572021484375)
    worn_armor_secondary_detail_normal_blend.location = (-1580.0, 207.62841796875)
    reroute_051.location = (-377.1822509765625, -3021.773681640625)
    mix_034.location = (-1880.0882568359375, -3702.36181640625)
    math_022.location = (-1880.9088134765625, -3699.33349609375)
    reroute_072.location = (-377.1822509765625, -396.0543212890625)
    worn_suit_primary_metalness.location = (20.0, -295.85546875)
    cloth_primary_transmission.location = (20.0, -544.3935546875)
    suit_primary_dye_color.location = (20.0, -20.0)
    mix_076.location = (-1880.0943603515625, -3703.18701171875)
    mix_067.location = (-1881.6595458984375, -3698.87744140625)
    worn_cloth_secondary_roughness_remap_w.location = (20.0, -148.361572265625)
    combine_xyz_013.location = (-1881.3702392578125, -3700.03369140625)
    reroute_011.location = (-377.1822509765625, -4522.427734375)
    combine_xyz_014.location = (-1881.3702392578125, -3701.15380859375)
    worn_suit_primary_roughness_remap_w.location = (20.0, -167.927734375)
    worn_suit_secondary_roughness_remap_z.location = (40.0, -136.1630859375)
    reroute_165.location = (-377.1822509765625, -4302.9296875)
    combine_xyz_047.location = (-1883.5101318359375, -3707.09375)
    reroute_108.location = (-377.1822509765625, 557.2484130859375)
    reroute_042.location = (-377.1822509765625, -3092.67236328125)
    mix_024.location = (-1880.0921630859375, -3702.05224609375)
    reroute_046.location = (-377.1822509765625, -3316.505615234375)
    reroute_180.location = (-377.1822509765625, -966.11474609375)
    suit_primary_wear_remap_y.location = (20.0, -244.09130859375)
    mix_001.location = (-1880.0882568359375, -3701.22412109375)
    mix_009.location = (-1880.0882568359375, -3701.49365234375)
    mix_046.location = (-1880.0882568359375, -3704.29248046875)
    armor_secondary_wear_remap_x.location = (20.0, -200.30789184570312)
    worn_armor_secondary_detail_diffuse_blend.location = (-1580.0, 239.5238037109375)
    math_005.location = (-1882.6995849609375, -3698.37744140625)
    reroute_088.location = (-377.1822509765625, -12.029052734375)
    combine_xyz_022.location = (-1881.3702392578125, -3702.63427734375)

    worn_suit_secondary_detail_diffuse_blend.width, worn_suit_secondary_detail_diffuse_blend.height = 140.0, 100.0
    suit_detail_diffuse_transform.width, suit_detail_diffuse_transform.height = 175.1947021484375, 100.0
    suit_primary_roughness_remap_w.width, suit_primary_roughness_remap_w.height = 156.03314208984375, 100.0
    armor_secondary_detail_roughness_blend.width, armor_secondary_detail_roughness_blend.height = 140.0, 100.0
    worn_suit_primary_roughness_remap_z.width, worn_suit_primary_roughness_remap_z.height = 156.03317260742188, 100.0
    suit_primary_wear_remap_z.width, suit_primary_wear_remap_z.height = 156.03314208984375, 100.0
    worn_armor_secondary_roughness_remap_w.width, worn_armor_secondary_roughness_remap_w.height = 156.46646118164062, 100.0
    worn_cloth_primary_dye_color.width, worn_cloth_primary_dye_color.height = 140.0, 100.0
    worn_suit_secondary_roughness_remap_w.width, worn_suit_secondary_roughness_remap_w.height = 156.46649169921875, 100.0
    suit_primary_iridescence.width, suit_primary_iridescence.height = 140.0, 100.0
    cloth_primary_fuzz.width, cloth_primary_fuzz.height = 140.0, 100.0
    armor_primary_wear_remap_w.width, armor_primary_wear_remap_w.height = 155.1663818359375, 100.0
    worn_armor_secondary_roughness_remap_z.width, worn_armor_secondary_roughness_remap_z.height = 156.03314208984375, 100.0
    armor_secondary_transmission.width, armor_secondary_transmission.height = 140.0, 100.0
    cloth_secondary_fuzz.width, cloth_secondary_fuzz.height = 140.0, 100.0
    armor_detail_normal_transform.width, armor_detail_normal_transform.height = 176.44476318359375, 100.0
    cloth_secondary_metalness.width, cloth_secondary_metalness.height = 140.0, 100.0
    worn_armor_secondary_metalness.width, worn_armor_secondary_metalness.height = 140.0, 100.0
    suit_secondary_detail_roughness_blend.width, suit_secondary_detail_roughness_blend.height = 140.0, 100.0
    cloth_secondary_wear_remap_z.width, cloth_secondary_wear_remap_z.height = 155.92791748046875, 100.0
    armor_primary_fuzz.width, armor_primary_fuzz.height = 140.0, 100.0
    worn_armor_primary_roughness_remap_x.width, worn_armor_primary_roughness_remap_x.height = 155.01483154296875, 100.0
    cloth_secondary_roughness_remap_z.width, cloth_secondary_roughness_remap_z.height = 156.39630126953125, 100.0
    worn_cloth_primary_roughness_remap_x.width, worn_cloth_primary_roughness_remap_x.height = 156.39630126953125, 100.0
    suit_primary_roughness_remap_z.width, suit_primary_roughness_remap_z.height = 156.03314208984375, 100.0
    worn_cloth_secondary_metalness.width, worn_cloth_secondary_metalness.height = 140.0, 100.0
    suit_secondary_roughness_remap_w.width, suit_secondary_roughness_remap_w.height = 155.599853515625, 100.0
    cloth_secondary_detail_roughness_blend.width, cloth_secondary_detail_roughness_blend.height = 140.0, 100.0
    armor_secondary_detail_normal_blend.width, armor_secondary_detail_normal_blend.height = 140.0, 100.0
    worn_cloth_primary_roughness_remap_z.width, worn_cloth_primary_roughness_remap_z.height = 156.86474609375, 100.0
    worn_cloth_secondary_detail_diffuse_blend.width, worn_cloth_secondary_detail_diffuse_blend.height = 140.0, 100.0
    worn_suit_primary_detail_diffuse_blend.width, worn_suit_primary_detail_diffuse_blend.height = 140.0, 100.0
    suit_secondary_wear_remap_x.width, suit_secondary_wear_remap_x.height = 156.46649169921875, 100.0
    armor_primary_transmission.width, armor_primary_transmission.height = 140.0, 100.0
    worn_armor_primary_roughness_remap_y.width, worn_armor_primary_roughness_remap_y.height = 155.46978759765625, 100.0
    cloth_secondary_roughness_remap_y.width, cloth_secondary_roughness_remap_y.height = 155.92782592773438, 100.0
    cloth_secondary_transmission.width, cloth_secondary_transmission.height = 140.0, 100.0
    cloth_primary_detail_normal_blend.width, cloth_primary_detail_normal_blend.height = 140.0, 100.0
    armor_secondary_detail_diffuse_blend.width, armor_secondary_detail_diffuse_blend.height = 140.0, 100.0
    worn_cloth_secondary_detail_roughness_blend.width, worn_cloth_secondary_detail_roughness_blend.height = 140.0, 100.0
    armor_primary_emission_color.width, armor_primary_emission_color.height = 140.0, 100.0
    suit_secondary_roughness_remap_z.width, suit_secondary_roughness_remap_z.height = 156.89984130859375, 100.0
    suit_primary_detail_normal_blend.width, suit_primary_detail_normal_blend.height = 140.0, 100.0
    worn_suit_secondary_dye_color.width, worn_suit_secondary_dye_color.height = 140.0, 100.0
    suit_primary_detail_roughness_blend.width, suit_primary_detail_roughness_blend.height = 140.0, 100.0
    cloth_secondary_detail_normal_blend.width, cloth_secondary_detail_normal_blend.height = 140.0, 100.0
    cloth_detail_diffuse_transform.width, cloth_detail_diffuse_transform.height = 175.1947021484375, 100.0
    suit_primary_roughness_remap_x.width, suit_primary_roughness_remap_x.height = 156.206298828125, 100.0
    cloth_primary_emission_color.width, cloth_primary_emission_color.height = 140.0, 100.0
    worn_cloth_primary_roughness_remap_y.width, worn_cloth_primary_roughness_remap_y.height = 156.86471557617188, 100.0
    worn_suit_primary_detail_roughness_blend.width, worn_suit_primary_detail_roughness_blend.height = 140.0, 100.0
    suit_secondary_wear_remap_w.width, suit_secondary_wear_remap_w.height = 156.8997802734375, 100.0
    worn_suit_secondary_detail_normal_blend.width, worn_suit_secondary_detail_normal_blend.height = 140.0, 100.0
    worn_cloth_secondary_roughness_remap_y.width, worn_cloth_secondary_roughness_remap_y.height = 156.46652221679688, 100.0
    cloth_secondary_detail_diffuse_blend.width, cloth_secondary_detail_diffuse_blend.height = 140.0, 100.0
    armor_secondary_dye_color.width, armor_secondary_dye_color.height = 140.0, 100.0
    suit_primary_detail_diffuse_map.width, suit_primary_detail_diffuse_map.height = 240.0, 100.0
    worn_cloth_secondary_detail_normal_blend.width, worn_cloth_secondary_detail_normal_blend.height = 140.0, 100.0
    armor_primary_roughness_remap_x.width, armor_primary_roughness_remap_x.height = 155.72564697265625, 100.0
    armor_secondary_metalness.width, armor_secondary_metalness.height = 140.0, 100.0
    cloth_detail_normal_transform.width, cloth_detail_normal_transform.height = 176.44476318359375, 100.0
    armor_primary_roughness_remap_w.width, armor_primary_roughness_remap_w.height = 156.45462036132812, 100.0
    cloth_secondary_roughness_remap_x.width, cloth_secondary_roughness_remap_x.height = 156.86474609375, 100.0
    armor_secondary_wear_remap_z.width, armor_secondary_wear_remap_z.height = 156.03314208984375, 100.0
    cloth_primary_roughness_remap_x.width, cloth_primary_roughness_remap_x.height = 155.92779541015625, 100.0
    armor_primary_dye_color.width, armor_primary_dye_color.height = 140.0, 100.0
    worn_suit_primary_detail_normal_blend.width, worn_suit_primary_detail_normal_blend.height = 140.0, 100.0
    suit_secondary_metalness.width, suit_secondary_metalness.height = 140.0, 100.0
    worn_armor_primary_roughness_remap_z.width, worn_armor_primary_roughness_remap_z.height = 155.92486572265625, 100.0
    suit_primary_wear_remap_w.width, suit_primary_wear_remap_w.height = 155.59979248046875, 100.0
    worn_armor_primary_roughness_remap_w.width, worn_armor_primary_roughness_remap_w.height = 156.83477783203125, 100.0
    cloth_secondary_emission_color.width, cloth_secondary_emission_color.height = 140.0, 100.0
    armor_primary_wear_remap_x.width, armor_primary_wear_remap_x.height = 154.7330322265625, 100.0
    suit_secondary_roughness_remap_y.width, suit_secondary_roughness_remap_y.height = 156.89984130859375, 100.0
    armor_primary_roughness_remap_y.width, armor_primary_roughness_remap_y.height = 156.39605712890625, 100.0
    worn_armor_primary_detail_diffuse_blend.width, worn_armor_primary_detail_diffuse_blend.height = 140.0, 100.0
    suit_primary_detail_normal_map.width, suit_primary_detail_normal_map.height = 240.0, 100.0
    cloth_primary_detail_normal_map.width, cloth_primary_detail_normal_map.height = 240.0, 100.0
    node.width, node.height = 140.0, 100.0
    cloth_primary_roughness_remap_y.width, cloth_primary_roughness_remap_y.height = 156.39620971679688, 100.0
    suit_secondary_wear_remap_y.width, suit_secondary_wear_remap_y.height = 156.46649169921875, 100.0
    worn_armor_primary_detail_roughness_blend.width, worn_armor_primary_detail_roughness_blend.height = 140.0, 100.0
    suit_primary_metalness.width, suit_primary_metalness.height = 140.0, 100.0
    cloth_secondary_wear_remap_w.width, cloth_secondary_wear_remap_w.height = 156.39642333984375, 100.0
    cloth_secondary_iridescence.width, cloth_secondary_iridescence.height = 140.0, 100.0
    armor_secondary_roughness_remap_y.width, armor_secondary_roughness_remap_y.height = 155.599853515625, 100.0
    suit_secondary_roughness_remap_x.width, suit_secondary_roughness_remap_x.height = 156.46649169921875, 100.0
    cloth_secondary_roughness_remap_w.width, cloth_secondary_roughness_remap_w.height = 156.39627075195312, 100.0
    armor_primary_roughness_remap_z.width, armor_primary_roughness_remap_z.height = 156.48974609375, 100.0
    worn_suit_secondary_roughness_remap_x.width, worn_suit_secondary_roughness_remap_x.height = 156.466552734375, 100.0
    suit_primary_detail_diffuse_blend.width, suit_primary_detail_diffuse_blend.height = 140.0, 100.0
    cloth_primary_roughness_remap_z.width, cloth_primary_roughness_remap_z.height = 156.86474609375, 100.0
    armor_secondary_fuzz.width, armor_secondary_fuzz.height = 140.0, 100.0
    worn_armor_secondary_roughness_remap_y.width, worn_armor_secondary_roughness_remap_y.height = 155.599853515625, 100.0
    cloth_primary_dye_color.width, cloth_primary_dye_color.height = 140.0, 100.0
    armor_primary_detail_diffuse_blend.width, armor_primary_detail_diffuse_blend.height = 140.0, 100.0
    suit_secondary_iridescence.width, suit_secondary_iridescence.height = 140.0, 100.0
    armor_primary_wear_remap_y.width, armor_primary_wear_remap_y.height = 154.73309326171875, 100.0
    suit_secondary_detail_diffuse_blend.width, suit_secondary_detail_diffuse_blend.height = 140.0, 100.0
    worn_suit_primary_roughness_remap_y.width, worn_suit_primary_roughness_remap_y.height = 156.03314208984375, 100.0
    worn_armor_primary_metalness.width, worn_armor_primary_metalness.height = 140.0, 100.0
    worn_cloth_secondary_roughness_remap_x.width, worn_cloth_secondary_roughness_remap_x.height = 155.599853515625, 100.0
    suit_primary_emission_color.width, suit_primary_emission_color.height = 140.0, 100.0
    cloth_primary_metalness.width, cloth_primary_metalness.height = 140.0, 100.0
    worn_cloth_primary_detail_diffuse_blend.width, worn_cloth_primary_detail_diffuse_blend.height = 140.0, 100.0
    armor_primary_metalness.width, armor_primary_metalness.height = 140.0, 100.0
    suit_secondary_fuzz.width, suit_secondary_fuzz.height = 140.0, 100.0
    cloth_primary_roughness_remap_w.width, cloth_primary_roughness_remap_w.height = 155.45928955078125, 100.0
    cloth_primary_detail_diffuse_blend.width, cloth_primary_detail_diffuse_blend.height = 140.0, 100.0
    cloth_primary_detail_diffuse_map.width, cloth_primary_detail_diffuse_map.height = 240.0, 100.0
    armor_primary_detail_normal_map.width, armor_primary_detail_normal_map.height = 240.0, 100.0
    worn_suit_primary_roughness_remap_x.width, worn_suit_primary_roughness_remap_x.height = 156.03314208984375, 100.0
    worn_cloth_secondary_roughness_remap_z.width, worn_cloth_secondary_roughness_remap_z.height = 156.03317260742188, 100.0
    suit_primary_transmission.width, suit_primary_transmission.height = 140.0, 100.0
    armor_secondary_emission_color.width, armor_secondary_emission_color.height = 140.0, 100.0
    suit_secondary_transmission.width, suit_secondary_transmission.height = 140.0, 100.0
    armor_primary_detail_diffuse_map.width, armor_primary_detail_diffuse_map.height = 240.0, 100.0
    armor_secondary_roughness_remap_x.width, armor_secondary_roughness_remap_x.height = 156.03314208984375, 100.0
    cloth_primary_iridescence.width, cloth_primary_iridescence.height = 140.0, 100.0
    cloth_primary_wear_remap_x.width, cloth_primary_wear_remap_x.height = 156.4664306640625, 100.0
    worn_armor_secondary_roughness_remap_x.width, worn_armor_secondary_roughness_remap_x.height = 155.16650390625, 100.0
    worn_suit_primary_dye_color.width, worn_suit_primary_dye_color.height = 140.0, 100.0
    armor_detail_diffuse_transform.width, armor_detail_diffuse_transform.height = 175.1947021484375, 100.0
    cloth_secondary_wear_remap_x.width, cloth_secondary_wear_remap_x.height = 156.3963623046875, 100.0
    armor_secondary_wear_remap_y.width, armor_secondary_wear_remap_y.height = 156.03314208984375, 100.0
    worn_cloth_primary_detail_roughness_blend.width, worn_cloth_primary_detail_roughness_blend.height = 140.0, 100.0
    armor_secondary_iridescence.width, armor_secondary_iridescence.height = 140.0, 100.0
    cloth_primary_detail_roughness_blend.width, cloth_primary_detail_roughness_blend.height = 140.0, 100.0
    worn_armor_primary_dye_color.width, worn_armor_primary_dye_color.height = 140.0, 100.0
    suit_detail_normal_transform.width, suit_detail_normal_transform.height = 176.44476318359375, 100.0
    suit_primary_roughness_remap_y.width, suit_primary_roughness_remap_y.height = 156.4664306640625, 100.0
    worn_armor_secondary_detail_roughness_blend.width, worn_armor_secondary_detail_roughness_blend.height = 140.0, 100.0
    cloth_primary_wear_remap_y.width, cloth_primary_wear_remap_y.height = 155.59979248046875, 100.0
    armor_primary_wear_remap_z.width, armor_primary_wear_remap_z.height = 155.59967041015625, 100.0
    suit_secondary_emission_color.width, suit_secondary_emission_color.height = 140.0, 100.0
    suit_primary_fuzz.width, suit_primary_fuzz.height = 140.0, 100.0
    worn_cloth_primary_detail_normal_blend.width, worn_cloth_primary_detail_normal_blend.height = 140.0, 100.0
    armor_secondary_wear_remap_w.width, armor_secondary_wear_remap_w.height = 156.033203125, 100.0
    worn_armor_secondary_dye_color.width, worn_armor_secondary_dye_color.height = 140.0, 100.0
    suit_secondary_dye_color.width, suit_secondary_dye_color.height = 140.0, 100.0
    armor_primary_detail_normal_blend.width, armor_primary_detail_normal_blend.height = 140.0, 100.0
    cloth_secondary_wear_remap_y.width, cloth_secondary_wear_remap_y.height = 155.92794799804688, 100.0
    worn_cloth_secondary_dye_color.width, worn_cloth_secondary_dye_color.height = 140.0, 100.0
    worn_cloth_primary_metalness.width, worn_cloth_primary_metalness.height = 140.0, 100.0
    armor_secondary_roughness_remap_z.width, armor_secondary_roughness_remap_z.height = 156.8997802734375, 100.0
    cloth_primary_wear_remap_z.width, cloth_primary_wear_remap_z.height = 156.4664306640625, 100.0
    worn_suit_secondary_metalness.width, worn_suit_secondary_metalness.height = 140.0, 100.0
    worn_suit_secondary_detail_roughness_blend.width, worn_suit_secondary_detail_roughness_blend.height = 140.0, 100.0
    cloth_secondary_dye_color.width, cloth_secondary_dye_color.height = 140.0, 100.0
    worn_cloth_primary_roughness_remap_w.width, worn_cloth_primary_roughness_remap_w.height = 156.39630126953125, 100.0
    worn_armor_primary_detail_normal_blend.width, worn_armor_primary_detail_normal_blend.height = 140.0, 100.0
    suit_secondary_detail_normal_blend.width, suit_secondary_detail_normal_blend.height = 140.0, 100.0
    worn_suit_secondary_roughness_remap_y.width, worn_suit_secondary_roughness_remap_y.height = 156.46649169921875, 100.0
    armor_primary_iridescence.width, armor_primary_iridescence.height = 140.0, 100.0
    suit_primary_wear_remap_x.width, suit_primary_wear_remap_x.height = 156.46649169921875, 100.0
    armor_secondary_roughness_remap_w.width, armor_secondary_roughness_remap_w.height = 155.599853515625, 100.0
    cloth_primary_wear_remap_w.width, cloth_primary_wear_remap_w.height = 155.16650390625, 100.0
    armor_primary_detail_roughness_blend.width, armor_primary_detail_roughness_blend.height = 140.0, 100.0
    suit_secondary_wear_remap_z.width, suit_secondary_wear_remap_z.height = 156.46646118164062, 100.0
    worn_armor_secondary_detail_normal_blend.width, worn_armor_secondary_detail_normal_blend.height = 140.0, 100.0
    worn_suit_primary_metalness.width, worn_suit_primary_metalness.height = 140.0, 100.0
    cloth_primary_transmission.width, cloth_primary_transmission.height = 140.0, 100.0
    suit_primary_dye_color.width, suit_primary_dye_color.height = 140.0, 100.0
    worn_cloth_secondary_roughness_remap_w.width, worn_cloth_secondary_roughness_remap_w.height = 155.599853515625, 100.0
    worn_suit_primary_roughness_remap_w.width, worn_suit_primary_roughness_remap_w.height = 155.599853515625, 100.0
    worn_suit_secondary_roughness_remap_z.width, worn_suit_secondary_roughness_remap_z.height = 156.46649169921875, 100.0
    suit_primary_wear_remap_y.width, suit_primary_wear_remap_y.height = 155.599853515625, 100.0
    armor_secondary_wear_remap_x.width, armor_secondary_wear_remap_x.height = 156.46649169921875, 100.0
    worn_armor_secondary_detail_diffuse_blend.width, worn_armor_secondary_detail_diffuse_blend.height = 140.0, 100.0

# Activate Node Connection usage
    link = Shader_Preset.links.new
    Shader_Preset.links.new(mix.outputs[0], mix_001.inputs[1])
    Shader_Preset.links.new(mix_001.outputs[0], mix_002.inputs[1])
    Shader_Preset.links.new(mix_002.outputs[0], mix_003.inputs[1])
    Shader_Preset.links.new(mix_003.outputs[0], mix_004.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_001.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_002.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_003.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_004.inputs[0])
    Shader_Preset.links.new(mix_005.outputs[0], mix_006.inputs[1])
    Shader_Preset.links.new(mix_006.outputs[0], mix_007.inputs[1])
    Shader_Preset.links.new(mix_007.outputs[0], mix_008.inputs[1])
    Shader_Preset.links.new(mix_008.outputs[0], mix_009.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_005.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_006.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_007.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_008.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_009.inputs[0])
    Shader_Preset.links.new(mix_010.outputs[0], mix_011.inputs[1])
    Shader_Preset.links.new(mix_011.outputs[0], mix_012.inputs[1])
    Shader_Preset.links.new(mix_012.outputs[0], mix_013.inputs[1])
    Shader_Preset.links.new(mix_013.outputs[0], mix_014.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_010.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_011.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_012.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_013.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_014.inputs[0])
    Shader_Preset.links.new(mix_015.outputs[0], mix_016.inputs[1])
    Shader_Preset.links.new(mix_016.outputs[0], mix_017.inputs[1])
    Shader_Preset.links.new(mix_017.outputs[0], mix_018.inputs[1])
    Shader_Preset.links.new(mix_018.outputs[0], mix_019.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_015.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_016.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_017.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_018.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_019.inputs[0])
    Shader_Preset.links.new(mix_020.outputs[0], mix_021.inputs[1])
    Shader_Preset.links.new(mix_021.outputs[0], mix_022.inputs[1])
    Shader_Preset.links.new(mix_022.outputs[0], mix_023.inputs[1])
    Shader_Preset.links.new(mix_023.outputs[0], mix_024.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_020.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_021.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_022.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_023.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_024.inputs[0])
    Shader_Preset.links.new(mix_025.outputs[0], mix_026.inputs[1])
    Shader_Preset.links.new(mix_026.outputs[0], mix_027.inputs[1])
    Shader_Preset.links.new(mix_027.outputs[0], mix_028.inputs[1])
    Shader_Preset.links.new(mix_028.outputs[0], mix_029.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_025.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_026.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_027.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_028.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_029.inputs[0])
    Shader_Preset.links.new(mix_030.outputs[0], mix_031.inputs[1])
    Shader_Preset.links.new(mix_031.outputs[0], mix_032.inputs[1])
    Shader_Preset.links.new(mix_032.outputs[0], mix_033.inputs[1])
    Shader_Preset.links.new(mix_033.outputs[0], mix_034.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_030.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_031.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_032.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_033.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_034.inputs[0])
    Shader_Preset.links.new(mix_040.outputs[0], mix_041.inputs[1])
    Shader_Preset.links.new(mix_041.outputs[0], mix_042.inputs[1])
    Shader_Preset.links.new(mix_042.outputs[0], mix_043.inputs[1])
    Shader_Preset.links.new(mix_043.outputs[0], mix_044.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_040.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_041.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_042.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_043.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_044.inputs[0])
    Shader_Preset.links.new(mix_045.outputs[0], mix_046.inputs[1])
    Shader_Preset.links.new(mix_046.outputs[0], mix_047.inputs[1])
    Shader_Preset.links.new(mix_047.outputs[0], mix_048.inputs[1])
    Shader_Preset.links.new(mix_048.outputs[0], mix_049.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_045.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_046.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_047.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_048.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_049.inputs[0])
    Shader_Preset.links.new(mix_050.outputs[0], mix_051.inputs[1])
    Shader_Preset.links.new(mix_051.outputs[0], mix_052.inputs[1])
    Shader_Preset.links.new(mix_052.outputs[0], mix_053.inputs[1])
    Shader_Preset.links.new(mix_053.outputs[0], mix_054.inputs[1])
    Shader_Preset.links.new(math.outputs[0], mix_050.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_051.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_052.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_053.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_054.inputs[0])
    Shader_Preset.links.new(armor_primary_detail_normal_map.outputs[0], reroute_105.inputs[0])
    Shader_Preset.links.new(cloth_primary_detail_normal_map.outputs[0], reroute_179.inputs[0])
    Shader_Preset.links.new(suit_primary_detail_normal_map.outputs[0], reroute_052.inputs[0])
    Shader_Preset.links.new(armor_primary_wear_remap_x.outputs[0], reroute_098.inputs[0])
    Shader_Preset.links.new(armor_primary_wear_remap_y.outputs[0], reroute_099.inputs[0])
    Shader_Preset.links.new(armor_primary_wear_remap_z.outputs[0], reroute_100.inputs[0])
    Shader_Preset.links.new(armor_primary_wear_remap_w.outputs[0], reroute_092.inputs[0])
    Shader_Preset.links.new(armor_secondary_wear_remap_x.outputs[0], reroute_083.inputs[0])
    Shader_Preset.links.new(armor_secondary_wear_remap_y.outputs[0], reroute_082.inputs[0])
    Shader_Preset.links.new(armor_secondary_wear_remap_z.outputs[0], reroute_081.inputs[0])
    Shader_Preset.links.new(armor_secondary_wear_remap_w.outputs[0], reroute_080.inputs[0])
    Shader_Preset.links.new(cloth_primary_wear_remap_x.outputs[0], reroute_186.inputs[0])
    Shader_Preset.links.new(cloth_primary_wear_remap_y.outputs[0], reroute_187.inputs[0])
    Shader_Preset.links.new(cloth_primary_wear_remap_z.outputs[0], reroute_188.inputs[0])
    Shader_Preset.links.new(cloth_primary_wear_remap_w.outputs[0], reroute_189.inputs[0])
    Shader_Preset.links.new(cloth_secondary_wear_remap_x.outputs[0], reroute_135.inputs[0])
    Shader_Preset.links.new(cloth_secondary_wear_remap_y.outputs[0], reroute_136.inputs[0])
    Shader_Preset.links.new(cloth_secondary_wear_remap_z.outputs[0], reroute_137.inputs[0])
    Shader_Preset.links.new(cloth_secondary_wear_remap_w.outputs[0], reroute_138.inputs[0])
    Shader_Preset.links.new(suit_primary_wear_remap_x.outputs[0], reroute_041.inputs[0])
    Shader_Preset.links.new(suit_primary_wear_remap_y.outputs[0], reroute_042.inputs[0])
    Shader_Preset.links.new(suit_primary_wear_remap_z.outputs[0], reroute_043.inputs[0])
    Shader_Preset.links.new(suit_primary_wear_remap_w.outputs[0], reroute_044.inputs[0])
    Shader_Preset.links.new(suit_secondary_wear_remap_x.outputs[0], reroute_155.inputs[0])
    Shader_Preset.links.new(suit_secondary_wear_remap_y.outputs[0], reroute_156.inputs[0])
    Shader_Preset.links.new(suit_secondary_wear_remap_z.outputs[0], reroute_157.inputs[0])
    Shader_Preset.links.new(suit_secondary_wear_remap_w.outputs[0], reroute_158.inputs[0])
    Shader_Preset.links.new(armor_primary_detail_diffuse_blend.outputs[0], reroute_101.inputs[0])
    Shader_Preset.links.new(armor_primary_detail_normal_blend.outputs[0], reroute_102.inputs[0])
    Shader_Preset.links.new(armor_secondary_detail_diffuse_blend.outputs[0], reroute_078.inputs[0])
    Shader_Preset.links.new(armor_secondary_detail_normal_blend.outputs[0], reroute_077.inputs[0])
    Shader_Preset.links.new(cloth_primary_detail_diffuse_blend.outputs[0], reroute_191.inputs[0])
    Shader_Preset.links.new(cloth_primary_detail_normal_blend.outputs[0], reroute_192.inputs[0])
    Shader_Preset.links.new(cloth_secondary_detail_diffuse_blend.outputs[0], reroute_140.inputs[0])
    Shader_Preset.links.new(cloth_secondary_detail_normal_blend.outputs[0], reroute_141.inputs[0])
    Shader_Preset.links.new(suit_primary_detail_diffuse_blend.outputs[0], reroute_048.inputs[0])
    Shader_Preset.links.new(suit_primary_detail_normal_blend.outputs[0], reroute_049.inputs[0])
    Shader_Preset.links.new(suit_secondary_detail_diffuse_blend.outputs[0], reroute_160.inputs[0])
    Shader_Preset.links.new(suit_secondary_detail_normal_blend.outputs[0], reroute_161.inputs[0])
    Shader_Preset.links.new(armor_primary_detail_roughness_blend.outputs[0], reroute_103.inputs[0])
    Shader_Preset.links.new(armor_secondary_detail_roughness_blend.outputs[0], reroute_076.inputs[0])
    Shader_Preset.links.new(cloth_primary_detail_roughness_blend.outputs[0], reroute_193.inputs[0])
    Shader_Preset.links.new(cloth_secondary_detail_roughness_blend.outputs[0], reroute_142.inputs[0])
    Shader_Preset.links.new(suit_primary_detail_roughness_blend.outputs[0], reroute_050.inputs[0])
    Shader_Preset.links.new(suit_secondary_detail_roughness_blend.outputs[0], reroute_162.inputs[0])
    Shader_Preset.links.new(mix_055.outputs[0], mix_056.inputs[1])
    Shader_Preset.links.new(mix_056.outputs[0], mix_057.inputs[1])
    Shader_Preset.links.new(mix_057.outputs[0], mix_058.inputs[1])
    Shader_Preset.links.new(mix_058.outputs[0], mix_059.inputs[1])
    Shader_Preset.links.new(armor_primary_roughness_remap_x.outputs[0], reroute_096.inputs[0])
    Shader_Preset.links.new(armor_primary_roughness_remap_y.outputs[0], reroute_095.inputs[0])
    Shader_Preset.links.new(armor_primary_roughness_remap_z.outputs[0], reroute_094.inputs[0])
    Shader_Preset.links.new(armor_primary_roughness_remap_w.outputs[0], reroute_093.inputs[0])
    Shader_Preset.links.new(armor_secondary_roughness_remap_x.outputs[0], reroute_089.inputs[0])
    Shader_Preset.links.new(armor_secondary_roughness_remap_y.outputs[0], reroute_088.inputs[0])
    Shader_Preset.links.new(armor_secondary_roughness_remap_z.outputs[0], reroute_087.inputs[0])
    Shader_Preset.links.new(armor_secondary_roughness_remap_w.outputs[0], reroute_086.inputs[0])
    Shader_Preset.links.new(cloth_primary_roughness_remap_x.outputs[0], reroute_182.inputs[0])
    Shader_Preset.links.new(cloth_primary_roughness_remap_y.outputs[0], reroute_180.inputs[0])
    Shader_Preset.links.new(cloth_primary_roughness_remap_z.outputs[0], reroute_183.inputs[0])
    Shader_Preset.links.new(cloth_primary_roughness_remap_w.outputs[0], reroute_181.inputs[0])
    Shader_Preset.links.new(cloth_secondary_roughness_remap_x.outputs[0], reroute_131.inputs[0])
    Shader_Preset.links.new(cloth_secondary_roughness_remap_y.outputs[0], reroute_129.inputs[0])
    Shader_Preset.links.new(cloth_secondary_roughness_remap_z.outputs[0], reroute_132.inputs[0])
    Shader_Preset.links.new(cloth_secondary_roughness_remap_w.outputs[0], reroute_130.inputs[0])
    Shader_Preset.links.new(suit_primary_roughness_remap_x.outputs[0], reroute_002.inputs[0])
    Shader_Preset.links.new(suit_primary_roughness_remap_y.outputs[0], reroute.inputs[0])
    Shader_Preset.links.new(suit_primary_roughness_remap_z.outputs[0], reroute_003.inputs[0])
    Shader_Preset.links.new(suit_primary_roughness_remap_w.outputs[0], reroute_001.inputs[0])
    Shader_Preset.links.new(suit_secondary_roughness_remap_x.outputs[0], reroute_151.inputs[0])
    Shader_Preset.links.new(suit_secondary_roughness_remap_y.outputs[0], reroute_152.inputs[0])
    Shader_Preset.links.new(suit_secondary_roughness_remap_z.outputs[0], reroute_150.inputs[0])
    Shader_Preset.links.new(suit_secondary_roughness_remap_w.outputs[0], reroute_149.inputs[0])
    Shader_Preset.links.new(armor_primary_detail_diffuse_map.outputs[0], reroute_104.inputs[0])
    Shader_Preset.links.new(cloth_primary_detail_diffuse_map.outputs[0], reroute_194.inputs[0])
    Shader_Preset.links.new(suit_primary_detail_diffuse_map.outputs[0], reroute_051.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_roughness_remap_x.outputs[0], reroute_172.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_roughness_remap_y.outputs[0], reroute_171.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_roughness_remap_z.outputs[0], reroute_170.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_roughness_remap_w.outputs[0], reroute_173.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_roughness_remap_x.outputs[0], reroute_120.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_roughness_remap_y.outputs[0], reroute_121.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_roughness_remap_z.outputs[0], reroute_122.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_roughness_remap_w.outputs[0], reroute_123.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_roughness_remap_x.outputs[0], reroute_060.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_roughness_remap_y.outputs[0], reroute_059.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_roughness_remap_z.outputs[0], reroute_057.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_roughness_remap_w.outputs[0], reroute_058.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_roughness_remap_x.outputs[0], reroute_006.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_roughness_remap_y.outputs[0], reroute_007.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_roughness_remap_z.outputs[0], reroute_008.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_roughness_remap_w.outputs[0], reroute_009.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_roughness_remap_x.outputs[0], reroute_111.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_roughness_remap_y.outputs[0], reroute_112.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_roughness_remap_z.outputs[0], reroute_113.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_roughness_remap_w.outputs[0], reroute_114.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_roughness_remap_x.outputs[0], reroute_011.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_roughness_remap_y.outputs[0], reroute_010.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_roughness_remap_z.outputs[0], reroute_012.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_roughness_remap_w.outputs[0], reroute_013.inputs[0])
    Shader_Preset.links.new(armor_primary_detail_diffuse_map.outputs[1], reroute_097.inputs[0])
    Shader_Preset.links.new(cloth_primary_detail_diffuse_map.outputs[1], reroute_184.inputs[0])
    Shader_Preset.links.new(suit_primary_detail_diffuse_map.outputs[1], reroute_004.inputs[0])
    Shader_Preset.links.new(armor_primary_dye_color.outputs[0], reroute_169.inputs[0])
    Shader_Preset.links.new(armor_secondary_dye_color.outputs[0], reroute_084.inputs[0])
    Shader_Preset.links.new(cloth_primary_dye_color.outputs[0], reroute_185.inputs[0])
    Shader_Preset.links.new(cloth_secondary_dye_color.outputs[0], reroute_134.inputs[0])
    Shader_Preset.links.new(suit_primary_dye_color.outputs[0], reroute_005.inputs[0])
    Shader_Preset.links.new(suit_secondary_dye_color.outputs[0], reroute_154.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_dye_color.outputs[0], reroute_174.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_dye_color.outputs[0], reroute_124.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_dye_color.outputs[0], reroute_061.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_dye_color.outputs[0], reroute_027.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_dye_color.outputs[0], reroute_115.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_dye_color.outputs[0], reroute_028.inputs[0])
    Shader_Preset.links.new(math.outputs[0], mix_055.inputs[0])
    Shader_Preset.links.new(math_001.outputs[0], mix_056.inputs[0])
    Shader_Preset.links.new(math_004.outputs[0], mix_057.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_058.inputs[0])
    Shader_Preset.links.new(math_008.outputs[0], mix_059.inputs[0])
    Shader_Preset.links.new(armor_primary_transmission.outputs[0], reroute_109.inputs[0])
    Shader_Preset.links.new(armor_primary_iridescence.outputs[0], reroute_107.inputs[0])
    Shader_Preset.links.new(armor_primary_metalness.outputs[0], reroute_106.inputs[0])
    Shader_Preset.links.new(armor_secondary_metalness.outputs[0], reroute_079.inputs[0])
    Shader_Preset.links.new(armor_secondary_iridescence.outputs[0], reroute_073.inputs[0])
    Shader_Preset.links.new(armor_secondary_transmission.outputs[0], reroute_071.inputs[0])
    Shader_Preset.links.new(cloth_primary_metalness.outputs[0], reroute_190.inputs[0])
    Shader_Preset.links.new(cloth_primary_iridescence.outputs[0], reroute_195.inputs[0])
    Shader_Preset.links.new(cloth_primary_transmission.outputs[0], reroute_196.inputs[0])
    Shader_Preset.links.new(cloth_secondary_metalness.outputs[0], reroute_139.inputs[0])
    Shader_Preset.links.new(cloth_secondary_iridescence.outputs[0], reroute_145.inputs[0])
    Shader_Preset.links.new(cloth_secondary_transmission.outputs[0], reroute_147.inputs[0])
    Shader_Preset.links.new(suit_primary_metalness.outputs[0], reroute_046.inputs[0])
    Shader_Preset.links.new(suit_primary_iridescence.outputs[0], reroute_053.inputs[0])
    Shader_Preset.links.new(suit_primary_transmission.outputs[0], reroute_055.inputs[0])
    Shader_Preset.links.new(suit_secondary_metalness.outputs[0], reroute_159.inputs[0])
    Shader_Preset.links.new(suit_secondary_iridescence.outputs[0], reroute_165.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_metalness.outputs[0], reroute_178.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_metalness.outputs[0], reroute_125.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_metalness.outputs[0], reroute_062.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_metalness.outputs[0], reroute_045.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_metalness.outputs[0], reroute_116.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_metalness.outputs[0], reroute_047.inputs[0])
    Shader_Preset.links.new(armor_primary_emission_color.outputs[0], reroute_110.inputs[0])
    Shader_Preset.links.new(armor_secondary_emission_color.outputs[0], reroute_090.inputs[0])
    Shader_Preset.links.new(cloth_primary_emission_color.outputs[0], reroute_197.inputs[0])
    Shader_Preset.links.new(cloth_secondary_emission_color.outputs[0], reroute_148.inputs[0])
    Shader_Preset.links.new(suit_primary_emission_color.outputs[0], reroute_091.inputs[0])
    Shader_Preset.links.new(suit_secondary_emission_color.outputs[0], reroute_168.inputs[0])
    Shader_Preset.links.new(reroute_096.outputs[0], combine_xyz_012.inputs[0])
    Shader_Preset.links.new(reroute_095.outputs[0], combine_xyz_012.inputs[1])
    Shader_Preset.links.new(reroute_094.outputs[0], combine_xyz_012.inputs[2])
    Shader_Preset.links.new(reroute_093.outputs[0], combine_xyz_013.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_detail_diffuse_blend.outputs[0], reroute_175.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_detail_normal_blend.outputs[0], reroute_176.inputs[0])
    Shader_Preset.links.new(worn_armor_primary_detail_roughness_blend.outputs[0], reroute_177.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_detail_diffuse_blend.outputs[0], reroute_126.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_detail_normal_blend.outputs[0], reroute_127.inputs[0])
    Shader_Preset.links.new(worn_armor_secondary_detail_roughness_blend.outputs[0], reroute_128.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_detail_diffuse_blend.outputs[0], reroute_063.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_detail_normal_blend.outputs[0], reroute_064.inputs[0])
    Shader_Preset.links.new(worn_cloth_primary_detail_roughness_blend.outputs[0], reroute_056.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_detail_diffuse_blend.outputs[0], reroute_065.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_detail_normal_blend.outputs[0], reroute_066.inputs[0])
    Shader_Preset.links.new(worn_cloth_secondary_detail_roughness_blend.outputs[0], reroute_067.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_detail_diffuse_blend.outputs[0], reroute_117.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_detail_normal_blend.outputs[0], reroute_118.inputs[0])
    Shader_Preset.links.new(worn_suit_primary_detail_roughness_blend.outputs[0], reroute_119.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_detail_diffuse_blend.outputs[0], reroute_068.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_detail_normal_blend.outputs[0], reroute_069.inputs[0])
    Shader_Preset.links.new(worn_suit_secondary_detail_roughness_blend.outputs[0], reroute_070.inputs[0])
    Shader_Preset.links.new(reroute_089.outputs[0], combine_xyz_014.inputs[0])
    Shader_Preset.links.new(reroute_088.outputs[0], combine_xyz_014.inputs[1])
    Shader_Preset.links.new(reroute_087.outputs[0], combine_xyz_014.inputs[2])
    Shader_Preset.links.new(reroute_086.outputs[0], combine_xyz_015.inputs[0])
    Shader_Preset.links.new(reroute_182.outputs[0], combine_xyz_016.inputs[0])
    Shader_Preset.links.new(reroute_180.outputs[0], combine_xyz_016.inputs[1])
    Shader_Preset.links.new(reroute_183.outputs[0], combine_xyz_016.inputs[2])
    Shader_Preset.links.new(reroute_181.outputs[0], combine_xyz_017.inputs[0])
    Shader_Preset.links.new(reroute_131.outputs[0], combine_xyz_018.inputs[0])
    Shader_Preset.links.new(reroute_129.outputs[0], combine_xyz_018.inputs[1])
    Shader_Preset.links.new(reroute_132.outputs[0], combine_xyz_018.inputs[2])
    Shader_Preset.links.new(reroute_130.outputs[0], combine_xyz_019.inputs[0])
    Shader_Preset.links.new(reroute_184.outputs[0], combine_xyz_017.inputs[1])
    Shader_Preset.links.new(reroute_097.outputs[0], combine_xyz_013.inputs[1])
    Shader_Preset.links.new(reroute_002.outputs[0], combine_xyz_024.inputs[0])
    Shader_Preset.links.new(reroute.outputs[0], combine_xyz_024.inputs[1])
    Shader_Preset.links.new(reroute_003.outputs[0], combine_xyz_024.inputs[2])
    Shader_Preset.links.new(reroute_001.outputs[0], combine_xyz_025.inputs[0])
    Shader_Preset.links.new(reroute_004.outputs[0], combine_xyz_025.inputs[1])
    Shader_Preset.links.new(reroute_151.outputs[0], combine_xyz_028.inputs[0])
    Shader_Preset.links.new(reroute_152.outputs[0], combine_xyz_028.inputs[1])
    Shader_Preset.links.new(reroute_150.outputs[0], combine_xyz_028.inputs[2])
    Shader_Preset.links.new(reroute_149.outputs[0], combine_xyz_029.inputs[0])
    Shader_Preset.links.new(reroute_169.outputs[0], mix.inputs[1])
    Shader_Preset.links.new(reroute_084.outputs[0], mix.inputs[2])
    Shader_Preset.links.new(reroute_185.outputs[0], mix_001.inputs[2])
    Shader_Preset.links.new(reroute_134.outputs[0], mix_002.inputs[2])
    Shader_Preset.links.new(reroute_005.outputs[0], mix_003.inputs[2])
    Shader_Preset.links.new(reroute_154.outputs[0], mix_004.inputs[2])
    Shader_Preset.links.new(reroute_172.outputs[0], combine_xyz_013.inputs[2])
    Shader_Preset.links.new(reroute_171.outputs[0], combine_xyz_020.inputs[0])
    Shader_Preset.links.new(reroute_170.outputs[0], combine_xyz_020.inputs[1])
    Shader_Preset.links.new(reroute_173.outputs[0], combine_xyz_020.inputs[2])
    Shader_Preset.links.new(combine_xyz_012.outputs[0], mix_020.inputs[1])
    Shader_Preset.links.new(combine_xyz_014.outputs[0], mix_020.inputs[2])
    Shader_Preset.links.new(combine_xyz_016.outputs[0], mix_021.inputs[2])
    Shader_Preset.links.new(combine_xyz_018.outputs[0], mix_022.inputs[2])
    Shader_Preset.links.new(combine_xyz_024.outputs[0], mix_023.inputs[2])
    Shader_Preset.links.new(combine_xyz_028.outputs[0], mix_024.inputs[2])
    Shader_Preset.links.new(combine_xyz_013.outputs[0], mix_025.inputs[1])
    Shader_Preset.links.new(combine_xyz_015.outputs[0], mix_025.inputs[2])
    Shader_Preset.links.new(combine_xyz_017.outputs[0], mix_026.inputs[2])
    Shader_Preset.links.new(combine_xyz_019.outputs[0], mix_027.inputs[2])
    Shader_Preset.links.new(combine_xyz_025.outputs[0], mix_028.inputs[2])
    Shader_Preset.links.new(combine_xyz_029.outputs[0], mix_029.inputs[2])
    Shader_Preset.links.new(combine_xyz_020.outputs[0], mix_030.inputs[1])
    Shader_Preset.links.new(combine_xyz_021.outputs[0], mix_030.inputs[2])
    Shader_Preset.links.new(combine_xyz_022.outputs[0], mix_031.inputs[2])
    Shader_Preset.links.new(combine_xyz_023.outputs[0], mix_032.inputs[2])
    Shader_Preset.links.new(combine_xyz_026.outputs[0], mix_033.inputs[2])
    Shader_Preset.links.new(combine_xyz_027.outputs[0], mix_034.inputs[2])
    Shader_Preset.links.new(reroute_120.outputs[0], combine_xyz_015.inputs[2])
    Shader_Preset.links.new(reroute_121.outputs[0], combine_xyz_021.inputs[0])
    Shader_Preset.links.new(reroute_122.outputs[0], combine_xyz_021.inputs[1])
    Shader_Preset.links.new(reroute_123.outputs[0], combine_xyz_021.inputs[2])
    Shader_Preset.links.new(reroute_058.outputs[0], combine_xyz_022.inputs[2])
    Shader_Preset.links.new(reroute_057.outputs[0], combine_xyz_022.inputs[1])
    Shader_Preset.links.new(reroute_059.outputs[0], combine_xyz_022.inputs[0])
    Shader_Preset.links.new(reroute_060.outputs[0], combine_xyz_017.inputs[2])
    Shader_Preset.links.new(reroute_009.outputs[0], combine_xyz_023.inputs[2])
    Shader_Preset.links.new(reroute_008.outputs[0], combine_xyz_023.inputs[1])
    Shader_Preset.links.new(reroute_007.outputs[0], combine_xyz_023.inputs[0])
    Shader_Preset.links.new(reroute_006.outputs[0], combine_xyz_019.inputs[2])
    Shader_Preset.links.new(reroute_114.outputs[0], combine_xyz_026.inputs[2])
    Shader_Preset.links.new(reroute_113.outputs[0], combine_xyz_026.inputs[1])
    Shader_Preset.links.new(reroute_112.outputs[0], combine_xyz_026.inputs[0])
    Shader_Preset.links.new(reroute_111.outputs[0], combine_xyz_025.inputs[2])
    Shader_Preset.links.new(reroute_013.outputs[0], combine_xyz_027.inputs[2])
    Shader_Preset.links.new(reroute_012.outputs[0], combine_xyz_027.inputs[1])
    Shader_Preset.links.new(reroute_010.outputs[0], combine_xyz_027.inputs[0])
    Shader_Preset.links.new(reroute_011.outputs[0], combine_xyz_029.inputs[2])
    Shader_Preset.links.new(reroute_018.outputs[0], armor_detail_diffuse_transform.inputs[0])
    Shader_Preset.links.new(reroute_174.outputs[0], mix_005.inputs[1])
    Shader_Preset.links.new(reroute_124.outputs[0], mix_005.inputs[2])
    Shader_Preset.links.new(reroute_061.outputs[0], mix_006.inputs[2])
    Shader_Preset.links.new(reroute_027.outputs[0], mix_007.inputs[2])
    Shader_Preset.links.new(reroute_115.outputs[0], mix_008.inputs[2])
    Shader_Preset.links.new(reroute_028.outputs[0], mix_009.inputs[2])
    Shader_Preset.links.new(reroute_098.outputs[0], combine_xyz.inputs[0])
    Shader_Preset.links.new(reroute_099.outputs[0], combine_xyz.inputs[1])
    Shader_Preset.links.new(reroute_100.outputs[0], combine_xyz.inputs[2])
    Shader_Preset.links.new(reroute_092.outputs[0], combine_xyz_001.inputs[0])
    Shader_Preset.links.new(combine_xyz.outputs[0], mix_010.inputs[1])
    Shader_Preset.links.new(combine_xyz_001.outputs[0], mix_015.inputs[1])
    Shader_Preset.links.new(reroute_083.outputs[0], combine_xyz_002.inputs[0])
    Shader_Preset.links.new(reroute_082.outputs[0], combine_xyz_002.inputs[1])
    Shader_Preset.links.new(reroute_081.outputs[0], combine_xyz_002.inputs[2])
    Shader_Preset.links.new(reroute_080.outputs[0], combine_xyz_003.inputs[0])
    Shader_Preset.links.new(reroute_186.outputs[0], combine_xyz_005.inputs[0])
    Shader_Preset.links.new(reroute_187.outputs[0], combine_xyz_005.inputs[1])
    Shader_Preset.links.new(reroute_188.outputs[0], combine_xyz_005.inputs[2])
    Shader_Preset.links.new(reroute_189.outputs[0], combine_xyz_004.inputs[0])
    Shader_Preset.links.new(reroute_135.outputs[0], combine_xyz_007.inputs[0])
    Shader_Preset.links.new(reroute_136.outputs[0], combine_xyz_007.inputs[1])
    Shader_Preset.links.new(reroute_137.outputs[0], combine_xyz_007.inputs[2])
    Shader_Preset.links.new(reroute_138.outputs[0], combine_xyz_006.inputs[0])
    Shader_Preset.links.new(reroute_041.outputs[0], combine_xyz_008.inputs[0])
    Shader_Preset.links.new(reroute_042.outputs[0], combine_xyz_008.inputs[1])
    Shader_Preset.links.new(reroute_043.outputs[0], combine_xyz_008.inputs[2])
    Shader_Preset.links.new(reroute_044.outputs[0], combine_xyz_009.inputs[0])
    Shader_Preset.links.new(reroute_155.outputs[0], combine_xyz_010.inputs[0])
    Shader_Preset.links.new(reroute_156.outputs[0], combine_xyz_010.inputs[1])
    Shader_Preset.links.new(reroute_157.outputs[0], combine_xyz_010.inputs[2])
    Shader_Preset.links.new(reroute_158.outputs[0], combine_xyz_011.inputs[0])
    Shader_Preset.links.new(armor_secondary_fuzz.outputs[0], reroute_072.inputs[0])
    Shader_Preset.links.new(cloth_primary_fuzz.outputs[0], reroute_365.inputs[0])
    Shader_Preset.links.new(cloth_secondary_fuzz.outputs[0], reroute_146.inputs[0])
    Shader_Preset.links.new(suit_primary_fuzz.outputs[0], reroute_054.inputs[0])
    Shader_Preset.links.new(suit_secondary_transmission.outputs[0], reroute_167.inputs[0])
    Shader_Preset.links.new(suit_secondary_fuzz.outputs[0], reroute_166.inputs[0])
    Shader_Preset.links.new(reroute_101.outputs[0], combine_xyz_030.inputs[0])
    Shader_Preset.links.new(reroute_102.outputs[0], combine_xyz_030.inputs[1])
    Shader_Preset.links.new(reroute_103.outputs[0], combine_xyz_030.inputs[2])
    Shader_Preset.links.new(reroute_175.outputs[0], combine_xyz_032.inputs[0])
    Shader_Preset.links.new(reroute_176.outputs[0], combine_xyz_032.inputs[1])
    Shader_Preset.links.new(reroute_177.outputs[0], combine_xyz_032.inputs[2])
    Shader_Preset.links.new(reroute_107.outputs[0], combine_xyz_031.inputs[0])
    Shader_Preset.links.new(reroute_108.outputs[0], combine_xyz_031.inputs[1])
    Shader_Preset.links.new(reroute_109.outputs[0], combine_xyz_031.inputs[2])
    Shader_Preset.links.new(reroute_178.outputs[0], combine_xyz_001.inputs[2])
    Shader_Preset.links.new(reroute_106.outputs[0], combine_xyz_001.inputs[1])
    Shader_Preset.links.new(reroute_079.outputs[0], combine_xyz_003.inputs[1])
    Shader_Preset.links.new(reroute_125.outputs[0], combine_xyz_003.inputs[2])
    Shader_Preset.links.new(reroute_190.outputs[0], combine_xyz_004.inputs[1])
    Shader_Preset.links.new(reroute_062.outputs[0], combine_xyz_004.inputs[2])
    Shader_Preset.links.new(reroute_139.outputs[0], combine_xyz_006.inputs[1])
    Shader_Preset.links.new(reroute_045.outputs[0], combine_xyz_006.inputs[2])
    Shader_Preset.links.new(reroute_046.outputs[0], combine_xyz_009.inputs[1])
    Shader_Preset.links.new(reroute_116.outputs[0], combine_xyz_009.inputs[2])
    Shader_Preset.links.new(reroute_159.outputs[0], combine_xyz_011.inputs[1])
    Shader_Preset.links.new(reroute_047.outputs[0], combine_xyz_011.inputs[2])
    Shader_Preset.links.new(combine_xyz_002.outputs[0], mix_010.inputs[2])
    Shader_Preset.links.new(combine_xyz_005.outputs[0], mix_011.inputs[2])
    Shader_Preset.links.new(combine_xyz_007.outputs[0], mix_012.inputs[2])
    Shader_Preset.links.new(combine_xyz_008.outputs[0], mix_013.inputs[2])
    Shader_Preset.links.new(combine_xyz_010.outputs[0], mix_014.inputs[2])
    Shader_Preset.links.new(combine_xyz_003.outputs[0], mix_015.inputs[2])
    Shader_Preset.links.new(combine_xyz_004.outputs[0], mix_016.inputs[2])
    Shader_Preset.links.new(combine_xyz_006.outputs[0], mix_017.inputs[2])
    Shader_Preset.links.new(combine_xyz_009.outputs[0], mix_018.inputs[2])
    Shader_Preset.links.new(combine_xyz_011.outputs[0], mix_019.inputs[2])
    Shader_Preset.links.new(reroute_078.outputs[0], combine_xyz_033.inputs[0])
    Shader_Preset.links.new(reroute_077.outputs[0], combine_xyz_033.inputs[1])
    Shader_Preset.links.new(reroute_076.outputs[0], combine_xyz_033.inputs[2])
    Shader_Preset.links.new(reroute_191.outputs[0], combine_xyz_034.inputs[0])
    Shader_Preset.links.new(reroute_192.outputs[0], combine_xyz_034.inputs[1])
    Shader_Preset.links.new(reroute_193.outputs[0], combine_xyz_034.inputs[2])
    Shader_Preset.links.new(reroute_140.outputs[0], combine_xyz_035.inputs[0])
    Shader_Preset.links.new(reroute_141.outputs[0], combine_xyz_035.inputs[1])
    Shader_Preset.links.new(reroute_142.outputs[0], combine_xyz_035.inputs[2])
    Shader_Preset.links.new(reroute_048.outputs[0], combine_xyz_036.inputs[0])
    Shader_Preset.links.new(reroute_049.outputs[0], combine_xyz_036.inputs[1])
    Shader_Preset.links.new(reroute_050.outputs[0], combine_xyz_036.inputs[2])
    Shader_Preset.links.new(reroute_160.outputs[0], combine_xyz_037.inputs[0])
    Shader_Preset.links.new(reroute_161.outputs[0], combine_xyz_037.inputs[1])
    Shader_Preset.links.new(reroute_162.outputs[0], combine_xyz_037.inputs[2])
    Shader_Preset.links.new(combine_xyz_030.outputs[0], mix_040.inputs[1])
    Shader_Preset.links.new(combine_xyz_033.outputs[0], mix_040.inputs[2])
    Shader_Preset.links.new(combine_xyz_034.outputs[0], mix_041.inputs[2])
    Shader_Preset.links.new(combine_xyz_035.outputs[0], mix_042.inputs[2])
    Shader_Preset.links.new(combine_xyz_036.outputs[0], mix_043.inputs[2])
    Shader_Preset.links.new(combine_xyz_037.outputs[0], mix_044.inputs[2])
    Shader_Preset.links.new(reroute_073.outputs[0], combine_xyz_038.inputs[0])
    Shader_Preset.links.new(reroute_072.outputs[0], combine_xyz_038.inputs[1])
    Shader_Preset.links.new(reroute_071.outputs[0], combine_xyz_038.inputs[2])
    Shader_Preset.links.new(reroute_195.outputs[0], combine_xyz_039.inputs[0])
    Shader_Preset.links.new(reroute_365.outputs[0], combine_xyz_039.inputs[1])
    Shader_Preset.links.new(reroute_196.outputs[0], combine_xyz_039.inputs[2])
    Shader_Preset.links.new(reroute_145.outputs[0], combine_xyz_040.inputs[0])
    Shader_Preset.links.new(reroute_146.outputs[0], combine_xyz_040.inputs[1])
    Shader_Preset.links.new(reroute_147.outputs[0], combine_xyz_040.inputs[2])
    Shader_Preset.links.new(reroute_053.outputs[0], combine_xyz_041.inputs[0])
    Shader_Preset.links.new(reroute_054.outputs[0], combine_xyz_041.inputs[1])
    Shader_Preset.links.new(reroute_055.outputs[0], combine_xyz_041.inputs[2])
    Shader_Preset.links.new(reroute_165.outputs[0], combine_xyz_042.inputs[0])
    Shader_Preset.links.new(reroute_166.outputs[0], combine_xyz_042.inputs[1])
    Shader_Preset.links.new(reroute_167.outputs[0], combine_xyz_042.inputs[2])
    Shader_Preset.links.new(combine_xyz_031.outputs[0], mix_045.inputs[1])
    Shader_Preset.links.new(combine_xyz_038.outputs[0], mix_045.inputs[2])
    Shader_Preset.links.new(combine_xyz_039.outputs[0], mix_046.inputs[2])
    Shader_Preset.links.new(combine_xyz_040.outputs[0], mix_047.inputs[2])
    Shader_Preset.links.new(combine_xyz_041.outputs[0], mix_048.inputs[2])
    Shader_Preset.links.new(combine_xyz_042.outputs[0], mix_049.inputs[2])
    Shader_Preset.links.new(reroute_126.outputs[0], combine_xyz_043.inputs[0])
    Shader_Preset.links.new(reroute_127.outputs[0], combine_xyz_043.inputs[1])
    Shader_Preset.links.new(reroute_128.outputs[0], combine_xyz_043.inputs[2])
    Shader_Preset.links.new(reroute_063.outputs[0], combine_xyz_044.inputs[0])
    Shader_Preset.links.new(reroute_064.outputs[0], combine_xyz_044.inputs[1])
    Shader_Preset.links.new(reroute_056.outputs[0], combine_xyz_044.inputs[2])
    Shader_Preset.links.new(reroute_065.outputs[0], combine_xyz_045.inputs[0])
    Shader_Preset.links.new(reroute_066.outputs[0], combine_xyz_045.inputs[1])
    Shader_Preset.links.new(reroute_067.outputs[0], combine_xyz_045.inputs[2])
    Shader_Preset.links.new(reroute_117.outputs[0], combine_xyz_046.inputs[0])
    Shader_Preset.links.new(reroute_118.outputs[0], combine_xyz_046.inputs[1])
    Shader_Preset.links.new(reroute_119.outputs[0], combine_xyz_046.inputs[2])
    Shader_Preset.links.new(reroute_068.outputs[0], combine_xyz_047.inputs[0])
    Shader_Preset.links.new(reroute_069.outputs[0], combine_xyz_047.inputs[1])
    Shader_Preset.links.new(reroute_070.outputs[0], combine_xyz_047.inputs[2])
    Shader_Preset.links.new(combine_xyz_032.outputs[0], mix_055.inputs[1])
    Shader_Preset.links.new(combine_xyz_043.outputs[0], mix_055.inputs[2])
    Shader_Preset.links.new(combine_xyz_044.outputs[0], mix_056.inputs[2])
    Shader_Preset.links.new(combine_xyz_045.outputs[0], mix_057.inputs[2])
    Shader_Preset.links.new(combine_xyz_046.outputs[0], mix_058.inputs[2])
    Shader_Preset.links.new(combine_xyz_047.outputs[0], mix_059.inputs[2])
    Shader_Preset.links.new(reroute_110.outputs[0], mix_050.inputs[1])
    Shader_Preset.links.new(reroute_090.outputs[0], mix_050.inputs[2])
    Shader_Preset.links.new(reroute_197.outputs[0], mix_051.inputs[2])
    Shader_Preset.links.new(reroute_148.outputs[0], mix_052.inputs[2])
    Shader_Preset.links.new(reroute_091.outputs[0], mix_053.inputs[2])
    Shader_Preset.links.new(reroute_168.outputs[0], mix_054.inputs[2])
    Shader_Preset.links.new(armor_primary_fuzz.outputs[0], reroute_108.inputs[0])
    Shader_Preset.links.new(math_023.outputs[0], math_024.inputs[0])
    Shader_Preset.links.new(separate_xyz_001.outputs[0], math.inputs[0])
    Shader_Preset.links.new(separate_xyz_001.outputs[0], math_001.inputs[0])
    Shader_Preset.links.new(separate_xyz_001.outputs[1], math_008.inputs[0])
    Shader_Preset.links.new(separate_xyz_001.outputs[1], math_007.inputs[0])
    Shader_Preset.links.new(separate_xyz_001.outputs[1], math_004.inputs[0])
    Shader_Preset.links.new(mix_071.outputs[0], separate_xyz_001.inputs[0])
    Shader_Preset.links.new(math_022.outputs[0], combine_xyz_048.inputs[0])
    Shader_Preset.links.new(math_024.outputs[0], combine_xyz_048.inputs[1])
    Shader_Preset.links.new(combine_xyz_048.outputs[0], mix_071.inputs[2])
    Shader_Preset.links.new(attribute.outputs[0], mix_071.inputs[1])
    Shader_Preset.links.new(separate_xyz_002.outputs[0], math_002.inputs[0])
    Shader_Preset.links.new(separate_xyz_002.outputs[1], math_015.inputs[0])
    Shader_Preset.links.new(separate_xyz_002.outputs[2], math_016.inputs[0])
    Shader_Preset.links.new(mix_069.outputs[0], mix_070.inputs[1])
    Shader_Preset.links.new(mix_070.outputs[0], mix_068.inputs[1])
    Shader_Preset.links.new(mix_065.outputs[0], mix_066.inputs[1])
    Shader_Preset.links.new(math_002.outputs[0], mix_069.inputs[0])
    Shader_Preset.links.new(math_015.outputs[0], mix_070.inputs[0])
    Shader_Preset.links.new(math_002.outputs[0], math_003.inputs[0])
    Shader_Preset.links.new(math_015.outputs[0], math_003.inputs[1])
    Shader_Preset.links.new(math_002.outputs[0], math_005.inputs[0])
    Shader_Preset.links.new(math_016.outputs[0], math_005.inputs[1])
    Shader_Preset.links.new(math_005.outputs[0], mix_066.inputs[0])
    Shader_Preset.links.new(math_016.outputs[0], mix_068.inputs[0])
    Shader_Preset.links.new(mix_068.outputs[0], mix_065.inputs[1])
    Shader_Preset.links.new(math_003.outputs[0], mix_065.inputs[0])
    Shader_Preset.links.new(math_012.outputs[0], math_010.inputs[0])
    Shader_Preset.links.new(math_010.outputs[0], math_013.inputs[0])
    Shader_Preset.links.new(mix_066.outputs[0], mix_067.inputs[1])
    Shader_Preset.links.new(mix_072.outputs[0], math_023.inputs[0])
    Shader_Preset.links.new(mix_067.outputs[0], mix_071.inputs[0])
    Shader_Preset.links.new(separate_xyz_002.outputs[2], math_017.inputs[0])
    Shader_Preset.links.new(math_017.outputs[0], math_010.inputs[1])
    Shader_Preset.links.new(separate_xyz_002.outputs[1], math_018.inputs[0])
    Shader_Preset.links.new(math_018.outputs[0], math_012.inputs[1])
    Shader_Preset.links.new(separate_xyz_002.outputs[0], math_019.inputs[0])
    Shader_Preset.links.new(math_019.outputs[0], math_012.inputs[0])
    Shader_Preset.links.new(math_013.outputs[0], mix_067.inputs[0])
    Shader_Preset.links.new(mix_067.outputs[0], mix_072.inputs[1])
    Shader_Preset.links.new(mix_072.outputs[0], math_022.inputs[0])
    Shader_Preset.links.new(math_021.outputs[0], math_013.inputs[1])
    Shader_Preset.links.new(mix_073.outputs[0], mix_074.inputs[1])
    Shader_Preset.links.new(math_001.outputs[0], mix_073.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_074.inputs[0])
    Shader_Preset.links.new(reroute_104.outputs[0], mix_073.inputs[1])
    Shader_Preset.links.new(reroute_194.outputs[0], mix_073.inputs[2])
    Shader_Preset.links.new(reroute_051.outputs[0], mix_074.inputs[2])
    Shader_Preset.links.new(math_001.outputs[0], node.inputs[0])
    Shader_Preset.links.new(math_007.outputs[0], mix_076.inputs[0])
    Shader_Preset.links.new(node.outputs[0], mix_076.inputs[1])
    Shader_Preset.links.new(reroute_105.outputs[0], node.inputs[1])
    Shader_Preset.links.new(reroute_179.outputs[0], node.inputs[2])
    Shader_Preset.links.new(reroute_052.outputs[0], mix_076.inputs[2])
    Shader_Preset.links.new(reroute_097.outputs[0], combine_xyz_015.inputs[1])
    Shader_Preset.links.new(reroute_184.outputs[0], combine_xyz_019.inputs[1])
    Shader_Preset.links.new(reroute_004.outputs[0], combine_xyz_029.inputs[1])
    Shader_Preset.links.new(armor_detail_diffuse_transform.outputs[0], reroute_014.inputs[0])
    Shader_Preset.links.new(reroute_014.outputs[0], reroute_015.inputs[0])
    Shader_Preset.links.new(reroute_015.outputs[0], armor_primary_detail_diffuse_map.inputs[0])
    Shader_Preset.links.new(reroute_016.outputs[0], reroute_017.inputs[0])
    Shader_Preset.links.new(armor_detail_normal_transform.outputs[0], reroute_016.inputs[0])
    Shader_Preset.links.new(reroute_017.outputs[0], armor_primary_detail_normal_map.inputs[0])
    Shader_Preset.links.new(detail_uvs.outputs[0], reroute_018.inputs[0])
    Shader_Preset.links.new(reroute_018.outputs[0], reroute_019.inputs[0])
    Shader_Preset.links.new(reroute_019.outputs[0], armor_detail_normal_transform.inputs[0])
    Shader_Preset.links.new(reroute_024.outputs[0], cloth_detail_diffuse_transform.inputs[0])
    Shader_Preset.links.new(cloth_detail_diffuse_transform.outputs[0], reroute_023.inputs[0])
    Shader_Preset.links.new(reroute_023.outputs[0], reroute_021.inputs[0])
    Shader_Preset.links.new(reroute_022.outputs[0], reroute_020.inputs[0])
    Shader_Preset.links.new(cloth_detail_normal_transform.outputs[0], reroute_022.inputs[0])
    Shader_Preset.links.new(reroute_024.outputs[0], reroute_025.inputs[0])
    Shader_Preset.links.new(reroute_025.outputs[0], cloth_detail_normal_transform.inputs[0])
    Shader_Preset.links.new(reroute_021.outputs[0], cloth_primary_detail_diffuse_map.inputs[0])
    Shader_Preset.links.new(reroute_020.outputs[0], cloth_primary_detail_normal_map.inputs[0])
    Shader_Preset.links.new(reroute_019.outputs[0], reroute_024.inputs[0])
    Shader_Preset.links.new(reroute_030.outputs[0], suit_detail_diffuse_transform.inputs[0])
    Shader_Preset.links.new(suit_detail_diffuse_transform.outputs[0], reroute_029.inputs[0])
    Shader_Preset.links.new(reroute_029.outputs[0], reroute_032.inputs[0])
    Shader_Preset.links.new(reroute_026.outputs[0], reroute_033.inputs[0])
    Shader_Preset.links.new(suit_detail_normal_transform.outputs[0], reroute_026.inputs[0])
    Shader_Preset.links.new(reroute_030.outputs[0], reroute_031.inputs[0])
    Shader_Preset.links.new(reroute_031.outputs[0], suit_detail_normal_transform.inputs[0])
    Shader_Preset.links.new(reroute_032.outputs[0], suit_primary_detail_diffuse_map.inputs[0])
    Shader_Preset.links.new(reroute_033.outputs[0], suit_primary_detail_normal_map.inputs[0])
    Shader_Preset.links.new(reroute_025.outputs[0], reroute_030.inputs[0])

#Group Input Connections
    Shader_Preset.links.new(group_input.outputs[0], separate_xyz_002.inputs[0])
    Shader_Preset.links.new(group_input.outputs[1], math_021.inputs[0])
    Shader_Preset.links.new(group_input.outputs[2], mix_072.inputs[0])
    Shader_Preset.links.new(group_input.outputs[2], mix_072.inputs[2])
#Group Output connections
    Shader_Preset.links.new(mix_004.outputs[0], group_output.inputs[0])
    Shader_Preset.links.new(mix_009.outputs[0], group_output.inputs[1])
    Shader_Preset.links.new(mix_014.outputs[0], group_output.inputs[2])
    Shader_Preset.links.new(mix_019.outputs[0], group_output.inputs[3])
    Shader_Preset.links.new(mix_024.outputs[0], group_output.inputs[4])
    Shader_Preset.links.new(mix_029.outputs[0], group_output.inputs[5])
    Shader_Preset.links.new(mix_034.outputs[0], group_output.inputs[6])
    Shader_Preset.links.new(mix_074.outputs[0], group_output.inputs[7])
    Shader_Preset.links.new(mix_076.outputs[0], group_output.inputs[8])
    Shader_Preset.links.new(mix_044.outputs[0], group_output.inputs[9])
    Shader_Preset.links.new(mix_059.outputs[0], group_output.inputs[10])
    Shader_Preset.links.new(mix_049.outputs[0], group_output.inputs[11])
    Shader_Preset.links.new(mix_054.outputs[0], group_output.inputs[12])

    return Shader_Preset

class NODE(bpy.types.Operator):
    bl_label = ("Generate SHADERNAMEENUM Shader Preset")
    bl_idname = "node.test_operator"

    def execute(self, context):
        custom_node_name = "SHADERNAMEENUM Shader Preset"
        global RIP_LOCATION
        GroupNode = create_Shader_Preset(self, context, custom_node_name, RIP_LOCATION)
        shaderpreset_node = context.view_layer.objects.active.active_material.node_tree.nodes.new('ShaderNodeGroup')
        shaderpreset_node.node_tree = bpy.data.node_groups[GroupNode.name]
        shaderpreset_node.use_custom_color = True
        shaderpreset_node.color = (0.101, 0.170, 0.297)

        return {'FINISHED'}

def register():
    global RIP_LOCATION
    RIP_LOCATION = os.path.abspath(bpy.context.space_data.text.filepath+"/../../../")
    bpy.utils.register_class(MAINPANEL)
    bpy.utils.register_class(NODE)

def unregister():
    bpy.utils.unregister_class(MAINPANEL)
    bpy.utils.unregister_class(NODE)

if __name__ == "__main__":
    register()
