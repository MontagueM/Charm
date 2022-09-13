import bpy
import os

RIP_LOCATION = None

class NODE_PT_MAINPANEL(bpy.types.Panel):
    bl_label = "Custom Node Group"
    bl_idname = "NODE_PT_MAINPANEL"
    bl_space_type = 'NODE_EDITOR'
    bl_region_type = 'UI'
    bl_category = 'D2 Shader'

    def draw(self, context):
        layout = self.layout

        row = layout.row()
        row.operator('node.test_operator')


def create_test_group(context, operator, group_name, riplocation):
    # Enable nodes
    bpy.context.scene.use_nodes = True

    test_group = bpy.data.node_groups.new(group_name, 'ShaderNodeTree')

    # Activate Node Connection usage
    link = test_group.links.new

    # Nodegroup Outputs
    test_group.inputs.new('NodeSocketFloat', 'Slot Override [1-6]')

    test_group.outputs.new('NodeSocketColor', 'Dye Color A')
    test_group.outputs.new('NodeSocketColor', 'Dye Color B')
    test_group.outputs.new('NodeSocketColor', 'Wear Remap_A')
    test_group.outputs.new('NodeSocketColor', 'Wear Remap_B')
    test_group.outputs.new('NodeSocketColor', 'Roughness Remap_A')
    test_group.outputs.new('NodeSocketColor', 'Roughness Remap_B')
    test_group.outputs.new('NodeSocketColor', 'Roughness Remap_C')
    test_group.outputs.new('NodeSocketColor', 'Detail Diffuse')
    test_group.outputs.new('NodeSocketColor', 'Detail Normal')
    test_group.outputs.new('NodeSocketColor', 'Detail Blends')
    test_group.outputs.new('NodeSocketColor', 'Worn Detail Blends')
    test_group.outputs.new('NodeSocketColor', 'Iridescence, Fuzz, Transmission')
    test_group.outputs.new('NodeSocketColor', 'Emission')

    # --------------------------------------------------------------------------------------------
    frame_007_1 = test_group.nodes.new('NodeFrame')
    frame_007_1.color = (0.0, 0.25, 0.25)
    frame_007_1.label = 'Worn Cloth Secondary'
    frame_007_1.location = (-680.0, -3440.0)
    frame_007_1.name = 'Frame.007'
    frame_007_1.use_custom_color = True

    frame_008_1 = test_group.nodes.new('NodeFrame')
    frame_008_1.color = (0.0, 0.0, 1.0)
    frame_008_1.label = 'Suit Primary'
    frame_008_1.location = (-680.0, -3860.0)
    frame_008_1.name = 'Frame.008'
    frame_008_1.use_custom_color = True

    frame_011_1 = test_group.nodes.new('NodeFrame')
    frame_011_1.color = (0.25, 0.25, 0.25)
    frame_011_1.label = 'Worn Suit Secondary'
    frame_011_1.location = (-700.0, -5900.0)
    frame_011_1.name = 'Frame.011'
    frame_011_1.use_custom_color = True

    frame_005_1 = test_group.nodes.new('NodeFrame')
    frame_005_1.color = (0.0, 0.25, 0.0)
    frame_005_1.label = 'Worn Cloth Primary'
    frame_005_1.location = (-680.0, -2280.0)
    frame_005_1.name = 'Frame.005'
    frame_005_1.use_custom_color = True

    frame_002_1 = test_group.nodes.new('NodeFrame')
    frame_002_1.color = (1.0, 1.0, 0.0)
    frame_002_1.label = 'Armor Secondary'
    frame_002_1.location = (-680.0, -120.0)
    frame_002_1.name = 'Frame.002'
    frame_002_1.use_custom_color = True

    frame_1 = test_group.nodes.new('NodeFrame')
    frame_1.color = (1.0, 0.0, 0.0)
    frame_1.label = 'Armor Primary'
    frame_1.location = (-660.0, 720.0)
    frame_1.name = 'Frame'
    frame_1.use_custom_color = True

    frame_009_1 = test_group.nodes.new('NodeFrame')
    frame_009_1.color = (0.0, 0.0, 0.25)
    frame_009_1.label = 'Worn Suit Primary'
    frame_009_1.location = (-680.0, -4660.0)
    frame_009_1.name = 'Frame.009'
    frame_009_1.use_custom_color = True

    frame_003_1 = test_group.nodes.new('NodeFrame')
    frame_003_1.color = (0.25, 0.25, 0.0)
    frame_003_1.label = 'Worn Armor Secondary'
    frame_003_1.location = (920.0, -1380.0)
    frame_003_1.name = 'Frame.003'
    frame_003_1.use_custom_color = True

    frame_006_1 = test_group.nodes.new('NodeFrame')
    frame_006_1.color = (0.0, 1.0, 1.0)
    frame_006_1.label = 'Cloth Secondary'
    frame_006_1.location = (-680.0, -2620.0)
    frame_006_1.name = 'Frame.006'
    frame_006_1.use_custom_color = True

    frame_010_1 = test_group.nodes.new('NodeFrame')
    frame_010_1.color = (1.0, 1.0, 1.0)
    frame_010_1.label = 'Suit Secondary'
    frame_010_1.location = (-700.0, -5080.0)
    frame_010_1.name = 'Frame.010'
    frame_010_1.use_custom_color = True

    frame_001_1 = test_group.nodes.new('NodeFrame')
    frame_001_1.color = (0.25, 0.0, 0.0)
    frame_001_1.label = 'Worn Armor Primary'
    frame_001_1.location = (-660.0, 300.0)
    frame_001_1.name = 'Frame.001'
    frame_001_1.use_custom_color = True

    frame_004_1 = test_group.nodes.new('NodeFrame')
    frame_004_1.color = (0.0, 1.0, 0.0)
    frame_004_1.label = 'Cloth Primary'
    frame_004_1.location = (-680.0, -1400.0)
    frame_004_1.name = 'Frame.004'
    frame_004_1.use_custom_color = True

    frame_012_1 = test_group.nodes.new('NodeFrame')
    frame_012_1.color = (0.0, 0.0, 0.0)
    frame_012_1.label = 'DO NOT TOUCH! DO NOT TOUCH! DO NOT TOUCH! DO NOT TOUCH!'
    frame_012_1.location = (2800.0, -1140.0)
    frame_012_1.name = 'Frame.012'
    frame_012_1.use_custom_color = True

    reroute_114_1 = test_group.nodes.new('NodeReroute')
    reroute_114_1.parent = test_group.nodes.get('Frame.008')
    reroute_114_1.location = (300.0, -100.0)

    reroute_116_1 = test_group.nodes.new('NodeReroute')
    reroute_116_1.parent = test_group.nodes.get('Frame.008')
    reroute_116_1.location = (300.0, -180.0)

    reroute_113_1 = test_group.nodes.new('NodeReroute')
    reroute_113_1.parent = test_group.nodes.get('Frame.008')
    reroute_113_1.location = (300.0, -60.0)

    reroute_115_1 = test_group.nodes.new('NodeReroute')
    reroute_115_1.parent = test_group.nodes.get('Frame.008')
    reroute_115_1.location = (300.0, -140.0)

    reroute_118_1 = test_group.nodes.new('NodeReroute')
    reroute_118_1.parent = test_group.nodes.get('Frame.008')
    reroute_118_1.location = (300.0, -240.0)

    reroute_112_1 = test_group.nodes.new('NodeReroute')
    reroute_112_1.parent = test_group.nodes.get('Frame.008')
    reroute_112_1.location = (300.0, -20.0)

    reroute_104_1 = test_group.nodes.new('NodeReroute')
    reroute_104_1.parent = test_group.nodes.get('Frame.007')
    reroute_104_1.location = (300.0, -60.0)

    reroute_105_1 = test_group.nodes.new('NodeReroute')
    reroute_105_1.parent = test_group.nodes.get('Frame.007')
    reroute_105_1.location = (300.0, -100.0)

    reroute_106_1 = test_group.nodes.new('NodeReroute')
    reroute_106_1.parent = test_group.nodes.get('Frame.007')
    reroute_106_1.location = (300.0, -140.0)

    reroute_107_1 = test_group.nodes.new('NodeReroute')
    reroute_107_1.parent = test_group.nodes.get('Frame.007')
    reroute_107_1.location = (300.0, -180.0)

    reroute_197_1 = test_group.nodes.new('NodeReroute')
    reroute_197_1.parent = test_group.nodes.get('Frame.011')
    reroute_197_1.location = (320.0, -120.0)

    reroute_196_1 = test_group.nodes.new('NodeReroute')
    reroute_196_1.parent = test_group.nodes.get('Frame.011')
    reroute_196_1.location = (320.0, -80.0)

    reroute_198_1 = test_group.nodes.new('NodeReroute')
    reroute_198_1.parent = test_group.nodes.get('Frame.011')
    reroute_198_1.location = (320.0, -160.0)

    reroute_199_1 = test_group.nodes.new('NodeReroute')
    reroute_199_1.parent = test_group.nodes.get('Frame.011')
    reroute_199_1.location = (320.0, -200.0)

    reroute_290_1 = test_group.nodes.new('NodeReroute')
    reroute_290_1.location = (-980.0, -360.0)

    reroute_288_1 = test_group.nodes.new('NodeReroute')
    reroute_288_1.location = (-980.0, 200.0)

    reroute_291_1 = test_group.nodes.new('NodeReroute')
    reroute_291_1.location = (-920.0, -160.0)

    reroute_289_1 = test_group.nodes.new('NodeReroute')
    reroute_289_1.location = (-980.0, 860.0)

    reroute_292_1 = test_group.nodes.new('NodeReroute')
    reroute_292_1.location = (-920.0, 660.0)

    reroute_293_1 = test_group.nodes.new('NodeReroute')
    reroute_293_1.location = (-920.0, -560.0)

    reroute_295_1 = test_group.nodes.new('NodeReroute')
    reroute_295_1.location = (-1720.0, -200.0)

    reroute_296_1 = test_group.nodes.new('NodeReroute')
    reroute_296_1.location = (-1720.0, 140.0)

    reroute_300_1 = test_group.nodes.new('NodeReroute')
    reroute_300_1.location = (-1720.0, -2500.0)

    reroute_299_1 = test_group.nodes.new('NodeReroute')
    reroute_299_1.location = (-1720.0, -2340.0)

    reroute_298_1 = test_group.nodes.new('NodeReroute')
    reroute_298_1.location = (-1720.0, -2680.0)

    reroute_301_1 = test_group.nodes.new('NodeReroute')
    reroute_301_1.location = (-1720.0, -4820.0)

    reroute_302_1 = test_group.nodes.new('NodeReroute')
    reroute_302_1.location = (-1720.0, -5160.0)

    uv_map_1 = test_group.nodes.new('ShaderNodeUVMap')
    uv_map_1.label = 'Detail UVs'
    uv_map_1.location = (-2320.0, -2460.0)
    uv_map_1.use_custom_color = False
    uv_map_1.uv_map = 'uv1'

    reroute_103_1 = test_group.nodes.new('NodeReroute')
    reroute_103_1.parent = test_group.nodes.get('Frame.007')
    reroute_103_1.location = (300.0, -20.0)

    reroute_195_1 = test_group.nodes.new('NodeReroute')
    reroute_195_1.parent = test_group.nodes.get('Frame.011')
    reroute_195_1.location = (320.0, -40.0)

    reroute_320_1 = test_group.nodes.new('NodeReroute')
    reroute_320_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_320_1.location = (-920.0, -2640.0)

    reroute_322_1 = test_group.nodes.new('NodeReroute')
    reroute_322_1.location = (-920.0, -3040.0)

    reroute_323_1 = test_group.nodes.new('NodeReroute')
    reroute_323_1.location = (-980.0, -1620.0)

    reroute_319_1 = test_group.nodes.new('NodeReroute')
    reroute_319_1.location = (-980.0, -2280.0)

    reroute_324_1 = test_group.nodes.new('NodeReroute')
    reroute_324_1.location = (-980.0, -2840.0)

    reroute_321_1 = test_group.nodes.new('NodeReroute')
    reroute_321_1.location = (-920.0, -1820.0)

    reroute_325_1 = test_group.nodes.new('NodeReroute')
    reroute_325_1.location = (-920.0, -5120.0)

    reroute_328_1 = test_group.nodes.new('NodeReroute')
    reroute_328_1.location = (-980.0, -4760.0)

    reroute_330_1 = test_group.nodes.new('NodeReroute')
    reroute_330_1.location = (-920.0, -4300.0)

    reroute_326_1 = test_group.nodes.new('NodeReroute')
    reroute_326_1.location = (-920.0, -5520.0)

    reroute_329_1 = test_group.nodes.new('NodeReroute')
    reroute_329_1.location = (-980.0, -5320.0)

    reroute_327_1 = test_group.nodes.new('NodeReroute')
    reroute_327_1.location = (-980.0, -4100.0)

    reroute_119_1 = test_group.nodes.new('NodeReroute')
    reroute_119_1.parent = test_group.nodes.get('Frame.008')
    reroute_119_1.location = (300.0, -260.0)

    reroute_120_1 = test_group.nodes.new('NodeReroute')
    reroute_120_1.parent = test_group.nodes.get('Frame.008')
    reroute_120_1.location = (300.0, -300.0)

    reroute_121_1 = test_group.nodes.new('NodeReroute')
    reroute_121_1.parent = test_group.nodes.get('Frame.008')
    reroute_121_1.location = (300.0, -340.0)

    reroute_122_1 = test_group.nodes.new('NodeReroute')
    reroute_122_1.parent = test_group.nodes.get('Frame.008')
    reroute_122_1.location = (300.0, -380.0)

    reroute_111_1 = test_group.nodes.new('NodeReroute')
    reroute_111_1.parent = test_group.nodes.get('Frame.007')
    reroute_111_1.location = (300.0, -340.0)

    reroute_127_1 = test_group.nodes.new('NodeReroute')
    reroute_127_1.parent = test_group.nodes.get('Frame.008')
    reroute_127_1.location = (300.0, -580.0)

    reroute_203_1 = test_group.nodes.new('NodeReroute')
    reroute_203_1.parent = test_group.nodes.get('Frame.011')
    reroute_203_1.location = (320.0, -360.0)

    reroute_123_1 = test_group.nodes.new('NodeReroute')
    reroute_123_1.parent = test_group.nodes.get('Frame.008')
    reroute_123_1.location = (300.0, -460.0)

    reroute_125_1 = test_group.nodes.new('NodeReroute')
    reroute_125_1.parent = test_group.nodes.get('Frame.008')
    reroute_125_1.location = (300.0, -500.0)

    reroute_126_1 = test_group.nodes.new('NodeReroute')
    reroute_126_1.parent = test_group.nodes.get('Frame.008')
    reroute_126_1.location = (300.0, -540.0)

    reroute_117_1 = test_group.nodes.new('NodeReroute')
    reroute_117_1.parent = test_group.nodes.get('Frame.008')
    reroute_117_1.location = (300.0, -220.0)

    reroute_124_1 = test_group.nodes.new('NodeReroute')
    reroute_124_1.parent = test_group.nodes.get('Frame.008')
    reroute_124_1.location = (300.0, -420.0)

    reroute_128_1 = test_group.nodes.new('NodeReroute')
    reroute_128_1.parent = test_group.nodes.get('Frame.008')
    reroute_128_1.location = (300.0, -620.0)

    reroute_367_1 = test_group.nodes.new('NodeReroute')
    reroute_367_1.parent = test_group.nodes.get('Frame.008')
    reroute_367_1.location = (300.0, -660.0)

    reroute_129_1 = test_group.nodes.new('NodeReroute')
    reroute_129_1.parent = test_group.nodes.get('Frame.008')
    reroute_129_1.location = (300.0, -700.0)

    reroute_082_1 = test_group.nodes.new('NodeReroute')
    reroute_082_1.parent = test_group.nodes.get('Frame.005')
    reroute_082_1.location = (300.0, -220.0)

    reroute_078_1 = test_group.nodes.new('NodeReroute')
    reroute_078_1.parent = test_group.nodes.get('Frame.005')
    reroute_078_1.location = (300.0, -60.0)

    reroute_079_1 = test_group.nodes.new('NodeReroute')
    reroute_079_1.parent = test_group.nodes.get('Frame.005')
    reroute_079_1.location = (300.0, -100.0)

    reroute_077_1 = test_group.nodes.new('NodeReroute')
    reroute_077_1.parent = test_group.nodes.get('Frame.005')
    reroute_077_1.location = (300.0, -20.0)

    reroute_076_1 = test_group.nodes.new('NodeReroute')
    reroute_076_1.parent = test_group.nodes.get('Frame.005')
    reroute_076_1.location = (300.0, 20.0)

    reroute_075_1 = test_group.nodes.new('NodeReroute')
    reroute_075_1.parent = test_group.nodes.get('Frame.005')
    reroute_075_1.location = (300.0, 60.0)

    reroute_083_1 = test_group.nodes.new('NodeReroute')
    reroute_083_1.parent = test_group.nodes.get('Frame.005')
    reroute_083_1.location = (300.0, -260.0)

    reroute_080_1 = test_group.nodes.new('NodeReroute')
    reroute_080_1.parent = test_group.nodes.get('Frame.005')
    reroute_080_1.location = (300.0, -140.0)

    reroute_081_1 = test_group.nodes.new('NodeReroute')
    reroute_081_1.parent = test_group.nodes.get('Frame.005')
    reroute_081_1.location = (300.0, -180.0)

    reroute_108_1 = test_group.nodes.new('NodeReroute')
    reroute_108_1.parent = test_group.nodes.get('Frame.007')
    reroute_108_1.location = (300.0, -220.0)

    reroute_109_1 = test_group.nodes.new('NodeReroute')
    reroute_109_1.parent = test_group.nodes.get('Frame.007')
    reroute_109_1.location = (300.0, -260.0)

    reroute_110_1 = test_group.nodes.new('NodeReroute')
    reroute_110_1.parent = test_group.nodes.get('Frame.007')
    reroute_110_1.location = (300.0, -300.0)

    reroute_200_1 = test_group.nodes.new('NodeReroute')
    reroute_200_1.parent = test_group.nodes.get('Frame.011')
    reroute_200_1.location = (320.0, -240.0)

    reroute_201_1 = test_group.nodes.new('NodeReroute')
    reroute_201_1.parent = test_group.nodes.get('Frame.011')
    reroute_201_1.location = (320.0, -280.0)

    reroute_202_1 = test_group.nodes.new('NodeReroute')
    reroute_202_1.parent = test_group.nodes.get('Frame.011')
    reroute_202_1.location = (320.0, -320.0)

    reroute_045_1 = test_group.nodes.new('NodeReroute')
    reroute_045_1.parent = test_group.nodes.get('Frame.002')
    reroute_045_1.location = (300.0, -720.0)

    reroute_364_1 = test_group.nodes.new('NodeReroute')
    reroute_364_1.parent = test_group.nodes.get('Frame.002')
    reroute_364_1.location = (300.0, -680.0)

    reroute_044_1 = test_group.nodes.new('NodeReroute')
    reroute_044_1.parent = test_group.nodes.get('Frame.002')
    reroute_044_1.location = (300.0, -640.0)

    reroute_039_1 = test_group.nodes.new('NodeReroute')
    reroute_039_1.parent = test_group.nodes.get('Frame.002')
    reroute_039_1.location = (300.0, -440.0)

    reroute_033_1 = test_group.nodes.new('NodeReroute')
    reroute_033_1.parent = test_group.nodes.get('Frame.002')
    reroute_033_1.location = (300.0, -240.0)

    reroute_042_1 = test_group.nodes.new('NodeReroute')
    reroute_042_1.parent = test_group.nodes.get('Frame.002')
    reroute_042_1.location = (300.0, -560.0)

    reroute_041_1 = test_group.nodes.new('NodeReroute')
    reroute_041_1.parent = test_group.nodes.get('Frame.002')
    reroute_041_1.location = (300.0, -520.0)

    reroute_040_1 = test_group.nodes.new('NodeReroute')
    reroute_040_1.parent = test_group.nodes.get('Frame.002')
    reroute_040_1.location = (300.0, -480.0)

    reroute_043_1 = test_group.nodes.new('NodeReroute')
    reroute_043_1.parent = test_group.nodes.get('Frame.002')
    reroute_043_1.location = (300.0, -600.0)

    reroute_038_1 = test_group.nodes.new('NodeReroute')
    reroute_038_1.parent = test_group.nodes.get('Frame.002')
    reroute_038_1.location = (300.0, -400.0)

    reroute_037_1 = test_group.nodes.new('NodeReroute')
    reroute_037_1.parent = test_group.nodes.get('Frame.002')
    reroute_037_1.location = (300.0, -360.0)

    reroute_036_1 = test_group.nodes.new('NodeReroute')
    reroute_036_1.parent = test_group.nodes.get('Frame.002')
    reroute_036_1.location = (300.0, -320.0)

    reroute_035_1 = test_group.nodes.new('NodeReroute')
    reroute_035_1.parent = test_group.nodes.get('Frame.002')
    reroute_035_1.location = (300.0, -280.0)

    reroute_028_1 = test_group.nodes.new('NodeReroute')
    reroute_028_1.parent = test_group.nodes.get('Frame.002')
    reroute_028_1.location = (300.0, -40.0)

    reroute_034_1 = test_group.nodes.new('NodeReroute')
    reroute_034_1.parent = test_group.nodes.get('Frame.002')
    reroute_034_1.location = (300.0, -260.0)

    reroute_032_1 = test_group.nodes.new('NodeReroute')
    reroute_032_1.parent = test_group.nodes.get('Frame.002')
    reroute_032_1.location = (300.0, -200.0)

    reroute_031_1 = test_group.nodes.new('NodeReroute')
    reroute_031_1.parent = test_group.nodes.get('Frame.002')
    reroute_031_1.location = (300.0, -160.0)

    reroute_030_1 = test_group.nodes.new('NodeReroute')
    reroute_030_1.parent = test_group.nodes.get('Frame.002')
    reroute_030_1.location = (300.0, -120.0)

    reroute_029_1 = test_group.nodes.new('NodeReroute')
    reroute_029_1.parent = test_group.nodes.get('Frame.002')
    reroute_029_1.location = (300.0, -80.0)

    reroute_046_1 = test_group.nodes.new('NodeReroute')
    reroute_046_1.parent = test_group.nodes.get('Frame.002')
    reroute_046_1.location = (300.0, -760.0)

    reroute_130_1 = test_group.nodes.new('NodeReroute')
    reroute_130_1.parent = test_group.nodes.get('Frame.008')
    reroute_130_1.location = (300.0, -740.0)

    reroute_010_1 = test_group.nodes.new('NodeReroute')
    reroute_010_1.parent = test_group.nodes.get('Frame')
    reroute_010_1.location = (280.0, 0.0)

    reroute_004_1 = test_group.nodes.new('NodeReroute')
    reroute_004_1.parent = test_group.nodes.get('Frame')
    reroute_004_1.location = (280.0, 200.0)

    reroute_003_1 = test_group.nodes.new('NodeReroute')
    reroute_003_1.parent = test_group.nodes.get('Frame')
    reroute_003_1.location = (280.0, 240.0)

    reroute_002_1 = test_group.nodes.new('NodeReroute')
    reroute_002_1.parent = test_group.nodes.get('Frame')
    reroute_002_1.location = (280.0, 280.0)

    reroute_001_1 = test_group.nodes.new('NodeReroute')
    reroute_001_1.parent = test_group.nodes.get('Frame')
    reroute_001_1.location = (280.0, 320.0)

    reroute_006_1 = test_group.nodes.new('NodeReroute')
    reroute_006_1.parent = test_group.nodes.get('Frame')
    reroute_006_1.location = (280.0, 140.0)

    reroute_007_1 = test_group.nodes.new('NodeReroute')
    reroute_007_1.parent = test_group.nodes.get('Frame')
    reroute_007_1.location = (280.0, 120.0)

    reroute_008_1 = test_group.nodes.new('NodeReroute')
    reroute_008_1.parent = test_group.nodes.get('Frame')
    reroute_008_1.location = (280.0, 80.0)

    reroute_009_1 = test_group.nodes.new('NodeReroute')
    reroute_009_1.parent = test_group.nodes.get('Frame')
    reroute_009_1.location = (280.0, 40.0)

    reroute_012_1 = test_group.nodes.new('NodeReroute')
    reroute_012_1.parent = test_group.nodes.get('Frame')
    reroute_012_1.location = (280.0, -80.0)

    reroute_013_1 = test_group.nodes.new('NodeReroute')
    reroute_013_1.parent = test_group.nodes.get('Frame')
    reroute_013_1.location = (280.0, -120.0)

    reroute_014_1 = test_group.nodes.new('NodeReroute')
    reroute_014_1.parent = test_group.nodes.get('Frame')
    reroute_014_1.location = (280.0, -160.0)

    reroute_005_1 = test_group.nodes.new('NodeReroute')
    reroute_005_1.parent = test_group.nodes.get('Frame')
    reroute_005_1.location = (280.0, 160.0)

    reroute_011_1 = test_group.nodes.new('NodeReroute')
    reroute_011_1.parent = test_group.nodes.get('Frame')
    reroute_011_1.location = (280.0, -40.0)

    reroute_015_1 = test_group.nodes.new('NodeReroute')
    reroute_015_1.parent = test_group.nodes.get('Frame')
    reroute_015_1.location = (280.0, -200.0)

    reroute_016_1 = test_group.nodes.new('NodeReroute')
    reroute_016_1.parent = test_group.nodes.get('Frame')
    reroute_016_1.location = (280.0, -240.0)

    reroute_363_1 = test_group.nodes.new('NodeReroute')
    reroute_363_1.parent = test_group.nodes.get('Frame')
    reroute_363_1.location = (280.0, -280.0)

    reroute_017_1 = test_group.nodes.new('NodeReroute')
    reroute_017_1.parent = test_group.nodes.get('Frame')
    reroute_017_1.location = (280.0, -320.0)

    reroute_018_1 = test_group.nodes.new('NodeReroute')
    reroute_018_1.parent = test_group.nodes.get('Frame')
    reroute_018_1.location = (280.0, -360.0)

    reroute_132_1 = test_group.nodes.new('NodeReroute')
    reroute_132_1.parent = test_group.nodes.get('Frame.009')
    reroute_132_1.location = (300.0, -80.0)

    reroute_133_1 = test_group.nodes.new('NodeReroute')
    reroute_133_1.parent = test_group.nodes.get('Frame.009')
    reroute_133_1.location = (300.0, -120.0)

    reroute_134_1 = test_group.nodes.new('NodeReroute')
    reroute_134_1.parent = test_group.nodes.get('Frame.009')
    reroute_134_1.location = (300.0, -160.0)

    reroute_135_1 = test_group.nodes.new('NodeReroute')
    reroute_135_1.parent = test_group.nodes.get('Frame.009')
    reroute_135_1.location = (300.0, -200.0)

    reroute_131_1 = test_group.nodes.new('NodeReroute')
    reroute_131_1.parent = test_group.nodes.get('Frame.009')
    reroute_131_1.location = (300.0, -40.0)

    reroute_139_1 = test_group.nodes.new('NodeReroute')
    reroute_139_1.parent = test_group.nodes.get('Frame.009')
    reroute_139_1.location = (300.0, -360.0)

    reroute_136_1 = test_group.nodes.new('NodeReroute')
    reroute_136_1.parent = test_group.nodes.get('Frame.009')
    reroute_136_1.location = (300.0, -240.0)

    reroute_137_1 = test_group.nodes.new('NodeReroute')
    reroute_137_1.parent = test_group.nodes.get('Frame.009')
    reroute_137_1.location = (300.0, -280.0)

    reroute_138_1 = test_group.nodes.new('NodeReroute')
    reroute_138_1.parent = test_group.nodes.get('Frame.009')
    reroute_138_1.location = (300.0, -320.0)

    armor_primary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    armor_primary_dye_color_1.parent = test_group.nodes.get('Frame')
    armor_primary_dye_color_1.label = 'Dye Color'
    armor_primary_dye_color_1.location = (0.0, 360.0)
    armor_primary_dye_color_1.hide = True

    armor_primary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame')
    armor_primary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    armor_primary_detail_diffuse_blend_1.location = (0.0, -80.0)
    armor_primary_detail_diffuse_blend_1.hide = True

    armor_primary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_detail_normal_blend_1.parent = test_group.nodes.get('Frame')
    armor_primary_detail_normal_blend_1.label = 'Detail Normal Blend'
    armor_primary_detail_normal_blend_1.location = (0.0, -120.0)
    armor_primary_detail_normal_blend_1.hide = True

    armor_primary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame')
    armor_primary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    armor_primary_detail_roughness_blend_1.location = (0.0, -160.0)
    armor_primary_detail_roughness_blend_1.hide = True

    armor_secondary_detail_diffuse_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    armor_secondary_detail_diffuse_map_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_detail_diffuse_map_1.label = 'Detail Diffuse Map'
    armor_secondary_detail_diffuse_map_1.location = (20.0, -240.0)
    armor_secondary_detail_diffuse_map_1.hide = True

    armor_secondary_detail_normal_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    armor_secondary_detail_normal_map_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_detail_normal_map_1.label = 'Detail Normal Map'
    armor_secondary_detail_normal_map_1.location = (20.0, -440.0)
    armor_secondary_detail_normal_map_1.hide = True

    armor_secondary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    armor_secondary_detail_diffuse_blend_1.location = (20.0, -480.0)
    armor_secondary_detail_diffuse_blend_1.hide = True

    armor_secondary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_detail_normal_blend_1.label = 'Detail Normal Blend'
    armor_secondary_detail_normal_blend_1.location = (20.0, -520.0)
    armor_secondary_detail_normal_blend_1.hide = True

    armor_secondary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    armor_secondary_detail_roughness_blend_1.location = (20.0, -560.0)
    armor_secondary_detail_roughness_blend_1.hide = True

    armor_secondary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_metalness_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_metalness_1.label = 'Metalness'
    armor_secondary_metalness_1.location = (20.0, -600.0)
    armor_secondary_metalness_1.hide = True

    armor_secondary_iridescence_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_iridescence_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_iridescence_1.label = 'Iridescence'
    armor_secondary_iridescence_1.location = (20.0, -640.0)
    armor_secondary_iridescence_1.hide = True

    armor_secondary_fuzz_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_fuzz_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_fuzz_1.label = 'Fuzz'
    armor_secondary_fuzz_1.location = (20.0, -680.0)
    armor_secondary_fuzz_1.hide = True

    armor_secondary_transmission_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_transmission_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_transmission_1.label = 'Transmission'
    armor_secondary_transmission_1.location = (20.0, -720.0)
    armor_secondary_transmission_1.hide = True

    armor_secondary_emission_color_1 = test_group.nodes.new('ShaderNodeRGB')
    armor_secondary_emission_color_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_emission_color_1.label = 'Emission Color'
    armor_secondary_emission_color_1.location = (20.0, -760.0)
    armor_secondary_emission_color_1.hide = True

    reroute_048_1 = test_group.nodes.new('NodeReroute')
    reroute_048_1.parent = test_group.nodes.get('Frame.003')
    reroute_048_1.location = (-1300.0, 360.0)

    reroute_049_1 = test_group.nodes.new('NodeReroute')
    reroute_049_1.parent = test_group.nodes.get('Frame.003')
    reroute_049_1.location = (-1300.0, 320.0)

    reroute_050_1 = test_group.nodes.new('NodeReroute')
    reroute_050_1.parent = test_group.nodes.get('Frame.003')
    reroute_050_1.location = (-1300.0, 280.0)

    reroute_051_1 = test_group.nodes.new('NodeReroute')
    reroute_051_1.parent = test_group.nodes.get('Frame.003')
    reroute_051_1.location = (-1300.0, 240.0)

    reroute_047_1 = test_group.nodes.new('NodeReroute')
    reroute_047_1.parent = test_group.nodes.get('Frame.003')
    reroute_047_1.location = (-1300.0, 400.0)

    reroute_055_1 = test_group.nodes.new('NodeReroute')
    reroute_055_1.parent = test_group.nodes.get('Frame.003')
    reroute_055_1.location = (-1300.0, 80.0)

    reroute_052_1 = test_group.nodes.new('NodeReroute')
    reroute_052_1.parent = test_group.nodes.get('Frame.003')
    reroute_052_1.location = (-1300.0, 200.0)

    reroute_053_1 = test_group.nodes.new('NodeReroute')
    reroute_053_1.parent = test_group.nodes.get('Frame.003')
    reroute_053_1.location = (-1300.0, 160.0)

    reroute_054_1 = test_group.nodes.new('NodeReroute')
    reroute_054_1.parent = test_group.nodes.get('Frame.003')
    reroute_054_1.location = (-1300.0, 120.0)

    worn_armor_secondary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    worn_armor_secondary_detail_diffuse_blend_1.location = (-1580.0, 200.0)
    worn_armor_secondary_detail_diffuse_blend_1.hide = True

    worn_armor_secondary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_detail_normal_blend_1.label = 'Detail Normal Blend'
    worn_armor_secondary_detail_normal_blend_1.location = (-1580.0, 160.0)
    worn_armor_secondary_detail_normal_blend_1.hide = True

    worn_armor_secondary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    worn_armor_secondary_detail_roughness_blend_1.location = (-1580.0, 120.0)
    worn_armor_secondary_detail_roughness_blend_1.hide = True

    armor_secondary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    armor_secondary_dye_color_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_dye_color_1.label = 'Dye Color'
    armor_secondary_dye_color_1.location = (20.0, -40.0)
    armor_secondary_dye_color_1.hide = True

    worn_armor_secondary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_metalness_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_metalness_1.label = 'Metalness'
    worn_armor_secondary_metalness_1.location = (-1580.0, 80.0)
    worn_armor_secondary_metalness_1.hide = True

    worn_armor_secondary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    worn_armor_secondary_dye_color_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_dye_color_1.label = 'Dye Color'
    worn_armor_secondary_dye_color_1.location = (-1580.0, 400.0)
    worn_armor_secondary_dye_color_1.hide = True

    worn_cloth_primary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    worn_cloth_primary_dye_color_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_dye_color_1.label = 'Dye Color'
    worn_cloth_primary_dye_color_1.location = (20.0, 60.0)
    worn_cloth_primary_dye_color_1.hide = True

    worn_cloth_primary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    worn_cloth_primary_detail_diffuse_blend_1.location = (20.0, -140.0)
    worn_cloth_primary_detail_diffuse_blend_1.hide = True

    worn_cloth_primary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_detail_normal_blend_1.label = 'Detail Normal Blend'
    worn_cloth_primary_detail_normal_blend_1.location = (20.0, -180.0)
    worn_cloth_primary_detail_normal_blend_1.hide = True

    worn_cloth_primary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    worn_cloth_primary_detail_roughness_blend_1.location = (20.0, -220.0)
    worn_cloth_primary_detail_roughness_blend_1.hide = True

    worn_cloth_primary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_metalness_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_metalness_1.label = 'Metalness'
    worn_cloth_primary_metalness_1.location = (20.0, -260.0)
    worn_cloth_primary_metalness_1.hide = True

    armor_detail_normal_transform_1 = test_group.nodes.new('ShaderNodeMapping')
    armor_detail_normal_transform_1.label = 'Detail Normal Transform'
    armor_detail_normal_transform_1.location = (-1200.0, -120.0)
    armor_detail_normal_transform_1.vector_type = 'POINT'

    cloth_detail_diffuse_transform_1 = test_group.nodes.new('ShaderNodeMapping')
    cloth_detail_diffuse_transform_1.label = 'Detail Diffuse Transform'
    cloth_detail_diffuse_transform_1.location = (-1200.0, -2240.0)
    cloth_detail_diffuse_transform_1.vector_type = 'POINT'

    suit_detail_diffuse_transform_1 = test_group.nodes.new('ShaderNodeMapping')
    suit_detail_diffuse_transform_1.label = 'Detail Diffuse Transform'
    suit_detail_diffuse_transform_1.location = (-1200.0, -4720.0)
    suit_detail_diffuse_transform_1.vector_type = 'POINT'

    suit_detail_normal_transform_1 = test_group.nodes.new('ShaderNodeMapping')
    suit_detail_normal_transform_1.label = 'Detail Normal Transform'
    suit_detail_normal_transform_1.location = (-1200.0, -5080.0)
    suit_detail_normal_transform_1.vector_type = 'POINT'

    cloth_detail_normal_transform_1 = test_group.nodes.new('ShaderNodeMapping')
    cloth_detail_normal_transform_1.label = 'Detail Normal Transform'
    cloth_detail_normal_transform_1.location = (-1200.0, -2600.0)
    cloth_detail_normal_transform_1.vector_type = 'POINT'

    reroute_086_1 = test_group.nodes.new('NodeReroute')
    reroute_086_1.parent = test_group.nodes.get('Frame.006')
    reroute_086_1.location = (300.0, -100.0)

    reroute_088_1 = test_group.nodes.new('NodeReroute')
    reroute_088_1.parent = test_group.nodes.get('Frame.006')
    reroute_088_1.location = (300.0, -180.0)

    reroute_085_1 = test_group.nodes.new('NodeReroute')
    reroute_085_1.parent = test_group.nodes.get('Frame.006')
    reroute_085_1.location = (300.0, -60.0)

    reroute_087_1 = test_group.nodes.new('NodeReroute')
    reroute_087_1.parent = test_group.nodes.get('Frame.006')
    reroute_087_1.location = (300.0, -140.0)

    reroute_090_1 = test_group.nodes.new('NodeReroute')
    reroute_090_1.parent = test_group.nodes.get('Frame.006')
    reroute_090_1.location = (300.0, -240.0)

    reroute_084_1 = test_group.nodes.new('NodeReroute')
    reroute_084_1.parent = test_group.nodes.get('Frame.006')
    reroute_084_1.location = (300.0, -20.0)

    reroute_091_1 = test_group.nodes.new('NodeReroute')
    reroute_091_1.parent = test_group.nodes.get('Frame.006')
    reroute_091_1.location = (300.0, -260.0)

    reroute_092_1 = test_group.nodes.new('NodeReroute')
    reroute_092_1.parent = test_group.nodes.get('Frame.006')
    reroute_092_1.location = (300.0, -300.0)

    reroute_093_1 = test_group.nodes.new('NodeReroute')
    reroute_093_1.parent = test_group.nodes.get('Frame.006')
    reroute_093_1.location = (300.0, -340.0)

    reroute_094_1 = test_group.nodes.new('NodeReroute')
    reroute_094_1.parent = test_group.nodes.get('Frame.006')
    reroute_094_1.location = (300.0, -380.0)

    reroute_099_1 = test_group.nodes.new('NodeReroute')
    reroute_099_1.parent = test_group.nodes.get('Frame.006')
    reroute_099_1.location = (300.0, -580.0)

    reroute_095_1 = test_group.nodes.new('NodeReroute')
    reroute_095_1.parent = test_group.nodes.get('Frame.006')
    reroute_095_1.location = (300.0, -460.0)

    reroute_097_1 = test_group.nodes.new('NodeReroute')
    reroute_097_1.parent = test_group.nodes.get('Frame.006')
    reroute_097_1.location = (300.0, -500.0)

    reroute_098_1 = test_group.nodes.new('NodeReroute')
    reroute_098_1.parent = test_group.nodes.get('Frame.006')
    reroute_098_1.location = (300.0, -540.0)

    reroute_089_1 = test_group.nodes.new('NodeReroute')
    reroute_089_1.parent = test_group.nodes.get('Frame.006')
    reroute_089_1.location = (300.0, -220.0)

    reroute_096_1 = test_group.nodes.new('NodeReroute')
    reroute_096_1.parent = test_group.nodes.get('Frame.006')
    reroute_096_1.location = (300.0, -420.0)

    reroute_100_1 = test_group.nodes.new('NodeReroute')
    reroute_100_1.parent = test_group.nodes.get('Frame.006')
    reroute_100_1.location = (300.0, -620.0)

    reroute_366_1 = test_group.nodes.new('NodeReroute')
    reroute_366_1.parent = test_group.nodes.get('Frame.006')
    reroute_366_1.location = (300.0, -660.0)

    reroute_101_1 = test_group.nodes.new('NodeReroute')
    reroute_101_1.parent = test_group.nodes.get('Frame.006')
    reroute_101_1.location = (300.0, -700.0)

    reroute_102_1 = test_group.nodes.new('NodeReroute')
    reroute_102_1.parent = test_group.nodes.get('Frame.006')
    reroute_102_1.location = (300.0, -740.0)

    cloth_secondary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    cloth_secondary_dye_color_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_dye_color_1.label = 'Dye Color'
    cloth_secondary_dye_color_1.location = (20.0, -20.0)
    cloth_secondary_dye_color_1.hide = True

    cloth_secondary_detail_diffuse_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    cloth_secondary_detail_diffuse_map_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_detail_diffuse_map_1.label = 'Detail Diffuse Map'
    cloth_secondary_detail_diffuse_map_1.location = (20.0, -220.0)
    cloth_secondary_detail_diffuse_map_1.hide = True

    cloth_secondary_detail_normal_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    cloth_secondary_detail_normal_map_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_detail_normal_map_1.label = 'Detail Normal Map'
    cloth_secondary_detail_normal_map_1.location = (20.0, -420.0)
    cloth_secondary_detail_normal_map_1.hide = True

    cloth_secondary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    cloth_secondary_detail_diffuse_blend_1.location = (20.0, -460.0)
    cloth_secondary_detail_diffuse_blend_1.hide = True

    cloth_secondary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_detail_normal_blend_1.label = 'Detail Normal Blend'
    cloth_secondary_detail_normal_blend_1.location = (20.0, -500.0)
    cloth_secondary_detail_normal_blend_1.hide = True

    cloth_secondary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    cloth_secondary_detail_roughness_blend_1.location = (20.0, -540.0)
    cloth_secondary_detail_roughness_blend_1.hide = True

    cloth_secondary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_metalness_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_metalness_1.label = 'Metalness'
    cloth_secondary_metalness_1.location = (20.0, -580.0)
    cloth_secondary_metalness_1.hide = True

    cloth_secondary_iridescence_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_iridescence_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_iridescence_1.label = 'Iridescence'
    cloth_secondary_iridescence_1.location = (20.0, -620.0)
    cloth_secondary_iridescence_1.hide = True

    cloth_secondary_fuzz_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_fuzz_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_fuzz_1.label = 'Fuzz'
    cloth_secondary_fuzz_1.location = (20.0, -660.0)
    cloth_secondary_fuzz_1.hide = True

    cloth_secondary_transmission_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_transmission_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_transmission_1.label = 'Transmission'
    cloth_secondary_transmission_1.location = (20.0, -700.0)
    cloth_secondary_transmission_1.hide = True

    cloth_secondary_emission_color_1 = test_group.nodes.new('ShaderNodeRGB')
    cloth_secondary_emission_color_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_emission_color_1.label = 'Emission Color'
    cloth_secondary_emission_color_1.location = (20.0, -740.0)
    cloth_secondary_emission_color_1.hide = True

    worn_cloth_secondary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    worn_cloth_secondary_dye_color_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_dye_color_1.label = 'Dye Color'
    worn_cloth_secondary_dye_color_1.location = (20.0, -20.0)
    worn_cloth_secondary_dye_color_1.hide = True

    worn_cloth_secondary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    worn_cloth_secondary_detail_diffuse_blend_1.location = (20.0, -220.0)
    worn_cloth_secondary_detail_diffuse_blend_1.hide = True

    worn_cloth_secondary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_detail_normal_blend_1.label = 'Detail Normal Blend'
    worn_cloth_secondary_detail_normal_blend_1.location = (20.0, -260.0)
    worn_cloth_secondary_detail_normal_blend_1.hide = True

    worn_cloth_secondary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    worn_cloth_secondary_detail_roughness_blend_1.location = (20.0, -300.0)
    worn_cloth_secondary_detail_roughness_blend_1.hide = True

    worn_cloth_secondary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_metalness_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_metalness_1.label = 'Metalness'
    worn_cloth_secondary_metalness_1.location = (20.0, -340.0)
    worn_cloth_secondary_metalness_1.hide = True

    suit_primary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    suit_primary_dye_color_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_dye_color_1.label = 'Dye Color'
    suit_primary_dye_color_1.location = (20.0, -20.0)
    suit_primary_dye_color_1.hide = True

    suit_primary_detail_diffuse_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    suit_primary_detail_diffuse_map_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_detail_diffuse_map_1.label = 'Detail Diffuse Map'
    suit_primary_detail_diffuse_map_1.location = (20.0, -220.0)
    suit_primary_detail_diffuse_map_1.hide = True

    suit_primary_detail_normal_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    suit_primary_detail_normal_map_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_detail_normal_map_1.label = 'Detail Normal Map'
    suit_primary_detail_normal_map_1.location = (20.0, -420.0)
    suit_primary_detail_normal_map_1.hide = True

    suit_primary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    suit_primary_detail_diffuse_blend_1.location = (20.0, -460.0)
    suit_primary_detail_diffuse_blend_1.hide = True

    suit_primary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_detail_normal_blend_1.label = 'Detail Normal Blend'
    suit_primary_detail_normal_blend_1.location = (20.0, -500.0)
    suit_primary_detail_normal_blend_1.hide = True

    suit_primary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    suit_primary_detail_roughness_blend_1.location = (20.0, -540.0)
    suit_primary_detail_roughness_blend_1.hide = True

    suit_primary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_metalness_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_metalness_1.label = 'Metalness'
    suit_primary_metalness_1.location = (20.0, -580.0)
    suit_primary_metalness_1.hide = True

    suit_primary_iridescence_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_iridescence_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_iridescence_1.label = 'Iridescence'
    suit_primary_iridescence_1.location = (20.0, -620.0)
    suit_primary_iridescence_1.hide = True

    suit_primary_fuzz_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_fuzz_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_fuzz_1.label = 'Fuzz'
    suit_primary_fuzz_1.location = (20.0, -660.0)
    suit_primary_fuzz_1.hide = True

    suit_primary_transmission_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_transmission_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_transmission_1.label = 'Transmission'
    suit_primary_transmission_1.location = (20.0, -700.0)
    suit_primary_transmission_1.hide = True

    suit_primary_emission_color_1 = test_group.nodes.new('ShaderNodeRGB')
    suit_primary_emission_color_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_emission_color_1.label = 'Emission Color'
    suit_primary_emission_color_1.location = (20.0, -740.0)
    suit_primary_emission_color_1.hide = True

    worn_suit_primary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    worn_suit_primary_dye_color_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_dye_color_1.label = 'Dye Color'
    worn_suit_primary_dye_color_1.location = (20.0, -40.0)
    worn_suit_primary_dye_color_1.hide = True

    worn_suit_primary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    worn_suit_primary_detail_diffuse_blend_1.location = (20.0, -240.0)
    worn_suit_primary_detail_diffuse_blend_1.hide = True

    worn_suit_primary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_detail_normal_blend_1.label = 'Detail Normal Blend'
    worn_suit_primary_detail_normal_blend_1.location = (20.0, -280.0)
    worn_suit_primary_detail_normal_blend_1.hide = True

    worn_suit_primary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    worn_suit_primary_detail_roughness_blend_1.location = (20.0, -320.0)
    worn_suit_primary_detail_roughness_blend_1.hide = True

    worn_suit_primary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_metalness_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_metalness_1.label = 'Metalness'
    worn_suit_primary_metalness_1.location = (20.0, -360.0)
    worn_suit_primary_metalness_1.hide = True

    reroute_180_1 = test_group.nodes.new('NodeReroute')
    reroute_180_1.parent = test_group.nodes.get('Frame.010')
    reroute_180_1.location = (320.0, -200.0)

    reroute_181_1 = test_group.nodes.new('NodeReroute')
    reroute_181_1.parent = test_group.nodes.get('Frame.010')
    reroute_181_1.location = (320.0, -160.0)

    reroute_177_1 = test_group.nodes.new('NodeReroute')
    reroute_177_1.parent = test_group.nodes.get('Frame.010')
    reroute_177_1.location = (320.0, -80.0)

    reroute_182_1 = test_group.nodes.new('NodeReroute')
    reroute_182_1.parent = test_group.nodes.get('Frame.010')
    reroute_182_1.location = (320.0, -120.0)

    reroute_178_1 = test_group.nodes.new('NodeReroute')
    reroute_178_1.parent = test_group.nodes.get('Frame.010')
    reroute_178_1.location = (320.0, -260.0)

    reroute_176_1 = test_group.nodes.new('NodeReroute')
    reroute_176_1.parent = test_group.nodes.get('Frame.010')
    reroute_176_1.location = (320.0, -40.0)

    reroute_183_1 = test_group.nodes.new('NodeReroute')
    reroute_183_1.parent = test_group.nodes.get('Frame.010')
    reroute_183_1.location = (320.0, -280.0)

    reroute_184_1 = test_group.nodes.new('NodeReroute')
    reroute_184_1.parent = test_group.nodes.get('Frame.010')
    reroute_184_1.location = (320.0, -320.0)

    reroute_185_1 = test_group.nodes.new('NodeReroute')
    reroute_185_1.parent = test_group.nodes.get('Frame.010')
    reroute_185_1.location = (320.0, -360.0)

    reroute_186_1 = test_group.nodes.new('NodeReroute')
    reroute_186_1.parent = test_group.nodes.get('Frame.010')
    reroute_186_1.location = (320.0, -400.0)

    reroute_190_1 = test_group.nodes.new('NodeReroute')
    reroute_190_1.parent = test_group.nodes.get('Frame.010')
    reroute_190_1.location = (320.0, -600.0)

    reroute_187_1 = test_group.nodes.new('NodeReroute')
    reroute_187_1.parent = test_group.nodes.get('Frame.010')
    reroute_187_1.location = (320.0, -480.0)

    reroute_188_1 = test_group.nodes.new('NodeReroute')
    reroute_188_1.parent = test_group.nodes.get('Frame.010')
    reroute_188_1.location = (320.0, -520.0)

    reroute_189_1 = test_group.nodes.new('NodeReroute')
    reroute_189_1.parent = test_group.nodes.get('Frame.010')
    reroute_189_1.location = (320.0, -560.0)

    reroute_179_1 = test_group.nodes.new('NodeReroute')
    reroute_179_1.parent = test_group.nodes.get('Frame.010')
    reroute_179_1.location = (320.0, -240.0)

    reroute_194_1 = test_group.nodes.new('NodeReroute')
    reroute_194_1.parent = test_group.nodes.get('Frame.010')
    reroute_194_1.location = (320.0, -440.0)

    reroute_191_1 = test_group.nodes.new('NodeReroute')
    reroute_191_1.parent = test_group.nodes.get('Frame.010')
    reroute_191_1.location = (320.0, -640.0)

    reroute_368_1 = test_group.nodes.new('NodeReroute')
    reroute_368_1.parent = test_group.nodes.get('Frame.010')
    reroute_368_1.location = (320.0, -680.0)

    reroute_192_1 = test_group.nodes.new('NodeReroute')
    reroute_192_1.parent = test_group.nodes.get('Frame.010')
    reroute_192_1.location = (320.0, -720.0)

    reroute_193_1 = test_group.nodes.new('NodeReroute')
    reroute_193_1.parent = test_group.nodes.get('Frame.010')
    reroute_193_1.location = (320.0, -760.0)

    suit_secondary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    suit_secondary_dye_color_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_dye_color_1.hide = True
    suit_secondary_dye_color_1.label = 'Dye Color'
    suit_secondary_dye_color_1.location = (40.0, -40.0)
    suit_secondary_dye_color_1.name = 'Suit Secondary Dye Color'
    suit_secondary_dye_color_1.width = 140.0

    suit_secondary_detail_diffuse_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    suit_secondary_detail_diffuse_map_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_detail_diffuse_map_1.hide = True
    suit_secondary_detail_diffuse_map_1.label = 'Detail Diffuse Map'
    suit_secondary_detail_diffuse_map_1.location = (40.0, -240.0)
    suit_secondary_detail_diffuse_map_1.name = 'Suit Secondary Detail Diffuse Map'

    suit_secondary_detail_normal_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    suit_secondary_detail_normal_map_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_detail_normal_map_1.hide = True
    suit_secondary_detail_normal_map_1.label = 'Detail Normal Map'
    suit_secondary_detail_normal_map_1.location = (40.0, -440.0)
    suit_secondary_detail_normal_map_1.name = 'Suit Secondary Detail Normal Map'

    suit_secondary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_detail_diffuse_blend_1.hide = True
    suit_secondary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    suit_secondary_detail_diffuse_blend_1.location = (40.0, -480.0)
    suit_secondary_detail_diffuse_blend_1.name = 'Suit Secondary Detail Diffuse Blend'
    suit_secondary_detail_diffuse_blend_1.width = 140.0

    suit_secondary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_detail_normal_blend_1.hide = True
    suit_secondary_detail_normal_blend_1.label = 'Detail Normal Blend'
    suit_secondary_detail_normal_blend_1.location = (40.0, -520.0)
    suit_secondary_detail_normal_blend_1.name = 'Suit Secondary Detail Normal Blend'
    suit_secondary_detail_normal_blend_1.width = 140.0

    suit_secondary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_detail_roughness_blend_1.hide = True
    suit_secondary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    suit_secondary_detail_roughness_blend_1.location = (40.0, -560.0)
    suit_secondary_detail_roughness_blend_1.name = 'Suit Secondary Detail Roughness Blend'
    suit_secondary_detail_roughness_blend_1.width = 140.0

    suit_secondary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_metalness_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_metalness_1.hide = True
    suit_secondary_metalness_1.label = 'Metalness'
    suit_secondary_metalness_1.location = (40.0, -600.0)
    suit_secondary_metalness_1.name = 'Suit Secondary Metalness'
    suit_secondary_metalness_1.width = 140.0

    suit_secondary_iridescence_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_iridescence_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_iridescence_1.hide = True
    suit_secondary_iridescence_1.label = 'Iridescence'
    suit_secondary_iridescence_1.location = (40.0, -640.0)
    suit_secondary_iridescence_1.name = 'Suit Secondary Iridescence'
    suit_secondary_iridescence_1.width = 140.0

    suit_secondary_fuzz_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_fuzz_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_fuzz_1.hide = True
    suit_secondary_fuzz_1.label = 'Fuzz'
    suit_secondary_fuzz_1.location = (40.0, -680.0)
    suit_secondary_fuzz_1.name = 'Suit Secondary Fuzz'
    suit_secondary_fuzz_1.width = 140.0

    suit_secondary_transmission_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_transmission_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_transmission_1.hide = True
    suit_secondary_transmission_1.label = 'Transmission'
    suit_secondary_transmission_1.location = (40.0, -720.0)
    suit_secondary_transmission_1.name = 'Suit Secondary Transmission'
    suit_secondary_transmission_1.width = 140.0

    suit_secondary_emission_color_1 = test_group.nodes.new('ShaderNodeRGB')
    suit_secondary_emission_color_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_emission_color_1.hide = True
    suit_secondary_emission_color_1.label = 'Emission Color'
    suit_secondary_emission_color_1.location = (40.0, -760.0)
    suit_secondary_emission_color_1.name = 'Suit Secondary Emission Color'
    suit_secondary_emission_color_1.width = 140.0

    worn_suit_secondary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    worn_suit_secondary_dye_color_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_dye_color_1.hide = True
    worn_suit_secondary_dye_color_1.label = 'Dye Color'
    worn_suit_secondary_dye_color_1.location = (40.0, -40.0)
    worn_suit_secondary_dye_color_1.name = 'Worn Suit Secondary Dye Color'
    worn_suit_secondary_dye_color_1.width = 140.0

    worn_suit_secondary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_detail_diffuse_blend_1.hide = True
    worn_suit_secondary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    worn_suit_secondary_detail_diffuse_blend_1.location = (40.0, -240.0)
    worn_suit_secondary_detail_diffuse_blend_1.name = 'Worn Suit Secondary Detail Diffuse Blend'
    worn_suit_secondary_detail_diffuse_blend_1.width = 140.0

    worn_suit_secondary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_detail_normal_blend_1.hide = True
    worn_suit_secondary_detail_normal_blend_1.label = 'Detail Normal Blend'
    worn_suit_secondary_detail_normal_blend_1.location = (40.0, -280.0)
    worn_suit_secondary_detail_normal_blend_1.name = 'Worn Suit Secondary Detail Normal Blend'
    worn_suit_secondary_detail_normal_blend_1.width = 140.0

    worn_suit_secondary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_detail_roughness_blend_1.hide = True
    worn_suit_secondary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    worn_suit_secondary_detail_roughness_blend_1.location = (40.0, -320.0)
    worn_suit_secondary_detail_roughness_blend_1.name = 'Worn Suit Secondary Detail Roughness Blend'
    worn_suit_secondary_detail_roughness_blend_1.width = 140.0

    worn_suit_secondary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_metalness_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_metalness_1.hide = True
    worn_suit_secondary_metalness_1.label = 'Metalness'
    worn_suit_secondary_metalness_1.location = (40.0, -360.0)
    worn_suit_secondary_metalness_1.name = 'Worn Suit Secondary Metalness'
    worn_suit_secondary_metalness_1.width = 140.0

    reroute_358_1 = test_group.nodes.new('NodeReroute')
    reroute_358_1.location = (5525.58154296875, -4880.0)
    reroute_358_1.name = 'Reroute.358'

    reroute_361_1 = test_group.nodes.new('NodeReroute')
    reroute_361_1.location = (5505.58154296875, -4900.0)
    reroute_361_1.name = 'Reroute.361'

    reroute_350_1 = test_group.nodes.new('NodeReroute')
    reroute_350_1.location = (5705.58154296875, -4687.86279296875)
    reroute_350_1.name = 'Reroute.350'

    reroute_351_1 = test_group.nodes.new('NodeReroute')
    reroute_351_1.location = (5685.58154296875, -4710.0302734375)
    reroute_351_1.name = 'Reroute.351'

    reroute_362_1 = test_group.nodes.new('NodeReroute')
    reroute_362_1.location = (5565.58154296875, -4838.69970703125)
    reroute_362_1.name = 'Reroute.362'

    reroute_357_1 = test_group.nodes.new('NodeReroute')
    reroute_357_1.location = (5545.58154296875, -4858.69970703125)
    reroute_357_1.name = 'Reroute.357'

    reroute_352_1 = test_group.nodes.new('NodeReroute')
    reroute_352_1.location = (5665.58154296875, -4731.76416015625)
    reroute_352_1.name = 'Reroute.352'

    reroute_353_1 = test_group.nodes.new('NodeReroute')
    reroute_353_1.location = (5645.58154296875, -4753.064453125)
    reroute_353_1.name = 'Reroute.353'

    reroute_354_1 = test_group.nodes.new('NodeReroute')
    reroute_354_1.location = (5625.58154296875, -4774.36474609375)
    reroute_354_1.name = 'Reroute.354'

    reroute_355_1 = test_group.nodes.new('NodeReroute')
    reroute_355_1.location = (5605.58154296875, -4795.6650390625)
    reroute_355_1.name = 'Reroute.355'

    reroute_356_1 = test_group.nodes.new('NodeReroute')
    reroute_356_1.location = (5585.58154296875, -4817.39892578125)
    reroute_356_1.name = 'Reroute.356'

    reroute_359_1 = test_group.nodes.new('NodeReroute')
    reroute_359_1.location = (5485.58154296875, -4921.09521484375)
    reroute_359_1.name = 'Reroute.359'

    reroute_1 = test_group.nodes.new('NodeReroute')
    reroute_1.parent = test_group.nodes.get('Frame')
    reroute_1.location = (280.0, 360.0)
    reroute_1.name = 'Reroute'

    armor_primary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_roughness_remap_x_1.parent = test_group.nodes.get('Frame')
    armor_primary_roughness_remap_x_1.hide = True
    armor_primary_roughness_remap_x_1.label = 'Roughness Remap_X'
    armor_primary_roughness_remap_x_1.location = (0.0, 320.0)
    armor_primary_roughness_remap_x_1.name = 'Armor Primary Roughness Remap X'
    armor_primary_roughness_remap_x_1.width = 155.72564697265625

    armor_primary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_roughness_remap_y_1.parent = test_group.nodes.get('Frame')
    armor_primary_roughness_remap_y_1.hide = True
    armor_primary_roughness_remap_y_1.label = 'Roughness Remap_Y'
    armor_primary_roughness_remap_y_1.location = (0.0, 280.0)
    armor_primary_roughness_remap_y_1.name = 'Armor Primary Roughness Remap Y'
    armor_primary_roughness_remap_y_1.width = 156.39605712890625

    armor_primary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_roughness_remap_z_1.parent = test_group.nodes.get('Frame')
    armor_primary_roughness_remap_z_1.hide = True
    armor_primary_roughness_remap_z_1.label = 'Roughness Remap_Z'
    armor_primary_roughness_remap_z_1.location = (0.0, 240.0)
    armor_primary_roughness_remap_z_1.name = 'Armor Primary Roughness Remap Z'
    armor_primary_roughness_remap_z_1.width = 156.48974609375

    armor_primary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_roughness_remap_w_1.parent = test_group.nodes.get('Frame')
    armor_primary_roughness_remap_w_1.hide = True
    armor_primary_roughness_remap_w_1.label = 'Roughness Remap_W'
    armor_primary_roughness_remap_w_1.location = (0.0, 200.0)
    armor_primary_roughness_remap_w_1.name = 'Armor Primary Roughness Remap W'
    armor_primary_roughness_remap_w_1.width = 156.45462036132812

    reroute_338_1 = test_group.nodes.new('NodeReroute')
    reroute_338_1.location = (5685.58154296875, -529.1972045898438)
    reroute_338_1.name = 'Reroute.338'

    reroute_339_1 = test_group.nodes.new('NodeReroute')
    reroute_339_1.location = (5665.58154296875, -729.1972045898438)
    reroute_339_1.name = 'Reroute.339'

    reroute_340_1 = test_group.nodes.new('NodeReroute')
    reroute_340_1.location = (5645.58154296875, -883.0657348632812)
    reroute_340_1.name = 'Reroute.340'

    reroute_341_1 = test_group.nodes.new('NodeReroute')
    reroute_341_1.location = (5625.58154296875, -1087.1534423828125)
    reroute_341_1.name = 'Reroute.341'

    reroute_342_1 = test_group.nodes.new('NodeReroute')
    reroute_342_1.location = (5605.58154296875, -1243.0657958984375)
    reroute_342_1.name = 'Reroute.342'

    reroute_343_1 = test_group.nodes.new('NodeReroute')
    reroute_343_1.location = (5585.58154296875, -1397.3006591796875)
    reroute_343_1.name = 'Reroute.343'

    reroute_349_1 = test_group.nodes.new('NodeReroute')
    reroute_349_1.location = (5565.58154296875, -2085.12109375)
    reroute_349_1.name = 'Reroute.349'

    reroute_344_1 = test_group.nodes.new('NodeReroute')
    reroute_344_1.location = (5545.58154296875, -2238.7197265625)
    reroute_344_1.name = 'Reroute.344'

    reroute_345_1 = test_group.nodes.new('NodeReroute')
    reroute_345_1.location = (5525.58154296875, -2531.7158203125)
    reroute_345_1.name = 'Reroute.345'

    reroute_346_1 = test_group.nodes.new('NodeReroute')
    reroute_346_1.location = (5485.58154296875, -3417.283447265625)
    reroute_346_1.name = 'Reroute.346'

    reroute_348_1 = test_group.nodes.new('NodeReroute')
    reroute_348_1.location = (5505.58154296875, -5003.7490234375)
    reroute_348_1.name = 'Reroute.348'

    reroute_360_1 = test_group.nodes.new('NodeReroute')
    reroute_360_1.location = (5465.58154296875, -4942.73828125)
    reroute_360_1.name = 'Reroute.360'

    reroute_347_1 = test_group.nodes.new('NodeReroute')
    reroute_347_1.location = (5465.58154296875, -6454.30517578125)
    reroute_347_1.name = 'Reroute.347'

    armor_primary_wear_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_wear_remap_x_1.parent = test_group.nodes.get('Frame')
    armor_primary_wear_remap_x_1.hide = True
    armor_primary_wear_remap_x_1.label = 'Wear Remap X'
    armor_primary_wear_remap_x_1.location = (0.888427734375, 121.0198974609375)
    armor_primary_wear_remap_x_1.name = 'Armor Primary Wear Remap X'
    armor_primary_wear_remap_x_1.width = 154.7330322265625

    armor_primary_wear_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_wear_remap_y_1.parent = test_group.nodes.get('Frame')
    armor_primary_wear_remap_y_1.hide = True
    armor_primary_wear_remap_y_1.label = 'Wear Remap Y'
    armor_primary_wear_remap_y_1.location = (0.888427734375, 81.0198974609375)
    armor_primary_wear_remap_y_1.name = 'Armor Primary Wear Remap Y'
    armor_primary_wear_remap_y_1.width = 154.73309326171875

    armor_primary_wear_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_wear_remap_z_1.parent = test_group.nodes.get('Frame')
    armor_primary_wear_remap_z_1.hide = True
    armor_primary_wear_remap_z_1.label = 'Wear Remap Z'
    armor_primary_wear_remap_z_1.location = (0.888427734375, 41.0198974609375)
    armor_primary_wear_remap_z_1.name = 'Armor Primary Wear Remap Z'
    armor_primary_wear_remap_z_1.width = 155.59967041015625

    armor_primary_wear_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_wear_remap_w_1.parent = test_group.nodes.get('Frame')
    armor_primary_wear_remap_w_1.hide = True
    armor_primary_wear_remap_w_1.label = 'Wear Remap W'
    armor_primary_wear_remap_w_1.location = (0.888427734375, 1.0198974609375)
    armor_primary_wear_remap_w_1.name = 'Armor Primary Wear Remap W'
    armor_primary_wear_remap_w_1.width = 155.1663818359375

    armor_detail_diffuse_transform_1 = test_group.nodes.new('ShaderNodeMapping')
    armor_detail_diffuse_transform_1.label = 'Detail Diffuse Transform'
    armor_detail_diffuse_transform_1.location = (-1200.0, 240.0)
    armor_detail_diffuse_transform_1.name = 'Armor Detail Diffuse Transform'

    armor_primary_detail_diffuse_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    armor_primary_detail_diffuse_map_1.parent = test_group.nodes.get('Frame')
    armor_primary_detail_diffuse_map_1.hide = True
    armor_primary_detail_diffuse_map_1.label = 'Detail Diffuse Map'
    armor_primary_detail_diffuse_map_1.location = (0.0, 160.0)
    armor_primary_detail_diffuse_map_1.name = 'Armor Primary Detail Diffuse Map'

    reroute_337_1 = test_group.nodes.new('NodeReroute')
    reroute_337_1.location = (5705.58154296875, -346.1314697265625)
    reroute_337_1.name = 'Reroute.337'

    armor_primary_detail_normal_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    armor_primary_detail_normal_map_1.parent = test_group.nodes.get('Frame')
    armor_primary_detail_normal_map_1.hide = True
    armor_primary_detail_normal_map_1.label = 'Detail Normal Map'
    armor_primary_detail_normal_map_1.location = (0.0, -40.0)
    armor_primary_detail_normal_map_1.name = 'Armor Primary Detail Normal Map'

    group_output_1 = test_group.nodes.new('NodeGroupOutput')
    group_output_1.is_active_output = True
    group_output_1.location = (5805.58154296875, -4656.068359375)
    group_output_1.name = 'Group Output'

    armor_primary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_metalness_1.parent = test_group.nodes.get('Frame')
    armor_primary_metalness_1.hide = True
    armor_primary_metalness_1.label = 'Metalness'
    armor_primary_metalness_1.location = (0.0, -200.0)
    armor_primary_metalness_1.name = 'Armor Primary Metalness'
    armor_primary_metalness_1.width = 140.0

    armor_primary_iridescence_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_iridescence_1.parent = test_group.nodes.get('Frame')
    armor_primary_iridescence_1.hide = True
    armor_primary_iridescence_1.label = 'Iridescence'
    armor_primary_iridescence_1.location = (0.0, -240.0)
    armor_primary_iridescence_1.name = 'Armor Primary Iridescence'
    armor_primary_iridescence_1.width = 140.0

    armor_primary_fuzz_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_fuzz_1.parent = test_group.nodes.get('Frame')
    armor_primary_fuzz_1.hide = True
    armor_primary_fuzz_1.label = 'Fuzz'
    armor_primary_fuzz_1.location = (0.0, -280.0)
    armor_primary_fuzz_1.name = 'Armor Primary Fuzz'
    armor_primary_fuzz_1.width = 140.0

    armor_primary_transmission_1 = test_group.nodes.new('ShaderNodeValue')
    armor_primary_transmission_1.parent = test_group.nodes.get('Frame')
    armor_primary_transmission_1.hide = True
    armor_primary_transmission_1.label = 'Transmission'
    armor_primary_transmission_1.location = (0.0, -320.0)
    armor_primary_transmission_1.name = 'Armor Primary Transmission'
    armor_primary_transmission_1.width = 140.0

    armor_primary_emission_color_1 = test_group.nodes.new('ShaderNodeRGB')
    armor_primary_emission_color_1.parent = test_group.nodes.get('Frame')
    armor_primary_emission_color_1.hide = True
    armor_primary_emission_color_1.label = 'Emission Color'
    armor_primary_emission_color_1.location = (0.0, -360.0)
    armor_primary_emission_color_1.name = 'Armor Primary Emission Color'
    armor_primary_emission_color_1.width = 140.0

    reroute_022_1 = test_group.nodes.new('NodeReroute')
    reroute_022_1.parent = test_group.nodes.get('Frame.001')
    reroute_022_1.location = (280.0, -160.0)
    reroute_022_1.name = 'Reroute.022'

    reroute_021_1 = test_group.nodes.new('NodeReroute')
    reroute_021_1.parent = test_group.nodes.get('Frame.001')
    reroute_021_1.location = (280.0, -120.0)
    reroute_021_1.name = 'Reroute.021'

    reroute_020_1 = test_group.nodes.new('NodeReroute')
    reroute_020_1.parent = test_group.nodes.get('Frame.001')
    reroute_020_1.location = (280.0, -80.0)
    reroute_020_1.name = 'Reroute.020'

    reroute_023_1 = test_group.nodes.new('NodeReroute')
    reroute_023_1.parent = test_group.nodes.get('Frame.001')
    reroute_023_1.location = (280.0, -200.0)
    reroute_023_1.name = 'Reroute.023'

    reroute_019_1 = test_group.nodes.new('NodeReroute')
    reroute_019_1.parent = test_group.nodes.get('Frame.001')
    reroute_019_1.location = (280.0, -40.0)
    reroute_019_1.name = 'Reroute.019'

    reroute_024_1 = test_group.nodes.new('NodeReroute')
    reroute_024_1.parent = test_group.nodes.get('Frame.001')
    reroute_024_1.location = (280.0, -240.0)
    reroute_024_1.name = 'Reroute.024'

    reroute_025_1 = test_group.nodes.new('NodeReroute')
    reroute_025_1.parent = test_group.nodes.get('Frame.001')
    reroute_025_1.location = (280.0, -280.0)
    reroute_025_1.name = 'Reroute.025'

    reroute_026_1 = test_group.nodes.new('NodeReroute')
    reroute_026_1.parent = test_group.nodes.get('Frame.001')
    reroute_026_1.location = (280.0, -320.0)
    reroute_026_1.name = 'Reroute.026'

    reroute_027_1 = test_group.nodes.new('NodeReroute')
    reroute_027_1.parent = test_group.nodes.get('Frame.001')
    reroute_027_1.location = (280.0, -360.0)
    reroute_027_1.name = 'Reroute.027'

    worn_armor_primary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_detail_diffuse_blend_1.hide = True
    worn_armor_primary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    worn_armor_primary_detail_diffuse_blend_1.location = (0.0, -240.0)
    worn_armor_primary_detail_diffuse_blend_1.name = 'Worn Armor Primary Detail Diffuse Blend'
    worn_armor_primary_detail_diffuse_blend_1.width = 140.0

    worn_armor_primary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_detail_normal_blend_1.hide = True
    worn_armor_primary_detail_normal_blend_1.label = 'Detail Normal Blend'
    worn_armor_primary_detail_normal_blend_1.location = (0.0, -280.0)
    worn_armor_primary_detail_normal_blend_1.name = 'Worn Armor Primary Detail Normal Blend'
    worn_armor_primary_detail_normal_blend_1.width = 140.0

    worn_armor_primary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_detail_roughness_blend_1.hide = True
    worn_armor_primary_detail_roughness_blend_1.label = 'Worn Armor Primary Detail Roughness Blend'
    worn_armor_primary_detail_roughness_blend_1.location = (0.0, -320.0)
    worn_armor_primary_detail_roughness_blend_1.name = 'Value.097'
    worn_armor_primary_detail_roughness_blend_1.width = 140.0

    worn_armor_primary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_metalness_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_metalness_1.hide = True
    worn_armor_primary_metalness_1.label = 'Metalness'
    worn_armor_primary_metalness_1.location = (0.0, -360.0)
    worn_armor_primary_metalness_1.name = 'Worn Armor Primary Metalness'
    worn_armor_primary_metalness_1.width = 140.0

    worn_armor_primary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    worn_armor_primary_dye_color_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_dye_color_1.hide = True
    worn_armor_primary_dye_color_1.label = 'Dye Color'
    worn_armor_primary_dye_color_1.location = (0.0, -40.0)
    worn_armor_primary_dye_color_1.name = 'Worn Armor Primary Dye Color'
    worn_armor_primary_dye_color_1.width = 140.0

    worn_armor_primary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_roughness_remap_x_1.hide = True
    worn_armor_primary_roughness_remap_x_1.label = 'Roughness Remap X'
    worn_armor_primary_roughness_remap_x_1.location = (0.0, -80.0)
    worn_armor_primary_roughness_remap_x_1.name = 'Worn Armor Primary Roughness Remap X'
    worn_armor_primary_roughness_remap_x_1.width = 155.01483154296875

    worn_armor_primary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_roughness_remap_y_1.hide = True
    worn_armor_primary_roughness_remap_y_1.label = 'Roughness Remap Y'
    worn_armor_primary_roughness_remap_y_1.location = (0.0, -120.0)
    worn_armor_primary_roughness_remap_y_1.name = 'Worn Armor Primary Roughness Remap Y'
    worn_armor_primary_roughness_remap_y_1.width = 155.46978759765625

    worn_armor_primary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_roughness_remap_z_1.hide = True
    worn_armor_primary_roughness_remap_z_1.label = 'Roughness Remap Z'
    worn_armor_primary_roughness_remap_z_1.location = (0.0, -160.0)
    worn_armor_primary_roughness_remap_z_1.name = 'Worn Armor Primary Roughness Remap Z'
    worn_armor_primary_roughness_remap_z_1.width = 155.92486572265625

    worn_armor_primary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_primary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.001')
    worn_armor_primary_roughness_remap_w_1.hide = True
    worn_armor_primary_roughness_remap_w_1.label = 'Roughness Remap W'
    worn_armor_primary_roughness_remap_w_1.location = (0.0, -200.0)
    worn_armor_primary_roughness_remap_w_1.name = 'Worn Armor Primary Roughness Remap W'
    worn_armor_primary_roughness_remap_w_1.width = 156.83477783203125

    armor_secondary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_roughness_remap_x_1.hide = True
    armor_secondary_roughness_remap_x_1.label = 'Roughness Remap X'
    armor_secondary_roughness_remap_x_1.location = (20.0, -80.0)
    armor_secondary_roughness_remap_x_1.name = 'Armor Secondary Roughness Remap X'
    armor_secondary_roughness_remap_x_1.width = 156.03314208984375

    armor_secondary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_roughness_remap_y_1.hide = True
    armor_secondary_roughness_remap_y_1.label = 'Roughness Remap Y'
    armor_secondary_roughness_remap_y_1.location = (20.0, -120.0)
    armor_secondary_roughness_remap_y_1.name = 'Armor Secondary Roughness Remap Y'
    armor_secondary_roughness_remap_y_1.width = 155.599853515625

    armor_secondary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_roughness_remap_z_1.hide = True
    armor_secondary_roughness_remap_z_1.label = 'Roughness Remap Z'
    armor_secondary_roughness_remap_z_1.location = (20.0, -160.0)
    armor_secondary_roughness_remap_z_1.name = 'Armor Secondary Roughness Remap Z'
    armor_secondary_roughness_remap_z_1.width = 156.8997802734375

    armor_secondary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_roughness_remap_w_1.hide = True
    armor_secondary_roughness_remap_w_1.label = 'Roughness Remap W'
    armor_secondary_roughness_remap_w_1.location = (20.0, -200.0)
    armor_secondary_roughness_remap_w_1.name = 'Armor Secondary Roughness Remap W'
    armor_secondary_roughness_remap_w_1.width = 155.599853515625

    armor_secondary_wear_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_wear_remap_x_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_wear_remap_x_1.hide = True
    armor_secondary_wear_remap_x_1.label = 'Wear Remap X'
    armor_secondary_wear_remap_x_1.location = (20.0, -280.0)
    armor_secondary_wear_remap_x_1.name = 'Armor Secondary Wear Remap X'
    armor_secondary_wear_remap_x_1.width = 156.46649169921875

    armor_secondary_wear_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_wear_remap_y_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_wear_remap_y_1.hide = True
    armor_secondary_wear_remap_y_1.label = 'Wear Remap Y'
    armor_secondary_wear_remap_y_1.location = (20.0, -320.0)
    armor_secondary_wear_remap_y_1.name = 'Armor Secondary Wear Remap Y'
    armor_secondary_wear_remap_y_1.width = 156.03314208984375

    armor_secondary_wear_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_wear_remap_z_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_wear_remap_z_1.hide = True
    armor_secondary_wear_remap_z_1.label = 'Wear Remap Z'
    armor_secondary_wear_remap_z_1.location = (20.0, -360.0)
    armor_secondary_wear_remap_z_1.name = 'Armor Secondary Wear Remap Z'
    armor_secondary_wear_remap_z_1.width = 156.03314208984375

    armor_secondary_wear_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    armor_secondary_wear_remap_w_1.parent = test_group.nodes.get('Frame.002')
    armor_secondary_wear_remap_w_1.hide = True
    armor_secondary_wear_remap_w_1.label = 'Wear Remap W'
    armor_secondary_wear_remap_w_1.location = (20.0, -400.0)
    armor_secondary_wear_remap_w_1.name = 'Armor Secondary Wear Remap W'
    armor_secondary_wear_remap_w_1.width = 156.033203125

    worn_armor_secondary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_roughness_remap_x_1.hide = True
    worn_armor_secondary_roughness_remap_x_1.label = 'Roughness Remap X'
    worn_armor_secondary_roughness_remap_x_1.location = (-1580.0, 360.0)
    worn_armor_secondary_roughness_remap_x_1.name = 'Worn Armor Secondary Roughness Remap X'
    worn_armor_secondary_roughness_remap_x_1.width = 155.16650390625

    worn_armor_secondary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_roughness_remap_y_1.hide = True
    worn_armor_secondary_roughness_remap_y_1.label = 'Roughness Remap Y'
    worn_armor_secondary_roughness_remap_y_1.location = (-1580.0, 320.0)
    worn_armor_secondary_roughness_remap_y_1.name = 'Worn Armor Secondary Roughness Remap Y'
    worn_armor_secondary_roughness_remap_y_1.width = 155.599853515625

    worn_armor_secondary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_roughness_remap_z_1.hide = True
    worn_armor_secondary_roughness_remap_z_1.label = 'Roughness Remap Z'
    worn_armor_secondary_roughness_remap_z_1.location = (-1580.0, 280.0)
    worn_armor_secondary_roughness_remap_z_1.name = 'Worn Armor Secondary Roughness Remap Z'
    worn_armor_secondary_roughness_remap_z_1.width = 156.03314208984375

    worn_armor_secondary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    worn_armor_secondary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.003')
    worn_armor_secondary_roughness_remap_w_1.hide = True
    worn_armor_secondary_roughness_remap_w_1.label = 'Roughness Remap W'
    worn_armor_secondary_roughness_remap_w_1.location = (-1580.0, 240.0)
    worn_armor_secondary_roughness_remap_w_1.name = 'Worn Armor Secondary Roughness Remap W'
    worn_armor_secondary_roughness_remap_w_1.width = 156.46646118164062

    reroute_067_1 = test_group.nodes.new('NodeReroute')
    reroute_067_1.parent = test_group.nodes.get('Frame.004')
    reroute_067_1.location = (300.0, -400.0)
    reroute_067_1.name = 'Reroute.067'

    reroute_058_1 = test_group.nodes.new('NodeReroute')
    reroute_058_1.parent = test_group.nodes.get('Frame.004')
    reroute_058_1.location = (300.0, -80.0)
    reroute_058_1.name = 'Reroute.058'

    reroute_060_1 = test_group.nodes.new('NodeReroute')
    reroute_060_1.parent = test_group.nodes.get('Frame.004')
    reroute_060_1.location = (300.0, -160.0)
    reroute_060_1.name = 'Reroute.060'

    reroute_057_1 = test_group.nodes.new('NodeReroute')
    reroute_057_1.parent = test_group.nodes.get('Frame.004')
    reroute_057_1.location = (300.0, -40.0)
    reroute_057_1.name = 'Reroute.057'

    reroute_059_1 = test_group.nodes.new('NodeReroute')
    reroute_059_1.parent = test_group.nodes.get('Frame.004')
    reroute_059_1.location = (300.0, -120.0)
    reroute_059_1.name = 'Reroute.059'

    reroute_062_1 = test_group.nodes.new('NodeReroute')
    reroute_062_1.parent = test_group.nodes.get('Frame.004')
    reroute_062_1.location = (300.0, -220.0)
    reroute_062_1.name = 'Reroute.062'

    reroute_056_1 = test_group.nodes.new('NodeReroute')
    reroute_056_1.parent = test_group.nodes.get('Frame.004')
    reroute_056_1.location = (300.0, 0.0)
    reroute_056_1.name = 'Reroute.056'

    reroute_063_1 = test_group.nodes.new('NodeReroute')
    reroute_063_1.parent = test_group.nodes.get('Frame.004')
    reroute_063_1.location = (300.0, -240.0)
    reroute_063_1.name = 'Reroute.063'

    reroute_064_1 = test_group.nodes.new('NodeReroute')
    reroute_064_1.parent = test_group.nodes.get('Frame.004')
    reroute_064_1.location = (300.0, -280.0)
    reroute_064_1.name = 'Reroute.064'

    reroute_065_1 = test_group.nodes.new('NodeReroute')
    reroute_065_1.parent = test_group.nodes.get('Frame.004')
    reroute_065_1.location = (300.0, -320.0)
    reroute_065_1.name = 'Reroute.065'

    reroute_066_1 = test_group.nodes.new('NodeReroute')
    reroute_066_1.parent = test_group.nodes.get('Frame.004')
    reroute_066_1.location = (300.0, -360.0)
    reroute_066_1.name = 'Reroute.066'

    reroute_071_1 = test_group.nodes.new('NodeReroute')
    reroute_071_1.parent = test_group.nodes.get('Frame.004')
    reroute_071_1.location = (300.0, -560.0)
    reroute_071_1.name = 'Reroute.071'

    reroute_068_1 = test_group.nodes.new('NodeReroute')
    reroute_068_1.parent = test_group.nodes.get('Frame.004')
    reroute_068_1.location = (300.0, -440.0)
    reroute_068_1.name = 'Reroute.068'

    reroute_069_1 = test_group.nodes.new('NodeReroute')
    reroute_069_1.parent = test_group.nodes.get('Frame.004')
    reroute_069_1.location = (300.0, -480.0)
    reroute_069_1.name = 'Reroute.069'

    reroute_070_1 = test_group.nodes.new('NodeReroute')
    reroute_070_1.parent = test_group.nodes.get('Frame.004')
    reroute_070_1.location = (300.0, -520.0)
    reroute_070_1.name = 'Reroute.070'

    reroute_061_1 = test_group.nodes.new('NodeReroute')
    reroute_061_1.parent = test_group.nodes.get('Frame.004')
    reroute_061_1.location = (300.0, -200.0)
    reroute_061_1.name = 'Reroute.061'

    reroute_072_1 = test_group.nodes.new('NodeReroute')
    reroute_072_1.parent = test_group.nodes.get('Frame.004')
    reroute_072_1.location = (300.0, -600.0)
    reroute_072_1.name = 'Reroute.072'

    reroute_365_1 = test_group.nodes.new('NodeReroute')
    reroute_365_1.parent = test_group.nodes.get('Frame.004')
    reroute_365_1.location = (300.0, -640.0)
    reroute_365_1.name = 'Reroute.365'

    reroute_073_1 = test_group.nodes.new('NodeReroute')
    reroute_073_1.parent = test_group.nodes.get('Frame.004')
    reroute_073_1.location = (300.0, -680.0)
    reroute_073_1.name = 'Reroute.073'

    reroute_074_1 = test_group.nodes.new('NodeReroute')
    reroute_074_1.parent = test_group.nodes.get('Frame.004')
    reroute_074_1.location = (300.0, -720.0)
    reroute_074_1.name = 'Reroute.074'

    cloth_primary_dye_color_1 = test_group.nodes.new('ShaderNodeRGB')
    cloth_primary_dye_color_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_dye_color_1.hide = True
    cloth_primary_dye_color_1.label = 'Dye Color'
    cloth_primary_dye_color_1.location = (20.0, 0.0)
    cloth_primary_dye_color_1.name = 'Cloth Primary Dye Color'
    cloth_primary_dye_color_1.width = 140.0

    cloth_primary_detail_diffuse_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    cloth_primary_detail_diffuse_map_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_detail_diffuse_map_1.hide = True
    cloth_primary_detail_diffuse_map_1.label = 'Detail Diffuse Map'
    cloth_primary_detail_diffuse_map_1.location = (20.0, -200.0)
    cloth_primary_detail_diffuse_map_1.name = 'Cloth Primary Detail Diffuse Map'

    cloth_primary_detail_normal_map_1 = test_group.nodes.new('ShaderNodeTexImage')
    cloth_primary_detail_normal_map_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_detail_normal_map_1.hide = True
    cloth_primary_detail_normal_map_1.label = 'Detail Normal Map'
    cloth_primary_detail_normal_map_1.location = (20.0, -400.0)
    cloth_primary_detail_normal_map_1.name = 'Cloth Primary Detail Normal Map'

    cloth_primary_detail_diffuse_blend_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_detail_diffuse_blend_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_detail_diffuse_blend_1.hide = True
    cloth_primary_detail_diffuse_blend_1.label = 'Detail Diffuse Blend'
    cloth_primary_detail_diffuse_blend_1.location = (20.0, -440.0)
    cloth_primary_detail_diffuse_blend_1.name = 'Cloth Primary Detail Diffuse Blend'
    cloth_primary_detail_diffuse_blend_1.width = 140.0

    cloth_primary_detail_normal_blend_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_detail_normal_blend_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_detail_normal_blend_1.hide = True
    cloth_primary_detail_normal_blend_1.label = 'Detail Normal Blend'
    cloth_primary_detail_normal_blend_1.location = (20.0, -480.0)
    cloth_primary_detail_normal_blend_1.name = 'Cloth Primary Detail Normal Blend'
    cloth_primary_detail_normal_blend_1.width = 140.0

    cloth_primary_detail_roughness_blend_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_detail_roughness_blend_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_detail_roughness_blend_1.hide = True
    cloth_primary_detail_roughness_blend_1.label = 'Detail Roughness Blend'
    cloth_primary_detail_roughness_blend_1.location = (20.0, -520.0)
    cloth_primary_detail_roughness_blend_1.name = 'Cloth Primary Detail Roughness Blend'
    cloth_primary_detail_roughness_blend_1.width = 140.0

    cloth_primary_metalness_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_metalness_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_metalness_1.hide = True
    cloth_primary_metalness_1.label = 'Metalness'
    cloth_primary_metalness_1.location = (20.0, -560.0)
    cloth_primary_metalness_1.name = 'Cloth Primary Metalness'
    cloth_primary_metalness_1.width = 140.0

    cloth_primary_iridescence_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_iridescence_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_iridescence_1.hide = True
    cloth_primary_iridescence_1.label = 'Iridescence'
    cloth_primary_iridescence_1.location = (20.0, -600.0)
    cloth_primary_iridescence_1.name = 'Cloth Primary Iridescence'
    cloth_primary_iridescence_1.width = 140.0

    cloth_primary_fuzz_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_fuzz_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_fuzz_1.hide = True
    cloth_primary_fuzz_1.label = 'Fuzz'
    cloth_primary_fuzz_1.location = (20.0, -640.0)
    cloth_primary_fuzz_1.name = 'Cloth Primary Fuzz'
    cloth_primary_fuzz_1.width = 140.0

    cloth_primary_transmission_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_transmission_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_transmission_1.hide = True
    cloth_primary_transmission_1.label = 'Transmission'
    cloth_primary_transmission_1.location = (20.0, -680.0)
    cloth_primary_transmission_1.name = 'Cloth Primary Transmission'
    cloth_primary_transmission_1.width = 140.0

    cloth_primary_emission_color_1 = test_group.nodes.new('ShaderNodeRGB')
    cloth_primary_emission_color_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_emission_color_1.hide = True
    cloth_primary_emission_color_1.label = 'Emission Color'
    cloth_primary_emission_color_1.location = (20.0, -720.0)
    cloth_primary_emission_color_1.name = 'Cloth Primary Emission Color'
    cloth_primary_emission_color_1.width = 140.0

    cloth_primary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_roughness_remap_x_1.hide = True
    cloth_primary_roughness_remap_x_1.label = 'Roughness Remap X'
    cloth_primary_roughness_remap_x_1.location = (20.0, -40.0)
    cloth_primary_roughness_remap_x_1.name = 'Cloth Primary Roughness Remap X'
    cloth_primary_roughness_remap_x_1.width = 155.92779541015625

    cloth_primary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_roughness_remap_y_1.hide = True
    cloth_primary_roughness_remap_y_1.label = 'Roughness Remap Y'
    cloth_primary_roughness_remap_y_1.location = (20.0, -80.0)
    cloth_primary_roughness_remap_y_1.name = 'Cloth Primary Roughness Remap Y'
    cloth_primary_roughness_remap_y_1.width = 156.39620971679688

    cloth_primary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_roughness_remap_z_1.hide = True
    cloth_primary_roughness_remap_z_1.label = 'Roughness Remap Z'
    cloth_primary_roughness_remap_z_1.location = (20.0, -120.0)
    cloth_primary_roughness_remap_z_1.name = 'Cloth Primary Roughness Remap Z'
    cloth_primary_roughness_remap_z_1.width = 156.86474609375

    cloth_primary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_roughness_remap_w_1.hide = True
    cloth_primary_roughness_remap_w_1.label = 'Roughness Remap W'
    cloth_primary_roughness_remap_w_1.location = (20.0, -160.0)
    cloth_primary_roughness_remap_w_1.name = 'Cloth Primary Roughness Remap W'
    cloth_primary_roughness_remap_w_1.width = 155.45928955078125

    cloth_primary_wear_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_wear_remap_x_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_wear_remap_x_1.hide = True
    cloth_primary_wear_remap_x_1.label = 'Wear Remap X'
    cloth_primary_wear_remap_x_1.location = (20.0, -240.0)
    cloth_primary_wear_remap_x_1.name = 'Cloth Primary Wear Remap X'
    cloth_primary_wear_remap_x_1.width = 156.4664306640625

    cloth_primary_wear_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_wear_remap_y_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_wear_remap_y_1.hide = True
    cloth_primary_wear_remap_y_1.label = 'Wear Remap Y'
    cloth_primary_wear_remap_y_1.location = (20.0, -280.0)
    cloth_primary_wear_remap_y_1.name = 'Cloth Primary Wear Remap Y'
    cloth_primary_wear_remap_y_1.width = 155.59979248046875

    cloth_primary_wear_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_wear_remap_z_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_wear_remap_z_1.hide = True
    cloth_primary_wear_remap_z_1.label = 'Wear Remap Z'
    cloth_primary_wear_remap_z_1.location = (20.0, -320.0)
    cloth_primary_wear_remap_z_1.name = 'Cloth Primary Wear Remap Z'
    cloth_primary_wear_remap_z_1.width = 156.4664306640625

    cloth_primary_wear_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_primary_wear_remap_w_1.parent = test_group.nodes.get('Frame.004')
    cloth_primary_wear_remap_w_1.hide = True
    cloth_primary_wear_remap_w_1.label = 'Wear Remap W'
    cloth_primary_wear_remap_w_1.location = (20.0, -360.0)
    cloth_primary_wear_remap_w_1.name = 'Cloth Primary Wear Remap W'
    cloth_primary_wear_remap_w_1.width = 155.16650390625

    worn_cloth_primary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_roughness_remap_x_1.hide = True
    worn_cloth_primary_roughness_remap_x_1.label = 'Roughness Remap X'
    worn_cloth_primary_roughness_remap_x_1.location = (20.0, 20.0)
    worn_cloth_primary_roughness_remap_x_1.name = 'Worn Cloth Primary Roughness Remap X'
    worn_cloth_primary_roughness_remap_x_1.width = 156.39630126953125

    worn_cloth_primary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_roughness_remap_y_1.hide = True
    worn_cloth_primary_roughness_remap_y_1.label = 'Roughness Remap Y'
    worn_cloth_primary_roughness_remap_y_1.location = (20.0, -20.0)
    worn_cloth_primary_roughness_remap_y_1.name = 'Worn Cloth Primary Roughness Remap Y'
    worn_cloth_primary_roughness_remap_y_1.width = 156.86471557617188

    worn_cloth_primary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_roughness_remap_z_1.hide = True
    worn_cloth_primary_roughness_remap_z_1.label = 'Roughness Remap Z'
    worn_cloth_primary_roughness_remap_z_1.location = (20.0, -60.0)
    worn_cloth_primary_roughness_remap_z_1.name = 'Worn Cloth Primary Roughness Remap Z'
    worn_cloth_primary_roughness_remap_z_1.width = 156.86474609375

    worn_cloth_primary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_primary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.005')
    worn_cloth_primary_roughness_remap_w_1.hide = True
    worn_cloth_primary_roughness_remap_w_1.label = 'Roughness Remap W'
    worn_cloth_primary_roughness_remap_w_1.location = (20.0, -100.0)
    worn_cloth_primary_roughness_remap_w_1.name = 'Worn Cloth Primary Roughness Remap W'
    worn_cloth_primary_roughness_remap_w_1.width = 156.39630126953125

    cloth_secondary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_roughness_remap_x_1.hide = True
    cloth_secondary_roughness_remap_x_1.label = 'Roughness Remap X'
    cloth_secondary_roughness_remap_x_1.location = (20.0, -60.0)
    cloth_secondary_roughness_remap_x_1.name = 'Cloth Secondary Roughness Remap X'
    cloth_secondary_roughness_remap_x_1.width = 156.86474609375

    cloth_secondary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_roughness_remap_y_1.hide = True
    cloth_secondary_roughness_remap_y_1.label = 'Roughness Remap Y'
    cloth_secondary_roughness_remap_y_1.location = (20.0, -100.0)
    cloth_secondary_roughness_remap_y_1.name = 'Cloth Secondary Roughness Remap Y'
    cloth_secondary_roughness_remap_y_1.width = 155.92782592773438

    cloth_secondary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_roughness_remap_z_1.hide = True
    cloth_secondary_roughness_remap_z_1.label = 'Roughness Remap Z'
    cloth_secondary_roughness_remap_z_1.location = (20.0, -140.0)
    cloth_secondary_roughness_remap_z_1.name = 'Cloth Secondary Roughness Remap Z'
    cloth_secondary_roughness_remap_z_1.width = 156.39630126953125

    cloth_secondary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_roughness_remap_w_1.hide = True
    cloth_secondary_roughness_remap_w_1.label = 'Roughness Remap W'
    cloth_secondary_roughness_remap_w_1.location = (20.0, -180.0)
    cloth_secondary_roughness_remap_w_1.name = 'Cloth Secondary Roughness Remap W'
    cloth_secondary_roughness_remap_w_1.width = 156.39627075195312

    cloth_secondary_wear_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_wear_remap_x_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_wear_remap_x_1.hide = True
    cloth_secondary_wear_remap_x_1.label = 'Wear Remap X'
    cloth_secondary_wear_remap_x_1.location = (20.0, -260.0)
    cloth_secondary_wear_remap_x_1.name = 'Cloth Secondary Wear Remap X'
    cloth_secondary_wear_remap_x_1.width = 156.3963623046875

    cloth_secondary_wear_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_wear_remap_y_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_wear_remap_y_1.hide = True
    cloth_secondary_wear_remap_y_1.label = 'Wear Remap Y'
    cloth_secondary_wear_remap_y_1.location = (20.0, -300.0)
    cloth_secondary_wear_remap_y_1.name = 'Cloth Secondary Wear Remap Y'
    cloth_secondary_wear_remap_y_1.width = 155.92794799804688

    cloth_secondary_wear_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_wear_remap_z_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_wear_remap_z_1.hide = True
    cloth_secondary_wear_remap_z_1.label = 'Wear Remap Z'
    cloth_secondary_wear_remap_z_1.location = (20.0, -340.0)
    cloth_secondary_wear_remap_z_1.name = 'Cloth Secondary Wear Remap Z'
    cloth_secondary_wear_remap_z_1.width = 155.92791748046875

    cloth_secondary_wear_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    cloth_secondary_wear_remap_w_1.parent = test_group.nodes.get('Frame.006')
    cloth_secondary_wear_remap_w_1.hide = True
    cloth_secondary_wear_remap_w_1.label = 'Wear Remap W'
    cloth_secondary_wear_remap_w_1.location = (20.0, -380.0)
    cloth_secondary_wear_remap_w_1.name = 'Cloth Secondary Wear Remap W'
    cloth_secondary_wear_remap_w_1.width = 156.39642333984375

    worn_cloth_secondary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_roughness_remap_x_1.hide = True
    worn_cloth_secondary_roughness_remap_x_1.label = 'Roughness Remap X'
    worn_cloth_secondary_roughness_remap_x_1.location = (20.0, -60.0)
    worn_cloth_secondary_roughness_remap_x_1.name = 'Worn Cloth Secondary Roughness Remap X'
    worn_cloth_secondary_roughness_remap_x_1.width = 155.599853515625

    worn_cloth_secondary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_roughness_remap_y_1.hide = True
    worn_cloth_secondary_roughness_remap_y_1.label = 'Roughness Remap Y'
    worn_cloth_secondary_roughness_remap_y_1.location = (20.0, -100.0)
    worn_cloth_secondary_roughness_remap_y_1.name = 'Worn Cloth Secondary Roughness Remap Y'
    worn_cloth_secondary_roughness_remap_y_1.width = 156.46652221679688

    worn_cloth_secondary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_roughness_remap_z_1.hide = True
    worn_cloth_secondary_roughness_remap_z_1.label = 'Roughness Remap Z'
    worn_cloth_secondary_roughness_remap_z_1.location = (20.0, -140.0)
    worn_cloth_secondary_roughness_remap_z_1.name = 'Worn Cloth Secondary Roughness Remap Z'
    worn_cloth_secondary_roughness_remap_z_1.width = 156.03317260742188

    worn_cloth_secondary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    worn_cloth_secondary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.007')
    worn_cloth_secondary_roughness_remap_w_1.hide = True
    worn_cloth_secondary_roughness_remap_w_1.label = 'Roughness Remap W'
    worn_cloth_secondary_roughness_remap_w_1.location = (20.0, -180.0)
    worn_cloth_secondary_roughness_remap_w_1.name = 'Worn Cloth Secondary Roughness Remap W'
    worn_cloth_secondary_roughness_remap_w_1.width = 155.599853515625

    suit_primary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_roughness_remap_x_1.hide = True
    suit_primary_roughness_remap_x_1.label = 'Roughness Remap X'
    suit_primary_roughness_remap_x_1.location = (20.0, -60.0)
    suit_primary_roughness_remap_x_1.name = 'Suit Primary Roughness Remap X'
    suit_primary_roughness_remap_x_1.width = 156.206298828125

    suit_primary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_roughness_remap_y_1.hide = True
    suit_primary_roughness_remap_y_1.label = 'Roughness Remap Y'
    suit_primary_roughness_remap_y_1.location = (20.0, -100.0)
    suit_primary_roughness_remap_y_1.name = 'Suit Primary Roughness Remap Y'
    suit_primary_roughness_remap_y_1.width = 156.4664306640625

    suit_primary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_roughness_remap_z_1.hide = True
    suit_primary_roughness_remap_z_1.label = 'Roughness Remap Z'
    suit_primary_roughness_remap_z_1.location = (20.0, -140.0)
    suit_primary_roughness_remap_z_1.name = 'Suit Primary Roughness Remap Z'
    suit_primary_roughness_remap_z_1.width = 156.03314208984375

    suit_primary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_roughness_remap_w_1.hide = True
    suit_primary_roughness_remap_w_1.label = 'Roughness Remap W'
    suit_primary_roughness_remap_w_1.location = (20.0, -180.0)
    suit_primary_roughness_remap_w_1.name = 'Suit Primary Roughness Remap W'
    suit_primary_roughness_remap_w_1.width = 156.03314208984375

    suit_primary_wear_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_wear_remap_x_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_wear_remap_x_1.hide = True
    suit_primary_wear_remap_x_1.label = 'Wear Remap X'
    suit_primary_wear_remap_x_1.location = (20.0, -260.0)
    suit_primary_wear_remap_x_1.name = 'Suit Primary Wear Remap X'
    suit_primary_wear_remap_x_1.width = 156.46649169921875

    suit_primary_wear_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_wear_remap_y_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_wear_remap_y_1.hide = True
    suit_primary_wear_remap_y_1.label = 'Wear Remap Y'
    suit_primary_wear_remap_y_1.location = (20.0, -300.0)
    suit_primary_wear_remap_y_1.name = 'Suit Primary Wear Remap Y'
    suit_primary_wear_remap_y_1.width = 155.599853515625

    suit_primary_wear_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_wear_remap_z_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_wear_remap_z_1.hide = True
    suit_primary_wear_remap_z_1.label = 'Wear Remap Z'
    suit_primary_wear_remap_z_1.location = (20.0, -340.0)
    suit_primary_wear_remap_z_1.name = 'Suit Primary Wear Remap Z'
    suit_primary_wear_remap_z_1.width = 156.03314208984375

    suit_primary_wear_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    suit_primary_wear_remap_w_1.parent = test_group.nodes.get('Frame.008')
    suit_primary_wear_remap_w_1.hide = True
    suit_primary_wear_remap_w_1.label = 'Wear Remap W'
    suit_primary_wear_remap_w_1.location = (20.0, -380.0)
    suit_primary_wear_remap_w_1.name = 'Suit Primary Wear Remap W'
    suit_primary_wear_remap_w_1.width = 155.59979248046875

    worn_suit_primary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_roughness_remap_x_1.hide = True
    worn_suit_primary_roughness_remap_x_1.label = 'Roughness Remap X'
    worn_suit_primary_roughness_remap_x_1.location = (20.0, -80.0)
    worn_suit_primary_roughness_remap_x_1.name = 'Worn Suit Primary Roughness Remap X'
    worn_suit_primary_roughness_remap_x_1.width = 156.03314208984375

    worn_suit_primary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_roughness_remap_y_1.hide = True
    worn_suit_primary_roughness_remap_y_1.label = 'Roughness Remap Y'
    worn_suit_primary_roughness_remap_y_1.location = (20.0, -120.0)
    worn_suit_primary_roughness_remap_y_1.name = 'Worn Suit Primary Roughness Remap Y'
    worn_suit_primary_roughness_remap_y_1.width = 156.03314208984375

    worn_suit_primary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_roughness_remap_z_1.hide = True
    worn_suit_primary_roughness_remap_z_1.label = 'Roughness Remap Z'
    worn_suit_primary_roughness_remap_z_1.location = (20.0, -160.0)
    worn_suit_primary_roughness_remap_z_1.name = 'Worn Suit Primary Roughness Remap Z'
    worn_suit_primary_roughness_remap_z_1.width = 156.03317260742188

    worn_suit_primary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_primary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.009')
    worn_suit_primary_roughness_remap_w_1.hide = True
    worn_suit_primary_roughness_remap_w_1.label = 'Roughness Remap W'
    worn_suit_primary_roughness_remap_w_1.location = (20.0, -200.0)
    worn_suit_primary_roughness_remap_w_1.name = 'Worn Suit Primary Roughness Remap W'
    worn_suit_primary_roughness_remap_w_1.width = 155.599853515625

    suit_secondary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_roughness_remap_x_1.hide = True
    suit_secondary_roughness_remap_x_1.label = 'Roughness Remap X'
    suit_secondary_roughness_remap_x_1.location = (40.0, -80.0)
    suit_secondary_roughness_remap_x_1.name = 'Suit Secondary Roughness Remap X'
    suit_secondary_roughness_remap_x_1.width = 156.46649169921875

    suit_secondary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_roughness_remap_y_1.hide = True
    suit_secondary_roughness_remap_y_1.label = 'Roughness Remap Y'
    suit_secondary_roughness_remap_y_1.location = (40.0, -120.0)
    suit_secondary_roughness_remap_y_1.name = 'Suit Secondary Roughness Remap Y'
    suit_secondary_roughness_remap_y_1.width = 156.89984130859375

    suit_secondary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_roughness_remap_z_1.hide = True
    suit_secondary_roughness_remap_z_1.label = 'Roughness Remap Z'
    suit_secondary_roughness_remap_z_1.location = (40.0, -160.0)
    suit_secondary_roughness_remap_z_1.name = 'Suit Secondary Roughness Remap Z'
    suit_secondary_roughness_remap_z_1.width = 156.89984130859375

    suit_secondary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_roughness_remap_w_1.hide = True
    suit_secondary_roughness_remap_w_1.label = 'Roughness Remap W'
    suit_secondary_roughness_remap_w_1.location = (40.0, -200.0)
    suit_secondary_roughness_remap_w_1.name = 'Suit Secondary Roughness Remap W'
    suit_secondary_roughness_remap_w_1.width = 155.599853515625

    suit_secondary_wear_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_wear_remap_x_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_wear_remap_x_1.hide = True
    suit_secondary_wear_remap_x_1.label = 'Wear Remap X'
    suit_secondary_wear_remap_x_1.location = (40.0, -280.0)
    suit_secondary_wear_remap_x_1.name = 'Suit Secondary Wear Remap X'
    suit_secondary_wear_remap_x_1.width = 156.46649169921875

    suit_secondary_wear_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_wear_remap_y_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_wear_remap_y_1.hide = True
    suit_secondary_wear_remap_y_1.label = 'Wear Remap Y'
    suit_secondary_wear_remap_y_1.location = (40.0, -320.0)
    suit_secondary_wear_remap_y_1.name = 'Suit Secondary Wear Remap Y'
    suit_secondary_wear_remap_y_1.width = 156.46649169921875

    suit_secondary_wear_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_wear_remap_z_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_wear_remap_z_1.hide = True
    suit_secondary_wear_remap_z_1.label = 'Wear Remap Z'
    suit_secondary_wear_remap_z_1.location = (40.0, -360.0)
    suit_secondary_wear_remap_z_1.name = 'Suit Secondary Wear Remap Z'
    suit_secondary_wear_remap_z_1.width = 156.46646118164062

    suit_secondary_wear_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    suit_secondary_wear_remap_w_1.parent = test_group.nodes.get('Frame.010')
    suit_secondary_wear_remap_w_1.hide = True
    suit_secondary_wear_remap_w_1.label = 'Wear Remap W'
    suit_secondary_wear_remap_w_1.location = (40.0, -400.0)
    suit_secondary_wear_remap_w_1.name = 'Suit Secondary Wear Remap W'
    suit_secondary_wear_remap_w_1.width = 156.8997802734375

    worn_suit_secondary_roughness_remap_x_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_roughness_remap_x_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_roughness_remap_x_1.hide = True
    worn_suit_secondary_roughness_remap_x_1.label = 'Roughness Remap X'
    worn_suit_secondary_roughness_remap_x_1.location = (40.0, -80.0)
    worn_suit_secondary_roughness_remap_x_1.name = 'Worn Suit Secondary Roughness Remap X'
    worn_suit_secondary_roughness_remap_x_1.width = 156.466552734375

    worn_suit_secondary_roughness_remap_y_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_roughness_remap_y_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_roughness_remap_y_1.hide = True
    worn_suit_secondary_roughness_remap_y_1.label = 'Roughness Remap Y'
    worn_suit_secondary_roughness_remap_y_1.location = (40.0, -120.0)
    worn_suit_secondary_roughness_remap_y_1.name = 'Worn Suit Secondary Roughness Remap Y'
    worn_suit_secondary_roughness_remap_y_1.width = 156.46649169921875

    worn_suit_secondary_roughness_remap_z_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_roughness_remap_z_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_roughness_remap_z_1.hide = True
    worn_suit_secondary_roughness_remap_z_1.label = 'Roughness Remap Z'
    worn_suit_secondary_roughness_remap_z_1.location = (40.0, -160.0)
    worn_suit_secondary_roughness_remap_z_1.name = 'Worn Suit Secondary Roughness Remap Z'
    worn_suit_secondary_roughness_remap_z_1.width = 156.46649169921875

    worn_suit_secondary_roughness_remap_w_1 = test_group.nodes.new('ShaderNodeValue')
    worn_suit_secondary_roughness_remap_w_1.parent = test_group.nodes.get('Frame.011')
    worn_suit_secondary_roughness_remap_w_1.hide = True
    worn_suit_secondary_roughness_remap_w_1.label = 'Roughness Remap W'
    worn_suit_secondary_roughness_remap_w_1.location = (40.0, -200.0)
    worn_suit_secondary_roughness_remap_w_1.name = 'Worn Suit Secondary Roughness Remap W'
    worn_suit_secondary_roughness_remap_w_1.width = 156.46649169921875

    group_input_1 = test_group.nodes.new('NodeGroupInput')
    group_input_1.parent = test_group.nodes.get('Frame.012')
    group_input_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    group_input_1.location = (-240.0, 2840.0)
    group_input_1.name = 'Group Input'
    group_input_1.width = 140.0

    math_023_1 = test_group.nodes.new('ShaderNodeMath')
    math_023_1.parent = test_group.nodes.get('Frame.012')
    math_023_1.label = 'DO NOT TOUCH'
    math_023_1.location = (740.0, 3600.0)
    math_023_1.name = 'Math.023'
    math_023_1.operation = 'SUBTRACT'
    math_023_1.inputs[1].default_value = 3.0

    math_024_1 = test_group.nodes.new('ShaderNodeMath')
    math_024_1.parent = test_group.nodes.get('Frame.012')
    math_024_1.label = 'DO NOT TOUCH'
    math_024_1.location = (960.0, 3600.0)
    math_024_1.name = 'Math.024'
    math_024_1.operation = 'MULTIPLY'
    math_024_1.inputs[1].default_value = 0.333

    math_022_1 = test_group.nodes.new('ShaderNodeMath')
    math_022_1.parent = test_group.nodes.get('Frame.012')
    math_022_1.label = 'DO NOT TOUCH'
    math_022_1.location = (800.0, 3840.0)
    math_022_1.name = 'Math.022'
    math_022_1.operation = 'MULTIPLY'
    math_022_1.inputs[1].default_value = 0.333

    combine_rgb_1 = test_group.nodes.new('ShaderNodeCombineRGB')
    combine_rgb_1.parent = test_group.nodes.get('Frame.012')
    combine_rgb_1.label = 'DO NOT TOUCH'
    combine_rgb_1.location = (1300.0, 3700.0)

    math_021_1 = test_group.nodes.new('ShaderNodeMath')
    math_021_1.parent = test_group.nodes.get('Frame.012')
    math_021_1.label = 'DO NOT TOUCH'
    math_021_1.location = (500.0, 3700.0)
    math_021_1.name = 'Math.021'
    math_021_1.operation = 'FLOOR'

    mix_071_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_071_1.parent = test_group.nodes.get('Frame.012')
    mix_071_1.label = 'DO NOT TOUCH'
    mix_071_1.location = (2320.0, 2500.0)
    mix_071_1.name = 'Mix.071'

    mix_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_1.parent = test_group.nodes.get('Frame.012')
    mix_1.blend_type = 'MIX'
    mix_1.hide = True
    mix_1.label = 'DyeColorA'
    mix_1.location = (2453.43701171875, 919.050048828125)
    mix_1.name = 'Mix'

    mix_001_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_001_1.parent = test_group.nodes.get('Frame.012')
    mix_001_1.blend_type = 'MIX'
    mix_001_1.hide = True
    mix_001_1.label = 'DyeColorA'
    mix_001_1.location = (2453.43701171875, 889.050048828125)
    mix_001_1.name = 'Mix.001'

    mix_002_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_002_1.parent = test_group.nodes.get('Frame.012')
    mix_002_1.blend_type = 'MIX'
    mix_002_1.hide = True
    mix_002_1.label = 'DyeColorA'
    mix_002_1.location = (2453.43701171875, 859.050048828125)
    mix_002_1.name = 'Mix.002'

    mix_003_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_003_1.parent = test_group.nodes.get('Frame.012')
    mix_003_1.blend_type = 'MIX'
    mix_003_1.hide = True
    mix_003_1.label = 'DyeColorA'
    mix_003_1.location = (2453.43701171875, 829.050048828125)
    mix_003_1.name = 'Mix.003'

    mix_004_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_004_1.parent = test_group.nodes.get('Frame.012')
    mix_004_1.blend_type = 'MIX'
    mix_004_1.hide = True
    mix_004_1.label = 'DyeColorA'
    mix_004_1.location = (2452.10791015625, 799.050048828125)
    mix_004_1.name = 'Mix.004'

    reroute_154_1 = test_group.nodes.new('NodeReroute')
    reroute_154_1.parent = test_group.nodes.get('Frame.012')
    reroute_154_1.location = (2412.10791015625, 899.050048828125)
    reroute_154_1.name = 'Reroute.154'

    reroute_153_1 = test_group.nodes.new('NodeReroute')
    reroute_153_1.parent = test_group.nodes.get('Frame.012')
    reroute_153_1.location = (2412.10791015625, 879.050048828125)
    reroute_153_1.name = 'Reroute.153'

    reroute_155_1 = test_group.nodes.new('NodeReroute')
    reroute_155_1.parent = test_group.nodes.get('Frame.012')
    reroute_155_1.location = (2412.10791015625, 859.050048828125)
    reroute_155_1.name = 'Reroute.155'

    reroute_156_1 = test_group.nodes.new('NodeReroute')
    reroute_156_1.parent = test_group.nodes.get('Frame.012')
    reroute_156_1.location = (2412.10791015625, 839.050048828125)
    reroute_156_1.name = 'Reroute.156'

    reroute_157_1 = test_group.nodes.new('NodeReroute')
    reroute_157_1.parent = test_group.nodes.get('Frame.012')
    reroute_157_1.location = (2412.10791015625, 819.050048828125)
    reroute_157_1.name = 'Reroute.157'

    reroute_152_1 = test_group.nodes.new('NodeReroute')
    reroute_152_1.parent = test_group.nodes.get('Frame.012')
    reroute_152_1.location = (2412.10791015625, 799.050048828125)
    reroute_152_1.name = 'Reroute.152'

    reroute_206_1 = test_group.nodes.new('NodeReroute')
    reroute_206_1.parent = test_group.nodes.get('Frame.012')
    reroute_206_1.location = (2412.10791015625, 119.050048828125)
    reroute_206_1.name = 'Reroute.206'

    reroute_211_1 = test_group.nodes.new('NodeReroute')
    reroute_211_1.parent = test_group.nodes.get('Frame.012')
    reroute_211_1.location = (2412.10791015625, 99.050048828125)
    reroute_211_1.name = 'Reroute.211'

    reroute_207_1 = test_group.nodes.new('NodeReroute')
    reroute_207_1.parent = test_group.nodes.get('Frame.012')
    reroute_207_1.location = (2412.10791015625, 79.050048828125)
    reroute_207_1.name = 'Reroute.207'

    reroute_208_1 = test_group.nodes.new('NodeReroute')
    reroute_208_1.parent = test_group.nodes.get('Frame.012')
    reroute_208_1.location = (2412.10791015625, 59.050048828125)
    reroute_208_1.name = 'Reroute.208'

    reroute_209_1 = test_group.nodes.new('NodeReroute')
    reroute_209_1.parent = test_group.nodes.get('Frame.012')
    reroute_209_1.location = (2412.10791015625, 39.050048828125)
    reroute_209_1.name = 'Reroute.209'

    reroute_210_1 = test_group.nodes.new('NodeReroute')
    reroute_210_1.parent = test_group.nodes.get('Frame.012')
    reroute_210_1.location = (2412.10791015625, 19.050048828125)
    reroute_210_1.name = 'Reroute.210'

    reroute_229_1 = test_group.nodes.new('NodeReroute')
    reroute_229_1.parent = test_group.nodes.get('Frame.012')
    reroute_229_1.location = (2412.10791015625, -0.949951171875)
    reroute_229_1.name = 'Reroute.229'

    reroute_225_1 = test_group.nodes.new('NodeReroute')
    reroute_225_1.parent = test_group.nodes.get('Frame.012')
    reroute_225_1.location = (2412.10791015625, -20.949951171875)
    reroute_225_1.name = 'Reroute.225'

    reroute_226_1 = test_group.nodes.new('NodeReroute')
    reroute_226_1.parent = test_group.nodes.get('Frame.012')
    reroute_226_1.location = (2412.10791015625, -40.949951171875)
    reroute_226_1.name = 'Reroute.226'

    reroute_227_1 = test_group.nodes.new('NodeReroute')
    reroute_227_1.parent = test_group.nodes.get('Frame.012')
    reroute_227_1.location = (2412.10791015625, -60.949951171875)
    reroute_227_1.name = 'Reroute.227'

    reroute_224_1 = test_group.nodes.new('NodeReroute')
    reroute_224_1.parent = test_group.nodes.get('Frame.012')
    reroute_224_1.location = (2412.10791015625, -80.949951171875)
    reroute_224_1.name = 'Reroute.224'

    reroute_228_1 = test_group.nodes.new('NodeReroute')
    reroute_228_1.parent = test_group.nodes.get('Frame.012')
    reroute_228_1.location = (2412.10791015625, -100.949951171875)
    reroute_228_1.name = 'Reroute.228'

    reroute_245_1 = test_group.nodes.new('NodeReroute')
    reroute_245_1.parent = test_group.nodes.get('Frame.012')
    reroute_245_1.location = (2412.10791015625, -120.949951171875)
    reroute_245_1.name = 'Reroute.245'

    reroute_244_1 = test_group.nodes.new('NodeReroute')
    reroute_244_1.parent = test_group.nodes.get('Frame.012')
    reroute_244_1.location = (2412.10791015625, -140.949951171875)
    reroute_244_1.name = 'Reroute.244'

    reroute_243_1 = test_group.nodes.new('NodeReroute')
    reroute_243_1.parent = test_group.nodes.get('Frame.012')
    reroute_243_1.location = (2412.10791015625, -160.949951171875)
    reroute_243_1.name = 'Reroute.243'

    reroute_242_1 = test_group.nodes.new('NodeReroute')
    reroute_242_1.parent = test_group.nodes.get('Frame.012')
    reroute_242_1.location = (2412.10791015625, -180.949951171875)
    reroute_242_1.name = 'Reroute.242'

    reroute_246_1 = test_group.nodes.new('NodeReroute')
    reroute_246_1.parent = test_group.nodes.get('Frame.012')
    reroute_246_1.location = (2412.10791015625, -200.949951171875)
    reroute_246_1.name = 'Reroute.246'

    reroute_247_1 = test_group.nodes.new('NodeReroute')
    reroute_247_1.parent = test_group.nodes.get('Frame.012')
    reroute_247_1.location = (2412.10791015625, -220.949951171875)
    reroute_247_1.name = 'Reroute.247'

    reroute_307_1 = test_group.nodes.new('NodeReroute')
    reroute_307_1.parent = test_group.nodes.get('Frame.012')
    reroute_307_1.location = (2411.4072265625, 719.050048828125)
    reroute_307_1.name = 'Reroute.307'

    reroute_308_1 = test_group.nodes.new('NodeReroute')
    reroute_308_1.parent = test_group.nodes.get('Frame.012')
    reroute_308_1.location = (2411.4072265625, 699.050048828125)
    reroute_308_1.name = 'Reroute.308'

    mix_005_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_005_1.parent = test_group.nodes.get('Frame.012')
    mix_005_1.blend_type = 'MIX'
    mix_005_1.hide = True
    mix_005_1.label = 'DyeColorB'
    mix_005_1.location = (2453.43701171875, 739.050048828125)
    mix_005_1.name = 'Mix.005'

    reroute_309_1 = test_group.nodes.new('NodeReroute')
    reroute_309_1.parent = test_group.nodes.get('Frame.012')
    reroute_309_1.location = (2411.4072265625, 679.050048828125)
    reroute_309_1.name = 'Reroute.309'

    mix_006_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_006_1.parent = test_group.nodes.get('Frame.012')
    mix_006_1.blend_type = 'MIX'
    mix_006_1.hide = True
    mix_006_1.label = 'DyeColorB'
    mix_006_1.location = (2453.43701171875, 709.050048828125)
    mix_006_1.name = 'Mix.006'

    reroute_310_1 = test_group.nodes.new('NodeReroute')
    reroute_310_1.parent = test_group.nodes.get('Frame.012')
    reroute_310_1.location = (2411.4072265625, 659.050048828125)
    reroute_310_1.name = 'Reroute.310'

    mix_007_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_007_1.parent = test_group.nodes.get('Frame.012')
    mix_007_1.blend_type = 'MIX'
    mix_007_1.hide = True
    mix_007_1.label = 'DyeColorB'
    mix_007_1.location = (2453.43701171875, 679.050048828125)
    mix_007_1.name = 'Mix.007'

    reroute_311_1 = test_group.nodes.new('NodeReroute')
    reroute_311_1.parent = test_group.nodes.get('Frame.012')
    reroute_311_1.location = (2411.4072265625, 639.050048828125)
    reroute_311_1.name = 'Reroute.311'

    mix_008_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_008_1.parent = test_group.nodes.get('Frame.012')
    mix_008_1.blend_type = 'MIX'
    mix_008_1.hide = True
    mix_008_1.label = 'DyeColorB'
    mix_008_1.location = (2453.43701171875, 649.050048828125)
    mix_008_1.name = 'Mix.008'

    reroute_312_1 = test_group.nodes.new('NodeReroute')
    reroute_312_1.parent = test_group.nodes.get('Frame.012')
    reroute_312_1.location = (2411.4072265625, 619.050048828125)
    reroute_312_1.name = 'Reroute.312'

    mix_009_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_009_1.parent = test_group.nodes.get('Frame.012')
    mix_009_1.blend_type = 'MIX'
    mix_009_1.hide = True
    mix_009_1.label = 'DyeColorB'
    mix_009_1.location = (2453.43701171875, 619.050048828125)
    mix_009_1.name = 'Mix.009'

    reroute_335_1 = test_group.nodes.new('NodeReroute')
    reroute_335_1.parent = test_group.nodes.get('Frame.012')
    reroute_335_1.location = (2411.4072265625, 499.050048828125)
    reroute_335_1.name = 'Reroute.335'

    reroute_336_1 = test_group.nodes.new('NodeReroute')
    reroute_336_1.parent = test_group.nodes.get('Frame.012')
    reroute_336_1.location = (2411.4072265625, 359.050048828125)
    reroute_336_1.name = 'Reroute.336'

    mix_034_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_034_1.parent = test_group.nodes.get('Frame.012')
    mix_034_1.blend_type = 'MIX'
    mix_034_1.hide = True
    mix_034_1.label = 'GlossRemap_C'
    mix_034_1.location = (2453.43701171875, -248.7454833984375)
    mix_034_1.name = 'Mix.034'

    mix_032_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_032_1.parent = test_group.nodes.get('Frame.012')
    mix_032_1.blend_type = 'MIX'
    mix_032_1.hide = True
    mix_032_1.label = 'GlossRemap_C'
    mix_032_1.location = (2453.43701171875, -188.7454833984375)
    mix_032_1.name = 'Mix.032'

    mix_033_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_033_1.parent = test_group.nodes.get('Frame.012')
    mix_033_1.blend_type = 'MIX'
    mix_033_1.hide = True
    mix_033_1.label = 'GlossRemap_C'
    mix_033_1.location = (2453.43701171875, -218.7454833984375)
    mix_033_1.name = 'Mix.033'

    mix_025_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_025_1.parent = test_group.nodes.get('Frame.012')
    mix_025_1.blend_type = 'MIX'
    mix_025_1.hide = True
    mix_025_1.label = 'GlossRemap_B'
    mix_025_1.location = (2449.39697265625, 26.30078125)
    mix_025_1.name = 'Mix.025'

    mix_026_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_026_1.parent = test_group.nodes.get('Frame.012')
    mix_026_1.blend_type = 'MIX'
    mix_026_1.hide = True
    mix_026_1.label = 'GlossRemap_B'
    mix_026_1.location = (2449.39697265625, -3.69921875)
    mix_026_1.name = 'Mix.026'

    mix_027_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_027_1.parent = test_group.nodes.get('Frame.012')
    mix_027_1.blend_type = 'MIX'
    mix_027_1.hide = True
    mix_027_1.label = 'GlossRemap_B'
    mix_027_1.location = (2449.39697265625, -33.69921875)
    mix_027_1.name = 'Mix.027'

    mix_028_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_028_1.parent = test_group.nodes.get('Frame.012')
    mix_028_1.blend_type = 'MIX'
    mix_028_1.hide = True
    mix_028_1.label = 'GlossRemap_B'
    mix_028_1.location = (2449.39697265625, -63.69921875)
    mix_028_1.name = 'Mix.028'

    mix_029_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_029_1.parent = test_group.nodes.get('Frame.012')
    mix_029_1.blend_type = 'MIX'
    mix_029_1.hide = True
    mix_029_1.label = 'GlossRemap_B'
    mix_029_1.location = (2449.39697265625, -93.69921875)
    mix_029_1.name = 'Mix.029'

    mix_021_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_021_1.parent = test_group.nodes.get('Frame.012')
    mix_021_1.blend_type = 'MIX'
    mix_021_1.hide = True
    mix_021_1.label = 'GlossRemap_A'
    mix_021_1.location = (2449.39697265625, 151.0975341796875)
    mix_021_1.name = 'Mix.021'

    mix_022_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_022_1.parent = test_group.nodes.get('Frame.012')
    mix_022_1.blend_type = 'MIX'
    mix_022_1.hide = True
    mix_022_1.label = 'GlossRemap_A'
    mix_022_1.location = (2449.39697265625, 121.0975341796875)
    mix_022_1.name = 'Mix.022'

    mix_023_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_023_1.parent = test_group.nodes.get('Frame.012')
    mix_023_1.blend_type = 'MIX'
    mix_023_1.hide = True
    mix_023_1.label = 'GlossRemap_A'
    mix_023_1.location = (2449.39697265625, 91.0975341796875)
    mix_023_1.name = 'Mix.023'

    mix_024_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_024_1.parent = test_group.nodes.get('Frame.012')
    mix_024_1.blend_type = 'MIX'
    mix_024_1.hide = True
    mix_024_1.label = 'GlossRemap_A'
    mix_024_1.location = (2449.39697265625, 61.0975341796875)
    mix_024_1.name = 'Mix.024'

    mix_030_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_030_1.parent = test_group.nodes.get('Frame.012')
    mix_030_1.blend_type = 'MIX'
    mix_030_1.hide = True
    mix_030_1.label = 'GlossRemap_C'
    mix_030_1.location = (2453.43701171875, -128.7454833984375)
    mix_030_1.name = 'Mix.030'

    mix_031_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_031_1.parent = test_group.nodes.get('Frame.012')
    mix_031_1.blend_type = 'MIX'
    mix_031_1.hide = True
    mix_031_1.label = 'GlossRemap_C'
    mix_031_1.location = (2453.43701171875, -158.7454833984375)
    mix_031_1.name = 'Mix.031'

    mix_020_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_020_1.parent = test_group.nodes.get('Frame.012')
    mix_020_1.blend_type = 'MIX'
    mix_020_1.hide = True
    mix_020_1.label = 'GlossRemap_A'
    mix_020_1.location = (2449.39697265625, 181.0975341796875)
    mix_020_1.name = 'Mix.020'

    reroute_381_1 = test_group.nodes.new('NodeReroute')
    reroute_381_1.parent = test_group.nodes.get('Frame.012')
    reroute_381_1.location = (2411.4072265625, 479.050048828125)
    reroute_381_1.name = 'Reroute.381'

    mix_010_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_010_1.parent = test_group.nodes.get('Frame.012')
    mix_010_1.blend_type = 'MIX'
    mix_010_1.hide = True
    mix_010_1.label = 'Wear Remap_A'
    mix_010_1.location = (2448.38623046875, 540.4315185546875)
    mix_010_1.name = 'Mix.010'

    reroute_382_1 = test_group.nodes.new('NodeReroute')
    reroute_382_1.parent = test_group.nodes.get('Frame.012')
    reroute_382_1.location = (2411.4072265625, 459.050048828125)
    reroute_382_1.name = 'Reroute.382'

    mix_011_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_011_1.parent = test_group.nodes.get('Frame.012')
    mix_011_1.blend_type = 'MIX'
    mix_011_1.hide = True
    mix_011_1.label = 'Wear Remap_A'
    mix_011_1.location = (2448.38623046875, 510.4315185546875)
    mix_011_1.name = 'Mix.011'

    reroute_383_1 = test_group.nodes.new('NodeReroute')
    reroute_383_1.parent = test_group.nodes.get('Frame.012')
    reroute_383_1.location = (2411.4072265625, 439.050048828125)
    reroute_383_1.name = 'Reroute.383'

    mix_012_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_012_1.parent = test_group.nodes.get('Frame.012')
    mix_012_1.blend_type = 'MIX'
    mix_012_1.hide = True
    mix_012_1.label = 'Wear Remap_A'
    mix_012_1.location = (2448.38623046875, 480.4315185546875)
    mix_012_1.name = 'Mix.012'

    reroute_384_1 = test_group.nodes.new('NodeReroute')
    reroute_384_1.parent = test_group.nodes.get('Frame.012')
    reroute_384_1.location = (2411.4072265625, 419.050048828125)
    reroute_384_1.name = 'Reroute.384'

    mix_013_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_013_1.parent = test_group.nodes.get('Frame.012')
    mix_013_1.blend_type = 'MIX'
    mix_013_1.hide = True
    mix_013_1.label = 'Wear Remap_A'
    mix_013_1.location = (2448.38623046875, 450.4315185546875)
    mix_013_1.name = 'Mix.013'

    reroute_385_1 = test_group.nodes.new('NodeReroute')
    reroute_385_1.parent = test_group.nodes.get('Frame.012')
    reroute_385_1.location = (2411.4072265625, 399.050048828125)
    reroute_385_1.name = 'Reroute.385'

    mix_014_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_014_1.parent = test_group.nodes.get('Frame.012')
    mix_014_1.blend_type = 'MIX'
    mix_014_1.hide = True
    mix_014_1.label = 'Wear Remap_A'
    mix_014_1.location = (2448.38623046875, 420.4315185546875)
    mix_014_1.name = 'Mix.014'

    reroute_415_1 = test_group.nodes.new('NodeReroute')
    reroute_415_1.parent = test_group.nodes.get('Frame.012')
    reroute_415_1.location = (2411.4072265625, 339.050048828125)
    reroute_415_1.name = 'Reroute.415'

    mix_015_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_015_1.parent = test_group.nodes.get('Frame.012')
    mix_015_1.blend_type = 'MIX'
    mix_015_1.hide = True
    mix_015_1.label = 'Wear Remap_B'
    mix_015_1.location = (2448.38623046875, 384.36602783203125)
    mix_015_1.name = 'Mix.015'

    reroute_416_1 = test_group.nodes.new('NodeReroute')
    reroute_416_1.parent = test_group.nodes.get('Frame.012')
    reroute_416_1.location = (2411.4072265625, 319.050048828125)
    reroute_416_1.name = 'Reroute.416'

    mix_016_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_016_1.parent = test_group.nodes.get('Frame.012')
    mix_016_1.blend_type = 'MIX'
    mix_016_1.hide = True
    mix_016_1.label = 'Wear Remap_B'
    mix_016_1.location = (2448.38623046875, 354.36602783203125)
    mix_016_1.name = 'Mix.016'

    reroute_417_1 = test_group.nodes.new('NodeReroute')
    reroute_417_1.parent = test_group.nodes.get('Frame.012')
    reroute_417_1.location = (2411.4072265625, 299.050048828125)
    reroute_417_1.name = 'Reroute.417'

    mix_017_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_017_1.parent = test_group.nodes.get('Frame.012')
    mix_017_1.blend_type = 'MIX'
    mix_017_1.hide = True
    mix_017_1.label = 'Wear Remap_B'
    mix_017_1.location = (2448.38623046875, 324.36602783203125)
    mix_017_1.name = 'Mix.017'

    reroute_418_1 = test_group.nodes.new('NodeReroute')
    reroute_418_1.parent = test_group.nodes.get('Frame.012')
    reroute_418_1.location = (2411.4072265625, 279.050048828125)
    reroute_418_1.name = 'Reroute.418'

    mix_018_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_018_1.parent = test_group.nodes.get('Frame.012')
    mix_018_1.blend_type = 'MIX'
    mix_018_1.hide = True
    mix_018_1.label = 'Wear Remap_B'
    mix_018_1.location = (2448.38623046875, 294.36602783203125)
    mix_018_1.name = 'Mix.018'

    reroute_419_1 = test_group.nodes.new('NodeReroute')
    reroute_419_1.parent = test_group.nodes.get('Frame.012')
    reroute_419_1.location = (2411.4072265625, 259.050048828125)
    reroute_419_1.name = 'Reroute.419'

    mix_019_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_019_1.parent = test_group.nodes.get('Frame.012')
    mix_019_1.blend_type = 'MIX'
    mix_019_1.hide = True
    mix_019_1.label = 'Wear Remap_B'
    mix_019_1.location = (2448.38623046875, 264.36602783203125)
    mix_019_1.name = 'Mix.019'

    reroute_432_1 = test_group.nodes.new('NodeReroute')
    reroute_432_1.parent = test_group.nodes.get('Frame.012')
    reroute_432_1.location = (2431.4072265625, -1273.06005859375)
    reroute_432_1.name = 'Reroute.432'

    reroute_433_1 = test_group.nodes.new('NodeReroute')
    reroute_433_1.parent = test_group.nodes.get('Frame.012')
    reroute_433_1.location = (2431.4072265625, -1293.06005859375)
    reroute_433_1.name = 'Reroute.433'

    mix_040_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_040_1.parent = test_group.nodes.get('Frame.012')
    mix_040_1.blend_type = 'MIX'
    mix_040_1.hide = True
    mix_040_1.label = 'Detail Blends'
    mix_040_1.location = (2453.43701171875, -1263.19775390625)
    mix_040_1.name = 'Mix.040'

    reroute_434_1 = test_group.nodes.new('NodeReroute')
    reroute_434_1.parent = test_group.nodes.get('Frame.012')
    reroute_434_1.location = (2431.4072265625, -1313.06005859375)
    reroute_434_1.name = 'Reroute.434'

    mix_041_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_041_1.parent = test_group.nodes.get('Frame.012')
    mix_041_1.blend_type = 'MIX'
    mix_041_1.hide = True
    mix_041_1.label = 'Detail Blends'
    mix_041_1.location = (2453.43701171875, -1293.19775390625)
    mix_041_1.name = 'Mix.041'

    reroute_435_1 = test_group.nodes.new('NodeReroute')
    reroute_435_1.parent = test_group.nodes.get('Frame.012')
    reroute_435_1.location = (2431.4072265625, -1333.06005859375)
    reroute_435_1.name = 'Reroute.435'

    mix_042_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_042_1.parent = test_group.nodes.get('Frame.012')
    mix_042_1.blend_type = 'MIX'
    mix_042_1.hide = True
    mix_042_1.label = 'Detail Blends'
    mix_042_1.location = (2453.43701171875, -1323.19775390625)
    mix_042_1.name = 'Mix.042'

    reroute_436_1 = test_group.nodes.new('NodeReroute')
    reroute_436_1.parent = test_group.nodes.get('Frame.012')
    reroute_436_1.location = (2431.4072265625, -1353.06005859375)
    reroute_436_1.name = 'Reroute.436'

    mix_043_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_043_1.parent = test_group.nodes.get('Frame.012')
    mix_043_1.blend_type = 'MIX'
    mix_043_1.hide = True
    mix_043_1.label = 'Detail Blends'
    mix_043_1.location = (2453.43701171875, -1353.19775390625)
    mix_043_1.name = 'Mix.043'

    reroute_437_1 = test_group.nodes.new('NodeReroute')
    reroute_437_1.parent = test_group.nodes.get('Frame.012')
    reroute_437_1.location = (2431.4072265625, -1373.06005859375)
    reroute_437_1.name = 'Reroute.437'

    mix_044_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_044_1.parent = test_group.nodes.get('Frame.012')
    mix_044_1.blend_type = 'MIX'
    mix_044_1.hide = True
    mix_044_1.label = 'Detail Blends'
    mix_044_1.location = (2453.43701171875, -1383.19775390625)
    mix_044_1.name = 'Mix.044'

    reroute_466_1 = test_group.nodes.new('NodeReroute')
    reroute_466_1.parent = test_group.nodes.get('Frame.012')
    reroute_466_1.location = (2411.4072265625, -840.949951171875)
    reroute_466_1.name = 'Reroute.466'

    reroute_461_1 = test_group.nodes.new('NodeReroute')
    reroute_461_1.parent = test_group.nodes.get('Frame.012')
    reroute_461_1.location = (2411.4072265625, -860.949951171875)
    reroute_461_1.name = 'Reroute.461'

    mix_064_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_064_1.parent = test_group.nodes.get('Frame.012')
    mix_064_1.blend_type = 'MIX'
    mix_064_1.hide = True
    mix_064_1.label = 'Detail Diffuse'
    mix_064_1.location = (2451.31884765625, -816.6573486328125)
    mix_064_1.name = 'Mix.064'

    reroute_462_1 = test_group.nodes.new('NodeReroute')
    reroute_462_1.parent = test_group.nodes.get('Frame.012')
    reroute_462_1.location = (2411.4072265625, -880.949951171875)
    reroute_462_1.name = 'Reroute.462'

    mix_060_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_060_1.parent = test_group.nodes.get('Frame.012')
    mix_060_1.blend_type = 'MIX'
    mix_060_1.hide = True
    mix_060_1.label = 'Detail Diffuse'
    mix_060_1.location = (2451.31884765625, -846.6573486328125)
    mix_060_1.name = 'Mix.060'

    reroute_463_1 = test_group.nodes.new('NodeReroute')
    reroute_463_1.parent = test_group.nodes.get('Frame.012')
    reroute_463_1.location = (2411.4072265625, -900.949951171875)
    reroute_463_1.name = 'Reroute.463'

    mix_061_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_061_1.parent = test_group.nodes.get('Frame.012')
    mix_061_1.blend_type = 'MIX'
    mix_061_1.hide = True
    mix_061_1.label = 'Detail Diffuse'
    mix_061_1.location = (2451.31884765625, -876.6573486328125)
    mix_061_1.name = 'Mix.061'

    reroute_464_1 = test_group.nodes.new('NodeReroute')
    reroute_464_1.parent = test_group.nodes.get('Frame.012')
    reroute_464_1.location = (2411.4072265625, -920.949951171875)
    reroute_464_1.name = 'Reroute.464'

    mix_062_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_062_1.parent = test_group.nodes.get('Frame.012')
    mix_062_1.blend_type = 'MIX'
    mix_062_1.hide = True
    mix_062_1.label = 'Detail Diffuse'
    mix_062_1.location = (2451.31884765625, -906.6573486328125)
    mix_062_1.name = 'Mix.062'

    reroute_478_1 = test_group.nodes.new('NodeReroute')
    reroute_478_1.parent = test_group.nodes.get('Frame.012')
    reroute_478_1.location = (2411.4072265625, -960.949951171875)
    reroute_478_1.name = 'Reroute.478'

    reroute_473_1 = test_group.nodes.new('NodeReroute')
    reroute_473_1.parent = test_group.nodes.get('Frame.012')
    reroute_473_1.location = (2411.4072265625, -980.949951171875)
    reroute_473_1.name = 'Reroute.473'

    mix_035_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_035_1.parent = test_group.nodes.get('Frame.012')
    mix_035_1.blend_type = 'MIX'
    mix_035_1.hide = True
    mix_035_1.label = 'TiledNormal'
    mix_035_1.location = (2453.43701171875, -969.553955078125)
    mix_035_1.name = 'Mix.035'

    reroute_474_1 = test_group.nodes.new('NodeReroute')
    reroute_474_1.parent = test_group.nodes.get('Frame.012')
    reroute_474_1.location = (2411.4072265625, -1000.949951171875)
    reroute_474_1.name = 'Reroute.474'

    mix_036_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_036_1.parent = test_group.nodes.get('Frame.012')
    mix_036_1.blend_type = 'MIX'
    mix_036_1.hide = True
    mix_036_1.label = 'TiledNormal'
    mix_036_1.location = (2453.43701171875, -999.553955078125)
    mix_036_1.name = 'Mix.036'

    reroute_475_1 = test_group.nodes.new('NodeReroute')
    reroute_475_1.parent = test_group.nodes.get('Frame.012')
    reroute_475_1.location = (2411.4072265625, -1020.949951171875)
    reroute_475_1.name = 'Reroute.475'

    mix_037_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_037_1.parent = test_group.nodes.get('Frame.012')
    mix_037_1.blend_type = 'MIX'
    mix_037_1.hide = True
    mix_037_1.label = 'TiledNormal'
    mix_037_1.location = (2453.43701171875, -1029.553955078125)
    mix_037_1.name = 'Mix.037'

    reroute_476_1 = test_group.nodes.new('NodeReroute')
    reroute_476_1.parent = test_group.nodes.get('Frame.012')
    reroute_476_1.location = (2411.4072265625, -1040.949951171875)
    reroute_476_1.name = 'Reroute.476'

    mix_038_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_038_1.parent = test_group.nodes.get('Frame.012')
    mix_038_1.blend_type = 'MIX'
    mix_038_1.hide = True
    mix_038_1.label = 'TiledNormal'
    mix_038_1.location = (2453.43701171875, -1059.553955078125)
    mix_038_1.name = 'Mix.038'

    reroute_477_1 = test_group.nodes.new('NodeReroute')
    reroute_477_1.parent = test_group.nodes.get('Frame.012')
    reroute_477_1.location = (2411.4072265625, -1060.949951171875)
    reroute_477_1.name = 'Reroute.477'

    mix_039_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_039_1.parent = test_group.nodes.get('Frame.012')
    mix_039_1.blend_type = 'MIX'
    mix_039_1.hide = True
    mix_039_1.label = 'TiledNormal'
    mix_039_1.location = (2453.43701171875, -1089.553955078125)
    mix_039_1.name = 'Mix.039'

    reroute_479_1 = test_group.nodes.new('NodeReroute')
    reroute_479_1.parent = test_group.nodes.get('Frame.012')
    reroute_479_1.location = (2411.4072265625, -2160.949951171875)
    reroute_479_1.name = 'Reroute.479'

    reroute_490_1 = test_group.nodes.new('NodeReroute')
    reroute_490_1.parent = test_group.nodes.get('Frame.012')
    reroute_490_1.location = (2411.4072265625, -2180.949951171875)
    reroute_490_1.name = 'Reroute.490'

    mix_045_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_045_1.parent = test_group.nodes.get('Frame.012')
    mix_045_1.blend_type = 'MIX'
    mix_045_1.hide = True
    mix_045_1.label = 'Iridescence, Fuzz,. Transmission'
    mix_045_1.location = (2453.43701171875, -2149.204833984375)
    mix_045_1.name = 'Mix.045'

    reroute_491_1 = test_group.nodes.new('NodeReroute')
    reroute_491_1.parent = test_group.nodes.get('Frame.012')
    reroute_491_1.location = (2411.4072265625, -2200.949951171875)
    reroute_491_1.name = 'Reroute.491'

    mix_046_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_046_1.parent = test_group.nodes.get('Frame.012')
    mix_046_1.blend_type = 'MIX'
    mix_046_1.hide = True
    mix_046_1.label = 'Iridescence, Fuzz,. Transmission'
    mix_046_1.location = (2453.43701171875, -2179.204833984375)
    mix_046_1.name = 'Mix.046'

    reroute_492_1 = test_group.nodes.new('NodeReroute')
    reroute_492_1.parent = test_group.nodes.get('Frame.012')
    reroute_492_1.location = (2411.4072265625, -2220.949951171875)
    reroute_492_1.name = 'Reroute.492'

    mix_047_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_047_1.parent = test_group.nodes.get('Frame.012')
    mix_047_1.blend_type = 'MIX'
    mix_047_1.hide = True
    mix_047_1.label = 'Iridescence, Fuzz,. Transmission'
    mix_047_1.location = (2453.43701171875, -2209.204833984375)
    mix_047_1.name = 'Mix.047'

    reroute_493_1 = test_group.nodes.new('NodeReroute')
    reroute_493_1.parent = test_group.nodes.get('Frame.012')
    reroute_493_1.location = (2411.4072265625, -2240.949951171875)
    reroute_493_1.name = 'Reroute.493'

    mix_048_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_048_1.parent = test_group.nodes.get('Frame.012')
    mix_048_1.blend_type = 'MIX'
    mix_048_1.hide = True
    mix_048_1.label = 'Iridescence, Fuzz,. Transmission'
    mix_048_1.location = (2453.43701171875, -2239.204833984375)
    mix_048_1.name = 'Mix.048'

    reroute_494_1 = test_group.nodes.new('NodeReroute')
    reroute_494_1.parent = test_group.nodes.get('Frame.012')
    reroute_494_1.location = (2411.4072265625, -2260.949951171875)
    reroute_494_1.name = 'Reroute.494'

    mix_049_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_049_1.parent = test_group.nodes.get('Frame.012')
    mix_049_1.blend_type = 'MIX'
    mix_049_1.hide = True
    mix_049_1.label = 'Iridescence, Fuzz,. Transmission'
    mix_049_1.location = (2453.43701171875, -2269.204833984375)
    mix_049_1.name = 'Mix.049'

    reroute_509_1 = test_group.nodes.new('NodeReroute')
    reroute_509_1.parent = test_group.nodes.get('Frame.012')
    reroute_509_1.location = (2411.4072265625, -3760.949951171875)
    reroute_509_1.name = 'Reroute.509'

    reroute_508_1 = test_group.nodes.new('NodeReroute')
    reroute_508_1.parent = test_group.nodes.get('Frame.012')
    reroute_508_1.location = (2411.4072265625, -3780.949951171875)
    reroute_508_1.name = 'Reroute.508'

    mix_055_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_055_1.parent = test_group.nodes.get('Frame.012')
    mix_055_1.blend_type = 'MIX'
    mix_055_1.hide = True
    mix_055_1.label = 'Worn Detail Blends'
    mix_055_1.location = (2454.43798828125, -3734.325927734375)
    mix_055_1.name = 'Mix.055'

    reroute_510_1 = test_group.nodes.new('NodeReroute')
    reroute_510_1.parent = test_group.nodes.get('Frame.012')
    reroute_510_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_510_1.hide = False
    reroute_510_1.location = (2411.4072265625, -3800.949951171875)
    reroute_510_1.mute = False
    reroute_510_1.name = 'Reroute.510'
    reroute_510_1.use_custom_color = False
    reroute_510_1.width = 16.0

    mix_056_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_056_1.parent = test_group.nodes.get('Frame.012')
    mix_056_1.blend_type = 'MIX'
    mix_056_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_056_1.hide = True
    mix_056_1.label = 'Worn Detail Blends'
    mix_056_1.location = (2454.43798828125, -3764.325927734375)
    mix_056_1.mute = False
    mix_056_1.name = 'Mix.056'
    mix_056_1.use_alpha = False
    mix_056_1.use_clamp = False
    mix_056_1.use_custom_color = False
    mix_056_1.width = 140.0
    mix_056_1.inputs[0].default_value = 0.5
    mix_056_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_056_1.inputs[2].default_value = (0.42800000309944153, 1.0, 0.890175998210907, 1.0)
    mix_056_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_511_1 = test_group.nodes.new('NodeReroute')
    reroute_511_1.parent = test_group.nodes.get('Frame.012')
    reroute_511_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_511_1.hide = False
    reroute_511_1.location = (2411.4072265625, -3820.949951171875)
    reroute_511_1.mute = False
    reroute_511_1.name = 'Reroute.511'
    reroute_511_1.use_custom_color = False
    reroute_511_1.width = 16.0

    mix_057_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_057_1.parent = test_group.nodes.get('Frame.012')
    mix_057_1.blend_type = 'MIX'
    mix_057_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_057_1.hide = True
    mix_057_1.label = 'Worn Detail Blends'
    mix_057_1.location = (2454.43798828125, -3794.325927734375)
    mix_057_1.mute = False
    mix_057_1.name = 'Mix.057'
    mix_057_1.use_alpha = False
    mix_057_1.use_clamp = False
    mix_057_1.use_custom_color = False
    mix_057_1.width = 140.0
    mix_057_1.inputs[0].default_value = 0.5
    mix_057_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_057_1.inputs[2].default_value = (0.0, 0.07869499921798706, 0.8942620158195496, 1.0)
    mix_057_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_512_1 = test_group.nodes.new('NodeReroute')
    reroute_512_1.parent = test_group.nodes.get('Frame.012')
    reroute_512_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_512_1.hide = False
    reroute_512_1.location = (2411.4072265625, -3840.949951171875)
    reroute_512_1.mute = False
    reroute_512_1.name = 'Reroute.512'
    reroute_512_1.use_custom_color = False
    reroute_512_1.width = 16.0

    mix_058_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_058_1.parent = test_group.nodes.get('Frame.012')
    mix_058_1.blend_type = 'MIX'
    mix_058_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_058_1.hide = True
    mix_058_1.label = 'Worn Detail Blends'
    mix_058_1.location = (2454.43798828125, -3824.325927734375)
    mix_058_1.mute = False
    mix_058_1.name = 'Mix.058'
    mix_058_1.use_alpha = False
    mix_058_1.use_clamp = False
    mix_058_1.use_custom_color = False
    mix_058_1.width = 140.0
    mix_058_1.inputs[0].default_value = 0.5
    mix_058_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_058_1.inputs[2].default_value = (0.42800000309944153, 1.0, 0.890175998210907, 1.0)
    mix_058_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_513_1 = test_group.nodes.new('NodeReroute')
    reroute_513_1.parent = test_group.nodes.get('Frame.012')
    reroute_513_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_513_1.hide = False
    reroute_513_1.location = (2411.4072265625, -3860.949951171875)
    reroute_513_1.mute = False
    reroute_513_1.name = 'Reroute.513'
    reroute_513_1.use_custom_color = False
    reroute_513_1.width = 16.0

    mix_059_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_059_1.parent = test_group.nodes.get('Frame.012')
    mix_059_1.blend_type = 'MIX'
    mix_059_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_059_1.hide = True
    mix_059_1.label = 'Worn Detail Blends'
    mix_059_1.location = (2454.43798828125, -3854.325927734375)
    mix_059_1.mute = False
    mix_059_1.name = 'Mix.059'
    mix_059_1.use_alpha = False
    mix_059_1.use_clamp = False
    mix_059_1.use_custom_color = False
    mix_059_1.width = 140.0
    mix_059_1.inputs[0].default_value = 0.5
    mix_059_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_059_1.inputs[2].default_value = (0.0, 0.07869499921798706, 0.8942620158195496, 1.0)
    mix_059_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_530_1 = test_group.nodes.new('NodeReroute')
    reroute_530_1.parent = test_group.nodes.get('Frame.012')
    reroute_530_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_530_1.hide = False
    reroute_530_1.location = (2411.4072265625, -5200.9501953125)
    reroute_530_1.mute = False
    reroute_530_1.name = 'Reroute.530'
    reroute_530_1.use_custom_color = False
    reroute_530_1.width = 16.0

    reroute_529_1 = test_group.nodes.new('NodeReroute')
    reroute_529_1.parent = test_group.nodes.get('Frame.012')
    reroute_529_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_529_1.hide = False
    reroute_529_1.location = (2411.4072265625, -5220.9501953125)
    reroute_529_1.mute = False
    reroute_529_1.name = 'Reroute.529'
    reroute_529_1.use_custom_color = False
    reroute_529_1.width = 16.0

    mix_050_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_050_1.parent = test_group.nodes.get('Frame.012')
    mix_050_1.blend_type = 'MIX'
    mix_050_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_050_1.hide = True
    mix_050_1.label = 'Emission'
    mix_050_1.location = (2453.43701171875, -5186.62109375)
    mix_050_1.mute = False
    mix_050_1.name = 'Mix.050'
    mix_050_1.use_alpha = False
    mix_050_1.use_clamp = False
    mix_050_1.use_custom_color = False
    mix_050_1.width = 140.0
    mix_050_1.inputs[0].default_value = 0.5
    mix_050_1.inputs[1].default_value = (0.07869499921798706, 0.07869499921798706, 0.8942620158195496, 1.0)
    mix_050_1.inputs[2].default_value = (0.42800000309944153, 1.0, 0.890175998210907, 1.0)
    mix_050_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_528_1 = test_group.nodes.new('NodeReroute')
    reroute_528_1.parent = test_group.nodes.get('Frame.012')
    reroute_528_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_528_1.hide = False
    reroute_528_1.location = (2411.4072265625, -5240.9501953125)
    reroute_528_1.mute = False
    reroute_528_1.name = 'Reroute.528'
    reroute_528_1.use_custom_color = False
    reroute_528_1.width = 16.0

    mix_051_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_051_1.parent = test_group.nodes.get('Frame.012')
    mix_051_1.blend_type = 'MIX'
    mix_051_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_051_1.hide = True
    mix_051_1.label = 'Emission'
    mix_051_1.location = (2453.43701171875, -5216.62109375)
    mix_051_1.mute = False
    mix_051_1.name = 'Mix.051'
    mix_051_1.use_alpha = False
    mix_051_1.use_clamp = False
    mix_051_1.use_custom_color = False
    mix_051_1.width = 140.0
    mix_051_1.inputs[0].default_value = 0.5
    mix_051_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_051_1.inputs[2].default_value = (0.42800000309944153, 1.0, 0.890175998210907, 1.0)
    mix_051_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_527_1 = test_group.nodes.new('NodeReroute')
    reroute_527_1.parent = test_group.nodes.get('Frame.012')
    reroute_527_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_527_1.hide = False
    reroute_527_1.location = (2411.4072265625, -5260.9501953125)
    reroute_527_1.mute = False
    reroute_527_1.name = 'Reroute.527'
    reroute_527_1.use_custom_color = False
    reroute_527_1.width = 16.0

    mix_052_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_052_1.parent = test_group.nodes.get('Frame.012')
    mix_052_1.blend_type = 'MIX'
    mix_052_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_052_1.hide = True
    mix_052_1.label = 'Emission'
    mix_052_1.location = (2453.43701171875, -5246.62109375)
    mix_052_1.mute = False
    mix_052_1.name = 'Mix.052'
    mix_052_1.use_alpha = False
    mix_052_1.use_clamp = False
    mix_052_1.use_custom_color = False
    mix_052_1.width = 140.0
    mix_052_1.inputs[0].default_value = 0.5
    mix_052_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_052_1.inputs[2].default_value = (0.0, 0.07869499921798706, 0.8942620158195496, 1.0)
    mix_052_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_526_1 = test_group.nodes.new('NodeReroute')
    reroute_526_1.parent = test_group.nodes.get('Frame.012')
    reroute_526_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_526_1.hide = False
    reroute_526_1.location = (2411.4072265625, -5280.9501953125)
    reroute_526_1.mute = False
    reroute_526_1.name = 'Reroute.526'
    reroute_526_1.use_custom_color = False
    reroute_526_1.width = 16.0

    mix_053_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_053_1.parent = test_group.nodes.get('Frame.012')
    mix_053_1.blend_type = 'MIX'
    mix_053_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_053_1.hide = True
    mix_053_1.label = 'Emission'
    mix_053_1.location = (2453.43701171875, -5276.62109375)
    mix_053_1.mute = False
    mix_053_1.name = 'Mix.053'
    mix_053_1.use_alpha = False
    mix_053_1.use_clamp = False
    mix_053_1.use_custom_color = False
    mix_053_1.width = 140.0
    mix_053_1.inputs[0].default_value = 0.5
    mix_053_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_053_1.inputs[2].default_value = (0.42800000309944153, 1.0, 0.890175998210907, 1.0)
    mix_053_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_525_1 = test_group.nodes.new('NodeReroute')
    reroute_525_1.parent = test_group.nodes.get('Frame.012')
    reroute_525_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_525_1.hide = False
    reroute_525_1.location = (2411.4072265625, -5300.9501953125)
    reroute_525_1.mute = False
    reroute_525_1.name = 'Reroute.525'
    reroute_525_1.use_custom_color = False
    reroute_525_1.width = 16.0

    mix_054_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_054_1.parent = test_group.nodes.get('Frame.012')
    mix_054_1.blend_type = 'MIX'
    mix_054_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_054_1.hide = True
    mix_054_1.label = 'Emission'
    mix_054_1.location = (2453.43701171875, -5306.62109375)
    mix_054_1.mute = False
    mix_054_1.name = 'Mix.054'
    mix_054_1.use_alpha = False
    mix_054_1.use_clamp = False
    mix_054_1.use_custom_color = False
    mix_054_1.width = 140.0
    mix_054_1.inputs[0].default_value = 0.5
    mix_054_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_054_1.inputs[2].default_value = (0.0, 0.07869499921798706, 0.8942620158195496, 1.0)
    mix_054_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    mix_063_1 = test_group.nodes.new('ShaderNodeMixRGB')
    mix_063_1.parent = test_group.nodes.get('Frame.012')
    mix_063_1.blend_type = 'MIX'
    mix_063_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    mix_063_1.hide = True
    mix_063_1.label = 'Detail Diffuse'
    mix_063_1.location = (2451.31884765625, -936.657470703125)
    mix_063_1.mute = False
    mix_063_1.name = 'Mix.063'
    mix_063_1.use_alpha = False
    mix_063_1.use_clamp = False
    mix_063_1.use_custom_color = False
    mix_063_1.width = 140.0
    mix_063_1.inputs[0].default_value = 0.5
    mix_063_1.inputs[1].default_value = (0.5, 0.5, 0.5, 1.0)
    mix_063_1.inputs[2].default_value = (0.0, 0.07869499921798706, 0.8942620158195496, 1.0)
    mix_063_1.outputs[0].default_value = (0.0, 0.0, 0.0, 0.0)

    reroute_465_1 = test_group.nodes.new('NodeReroute')
    reroute_465_1.parent = test_group.nodes.get('Frame.012')
    reroute_465_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_465_1.hide = False
    reroute_465_1.location = (2411.4072265625, -940.949951171875)
    reroute_465_1.mute = False
    reroute_465_1.name = 'Reroute.465'
    reroute_465_1.use_custom_color = False
    reroute_465_1.width = 16.0

    reroute_518_1 = test_group.nodes.new('NodeReroute')
    reroute_518_1.parent = test_group.nodes.get('Frame.012')
    reroute_518_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_518_1.hide = False
    reroute_518_1.label = 'DO NOT TOUCH'
    reroute_518_1.location = (-1148.5927734375, -5300.9501953125)
    reroute_518_1.mute = False
    reroute_518_1.name = 'Reroute.518'
    reroute_518_1.use_custom_color = False
    reroute_518_1.width = 16.0

    reroute_523_1 = test_group.nodes.new('NodeReroute')
    reroute_523_1.parent = test_group.nodes.get('Frame.012')
    reroute_523_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_523_1.hide = False
    reroute_523_1.label = 'DO NOT TOUCH'
    reroute_523_1.location = (-1148.5927734375, -4700.9501953125)
    reroute_523_1.mute = False
    reroute_523_1.name = 'Reroute.523'
    reroute_523_1.use_custom_color = False
    reroute_523_1.width = 16.0

    reroute_148_1 = test_group.nodes.new('NodeReroute')
    reroute_148_1.parent = test_group.nodes.get('Frame.012')
    reroute_148_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_148_1.hide = False
    reroute_148_1.label = 'DO NOT TOUCH'
    reroute_148_1.location = (1852.10791015625, -2740.949951171875)
    reroute_148_1.mute = False
    reroute_148_1.name = 'Reroute.148'
    reroute_148_1.use_custom_color = False
    reroute_148_1.width = 16.0

    reroute_150_1 = test_group.nodes.new('NodeReroute')
    reroute_150_1.parent = test_group.nodes.get('Frame.012')
    reroute_150_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_150_1.hide = False
    reroute_150_1.label = 'DO NOT TOUCH'
    reroute_150_1.location = (1832.10791015625, -3980.949951171875)
    reroute_150_1.mute = False
    reroute_150_1.name = 'Reroute.150'
    reroute_150_1.use_custom_color = False
    reroute_150_1.width = 16.0

    reroute_146_1 = test_group.nodes.new('NodeReroute')
    reroute_146_1.parent = test_group.nodes.get('Frame.012')
    reroute_146_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_146_1.hide = False
    reroute_146_1.label = 'DO NOT TOUCH'
    reroute_146_1.location = (1872.10791015625, -1480.949951171875)
    reroute_146_1.mute = False
    reroute_146_1.name = 'Reroute.146'
    reroute_146_1.use_custom_color = False
    reroute_146_1.width = 16.0

    reroute_144_1 = test_group.nodes.new('NodeReroute')
    reroute_144_1.parent = test_group.nodes.get('Frame.012')
    reroute_144_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_144_1.hide = False
    reroute_144_1.label = 'DO NOT TOUCH'
    reroute_144_1.location = (1892.10791015625, -260.949951171875)
    reroute_144_1.mute = False
    reroute_144_1.name = 'Reroute.144'
    reroute_144_1.use_custom_color = False
    reroute_144_1.width = 16.0

    reroute_140_1 = test_group.nodes.new('NodeReroute')
    reroute_140_1.parent = test_group.nodes.get('Frame.012')
    reroute_140_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_140_1.hide = False
    reroute_140_1.label = 'DO NOT TOUCH'
    reroute_140_1.location = (1932.10791015625, 2219.050048828125)
    reroute_140_1.mute = False
    reroute_140_1.name = 'Reroute.140'
    reroute_140_1.use_custom_color = False
    reroute_140_1.width = 16.0

    reroute_143_1 = test_group.nodes.new('NodeReroute')
    reroute_143_1.parent = test_group.nodes.get('Frame.012')
    reroute_143_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_143_1.hide = False
    reroute_143_1.label = 'DO NOT TOUCH'
    reroute_143_1.location = (1912.10791015625, 979.050048828125)
    reroute_143_1.mute = False
    reroute_143_1.name = 'Reroute.143'
    reroute_143_1.use_custom_color = False
    reroute_143_1.width = 16.0

    reroute_141_1 = test_group.nodes.new('NodeReroute')
    reroute_141_1.parent = test_group.nodes.get('Frame.012')
    reroute_141_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_141_1.hide = False
    reroute_141_1.label = 'DO NOT TOUCH'
    reroute_141_1.location = (1932.10791015625, 899.050048828125)
    reroute_141_1.mute = False
    reroute_141_1.name = 'Reroute.141'
    reroute_141_1.use_custom_color = False
    reroute_141_1.width = 16.0

    reroute_142_1 = test_group.nodes.new('NodeReroute')
    reroute_142_1.parent = test_group.nodes.get('Frame.012')
    reroute_142_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_142_1.hide = False
    reroute_142_1.label = 'DO NOT TOUCH'
    reroute_142_1.location = (1912.10791015625, 879.050048828125)
    reroute_142_1.mute = False
    reroute_142_1.name = 'Reroute.142'
    reroute_142_1.use_custom_color = False
    reroute_142_1.width = 16.0

    reroute_145_1 = test_group.nodes.new('NodeReroute')
    reroute_145_1.parent = test_group.nodes.get('Frame.012')
    reroute_145_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_145_1.hide = False
    reroute_145_1.label = 'DO NOT TOUCH'
    reroute_145_1.location = (1892.10791015625, 859.050048828125)
    reroute_145_1.mute = False
    reroute_145_1.name = 'Reroute.145'
    reroute_145_1.use_custom_color = False
    reroute_145_1.width = 16.0

    reroute_147_1 = test_group.nodes.new('NodeReroute')
    reroute_147_1.parent = test_group.nodes.get('Frame.012')
    reroute_147_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_147_1.hide = False
    reroute_147_1.label = 'DO NOT TOUCH'
    reroute_147_1.location = (1872.10791015625, 839.050048828125)
    reroute_147_1.mute = False
    reroute_147_1.name = 'Reroute.147'
    reroute_147_1.use_custom_color = False
    reroute_147_1.width = 16.0

    reroute_149_1 = test_group.nodes.new('NodeReroute')
    reroute_149_1.parent = test_group.nodes.get('Frame.012')
    reroute_149_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_149_1.hide = False
    reroute_149_1.label = 'DO NOT TOUCH'
    reroute_149_1.location = (1852.10791015625, 819.050048828125)
    reroute_149_1.mute = False
    reroute_149_1.name = 'Reroute.149'
    reroute_149_1.use_custom_color = False
    reroute_149_1.width = 16.0

    reroute_151_1 = test_group.nodes.new('NodeReroute')
    reroute_151_1.parent = test_group.nodes.get('Frame.012')
    reroute_151_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_151_1.hide = False
    reroute_151_1.label = 'DO NOT TOUCH'
    reroute_151_1.location = (1832.10791015625, 799.050048828125)
    reroute_151_1.mute = False
    reroute_151_1.name = 'Reroute.151'
    reroute_151_1.use_custom_color = False
    reroute_151_1.width = 16.0

    reroute_294_1 = test_group.nodes.new('NodeReroute')
    reroute_294_1.parent = test_group.nodes.get('Frame.012')
    reroute_294_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_294_1.hide = False
    reroute_294_1.label = 'DO NOT TOUCH'
    reroute_294_1.location = (1811.4072265625, 719.050048828125)
    reroute_294_1.mute = False
    reroute_294_1.name = 'Reroute.294'
    reroute_294_1.use_custom_color = False
    reroute_294_1.width = 16.0

    reroute_303_1 = test_group.nodes.new('NodeReroute')
    reroute_303_1.parent = test_group.nodes.get('Frame.012')
    reroute_303_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_303_1.hide = False
    reroute_303_1.label = 'DO NOT TOUCH'
    reroute_303_1.location = (1771.4072265625, 679.050048828125)
    reroute_303_1.mute = False
    reroute_303_1.name = 'Reroute.303'
    reroute_303_1.use_custom_color = False
    reroute_303_1.width = 16.0

    reroute_304_1 = test_group.nodes.new('NodeReroute')
    reroute_304_1.parent = test_group.nodes.get('Frame.012')
    reroute_304_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_304_1.hide = False
    reroute_304_1.label = 'DO NOT TOUCH'
    reroute_304_1.location = (1751.4072265625, 659.050048828125)
    reroute_304_1.mute = False
    reroute_304_1.name = 'Reroute.304'
    reroute_304_1.use_custom_color = False
    reroute_304_1.width = 16.0

    reroute_305_1 = test_group.nodes.new('NodeReroute')
    reroute_305_1.parent = test_group.nodes.get('Frame.012')
    reroute_305_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_305_1.hide = False
    reroute_305_1.label = 'DO NOT TOUCH'
    reroute_305_1.location = (1731.4072265625, 639.050048828125)
    reroute_305_1.mute = False
    reroute_305_1.name = 'Reroute.305'
    reroute_305_1.use_custom_color = False
    reroute_305_1.width = 16.0

    reroute_306_1 = test_group.nodes.new('NodeReroute')
    reroute_306_1.parent = test_group.nodes.get('Frame.012')
    reroute_306_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_306_1.hide = False
    reroute_306_1.label = 'DO NOT TOUCH'
    reroute_306_1.location = (1711.4072265625, 619.050048828125)
    reroute_306_1.mute = False
    reroute_306_1.name = 'Reroute.306'
    reroute_306_1.use_custom_color = False
    reroute_306_1.width = 16.0

    reroute_313_1 = test_group.nodes.new('NodeReroute')
    reroute_313_1.parent = test_group.nodes.get('Frame.012')
    reroute_313_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_313_1.hide = False
    reroute_313_1.label = 'DO NOT TOUCH'
    reroute_313_1.location = (1811.4072265625, 1399.050048828125)
    reroute_313_1.mute = False
    reroute_313_1.name = 'Reroute.313'
    reroute_313_1.use_custom_color = False
    reroute_313_1.width = 16.0

    reroute_314_1 = test_group.nodes.new('NodeReroute')
    reroute_314_1.parent = test_group.nodes.get('Frame.012')
    reroute_314_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_314_1.hide = False
    reroute_314_1.label = 'DO NOT TOUCH'
    reroute_314_1.location = (1791.4072265625, 159.050048828125)
    reroute_314_1.mute = False
    reroute_314_1.name = 'Reroute.314'
    reroute_314_1.use_custom_color = False
    reroute_314_1.width = 16.0

    reroute_297_1 = test_group.nodes.new('NodeReroute')
    reroute_297_1.parent = test_group.nodes.get('Frame.012')
    reroute_297_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_297_1.hide = False
    reroute_297_1.label = 'DO NOT TOUCH'
    reroute_297_1.location = (1791.4072265625, 699.050048828125)
    reroute_297_1.mute = False
    reroute_297_1.name = 'Reroute.297'
    reroute_297_1.use_custom_color = False
    reroute_297_1.width = 16.0

    reroute_315_1 = test_group.nodes.new('NodeReroute')
    reroute_315_1.parent = test_group.nodes.get('Frame.012')
    reroute_315_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_315_1.hide = False
    reroute_315_1.label = 'DO NOT TOUCH'
    reroute_315_1.location = (1771.4072265625, -1080.949951171875)
    reroute_315_1.mute = False
    reroute_315_1.name = 'Reroute.315'
    reroute_315_1.use_custom_color = False
    reroute_315_1.width = 16.0

    reroute_316_1 = test_group.nodes.new('NodeReroute')
    reroute_316_1.parent = test_group.nodes.get('Frame.012')
    reroute_316_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_316_1.hide = False
    reroute_316_1.label = 'DO NOT TOUCH'
    reroute_316_1.location = (1751.4072265625, -2320.949951171875)
    reroute_316_1.mute = False
    reroute_316_1.name = 'Reroute.316'
    reroute_316_1.use_custom_color = False
    reroute_316_1.width = 16.0

    reroute_317_1 = test_group.nodes.new('NodeReroute')
    reroute_317_1.parent = test_group.nodes.get('Frame.012')
    reroute_317_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_317_1.hide = False
    reroute_317_1.label = 'DO NOT TOUCH'
    reroute_317_1.location = (1731.4072265625, -3560.949951171875)
    reroute_317_1.mute = False
    reroute_317_1.name = 'Reroute.317'
    reroute_317_1.use_custom_color = False
    reroute_317_1.width = 16.0

    reroute_280_1 = test_group.nodes.new('NodeReroute')
    reroute_280_1.parent = test_group.nodes.get('Frame.012')
    reroute_280_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_280_1.hide = False
    reroute_280_1.label = 'DO NOT TOUCH'
    reroute_280_1.location = (1091.4072265625, -4840.9501953125)
    reroute_280_1.mute = False
    reroute_280_1.name = 'Reroute.280'
    reroute_280_1.use_custom_color = False
    reroute_280_1.width = 16.0

    reroute_281_1 = test_group.nodes.new('NodeReroute')
    reroute_281_1.parent = test_group.nodes.get('Frame.012')
    reroute_281_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_281_1.hide = False
    reroute_281_1.label = 'DO NOT TOUCH'
    reroute_281_1.location = (1111.4072265625, -4880.9501953125)
    reroute_281_1.mute = False
    reroute_281_1.name = 'Reroute.281'
    reroute_281_1.use_custom_color = False
    reroute_281_1.width = 16.0

    reroute_282_1 = test_group.nodes.new('NodeReroute')
    reroute_282_1.parent = test_group.nodes.get('Frame.012')
    reroute_282_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_282_1.hide = False
    reroute_282_1.label = 'DO NOT TOUCH'
    reroute_282_1.location = (1131.4072265625, -4920.9501953125)
    reroute_282_1.mute = False
    reroute_282_1.name = 'Reroute.282'
    reroute_282_1.use_custom_color = False
    reroute_282_1.width = 16.0

    reroute_283_1 = test_group.nodes.new('NodeReroute')
    reroute_283_1.parent = test_group.nodes.get('Frame.012')
    reroute_283_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_283_1.hide = False
    reroute_283_1.label = 'DO NOT TOUCH'
    reroute_283_1.location = (1151.4072265625, -4960.9501953125)
    reroute_283_1.mute = False
    reroute_283_1.name = 'Reroute.283'
    reroute_283_1.use_custom_color = False
    reroute_283_1.width = 16.0

    reroute_204_1 = test_group.nodes.new('NodeReroute')
    reroute_204_1.parent = test_group.nodes.get('Frame.012')
    reroute_204_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_204_1.hide = False
    reroute_204_1.label = 'DO NOT TOUCH'
    reroute_204_1.location = (1611.4072265625, -2800.949951171875)
    reroute_204_1.mute = False
    reroute_204_1.name = 'Reroute.204'
    reroute_204_1.use_custom_color = False
    reroute_204_1.width = 16.0

    reroute_218_1 = test_group.nodes.new('NodeReroute')
    reroute_218_1.parent = test_group.nodes.get('Frame.012')
    reroute_218_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_218_1.hide = False
    reroute_218_1.label = 'DO NOT TOUCH'
    reroute_218_1.location = (1491.4072265625, -2920.949951171875)
    reroute_218_1.mute = False
    reroute_218_1.name = 'Reroute.218'
    reroute_218_1.use_custom_color = False
    reroute_218_1.width = 16.0

    reroute_237_1 = test_group.nodes.new('NodeReroute')
    reroute_237_1.parent = test_group.nodes.get('Frame.012')
    reroute_237_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_237_1.hide = False
    reroute_237_1.label = 'DO NOT TOUCH'
    reroute_237_1.location = (1371.4072265625, -3040.949951171875)
    reroute_237_1.mute = False
    reroute_237_1.name = 'Reroute.237'
    reroute_237_1.use_custom_color = False
    reroute_237_1.width = 16.0

    reroute_205_1 = test_group.nodes.new('NodeReroute')
    reroute_205_1.parent = test_group.nodes.get('Frame.012')
    reroute_205_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_205_1.hide = False
    reroute_205_1.label = 'DO NOT TOUCH'
    reroute_205_1.location = (1591.4072265625, -4040.949951171875)
    reroute_205_1.mute = False
    reroute_205_1.name = 'Reroute.205'
    reroute_205_1.use_custom_color = False
    reroute_205_1.width = 16.0

    reroute_223_1 = test_group.nodes.new('NodeReroute')
    reroute_223_1.parent = test_group.nodes.get('Frame.012')
    reroute_223_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_223_1.hide = False
    reroute_223_1.label = 'DO NOT TOUCH'
    reroute_223_1.location = (1471.4072265625, -4160.9501953125)
    reroute_223_1.mute = False
    reroute_223_1.name = 'Reroute.223'
    reroute_223_1.use_custom_color = False
    reroute_223_1.width = 16.0

    reroute_236_1 = test_group.nodes.new('NodeReroute')
    reroute_236_1.parent = test_group.nodes.get('Frame.012')
    reroute_236_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_236_1.hide = False
    reroute_236_1.label = 'DO NOT TOUCH'
    reroute_236_1.location = (1351.4072265625, -4280.9501953125)
    reroute_236_1.mute = False
    reroute_236_1.name = 'Reroute.236'
    reroute_236_1.use_custom_color = False
    reroute_236_1.width = 16.0

    reroute_273_1 = test_group.nodes.new('NodeReroute')
    reroute_273_1.parent = test_group.nodes.get('Frame.012')
    reroute_273_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_273_1.hide = False
    reroute_273_1.label = 'DO NOT TOUCH'
    reroute_273_1.location = (1091.4072265625, -3600.949951171875)
    reroute_273_1.mute = False
    reroute_273_1.name = 'Reroute.273'
    reroute_273_1.use_custom_color = False
    reroute_273_1.width = 16.0

    reroute_274_1 = test_group.nodes.new('NodeReroute')
    reroute_274_1.parent = test_group.nodes.get('Frame.012')
    reroute_274_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_274_1.hide = False
    reroute_274_1.label = 'DO NOT TOUCH'
    reroute_274_1.location = (1111.4072265625, -3640.949951171875)
    reroute_274_1.mute = False
    reroute_274_1.name = 'Reroute.274'
    reroute_274_1.use_custom_color = False
    reroute_274_1.width = 16.0

    reroute_275_1 = test_group.nodes.new('NodeReroute')
    reroute_275_1.parent = test_group.nodes.get('Frame.012')
    reroute_275_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_275_1.hide = False
    reroute_275_1.label = 'DO NOT TOUCH'
    reroute_275_1.location = (1131.4072265625, -3680.949951171875)
    reroute_275_1.mute = False
    reroute_275_1.name = 'Reroute.275'
    reroute_275_1.use_custom_color = False
    reroute_275_1.width = 16.0

    reroute_276_1 = test_group.nodes.new('NodeReroute')
    reroute_276_1.parent = test_group.nodes.get('Frame.012')
    reroute_276_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_276_1.hide = False
    reroute_276_1.label = 'DO NOT TOUCH'
    reroute_276_1.location = (1151.4072265625, -3720.949951171875)
    reroute_276_1.mute = False
    reroute_276_1.name = 'Reroute.276'
    reroute_276_1.use_custom_color = False
    reroute_276_1.width = 16.0

    reroute_284_1 = test_group.nodes.new('NodeReroute')
    reroute_284_1.parent = test_group.nodes.get('Frame.012')
    reroute_284_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_284_1.hide = False
    reroute_284_1.label = 'DO NOT TOUCH'
    reroute_284_1.location = (1151.4072265625, -4340.9501953125)
    reroute_284_1.mute = False
    reroute_284_1.name = 'Reroute.284'
    reroute_284_1.use_custom_color = False
    reroute_284_1.width = 16.0

    reroute_285_1 = test_group.nodes.new('NodeReroute')
    reroute_285_1.parent = test_group.nodes.get('Frame.012')
    reroute_285_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_285_1.hide = False
    reroute_285_1.label = 'DO NOT TOUCH'
    reroute_285_1.location = (1131.4072265625, -4320.9501953125)
    reroute_285_1.mute = False
    reroute_285_1.name = 'Reroute.285'
    reroute_285_1.use_custom_color = False
    reroute_285_1.width = 16.0

    reroute_286_1 = test_group.nodes.new('NodeReroute')
    reroute_286_1.parent = test_group.nodes.get('Frame.012')
    reroute_286_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_286_1.hide = False
    reroute_286_1.label = 'DO NOT TOUCH'
    reroute_286_1.location = (1111.4072265625, -4300.9501953125)
    reroute_286_1.mute = False
    reroute_286_1.name = 'Reroute.286'
    reroute_286_1.use_custom_color = False
    reroute_286_1.width = 16.0

    reroute_287_1 = test_group.nodes.new('NodeReroute')
    reroute_287_1.parent = test_group.nodes.get('Frame.012')
    reroute_287_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_287_1.hide = False
    reroute_287_1.label = 'DO NOT TOUCH'
    reroute_287_1.location = (1091.4072265625, -4220.9501953125)
    reroute_287_1.mute = False
    reroute_287_1.name = 'Reroute.287'
    reroute_287_1.use_custom_color = False
    reroute_287_1.width = 16.0

    reroute_269_1 = test_group.nodes.new('NodeReroute')
    reroute_269_1.parent = test_group.nodes.get('Frame.012')
    reroute_269_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_269_1.hide = False
    reroute_269_1.label = 'DO NOT TOUCH'
    reroute_269_1.location = (1111.4072265625, -2380.949951171875)
    reroute_269_1.mute = False
    reroute_269_1.name = 'Reroute.269'
    reroute_269_1.use_custom_color = False
    reroute_269_1.width = 16.0

    reroute_270_1 = test_group.nodes.new('NodeReroute')
    reroute_270_1.parent = test_group.nodes.get('Frame.012')
    reroute_270_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_270_1.hide = False
    reroute_270_1.label = 'DO NOT TOUCH'
    reroute_270_1.location = (1131.4072265625, -2420.949951171875)
    reroute_270_1.mute = False
    reroute_270_1.name = 'Reroute.270'
    reroute_270_1.use_custom_color = False
    reroute_270_1.width = 16.0

    reroute_271_1 = test_group.nodes.new('NodeReroute')
    reroute_271_1.parent = test_group.nodes.get('Frame.012')
    reroute_271_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_271_1.hide = False
    reroute_271_1.label = 'DO NOT TOUCH'
    reroute_271_1.location = (1151.4072265625, -2460.949951171875)
    reroute_271_1.mute = False
    reroute_271_1.name = 'Reroute.271'
    reroute_271_1.use_custom_color = False
    reroute_271_1.width = 16.0

    reroute_272_1 = test_group.nodes.new('NodeReroute')
    reroute_272_1.parent = test_group.nodes.get('Frame.012')
    reroute_272_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_272_1.hide = False
    reroute_272_1.label = 'DO NOT TOUCH'
    reroute_272_1.location = (1151.4072265625, -3100.949951171875)
    reroute_272_1.mute = False
    reroute_272_1.name = 'Reroute.272'
    reroute_272_1.use_custom_color = False
    reroute_272_1.width = 16.0

    reroute_277_1 = test_group.nodes.new('NodeReroute')
    reroute_277_1.parent = test_group.nodes.get('Frame.012')
    reroute_277_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_277_1.hide = False
    reroute_277_1.label = 'DO NOT TOUCH'
    reroute_277_1.location = (1131.4072265625, -3080.949951171875)
    reroute_277_1.mute = False
    reroute_277_1.name = 'Reroute.277'
    reroute_277_1.use_custom_color = False
    reroute_277_1.width = 16.0

    reroute_278_1 = test_group.nodes.new('NodeReroute')
    reroute_278_1.parent = test_group.nodes.get('Frame.012')
    reroute_278_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_278_1.hide = False
    reroute_278_1.label = 'DO NOT TOUCH'
    reroute_278_1.location = (1111.4072265625, -3060.949951171875)
    reroute_278_1.mute = False
    reroute_278_1.name = 'Reroute.278'
    reroute_278_1.use_custom_color = False
    reroute_278_1.width = 16.0

    reroute_279_1 = test_group.nodes.new('NodeReroute')
    reroute_279_1.parent = test_group.nodes.get('Frame.012')
    reroute_279_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_279_1.hide = False
    reroute_279_1.label = 'DO NOT TOUCH'
    reroute_279_1.location = (1091.4072265625, -2980.949951171875)
    reroute_279_1.mute = False
    reroute_279_1.name = 'Reroute.279'
    reroute_279_1.use_custom_color = False
    reroute_279_1.width = 16.0

    reroute_238_1 = test_group.nodes.new('NodeReroute')
    reroute_238_1.parent = test_group.nodes.get('Frame.012')
    reroute_238_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_238_1.hide = False
    reroute_238_1.label = 'DO NOT TOUCH'
    reroute_238_1.location = (1391.4072265625, -1780.949951171875)
    reroute_238_1.mute = False
    reroute_238_1.name = 'Reroute.238'
    reroute_238_1.use_custom_color = False
    reroute_238_1.width = 16.0

    reroute_266_1 = test_group.nodes.new('NodeReroute')
    reroute_266_1.parent = test_group.nodes.get('Frame.012')
    reroute_266_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_266_1.hide = False
    reroute_266_1.label = 'DO NOT TOUCH'
    reroute_266_1.location = (1151.4072265625, -1840.949951171875)
    reroute_266_1.mute = False
    reroute_266_1.name = 'Reroute.266'
    reroute_266_1.use_custom_color = False
    reroute_266_1.width = 16.0

    reroute_268_1 = test_group.nodes.new('NodeReroute')
    reroute_268_1.parent = test_group.nodes.get('Frame.012')
    reroute_268_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_268_1.hide = False
    reroute_268_1.label = 'DO NOT TOUCH'
    reroute_268_1.location = (1091.4072265625, -2340.949951171875)
    reroute_268_1.mute = False
    reroute_268_1.name = 'Reroute.268'
    reroute_268_1.use_custom_color = False
    reroute_268_1.width = 16.0

    reroute_264_1 = test_group.nodes.new('NodeReroute')
    reroute_264_1.parent = test_group.nodes.get('Frame.012')
    reroute_264_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_264_1.hide = False
    reroute_264_1.label = 'DO NOT TOUCH'
    reroute_264_1.location = (1131.4072265625, -1820.949951171875)
    reroute_264_1.mute = False
    reroute_264_1.name = 'Reroute.264'
    reroute_264_1.use_custom_color = False
    reroute_264_1.width = 16.0

    reroute_265_1 = test_group.nodes.new('NodeReroute')
    reroute_265_1.parent = test_group.nodes.get('Frame.012')
    reroute_265_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_265_1.hide = False
    reroute_265_1.label = 'DO NOT TOUCH'
    reroute_265_1.location = (1111.4072265625, -1800.949951171875)
    reroute_265_1.mute = False
    reroute_265_1.name = 'Reroute.265'
    reroute_265_1.use_custom_color = False
    reroute_265_1.width = 16.0

    reroute_175_1 = test_group.nodes.new('NodeReroute')
    reroute_175_1.parent = test_group.nodes.get('Frame.012')
    reroute_175_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_175_1.hide = False
    reroute_175_1.label = 'DO NOT TOUCH'
    reroute_175_1.location = (1631.4072265625, -1540.949951171875)
    reroute_175_1.mute = False
    reroute_175_1.name = 'Reroute.175'
    reroute_175_1.use_custom_color = False
    reroute_175_1.width = 16.0

    reroute_222_1 = test_group.nodes.new('NodeReroute')
    reroute_222_1.parent = test_group.nodes.get('Frame.012')
    reroute_222_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_222_1.hide = False
    reroute_222_1.label = 'DO NOT TOUCH'
    reroute_222_1.location = (1511.4072265625, -1660.949951171875)
    reroute_222_1.mute = False
    reroute_222_1.name = 'Reroute.222'
    reroute_222_1.use_custom_color = False
    reroute_222_1.width = 16.0

    reroute_256_1 = test_group.nodes.new('NodeReroute')
    reroute_256_1.parent = test_group.nodes.get('Frame.012')
    reroute_256_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_256_1.hide = False
    reroute_256_1.label = 'DO NOT TOUCH'
    reroute_256_1.location = (1091.4072265625, -1120.949951171875)
    reroute_256_1.mute = False
    reroute_256_1.name = 'Reroute.256'
    reroute_256_1.use_custom_color = False
    reroute_256_1.width = 16.0

    reroute_257_1 = test_group.nodes.new('NodeReroute')
    reroute_257_1.parent = test_group.nodes.get('Frame.012')
    reroute_257_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_257_1.hide = False
    reroute_257_1.label = 'DO NOT TOUCH'
    reroute_257_1.location = (1111.4072265625, -1160.949951171875)
    reroute_257_1.mute = False
    reroute_257_1.name = 'Reroute.257'
    reroute_257_1.use_custom_color = False
    reroute_257_1.width = 16.0

    reroute_258_1 = test_group.nodes.new('NodeReroute')
    reroute_258_1.parent = test_group.nodes.get('Frame.012')
    reroute_258_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_258_1.hide = False
    reroute_258_1.label = 'DO NOT TOUCH'
    reroute_258_1.location = (1131.4072265625, -1200.949951171875)
    reroute_258_1.mute = False
    reroute_258_1.name = 'Reroute.258'
    reroute_258_1.use_custom_color = False
    reroute_258_1.width = 16.0

    reroute_259_1 = test_group.nodes.new('NodeReroute')
    reroute_259_1.parent = test_group.nodes.get('Frame.012')
    reroute_259_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_259_1.hide = False
    reroute_259_1.label = 'DO NOT TOUCH'
    reroute_259_1.location = (1151.4072265625, -1240.949951171875)
    reroute_259_1.mute = False
    reroute_259_1.name = 'Reroute.259'
    reroute_259_1.use_custom_color = False
    reroute_259_1.width = 16.0

    reroute_267_1 = test_group.nodes.new('NodeReroute')
    reroute_267_1.parent = test_group.nodes.get('Frame.012')
    reroute_267_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_267_1.hide = False
    reroute_267_1.label = 'DO NOT TOUCH'
    reroute_267_1.location = (1091.4072265625, -1720.949951171875)
    reroute_267_1.mute = False
    reroute_267_1.name = 'Reroute.267'
    reroute_267_1.use_custom_color = False
    reroute_267_1.width = 16.0

    reroute_172_1 = test_group.nodes.new('NodeReroute')
    reroute_172_1.parent = test_group.nodes.get('Frame.012')
    reroute_172_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_172_1.hide = False
    reroute_172_1.label = 'DO NOT TOUCH'
    reroute_172_1.location = (1691.4072265625, 2159.050048828125)
    reroute_172_1.mute = False
    reroute_172_1.name = 'Reroute.172'
    reroute_172_1.use_custom_color = False
    reroute_172_1.width = 16.0

    reroute_219_1 = test_group.nodes.new('NodeReroute')
    reroute_219_1.parent = test_group.nodes.get('Frame.012')
    reroute_219_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_219_1.hide = False
    reroute_219_1.label = 'DO NOT TOUCH'
    reroute_219_1.location = (1571.4072265625, 2039.050048828125)
    reroute_219_1.mute = False
    reroute_219_1.name = 'Reroute.219'
    reroute_219_1.use_custom_color = False
    reroute_219_1.width = 16.0

    reroute_241_1 = test_group.nodes.new('NodeReroute')
    reroute_241_1.parent = test_group.nodes.get('Frame.012')
    reroute_241_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_241_1.hide = False
    reroute_241_1.label = 'DO NOT TOUCH'
    reroute_241_1.location = (1451.4072265625, 1919.050048828125)
    reroute_241_1.mute = False
    reroute_241_1.name = 'Reroute.241'
    reroute_241_1.use_custom_color = False
    reroute_241_1.width = 16.0

    reroute_174_1 = test_group.nodes.new('NodeReroute')
    reroute_174_1.parent = test_group.nodes.get('Frame.012')
    reroute_174_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_174_1.hide = False
    reroute_174_1.label = 'DO NOT TOUCH'
    reroute_174_1.location = (1651.4072265625, -320.949951171875)
    reroute_174_1.mute = False
    reroute_174_1.name = 'Reroute.174'
    reroute_174_1.use_custom_color = False
    reroute_174_1.width = 16.0

    reroute_221_1 = test_group.nodes.new('NodeReroute')
    reroute_221_1.parent = test_group.nodes.get('Frame.012')
    reroute_221_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_221_1.hide = False
    reroute_221_1.label = 'DO NOT TOUCH'
    reroute_221_1.location = (1531.4072265625, -440.949951171875)
    reroute_221_1.mute = False
    reroute_221_1.name = 'Reroute.221'
    reroute_221_1.use_custom_color = False
    reroute_221_1.width = 16.0

    reroute_239_1 = test_group.nodes.new('NodeReroute')
    reroute_239_1.parent = test_group.nodes.get('Frame.012')
    reroute_239_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_239_1.hide = False
    reroute_239_1.label = 'DO NOT TOUCH'
    reroute_239_1.location = (1411.4072265625, -560.949951171875)
    reroute_239_1.mute = False
    reroute_239_1.name = 'Reroute.239'
    reroute_239_1.use_custom_color = False
    reroute_239_1.width = 16.0

    reroute_170_1 = test_group.nodes.new('NodeReroute')
    reroute_170_1.parent = test_group.nodes.get('Frame.012')
    reroute_170_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_170_1.hide = False
    reroute_170_1.label = 'DO NOT TOUCH'
    reroute_170_1.location = (1611.4072265625, 39.050048828125)
    reroute_170_1.mute = False
    reroute_170_1.name = 'Reroute.170'
    reroute_170_1.use_custom_color = False
    reroute_170_1.width = 16.0

    reroute_166_1 = test_group.nodes.new('NodeReroute')
    reroute_166_1.parent = test_group.nodes.get('Frame.012')
    reroute_166_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_166_1.hide = False
    reroute_166_1.label = 'DO NOT TOUCH'
    reroute_166_1.location = (1691.4072265625, 119.050048828125)
    reroute_166_1.mute = False
    reroute_166_1.name = 'Reroute.166'
    reroute_166_1.use_custom_color = False
    reroute_166_1.width = 16.0

    reroute_167_1 = test_group.nodes.new('NodeReroute')
    reroute_167_1.parent = test_group.nodes.get('Frame.012')
    reroute_167_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_167_1.hide = False
    reroute_167_1.label = 'DO NOT TOUCH'
    reroute_167_1.location = (1671.4072265625, 99.050048828125)
    reroute_167_1.mute = False
    reroute_167_1.name = 'Reroute.167'
    reroute_167_1.use_custom_color = False
    reroute_167_1.width = 16.0

    reroute_168_1 = test_group.nodes.new('NodeReroute')
    reroute_168_1.parent = test_group.nodes.get('Frame.012')
    reroute_168_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_168_1.hide = False
    reroute_168_1.label = 'DO NOT TOUCH'
    reroute_168_1.location = (1651.4072265625, 79.050048828125)
    reroute_168_1.mute = False
    reroute_168_1.name = 'Reroute.168'
    reroute_168_1.use_custom_color = False
    reroute_168_1.width = 16.0

    reroute_169_1 = test_group.nodes.new('NodeReroute')
    reroute_169_1.parent = test_group.nodes.get('Frame.012')
    reroute_169_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_169_1.hide = False
    reroute_169_1.label = 'DO NOT TOUCH'
    reroute_169_1.location = (1631.4072265625, 59.050048828125)
    reroute_169_1.mute = False
    reroute_169_1.name = 'Reroute.169'
    reroute_169_1.use_custom_color = False
    reroute_169_1.width = 16.0

    reroute_171_1 = test_group.nodes.new('NodeReroute')
    reroute_171_1.parent = test_group.nodes.get('Frame.012')
    reroute_171_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_171_1.hide = False
    reroute_171_1.label = 'DO NOT TOUCH'
    reroute_171_1.location = (1591.4072265625, 19.050048828125)
    reroute_171_1.mute = False
    reroute_171_1.name = 'Reroute.171'
    reroute_171_1.use_custom_color = False
    reroute_171_1.width = 16.0

    reroute_212_1 = test_group.nodes.new('NodeReroute')
    reroute_212_1.parent = test_group.nodes.get('Frame.012')
    reroute_212_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_212_1.hide = False
    reroute_212_1.label = 'DO NOT TOUCH'
    reroute_212_1.location = (1491.4072265625, -80.949951171875)
    reroute_212_1.mute = False
    reroute_212_1.name = 'Reroute.212'
    reroute_212_1.use_custom_color = False
    reroute_212_1.width = 16.0

    reroute_216_1 = test_group.nodes.new('NodeReroute')
    reroute_216_1.parent = test_group.nodes.get('Frame.012')
    reroute_216_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_216_1.hide = False
    reroute_216_1.label = 'DO NOT TOUCH'
    reroute_216_1.location = (1511.4072265625, -60.949951171875)
    reroute_216_1.mute = False
    reroute_216_1.name = 'Reroute.216'
    reroute_216_1.use_custom_color = False
    reroute_216_1.width = 16.0

    reroute_215_1 = test_group.nodes.new('NodeReroute')
    reroute_215_1.parent = test_group.nodes.get('Frame.012')
    reroute_215_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_215_1.hide = False
    reroute_215_1.label = 'DO NOT TOUCH'
    reroute_215_1.location = (1531.4072265625, -40.949951171875)
    reroute_215_1.mute = False
    reroute_215_1.name = 'Reroute.215'
    reroute_215_1.use_custom_color = False
    reroute_215_1.width = 16.0

    reroute_214_1 = test_group.nodes.new('NodeReroute')
    reroute_214_1.parent = test_group.nodes.get('Frame.012')
    reroute_214_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_214_1.hide = False
    reroute_214_1.label = 'DO NOT TOUCH'
    reroute_214_1.location = (1551.4072265625, -20.949951171875)
    reroute_214_1.mute = False
    reroute_214_1.name = 'Reroute.214'
    reroute_214_1.use_custom_color = False
    reroute_214_1.width = 16.0

    reroute_213_1 = test_group.nodes.new('NodeReroute')
    reroute_213_1.parent = test_group.nodes.get('Frame.012')
    reroute_213_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_213_1.hide = False
    reroute_213_1.label = 'DO NOT TOUCH'
    reroute_213_1.location = (1571.4072265625, -0.949951171875)
    reroute_213_1.mute = False
    reroute_213_1.name = 'Reroute.213'
    reroute_213_1.use_custom_color = False
    reroute_213_1.width = 16.0

    reroute_217_1 = test_group.nodes.new('NodeReroute')
    reroute_217_1.parent = test_group.nodes.get('Frame.012')
    reroute_217_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_217_1.hide = False
    reroute_217_1.label = 'DO NOT TOUCH'
    reroute_217_1.location = (1471.4072265625, -100.949951171875)
    reroute_217_1.mute = False
    reroute_217_1.name = 'Reroute.217'
    reroute_217_1.use_custom_color = False
    reroute_217_1.width = 16.0

    reroute_235_1 = test_group.nodes.new('NodeReroute')
    reroute_235_1.parent = test_group.nodes.get('Frame.012')
    reroute_235_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_235_1.hide = False
    reroute_235_1.label = 'DO NOT TOUCH'
    reroute_235_1.location = (1451.4072265625, -120.949951171875)
    reroute_235_1.mute = False
    reroute_235_1.name = 'Reroute.235'
    reroute_235_1.use_custom_color = False
    reroute_235_1.width = 16.0

    reroute_234_1 = test_group.nodes.new('NodeReroute')
    reroute_234_1.parent = test_group.nodes.get('Frame.012')
    reroute_234_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_234_1.hide = False
    reroute_234_1.label = 'DO NOT TOUCH'
    reroute_234_1.location = (1431.4072265625, -140.949951171875)
    reroute_234_1.mute = False
    reroute_234_1.name = 'Reroute.234'
    reroute_234_1.use_custom_color = False
    reroute_234_1.width = 16.0

    reroute_233_1 = test_group.nodes.new('NodeReroute')
    reroute_233_1.parent = test_group.nodes.get('Frame.012')
    reroute_233_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_233_1.hide = False
    reroute_233_1.label = 'DO NOT TOUCH'
    reroute_233_1.location = (1411.4072265625, -160.949951171875)
    reroute_233_1.mute = False
    reroute_233_1.name = 'Reroute.233'
    reroute_233_1.use_custom_color = False
    reroute_233_1.width = 16.0

    reroute_232_1 = test_group.nodes.new('NodeReroute')
    reroute_232_1.parent = test_group.nodes.get('Frame.012')
    reroute_232_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_232_1.hide = False
    reroute_232_1.label = 'DO NOT TOUCH'
    reroute_232_1.location = (1391.4072265625, -180.949951171875)
    reroute_232_1.mute = False
    reroute_232_1.name = 'Reroute.232'
    reroute_232_1.use_custom_color = False
    reroute_232_1.width = 16.0

    reroute_231_1 = test_group.nodes.new('NodeReroute')
    reroute_231_1.parent = test_group.nodes.get('Frame.012')
    reroute_231_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_231_1.hide = False
    reroute_231_1.label = 'DO NOT TOUCH'
    reroute_231_1.location = (1371.4072265625, -200.949951171875)
    reroute_231_1.mute = False
    reroute_231_1.name = 'Reroute.231'
    reroute_231_1.use_custom_color = False
    reroute_231_1.width = 16.0

    reroute_230_1 = test_group.nodes.new('NodeReroute')
    reroute_230_1.parent = test_group.nodes.get('Frame.012')
    reroute_230_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_230_1.hide = False
    reroute_230_1.label = 'DO NOT TOUCH'
    reroute_230_1.location = (1351.4072265625, -220.949951171875)
    reroute_230_1.mute = False
    reroute_230_1.name = 'Reroute.230'
    reroute_230_1.use_custom_color = False
    reroute_230_1.width = 16.0

    reroute_162_1 = test_group.nodes.new('NodeReroute')
    reroute_162_1.parent = test_group.nodes.get('Frame.012')
    reroute_162_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_162_1.hide = False
    reroute_162_1.label = 'DO NOT TOUCH'
    reroute_162_1.location = (1151.4072265625, 1239.050048828125)
    reroute_162_1.mute = False
    reroute_162_1.name = 'Reroute.162'
    reroute_162_1.use_custom_color = False
    reroute_162_1.width = 16.0

    reroute_158_1 = test_group.nodes.new('NodeReroute')
    reroute_158_1.parent = test_group.nodes.get('Frame.012')
    reroute_158_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_158_1.hide = False
    reroute_158_1.label = 'DO NOT TOUCH'
    reroute_158_1.location = (1091.4072265625, 1359.050048828125)
    reroute_158_1.mute = False
    reroute_158_1.name = 'Reroute.158'
    reroute_158_1.use_custom_color = False
    reroute_158_1.width = 16.0

    reroute_160_1 = test_group.nodes.new('NodeReroute')
    reroute_160_1.parent = test_group.nodes.get('Frame.012')
    reroute_160_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_160_1.hide = False
    reroute_160_1.label = 'DO NOT TOUCH'
    reroute_160_1.location = (1111.4072265625, 1319.050048828125)
    reroute_160_1.mute = False
    reroute_160_1.name = 'Reroute.160'
    reroute_160_1.use_custom_color = False
    reroute_160_1.width = 16.0

    reroute_159_1 = test_group.nodes.new('NodeReroute')
    reroute_159_1.parent = test_group.nodes.get('Frame.012')
    reroute_159_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_159_1.hide = False
    reroute_159_1.label = 'DO NOT TOUCH'
    reroute_159_1.location = (1091.4072265625, 1979.050048828125)
    reroute_159_1.mute = False
    reroute_159_1.name = 'Reroute.159'
    reroute_159_1.use_custom_color = False
    reroute_159_1.width = 16.0

    reroute_165_1 = test_group.nodes.new('NodeReroute')
    reroute_165_1.parent = test_group.nodes.get('Frame.012')
    reroute_165_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_165_1.hide = False
    reroute_165_1.label = 'DO NOT TOUCH'
    reroute_165_1.location = (1151.4072265625, 1859.050048828125)
    reroute_165_1.mute = False
    reroute_165_1.name = 'Reroute.165'
    reroute_165_1.use_custom_color = False
    reroute_165_1.width = 16.0

    reroute_164_1 = test_group.nodes.new('NodeReroute')
    reroute_164_1.parent = test_group.nodes.get('Frame.012')
    reroute_164_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_164_1.hide = False
    reroute_164_1.label = 'DO NOT TOUCH'
    reroute_164_1.location = (1131.4072265625, 1879.050048828125)
    reroute_164_1.mute = False
    reroute_164_1.name = 'Reroute.164'
    reroute_164_1.use_custom_color = False
    reroute_164_1.width = 16.0

    reroute_163_1 = test_group.nodes.new('NodeReroute')
    reroute_163_1.parent = test_group.nodes.get('Frame.012')
    reroute_163_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_163_1.hide = False
    reroute_163_1.label = 'DO NOT TOUCH'
    reroute_163_1.location = (1111.4072265625, 1899.050048828125)
    reroute_163_1.mute = False
    reroute_163_1.name = 'Reroute.163'
    reroute_163_1.use_custom_color = False
    reroute_163_1.width = 16.0

    reroute_161_1 = test_group.nodes.new('NodeReroute')
    reroute_161_1.parent = test_group.nodes.get('Frame.012')
    reroute_161_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_161_1.hide = False
    reroute_161_1.label = 'DO NOT TOUCH'
    reroute_161_1.location = (1131.4072265625, 1279.050048828125)
    reroute_161_1.mute = False
    reroute_161_1.name = 'Reroute.161'
    reroute_161_1.use_custom_color = False
    reroute_161_1.width = 16.0

    reroute_249_1 = test_group.nodes.new('NodeReroute')
    reroute_249_1.parent = test_group.nodes.get('Frame.012')
    reroute_249_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_249_1.hide = False
    reroute_249_1.label = 'DO NOT TOUCH'
    reroute_249_1.location = (1091.4072265625, 739.050048828125)
    reroute_249_1.mute = False
    reroute_249_1.name = 'Reroute.249'
    reroute_249_1.use_custom_color = False
    reroute_249_1.width = 16.0

    reroute_248_1 = test_group.nodes.new('NodeReroute')
    reroute_248_1.parent = test_group.nodes.get('Frame.012')
    reroute_248_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_248_1.hide = False
    reroute_248_1.label = 'DO NOT TOUCH'
    reroute_248_1.location = (1091.4072265625, 119.050048828125)
    reroute_248_1.mute = False
    reroute_248_1.name = 'Reroute.248'
    reroute_248_1.use_custom_color = False
    reroute_248_1.width = 16.0

    reroute_250_1 = test_group.nodes.new('NodeReroute')
    reroute_250_1.parent = test_group.nodes.get('Frame.012')
    reroute_250_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_250_1.hide = False
    reroute_250_1.label = 'DO NOT TOUCH'
    reroute_250_1.location = (1111.4072265625, 79.050048828125)
    reroute_250_1.mute = False
    reroute_250_1.name = 'Reroute.250'
    reroute_250_1.use_custom_color = False
    reroute_250_1.width = 16.0

    reroute_251_1 = test_group.nodes.new('NodeReroute')
    reroute_251_1.parent = test_group.nodes.get('Frame.012')
    reroute_251_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_251_1.hide = False
    reroute_251_1.label = 'DO NOT TOUCH'
    reroute_251_1.location = (1131.4072265625, 39.050048828125)
    reroute_251_1.mute = False
    reroute_251_1.name = 'Reroute.251'
    reroute_251_1.use_custom_color = False
    reroute_251_1.width = 16.0

    reroute_252_1 = test_group.nodes.new('NodeReroute')
    reroute_252_1.parent = test_group.nodes.get('Frame.012')
    reroute_252_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_252_1.hide = False
    reroute_252_1.label = 'DO NOT TOUCH'
    reroute_252_1.location = (1151.4072265625, -0.949951171875)
    reroute_252_1.mute = False
    reroute_252_1.name = 'Reroute.252'
    reroute_252_1.use_custom_color = False
    reroute_252_1.width = 16.0

    reroute_253_1 = test_group.nodes.new('NodeReroute')
    reroute_253_1.parent = test_group.nodes.get('Frame.012')
    reroute_253_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_253_1.hide = False
    reroute_253_1.label = 'DO NOT TOUCH'
    reroute_253_1.location = (1111.4072265625, 659.050048828125)
    reroute_253_1.mute = False
    reroute_253_1.name = 'Reroute.253'
    reroute_253_1.use_custom_color = False
    reroute_253_1.width = 16.0

    reroute_254_1 = test_group.nodes.new('NodeReroute')
    reroute_254_1.parent = test_group.nodes.get('Frame.012')
    reroute_254_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_254_1.hide = False
    reroute_254_1.label = 'DO NOT TOUCH'
    reroute_254_1.location = (1131.4072265625, 639.050048828125)
    reroute_254_1.mute = False
    reroute_254_1.name = 'Reroute.254'
    reroute_254_1.use_custom_color = False
    reroute_254_1.width = 16.0

    reroute_255_1 = test_group.nodes.new('NodeReroute')
    reroute_255_1.parent = test_group.nodes.get('Frame.012')
    reroute_255_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_255_1.hide = False
    reroute_255_1.label = 'DO NOT TOUCH'
    reroute_255_1.location = (1151.4072265625, 619.050048828125)
    reroute_255_1.mute = False
    reroute_255_1.name = 'Reroute.255'
    reroute_255_1.use_custom_color = False
    reroute_255_1.width = 16.0

    reroute_261_1 = test_group.nodes.new('NodeReroute')
    reroute_261_1.parent = test_group.nodes.get('Frame.012')
    reroute_261_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_261_1.hide = False
    reroute_261_1.label = 'DO NOT TOUCH'
    reroute_261_1.location = (1131.4072265625, -600.949951171875)
    reroute_261_1.mute = False
    reroute_261_1.name = 'Reroute.261'
    reroute_261_1.use_custom_color = False
    reroute_261_1.width = 16.0

    reroute_262_1 = test_group.nodes.new('NodeReroute')
    reroute_262_1.parent = test_group.nodes.get('Frame.012')
    reroute_262_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_262_1.hide = False
    reroute_262_1.label = 'DO NOT TOUCH'
    reroute_262_1.location = (1111.4072265625, -580.949951171875)
    reroute_262_1.mute = False
    reroute_262_1.name = 'Reroute.262'
    reroute_262_1.use_custom_color = False
    reroute_262_1.width = 16.0

    reroute_260_1 = test_group.nodes.new('NodeReroute')
    reroute_260_1.parent = test_group.nodes.get('Frame.012')
    reroute_260_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_260_1.hide = False
    reroute_260_1.label = 'DO NOT TOUCH'
    reroute_260_1.location = (1151.4072265625, -620.949951171875)
    reroute_260_1.mute = False
    reroute_260_1.name = 'Reroute.260'
    reroute_260_1.use_custom_color = False
    reroute_260_1.width = 16.0

    reroute_263_1 = test_group.nodes.new('NodeReroute')
    reroute_263_1.parent = test_group.nodes.get('Frame.012')
    reroute_263_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_263_1.hide = False
    reroute_263_1.label = 'DO NOT TOUCH'
    reroute_263_1.location = (1091.4072265625, -500.949951171875)
    reroute_263_1.mute = False
    reroute_263_1.name = 'Reroute.263'
    reroute_263_1.use_custom_color = False
    reroute_263_1.width = 16.0

    reroute_318_1 = test_group.nodes.new('NodeReroute')
    reroute_318_1.parent = test_group.nodes.get('Frame.012')
    reroute_318_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_318_1.hide = False
    reroute_318_1.label = 'DO NOT TOUCH'
    reroute_318_1.location = (1711.4072265625, -4800.9501953125)
    reroute_318_1.mute = False
    reroute_318_1.name = 'Reroute.318'
    reroute_318_1.use_custom_color = False
    reroute_318_1.width = 16.0

    reroute_331_1 = test_group.nodes.new('NodeReroute')
    reroute_331_1.parent = test_group.nodes.get('Frame.012')
    reroute_331_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_331_1.hide = False
    reroute_331_1.label = 'DO NOT TOUCH'
    reroute_331_1.location = (851.4072265625, 1939.050048828125)
    reroute_331_1.mute = False
    reroute_331_1.name = 'Reroute.331'
    reroute_331_1.use_custom_color = False
    reroute_331_1.width = 16.0

    reroute_333_1 = test_group.nodes.new('NodeReroute')
    reroute_333_1.parent = test_group.nodes.get('Frame.012')
    reroute_333_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_333_1.hide = False
    reroute_333_1.label = 'DO NOT TOUCH'
    reroute_333_1.location = (851.4072265625, 499.050048828125)
    reroute_333_1.mute = False
    reroute_333_1.name = 'Reroute.333'
    reroute_333_1.use_custom_color = False
    reroute_333_1.width = 16.0

    reroute_332_1 = test_group.nodes.new('NodeReroute')
    reroute_332_1.parent = test_group.nodes.get('Frame.012')
    reroute_332_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_332_1.hide = False
    reroute_332_1.label = 'DO NOT TOUCH'
    reroute_332_1.location = (711.4072265625, 1819.050048828125)
    reroute_332_1.mute = False
    reroute_332_1.name = 'Reroute.332'
    reroute_332_1.use_custom_color = False
    reroute_332_1.width = 16.0

    reroute_380_1 = test_group.nodes.new('NodeReroute')
    reroute_380_1.parent = test_group.nodes.get('Frame.012')
    reroute_380_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_380_1.hide = False
    reroute_380_1.label = 'DO NOT TOUCH'
    reroute_380_1.location = (831.4072265625, 699.050048828125)
    reroute_380_1.mute = False
    reroute_380_1.name = 'Reroute.380'
    reroute_380_1.use_custom_color = False
    reroute_380_1.width = 16.0

    reroute_375_1 = test_group.nodes.new('NodeReroute')
    reroute_375_1.parent = test_group.nodes.get('Frame.012')
    reroute_375_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_375_1.hide = False
    reroute_375_1.label = 'DO NOT TOUCH'
    reroute_375_1.location = (831.4072265625, 479.050048828125)
    reroute_375_1.mute = False
    reroute_375_1.name = 'Reroute.375'
    reroute_375_1.use_custom_color = False
    reroute_375_1.width = 16.0

    reroute_376_1 = test_group.nodes.new('NodeReroute')
    reroute_376_1.parent = test_group.nodes.get('Frame.012')
    reroute_376_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_376_1.hide = False
    reroute_376_1.label = 'DO NOT TOUCH'
    reroute_376_1.location = (811.4072265625, 459.050048828125)
    reroute_376_1.mute = False
    reroute_376_1.name = 'Reroute.376'
    reroute_376_1.use_custom_color = False
    reroute_376_1.width = 16.0

    reroute_377_1 = test_group.nodes.new('NodeReroute')
    reroute_377_1.parent = test_group.nodes.get('Frame.012')
    reroute_377_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_377_1.hide = False
    reroute_377_1.label = 'DO NOT TOUCH'
    reroute_377_1.location = (791.4072265625, 439.050048828125)
    reroute_377_1.mute = False
    reroute_377_1.name = 'Reroute.377'
    reroute_377_1.use_custom_color = False
    reroute_377_1.width = 16.0

    reroute_378_1 = test_group.nodes.new('NodeReroute')
    reroute_378_1.parent = test_group.nodes.get('Frame.012')
    reroute_378_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_378_1.hide = False
    reroute_378_1.label = 'DO NOT TOUCH'
    reroute_378_1.location = (771.4072265625, 419.050048828125)
    reroute_378_1.mute = False
    reroute_378_1.name = 'Reroute.378'
    reroute_378_1.use_custom_color = False
    reroute_378_1.width = 16.0

    reroute_379_1 = test_group.nodes.new('NodeReroute')
    reroute_379_1.parent = test_group.nodes.get('Frame.012')
    reroute_379_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_379_1.hide = False
    reroute_379_1.label = 'DO NOT TOUCH'
    reroute_379_1.location = (751.4072265625, 399.050048828125)
    reroute_379_1.mute = False
    reroute_379_1.name = 'Reroute.379'
    reroute_379_1.use_custom_color = False
    reroute_379_1.width = 16.0

    reroute_406_1 = test_group.nodes.new('NodeReroute')
    reroute_406_1.parent = test_group.nodes.get('Frame.012')
    reroute_406_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_406_1.hide = False
    reroute_406_1.label = 'DO NOT TOUCH'
    reroute_406_1.location = (811.4072265625, -540.949951171875)
    reroute_406_1.mute = False
    reroute_406_1.name = 'Reroute.406'
    reroute_406_1.use_custom_color = False
    reroute_406_1.width = 16.0

    reroute_407_1 = test_group.nodes.new('NodeReroute')
    reroute_407_1.parent = test_group.nodes.get('Frame.012')
    reroute_407_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_407_1.hide = False
    reroute_407_1.label = 'DO NOT TOUCH'
    reroute_407_1.location = (791.4072265625, -1780.949951171875)
    reroute_407_1.mute = False
    reroute_407_1.name = 'Reroute.407'
    reroute_407_1.use_custom_color = False
    reroute_407_1.width = 16.0

    reroute_408_1 = test_group.nodes.new('NodeReroute')
    reroute_408_1.parent = test_group.nodes.get('Frame.012')
    reroute_408_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_408_1.hide = False
    reroute_408_1.label = 'DO NOT TOUCH'
    reroute_408_1.location = (771.4072265625, -3020.949951171875)
    reroute_408_1.mute = False
    reroute_408_1.name = 'Reroute.408'
    reroute_408_1.use_custom_color = False
    reroute_408_1.width = 16.0

    reroute_409_1 = test_group.nodes.new('NodeReroute')
    reroute_409_1.parent = test_group.nodes.get('Frame.012')
    reroute_409_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_409_1.hide = False
    reroute_409_1.label = 'DO NOT TOUCH'
    reroute_409_1.location = (751.4072265625, -4260.9501953125)
    reroute_409_1.mute = False
    reroute_409_1.name = 'Reroute.409'
    reroute_409_1.use_custom_color = False
    reroute_409_1.width = 16.0

    reroute_240_1 = test_group.nodes.new('NodeReroute')
    reroute_240_1.parent = test_group.nodes.get('Frame.012')
    reroute_240_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_240_1.hide = False
    reroute_240_1.label = 'DO NOT TOUCH'
    reroute_240_1.location = (1431.4072265625, 679.050048828125)
    reroute_240_1.mute = False
    reroute_240_1.name = 'Reroute.240'
    reroute_240_1.use_custom_color = False
    reroute_240_1.width = 16.0

    reroute_220_1 = test_group.nodes.new('NodeReroute')
    reroute_220_1.parent = test_group.nodes.get('Frame.012')
    reroute_220_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_220_1.hide = False
    reroute_220_1.label = 'DO NOT TOUCH'
    reroute_220_1.location = (1551.4072265625, 799.050048828125)
    reroute_220_1.mute = False
    reroute_220_1.name = 'Reroute.220'
    reroute_220_1.use_custom_color = False
    reroute_220_1.width = 16.0

    reroute_173_1 = test_group.nodes.new('NodeReroute')
    reroute_173_1.parent = test_group.nodes.get('Frame.012')
    reroute_173_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_173_1.hide = False
    reroute_173_1.label = 'DO NOT TOUCH'
    reroute_173_1.location = (1671.4072265625, 919.050048828125)
    reroute_173_1.mute = False
    reroute_173_1.name = 'Reroute.173'
    reroute_173_1.use_custom_color = False
    reroute_173_1.width = 16.0

    reroute_420_1 = test_group.nodes.new('NodeReroute')
    reroute_420_1.parent = test_group.nodes.get('Frame.012')
    reroute_420_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_420_1.hide = False
    reroute_420_1.label = 'DO NOT TOUCH'
    reroute_420_1.location = (691.4072265625, 579.050048828125)
    reroute_420_1.mute = False
    reroute_420_1.name = 'Reroute.420'
    reroute_420_1.use_custom_color = False
    reroute_420_1.width = 16.0

    reroute_421_1 = test_group.nodes.new('NodeReroute')
    reroute_421_1.parent = test_group.nodes.get('Frame.012')
    reroute_421_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_421_1.hide = False
    reroute_421_1.label = 'DO NOT TOUCH'
    reroute_421_1.location = (671.4072265625, -660.949951171875)
    reroute_421_1.mute = False
    reroute_421_1.name = 'Reroute.421'
    reroute_421_1.use_custom_color = False
    reroute_421_1.width = 16.0

    reroute_422_1 = test_group.nodes.new('NodeReroute')
    reroute_422_1.parent = test_group.nodes.get('Frame.012')
    reroute_422_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_422_1.hide = False
    reroute_422_1.label = 'DO NOT TOUCH'
    reroute_422_1.location = (651.4072265625, -1900.949951171875)
    reroute_422_1.mute = False
    reroute_422_1.name = 'Reroute.422'
    reroute_422_1.use_custom_color = False
    reroute_422_1.width = 16.0

    reroute_423_1 = test_group.nodes.new('NodeReroute')
    reroute_423_1.parent = test_group.nodes.get('Frame.012')
    reroute_423_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_423_1.hide = False
    reroute_423_1.label = 'DO NOT TOUCH'
    reroute_423_1.location = (631.4072265625, -3140.949951171875)
    reroute_423_1.mute = False
    reroute_423_1.name = 'Reroute.423'
    reroute_423_1.use_custom_color = False
    reroute_423_1.width = 16.0

    reroute_393_1 = test_group.nodes.new('NodeReroute')
    reroute_393_1.parent = test_group.nodes.get('Frame.012')
    reroute_393_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_393_1.hide = False
    reroute_393_1.label = 'DO NOT TOUCH'
    reroute_393_1.location = (411.4072265625, -1400.949951171875)
    reroute_393_1.mute = False
    reroute_393_1.name = 'Reroute.393'
    reroute_393_1.use_custom_color = False
    reroute_393_1.width = 16.0

    reroute_397_1 = test_group.nodes.new('NodeReroute')
    reroute_397_1.parent = test_group.nodes.get('Frame.012')
    reroute_397_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_397_1.hide = False
    reroute_397_1.label = 'DO NOT TOUCH'
    reroute_397_1.location = (411.4072265625, -2640.949951171875)
    reroute_397_1.mute = False
    reroute_397_1.name = 'Reroute.397'
    reroute_397_1.use_custom_color = False
    reroute_397_1.width = 16.0

    reroute_396_1 = test_group.nodes.new('NodeReroute')
    reroute_396_1.parent = test_group.nodes.get('Frame.012')
    reroute_396_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_396_1.hide = False
    reroute_396_1.label = 'DO NOT TOUCH'
    reroute_396_1.location = (411.4072265625, -1960.949951171875)
    reroute_396_1.mute = False
    reroute_396_1.name = 'Reroute.396'
    reroute_396_1.use_custom_color = False
    reroute_396_1.width = 16.0

    reroute_395_1 = test_group.nodes.new('NodeReroute')
    reroute_395_1.parent = test_group.nodes.get('Frame.012')
    reroute_395_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_395_1.hide = False
    reroute_395_1.label = 'DO NOT TOUCH'
    reroute_395_1.location = (391.4072265625, -1940.949951171875)
    reroute_395_1.mute = False
    reroute_395_1.name = 'Reroute.395'
    reroute_395_1.use_custom_color = False
    reroute_395_1.width = 16.0

    reroute_394_1 = test_group.nodes.new('NodeReroute')
    reroute_394_1.parent = test_group.nodes.get('Frame.012')
    reroute_394_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_394_1.hide = False
    reroute_394_1.label = 'DO NOT TOUCH'
    reroute_394_1.location = (391.4072265625, -2060.949951171875)
    reroute_394_1.mute = False
    reroute_394_1.name = 'Reroute.394'
    reroute_394_1.use_custom_color = False
    reroute_394_1.width = 16.0

    reroute_401_1 = test_group.nodes.new('NodeReroute')
    reroute_401_1.parent = test_group.nodes.get('Frame.012')
    reroute_401_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_401_1.hide = False
    reroute_401_1.label = 'DO NOT TOUCH'
    reroute_401_1.location = (411.4072265625, -3880.949951171875)
    reroute_401_1.mute = False
    reroute_401_1.name = 'Reroute.401'
    reroute_401_1.use_custom_color = False
    reroute_401_1.width = 16.0

    reroute_400_1 = test_group.nodes.new('NodeReroute')
    reroute_400_1.parent = test_group.nodes.get('Frame.012')
    reroute_400_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_400_1.hide = False
    reroute_400_1.label = 'DO NOT TOUCH'
    reroute_400_1.location = (411.4072265625, -3200.949951171875)
    reroute_400_1.mute = False
    reroute_400_1.name = 'Reroute.400'
    reroute_400_1.use_custom_color = False
    reroute_400_1.width = 16.0

    reroute_399_1 = test_group.nodes.new('NodeReroute')
    reroute_399_1.parent = test_group.nodes.get('Frame.012')
    reroute_399_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_399_1.hide = False
    reroute_399_1.label = 'DO NOT TOUCH'
    reroute_399_1.location = (391.4072265625, -3180.949951171875)
    reroute_399_1.mute = False
    reroute_399_1.name = 'Reroute.399'
    reroute_399_1.use_custom_color = False
    reroute_399_1.width = 16.0

    reroute_398_1 = test_group.nodes.new('NodeReroute')
    reroute_398_1.parent = test_group.nodes.get('Frame.012')
    reroute_398_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_398_1.hide = False
    reroute_398_1.label = 'DO NOT TOUCH'
    reroute_398_1.location = (391.4072265625, -3300.949951171875)
    reroute_398_1.mute = False
    reroute_398_1.name = 'Reroute.398'
    reroute_398_1.use_custom_color = False
    reroute_398_1.width = 16.0

    reroute_403_1 = test_group.nodes.new('NodeReroute')
    reroute_403_1.parent = test_group.nodes.get('Frame.012')
    reroute_403_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_403_1.hide = False
    reroute_403_1.label = 'DO NOT TOUCH'
    reroute_403_1.location = (391.4072265625, -4420.9501953125)
    reroute_403_1.mute = False
    reroute_403_1.name = 'Reroute.403'
    reroute_403_1.use_custom_color = False
    reroute_403_1.width = 16.0

    reroute_402_1 = test_group.nodes.new('NodeReroute')
    reroute_402_1.parent = test_group.nodes.get('Frame.012')
    reroute_402_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_402_1.hide = False
    reroute_402_1.label = 'DO NOT TOUCH'
    reroute_402_1.location = (391.4072265625, -4540.9501953125)
    reroute_402_1.mute = False
    reroute_402_1.name = 'Reroute.402'
    reroute_402_1.use_custom_color = False
    reroute_402_1.width = 16.0

    reroute_404_1 = test_group.nodes.new('NodeReroute')
    reroute_404_1.parent = test_group.nodes.get('Frame.012')
    reroute_404_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_404_1.hide = False
    reroute_404_1.label = 'DO NOT TOUCH'
    reroute_404_1.location = (411.4072265625, -4440.9501953125)
    reroute_404_1.mute = False
    reroute_404_1.name = 'Reroute.404'
    reroute_404_1.use_custom_color = False
    reroute_404_1.width = 16.0

    reroute_405_1 = test_group.nodes.new('NodeReroute')
    reroute_405_1.parent = test_group.nodes.get('Frame.012')
    reroute_405_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_405_1.hide = False
    reroute_405_1.label = 'DO NOT TOUCH'
    reroute_405_1.location = (411.4072265625, -5120.9501953125)
    reroute_405_1.mute = False
    reroute_405_1.name = 'Reroute.405'
    reroute_405_1.use_custom_color = False
    reroute_405_1.width = 16.0

    reroute_371_1 = test_group.nodes.new('NodeReroute')
    reroute_371_1.parent = test_group.nodes.get('Frame.012')
    reroute_371_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_371_1.hide = False
    reroute_371_1.label = 'DO NOT TOUCH'
    reroute_371_1.location = (411.4072265625, 1079.050048828125)
    reroute_371_1.mute = False
    reroute_371_1.name = 'Reroute.371'
    reroute_371_1.use_custom_color = False
    reroute_371_1.width = 16.0

    reroute_372_1 = test_group.nodes.new('NodeReroute')
    reroute_372_1.parent = test_group.nodes.get('Frame.012')
    reroute_372_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_372_1.hide = False
    reroute_372_1.label = 'DO NOT TOUCH'
    reroute_372_1.location = (391.4072265625, 1659.050048828125)
    reroute_372_1.mute = False
    reroute_372_1.name = 'Reroute.372'
    reroute_372_1.use_custom_color = False
    reroute_372_1.width = 16.0

    reroute_373_1 = test_group.nodes.new('NodeReroute')
    reroute_373_1.parent = test_group.nodes.get('Frame.012')
    reroute_373_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_373_1.hide = False
    reroute_373_1.label = 'DO NOT TOUCH'
    reroute_373_1.location = (411.4072265625, 1759.050048828125)
    reroute_373_1.mute = False
    reroute_373_1.name = 'Reroute.373'
    reroute_373_1.use_custom_color = False
    reroute_373_1.width = 16.0

    reroute_374_1 = test_group.nodes.new('NodeReroute')
    reroute_374_1.parent = test_group.nodes.get('Frame.012')
    reroute_374_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_374_1.hide = False
    reroute_374_1.label = 'DO NOT TOUCH'
    reroute_374_1.location = (391.4072265625, 1779.050048828125)
    reroute_374_1.mute = False
    reroute_374_1.name = 'Reroute.374'
    reroute_374_1.use_custom_color = False
    reroute_374_1.width = 16.0

    reroute_387_1 = test_group.nodes.new('NodeReroute')
    reroute_387_1.parent = test_group.nodes.get('Frame.012')
    reroute_387_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_387_1.hide = False
    reroute_387_1.label = 'DO NOT TOUCH'
    reroute_387_1.location = (391.4072265625, 539.050048828125)
    reroute_387_1.mute = False
    reroute_387_1.name = 'Reroute.387'
    reroute_387_1.use_custom_color = False
    reroute_387_1.width = 16.0

    reroute_386_1 = test_group.nodes.new('NodeReroute')
    reroute_386_1.parent = test_group.nodes.get('Frame.012')
    reroute_386_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_386_1.hide = False
    reroute_386_1.label = 'DO NOT TOUCH'
    reroute_386_1.location = (391.4072265625, 419.050048828125)
    reroute_386_1.mute = False
    reroute_386_1.name = 'Reroute.386'
    reroute_386_1.use_custom_color = False
    reroute_386_1.width = 16.0

    reroute_388_1 = test_group.nodes.new('NodeReroute')
    reroute_388_1.parent = test_group.nodes.get('Frame.012')
    reroute_388_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_388_1.hide = False
    reroute_388_1.label = 'DO NOT TOUCH'
    reroute_388_1.location = (411.4072265625, 519.050048828125)
    reroute_388_1.mute = False
    reroute_388_1.name = 'Reroute.388'
    reroute_388_1.use_custom_color = False
    reroute_388_1.width = 16.0

    reroute_389_1 = test_group.nodes.new('NodeReroute')
    reroute_389_1.parent = test_group.nodes.get('Frame.012')
    reroute_389_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_389_1.hide = False
    reroute_389_1.label = 'DO NOT TOUCH'
    reroute_389_1.location = (411.4072265625, -160.949951171875)
    reroute_389_1.mute = False
    reroute_389_1.name = 'Reroute.389'
    reroute_389_1.use_custom_color = False
    reroute_389_1.width = 16.0

    reroute_392_1 = test_group.nodes.new('NodeReroute')
    reroute_392_1.parent = test_group.nodes.get('Frame.012')
    reroute_392_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_392_1.hide = False
    reroute_392_1.label = 'DO NOT TOUCH'
    reroute_392_1.location = (411.4072265625, -720.949951171875)
    reroute_392_1.mute = False
    reroute_392_1.name = 'Reroute.392'
    reroute_392_1.use_custom_color = False
    reroute_392_1.width = 16.0

    reroute_390_1 = test_group.nodes.new('NodeReroute')
    reroute_390_1.parent = test_group.nodes.get('Frame.012')
    reroute_390_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_390_1.hide = False
    reroute_390_1.label = 'DO NOT TOUCH'
    reroute_390_1.location = (391.4072265625, -820.949951171875)
    reroute_390_1.mute = False
    reroute_390_1.name = 'Reroute.390'
    reroute_390_1.use_custom_color = False
    reroute_390_1.width = 16.0

    reroute_391_1 = test_group.nodes.new('NodeReroute')
    reroute_391_1.parent = test_group.nodes.get('Frame.012')
    reroute_391_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_391_1.hide = False
    reroute_391_1.label = 'DO NOT TOUCH'
    reroute_391_1.location = (391.4072265625, -700.949951171875)
    reroute_391_1.mute = False
    reroute_391_1.name = 'Reroute.391'
    reroute_391_1.use_custom_color = False
    reroute_391_1.width = 16.0

    reroute_424_1 = test_group.nodes.new('NodeReroute')
    reroute_424_1.parent = test_group.nodes.get('Frame.012')
    reroute_424_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_424_1.hide = False
    reroute_424_1.label = 'DO NOT TOUCH'
    reroute_424_1.location = (611.4072265625, -4380.9501953125)
    reroute_424_1.mute = False
    reroute_424_1.name = 'Reroute.424'
    reroute_424_1.use_custom_color = False
    reroute_424_1.width = 16.0

    reroute_425_1 = test_group.nodes.new('NodeReroute')
    reroute_425_1.parent = test_group.nodes.get('Frame.012')
    reroute_425_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_425_1.hide = False
    reroute_425_1.label = 'DO NOT TOUCH'
    reroute_425_1.location = (251.4072265625, 1779.050048828125)
    reroute_425_1.mute = False
    reroute_425_1.name = 'Reroute.425'
    reroute_425_1.use_custom_color = False
    reroute_425_1.width = 16.0

    reroute_442_1 = test_group.nodes.new('NodeReroute')
    reroute_442_1.parent = test_group.nodes.get('Frame.012')
    reroute_442_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_442_1.hide = False
    reroute_442_1.label = 'DO NOT TOUCH'
    reroute_442_1.location = (231.4072265625, 539.050048828125)
    reroute_442_1.mute = False
    reroute_442_1.name = 'Reroute.442'
    reroute_442_1.use_custom_color = False
    reroute_442_1.width = 16.0

    reroute_438_1 = test_group.nodes.new('NodeReroute')
    reroute_438_1.parent = test_group.nodes.get('Frame.012')
    reroute_438_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_438_1.hide = False
    reroute_438_1.label = 'DO NOT TOUCH'
    reroute_438_1.location = (211.4072265625, -700.949951171875)
    reroute_438_1.mute = False
    reroute_438_1.name = 'Reroute.438'
    reroute_438_1.use_custom_color = False
    reroute_438_1.width = 16.0

    reroute_439_1 = test_group.nodes.new('NodeReroute')
    reroute_439_1.parent = test_group.nodes.get('Frame.012')
    reroute_439_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_439_1.hide = False
    reroute_439_1.label = 'DO NOT TOUCH'
    reroute_439_1.location = (191.4072265625, -1940.949951171875)
    reroute_439_1.mute = False
    reroute_439_1.name = 'Reroute.439'
    reroute_439_1.use_custom_color = False
    reroute_439_1.width = 16.0

    reroute_440_1 = test_group.nodes.new('NodeReroute')
    reroute_440_1.parent = test_group.nodes.get('Frame.012')
    reroute_440_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_440_1.hide = False
    reroute_440_1.label = 'DO NOT TOUCH'
    reroute_440_1.location = (171.4072265625, -3180.949951171875)
    reroute_440_1.mute = False
    reroute_440_1.name = 'Reroute.440'
    reroute_440_1.use_custom_color = False
    reroute_440_1.width = 16.0

    reroute_441_1 = test_group.nodes.new('NodeReroute')
    reroute_441_1.parent = test_group.nodes.get('Frame.012')
    reroute_441_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_441_1.hide = False
    reroute_441_1.label = 'DO NOT TOUCH'
    reroute_441_1.location = (151.4072265625, -4420.9501953125)
    reroute_441_1.mute = False
    reroute_441_1.name = 'Reroute.441'
    reroute_441_1.use_custom_color = False
    reroute_441_1.width = 16.0

    reroute_426_1 = test_group.nodes.new('NodeReroute')
    reroute_426_1.parent = test_group.nodes.get('Frame.012')
    reroute_426_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_426_1.hide = False
    reroute_426_1.label = 'DO NOT TOUCH'
    reroute_426_1.location = (251.4072265625, -1273.06005859375)
    reroute_426_1.mute = False
    reroute_426_1.name = 'Reroute.426'
    reroute_426_1.use_custom_color = False
    reroute_426_1.width = 16.0

    reroute_428_1 = test_group.nodes.new('NodeReroute')
    reroute_428_1.parent = test_group.nodes.get('Frame.012')
    reroute_428_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_428_1.hide = False
    reroute_428_1.label = 'DO NOT TOUCH'
    reroute_428_1.location = (211.4072265625, -1313.06005859375)
    reroute_428_1.mute = False
    reroute_428_1.name = 'Reroute.428'
    reroute_428_1.use_custom_color = False
    reroute_428_1.width = 16.0

    reroute_430_1 = test_group.nodes.new('NodeReroute')
    reroute_430_1.parent = test_group.nodes.get('Frame.012')
    reroute_430_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_430_1.hide = False
    reroute_430_1.label = 'DO NOT TOUCH'
    reroute_430_1.location = (171.4072265625, -1353.06005859375)
    reroute_430_1.mute = False
    reroute_430_1.name = 'Reroute.430'
    reroute_430_1.use_custom_color = False
    reroute_430_1.width = 16.0

    reroute_431_1 = test_group.nodes.new('NodeReroute')
    reroute_431_1.parent = test_group.nodes.get('Frame.012')
    reroute_431_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_431_1.hide = False
    reroute_431_1.label = 'DO NOT TOUCH'
    reroute_431_1.location = (151.4072265625, -1373.06005859375)
    reroute_431_1.mute = False
    reroute_431_1.name = 'Reroute.431'
    reroute_431_1.use_custom_color = False
    reroute_431_1.width = 16.0

    reroute_427_1 = test_group.nodes.new('NodeReroute')
    reroute_427_1.parent = test_group.nodes.get('Frame.012')
    reroute_427_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_427_1.hide = False
    reroute_427_1.label = 'DO NOT TOUCH'
    reroute_427_1.location = (231.4072265625, -1293.06005859375)
    reroute_427_1.mute = False
    reroute_427_1.name = 'Reroute.427'
    reroute_427_1.use_custom_color = False
    reroute_427_1.width = 16.0

    reroute_429_1 = test_group.nodes.new('NodeReroute')
    reroute_429_1.parent = test_group.nodes.get('Frame.012')
    reroute_429_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_429_1.hide = False
    reroute_429_1.label = 'DO NOT TOUCH'
    reroute_429_1.location = (191.4072265625, -1333.06005859375)
    reroute_429_1.mute = False
    reroute_429_1.name = 'Reroute.429'
    reroute_429_1.use_custom_color = False
    reroute_429_1.width = 16.0

    reroute_460_1 = test_group.nodes.new('NodeReroute')
    reroute_460_1.parent = test_group.nodes.get('Frame.012')
    reroute_460_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_460_1.hide = False
    reroute_460_1.label = 'DO NOT TOUCH'
    reroute_460_1.location = (-68.5927734375, 2019.050048828125)
    reroute_460_1.mute = False
    reroute_460_1.name = 'Reroute.460'
    reroute_460_1.use_custom_color = False
    reroute_460_1.width = 16.0

    reroute_455_1 = test_group.nodes.new('NodeReroute')
    reroute_455_1.parent = test_group.nodes.get('Frame.012')
    reroute_455_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_455_1.hide = False
    reroute_455_1.label = 'DO NOT TOUCH'
    reroute_455_1.location = (-88.5927734375, 779.050048828125)
    reroute_455_1.mute = False
    reroute_455_1.name = 'Reroute.455'
    reroute_455_1.use_custom_color = False
    reroute_455_1.width = 16.0

    reroute_456_1 = test_group.nodes.new('NodeReroute')
    reroute_456_1.parent = test_group.nodes.get('Frame.012')
    reroute_456_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_456_1.hide = False
    reroute_456_1.label = 'DO NOT TOUCH'
    reroute_456_1.location = (-108.5927734375, -460.949951171875)
    reroute_456_1.mute = False
    reroute_456_1.name = 'Reroute.456'
    reroute_456_1.use_custom_color = False
    reroute_456_1.width = 16.0

    reroute_457_1 = test_group.nodes.new('NodeReroute')
    reroute_457_1.parent = test_group.nodes.get('Frame.012')
    reroute_457_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_457_1.hide = False
    reroute_457_1.label = 'DO NOT TOUCH'
    reroute_457_1.location = (-128.5927734375, -1700.949951171875)
    reroute_457_1.mute = False
    reroute_457_1.name = 'Reroute.457'
    reroute_457_1.use_custom_color = False
    reroute_457_1.width = 16.0

    reroute_458_1 = test_group.nodes.new('NodeReroute')
    reroute_458_1.parent = test_group.nodes.get('Frame.012')
    reroute_458_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_458_1.hide = False
    reroute_458_1.label = 'DO NOT TOUCH'
    reroute_458_1.location = (-148.5927734375, -2940.949951171875)
    reroute_458_1.mute = False
    reroute_458_1.name = 'Reroute.458'
    reroute_458_1.use_custom_color = False
    reroute_458_1.width = 16.0

    reroute_443_1 = test_group.nodes.new('NodeReroute')
    reroute_443_1.parent = test_group.nodes.get('Frame.012')
    reroute_443_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_443_1.hide = False
    reroute_443_1.label = 'DO NOT TOUCH'
    reroute_443_1.location = (-68.5927734375, -840.949951171875)
    reroute_443_1.mute = False
    reroute_443_1.name = 'Reroute.443'
    reroute_443_1.use_custom_color = False
    reroute_443_1.width = 16.0

    reroute_444_1 = test_group.nodes.new('NodeReroute')
    reroute_444_1.parent = test_group.nodes.get('Frame.012')
    reroute_444_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_444_1.hide = False
    reroute_444_1.label = 'DO NOT TOUCH'
    reroute_444_1.location = (-88.5927734375, -860.949951171875)
    reroute_444_1.mute = False
    reroute_444_1.name = 'Reroute.444'
    reroute_444_1.use_custom_color = False
    reroute_444_1.width = 16.0

    reroute_445_1 = test_group.nodes.new('NodeReroute')
    reroute_445_1.parent = test_group.nodes.get('Frame.012')
    reroute_445_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_445_1.hide = False
    reroute_445_1.label = 'DO NOT TOUCH'
    reroute_445_1.location = (-108.5927734375, -880.949951171875)
    reroute_445_1.mute = False
    reroute_445_1.name = 'Reroute.445'
    reroute_445_1.use_custom_color = False
    reroute_445_1.width = 16.0

    reroute_446_1 = test_group.nodes.new('NodeReroute')
    reroute_446_1.parent = test_group.nodes.get('Frame.012')
    reroute_446_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_446_1.hide = False
    reroute_446_1.label = 'DO NOT TOUCH'
    reroute_446_1.location = (-128.5927734375, -900.949951171875)
    reroute_446_1.mute = False
    reroute_446_1.name = 'Reroute.446'
    reroute_446_1.use_custom_color = False
    reroute_446_1.width = 16.0

    reroute_447_1 = test_group.nodes.new('NodeReroute')
    reroute_447_1.parent = test_group.nodes.get('Frame.012')
    reroute_447_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_447_1.hide = False
    reroute_447_1.label = 'DO NOT TOUCH'
    reroute_447_1.location = (-148.5927734375, -920.949951171875)
    reroute_447_1.mute = False
    reroute_447_1.name = 'Reroute.447'
    reroute_447_1.use_custom_color = False
    reroute_447_1.width = 16.0

    reroute_454_1 = test_group.nodes.new('NodeReroute')
    reroute_454_1.parent = test_group.nodes.get('Frame.012')
    reroute_454_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_454_1.hide = False
    reroute_454_1.label = 'DO NOT TOUCH'
    reroute_454_1.location = (-188.5927734375, -960.949951171875)
    reroute_454_1.mute = False
    reroute_454_1.name = 'Reroute.454'
    reroute_454_1.use_custom_color = False
    reroute_454_1.width = 16.0

    reroute_449_1 = test_group.nodes.new('NodeReroute')
    reroute_449_1.parent = test_group.nodes.get('Frame.012')
    reroute_449_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_449_1.hide = False
    reroute_449_1.label = 'DO NOT TOUCH'
    reroute_449_1.location = (-208.5927734375, -980.949951171875)
    reroute_449_1.mute = False
    reroute_449_1.name = 'Reroute.449'
    reroute_449_1.use_custom_color = False
    reroute_449_1.width = 16.0

    reroute_450_1 = test_group.nodes.new('NodeReroute')
    reroute_450_1.parent = test_group.nodes.get('Frame.012')
    reroute_450_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_450_1.hide = False
    reroute_450_1.label = 'DO NOT TOUCH'
    reroute_450_1.location = (-228.5927734375, -1000.949951171875)
    reroute_450_1.mute = False
    reroute_450_1.name = 'Reroute.450'
    reroute_450_1.use_custom_color = False
    reroute_450_1.width = 16.0

    reroute_451_1 = test_group.nodes.new('NodeReroute')
    reroute_451_1.parent = test_group.nodes.get('Frame.012')
    reroute_451_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_451_1.hide = False
    reroute_451_1.label = 'DO NOT TOUCH'
    reroute_451_1.location = (-248.5927734375, -1020.949951171875)
    reroute_451_1.mute = False
    reroute_451_1.name = 'Reroute.451'
    reroute_451_1.use_custom_color = False
    reroute_451_1.width = 16.0

    reroute_452_1 = test_group.nodes.new('NodeReroute')
    reroute_452_1.parent = test_group.nodes.get('Frame.012')
    reroute_452_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_452_1.hide = False
    reroute_452_1.label = 'DO NOT TOUCH'
    reroute_452_1.location = (-268.5927734375, -1040.949951171875)
    reroute_452_1.mute = False
    reroute_452_1.name = 'Reroute.452'
    reroute_452_1.use_custom_color = False
    reroute_452_1.width = 16.0

    reroute_453_1 = test_group.nodes.new('NodeReroute')
    reroute_453_1.parent = test_group.nodes.get('Frame.012')
    reroute_453_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_453_1.hide = False
    reroute_453_1.label = 'DO NOT TOUCH'
    reroute_453_1.location = (-288.5927734375, -1060.949951171875)
    reroute_453_1.mute = False
    reroute_453_1.name = 'Reroute.453'
    reroute_453_1.use_custom_color = False
    reroute_453_1.width = 16.0

    reroute_467_1 = test_group.nodes.new('NodeReroute')
    reroute_467_1.parent = test_group.nodes.get('Frame.012')
    reroute_467_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_467_1.hide = False
    reroute_467_1.label = 'DO NOT TOUCH'
    reroute_467_1.location = (-208.5927734375, 579.050048828125)
    reroute_467_1.mute = False
    reroute_467_1.name = 'Reroute.467'
    reroute_467_1.use_custom_color = False
    reroute_467_1.width = 16.0

    reroute_468_1 = test_group.nodes.new('NodeReroute')
    reroute_468_1.parent = test_group.nodes.get('Frame.012')
    reroute_468_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_468_1.hide = False
    reroute_468_1.label = 'DO NOT TOUCH'
    reroute_468_1.location = (-228.5927734375, -680.949951171875)
    reroute_468_1.mute = False
    reroute_468_1.name = 'Reroute.468'
    reroute_468_1.use_custom_color = False
    reroute_468_1.width = 16.0

    reroute_469_1 = test_group.nodes.new('NodeReroute')
    reroute_469_1.parent = test_group.nodes.get('Frame.012')
    reroute_469_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_469_1.hide = False
    reroute_469_1.label = 'DO NOT TOUCH'
    reroute_469_1.location = (-248.5927734375, -1920.949951171875)
    reroute_469_1.mute = False
    reroute_469_1.name = 'Reroute.469'
    reroute_469_1.use_custom_color = False
    reroute_469_1.width = 16.0

    reroute_470_1 = test_group.nodes.new('NodeReroute')
    reroute_470_1.parent = test_group.nodes.get('Frame.012')
    reroute_470_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_470_1.hide = False
    reroute_470_1.label = 'DO NOT TOUCH'
    reroute_470_1.location = (-268.5927734375, -3160.949951171875)
    reroute_470_1.mute = False
    reroute_470_1.name = 'Reroute.470'
    reroute_470_1.use_custom_color = False
    reroute_470_1.width = 16.0

    reroute_471_1 = test_group.nodes.new('NodeReroute')
    reroute_471_1.parent = test_group.nodes.get('Frame.012')
    reroute_471_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_471_1.hide = False
    reroute_471_1.label = 'DO NOT TOUCH'
    reroute_471_1.location = (-288.5927734375, -4400.9501953125)
    reroute_471_1.mute = False
    reroute_471_1.name = 'Reroute.471'
    reroute_471_1.use_custom_color = False
    reroute_471_1.width = 16.0

    reroute_334_1 = test_group.nodes.new('NodeReroute')
    reroute_334_1.parent = test_group.nodes.get('Frame.012')
    reroute_334_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_334_1.hide = False
    reroute_334_1.label = 'DO NOT TOUCH'
    reroute_334_1.location = (711.4072265625, 359.050048828125)
    reroute_334_1.mute = False
    reroute_334_1.name = 'Reroute.334'
    reroute_334_1.use_custom_color = False
    reroute_334_1.width = 16.0

    reroute_410_1 = test_group.nodes.new('NodeReroute')
    reroute_410_1.parent = test_group.nodes.get('Frame.012')
    reroute_410_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_410_1.hide = False
    reroute_410_1.label = 'DO NOT TOUCH'
    reroute_410_1.location = (691.4072265625, 339.050048828125)
    reroute_410_1.mute = False
    reroute_410_1.name = 'Reroute.410'
    reroute_410_1.use_custom_color = False
    reroute_410_1.width = 16.0

    reroute_411_1 = test_group.nodes.new('NodeReroute')
    reroute_411_1.parent = test_group.nodes.get('Frame.012')
    reroute_411_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_411_1.hide = False
    reroute_411_1.label = 'DO NOT TOUCH'
    reroute_411_1.location = (671.4072265625, 319.050048828125)
    reroute_411_1.mute = False
    reroute_411_1.name = 'Reroute.411'
    reroute_411_1.use_custom_color = False
    reroute_411_1.width = 16.0

    reroute_412_1 = test_group.nodes.new('NodeReroute')
    reroute_412_1.parent = test_group.nodes.get('Frame.012')
    reroute_412_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_412_1.hide = False
    reroute_412_1.label = 'DO NOT TOUCH'
    reroute_412_1.location = (651.4072265625, 299.050048828125)
    reroute_412_1.mute = False
    reroute_412_1.name = 'Reroute.412'
    reroute_412_1.use_custom_color = False
    reroute_412_1.width = 16.0

    reroute_413_1 = test_group.nodes.new('NodeReroute')
    reroute_413_1.parent = test_group.nodes.get('Frame.012')
    reroute_413_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_413_1.hide = False
    reroute_413_1.label = 'DO NOT TOUCH'
    reroute_413_1.location = (631.4072265625, 279.050048828125)
    reroute_413_1.mute = False
    reroute_413_1.name = 'Reroute.413'
    reroute_413_1.use_custom_color = False
    reroute_413_1.width = 16.0

    reroute_414_1 = test_group.nodes.new('NodeReroute')
    reroute_414_1.parent = test_group.nodes.get('Frame.012')
    reroute_414_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_414_1.hide = False
    reroute_414_1.label = 'DO NOT TOUCH'
    reroute_414_1.location = (611.4072265625, 259.050048828125)
    reroute_414_1.mute = False
    reroute_414_1.name = 'Reroute.414'
    reroute_414_1.use_custom_color = False
    reroute_414_1.width = 16.0

    reroute_485_1 = test_group.nodes.new('NodeReroute')
    reroute_485_1.parent = test_group.nodes.get('Frame.012')
    reroute_485_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_485_1.hide = False
    reroute_485_1.label = 'DO NOT TOUCH'
    reroute_485_1.location = (-388.5927734375, 359.050048828125)
    reroute_485_1.mute = False
    reroute_485_1.name = 'Reroute.485'
    reroute_485_1.use_custom_color = False
    reroute_485_1.width = 16.0

    reroute_486_1 = test_group.nodes.new('NodeReroute')
    reroute_486_1.parent = test_group.nodes.get('Frame.012')
    reroute_486_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_486_1.hide = False
    reroute_486_1.label = 'DO NOT TOUCH'
    reroute_486_1.location = (-408.5927734375, -880.949951171875)
    reroute_486_1.mute = False
    reroute_486_1.name = 'Reroute.486'
    reroute_486_1.use_custom_color = False
    reroute_486_1.width = 16.0

    reroute_488_1 = test_group.nodes.new('NodeReroute')
    reroute_488_1.parent = test_group.nodes.get('Frame.012')
    reroute_488_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_488_1.hide = False
    reroute_488_1.label = 'DO NOT TOUCH'
    reroute_488_1.location = (-448.5927734375, -3360.949951171875)
    reroute_488_1.mute = False
    reroute_488_1.name = 'Reroute.488'
    reroute_488_1.use_custom_color = False
    reroute_488_1.width = 16.0

    reroute_489_1 = test_group.nodes.new('NodeReroute')
    reroute_489_1.parent = test_group.nodes.get('Frame.012')
    reroute_489_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_489_1.hide = False
    reroute_489_1.label = 'DO NOT TOUCH'
    reroute_489_1.location = (-468.5927734375, -4600.9501953125)
    reroute_489_1.mute = False
    reroute_489_1.name = 'Reroute.489'
    reroute_489_1.use_custom_color = False
    reroute_489_1.width = 16.0

    reroute_487_1 = test_group.nodes.new('NodeReroute')
    reroute_487_1.parent = test_group.nodes.get('Frame.012')
    reroute_487_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_487_1.hide = False
    reroute_487_1.label = 'DO NOT TOUCH'
    reroute_487_1.location = (-428.5927734375, -2120.949951171875)
    reroute_487_1.mute = False
    reroute_487_1.name = 'Reroute.487'
    reroute_487_1.use_custom_color = False
    reroute_487_1.width = 16.0

    reroute_369_1 = test_group.nodes.new('NodeReroute')
    reroute_369_1.parent = test_group.nodes.get('Frame.012')
    reroute_369_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_369_1.hide = False
    reroute_369_1.label = 'DO NOT TOUCH'
    reroute_369_1.location = (-368.5927734375, -2160.949951171875)
    reroute_369_1.mute = False
    reroute_369_1.name = 'Reroute.369'
    reroute_369_1.use_custom_color = False
    reroute_369_1.width = 16.0

    reroute_480_1 = test_group.nodes.new('NodeReroute')
    reroute_480_1.parent = test_group.nodes.get('Frame.012')
    reroute_480_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_480_1.hide = False
    reroute_480_1.label = 'DO NOT TOUCH'
    reroute_480_1.location = (-388.5927734375, -2180.949951171875)
    reroute_480_1.mute = False
    reroute_480_1.name = 'Reroute.480'
    reroute_480_1.use_custom_color = False
    reroute_480_1.width = 16.0

    reroute_481_1 = test_group.nodes.new('NodeReroute')
    reroute_481_1.parent = test_group.nodes.get('Frame.012')
    reroute_481_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_481_1.hide = False
    reroute_481_1.label = 'DO NOT TOUCH'
    reroute_481_1.location = (-408.5927734375, -2200.949951171875)
    reroute_481_1.mute = False
    reroute_481_1.name = 'Reroute.481'
    reroute_481_1.use_custom_color = False
    reroute_481_1.width = 16.0

    reroute_482_1 = test_group.nodes.new('NodeReroute')
    reroute_482_1.parent = test_group.nodes.get('Frame.012')
    reroute_482_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_482_1.hide = False
    reroute_482_1.label = 'DO NOT TOUCH'
    reroute_482_1.location = (-428.5927734375, -2220.949951171875)
    reroute_482_1.mute = False
    reroute_482_1.name = 'Reroute.482'
    reroute_482_1.use_custom_color = False
    reroute_482_1.width = 16.0

    reroute_483_1 = test_group.nodes.new('NodeReroute')
    reroute_483_1.parent = test_group.nodes.get('Frame.012')
    reroute_483_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_483_1.hide = False
    reroute_483_1.label = 'DO NOT TOUCH'
    reroute_483_1.location = (-448.5927734375, -2240.949951171875)
    reroute_483_1.mute = False
    reroute_483_1.name = 'Reroute.483'
    reroute_483_1.use_custom_color = False
    reroute_483_1.width = 16.0

    reroute_484_1 = test_group.nodes.new('NodeReroute')
    reroute_484_1.parent = test_group.nodes.get('Frame.012')
    reroute_484_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_484_1.hide = False
    reroute_484_1.label = 'DO NOT TOUCH'
    reroute_484_1.location = (-468.5927734375, -2260.949951171875)
    reroute_484_1.mute = False
    reroute_484_1.name = 'Reroute.484'
    reroute_484_1.use_custom_color = False
    reroute_484_1.width = 16.0

    reroute_370_1 = test_group.nodes.new('NodeReroute')
    reroute_370_1.parent = test_group.nodes.get('Frame.012')
    reroute_370_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_370_1.hide = False
    reroute_370_1.label = 'DO NOT TOUCH'
    reroute_370_1.location = (-368.5927734375, 1599.050048828125)
    reroute_370_1.mute = False
    reroute_370_1.name = 'Reroute.370'
    reroute_370_1.use_custom_color = False
    reroute_370_1.width = 16.0

    reroute_500_1 = test_group.nodes.new('NodeReroute')
    reroute_500_1.parent = test_group.nodes.get('Frame.012')
    reroute_500_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_500_1.hide = False
    reroute_500_1.label = 'DO NOT TOUCH'
    reroute_500_1.location = (-708.5927734375, -3760.949951171875)
    reroute_500_1.mute = False
    reroute_500_1.name = 'Reroute.500'
    reroute_500_1.use_custom_color = False
    reroute_500_1.width = 16.0

    reroute_501_1 = test_group.nodes.new('NodeReroute')
    reroute_501_1.parent = test_group.nodes.get('Frame.012')
    reroute_501_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_501_1.hide = False
    reroute_501_1.label = 'DO NOT TOUCH'
    reroute_501_1.location = (-728.5927734375, -3780.949951171875)
    reroute_501_1.mute = False
    reroute_501_1.name = 'Reroute.501'
    reroute_501_1.use_custom_color = False
    reroute_501_1.width = 16.0

    reroute_496_1 = test_group.nodes.new('NodeReroute')
    reroute_496_1.parent = test_group.nodes.get('Frame.012')
    reroute_496_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_496_1.hide = False
    reroute_496_1.label = 'DO NOT TOUCH'
    reroute_496_1.location = (-748.5927734375, -3800.949951171875)
    reroute_496_1.mute = False
    reroute_496_1.name = 'Reroute.496'
    reroute_496_1.use_custom_color = False
    reroute_496_1.width = 16.0

    reroute_497_1 = test_group.nodes.new('NodeReroute')
    reroute_497_1.parent = test_group.nodes.get('Frame.012')
    reroute_497_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_497_1.hide = False
    reroute_497_1.label = 'DO NOT TOUCH'
    reroute_497_1.location = (-768.5927734375, -3820.949951171875)
    reroute_497_1.mute = False
    reroute_497_1.name = 'Reroute.497'
    reroute_497_1.use_custom_color = False
    reroute_497_1.width = 16.0

    reroute_498_1 = test_group.nodes.new('NodeReroute')
    reroute_498_1.parent = test_group.nodes.get('Frame.012')
    reroute_498_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_498_1.hide = False
    reroute_498_1.label = 'DO NOT TOUCH'
    reroute_498_1.location = (-788.5927734375, -3840.949951171875)
    reroute_498_1.mute = False
    reroute_498_1.name = 'Reroute.498'
    reroute_498_1.use_custom_color = False
    reroute_498_1.width = 16.0

    reroute_499_1 = test_group.nodes.new('NodeReroute')
    reroute_499_1.parent = test_group.nodes.get('Frame.012')
    reroute_499_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_499_1.hide = False
    reroute_499_1.label = 'DO NOT TOUCH'
    reroute_499_1.location = (-808.5927734375, -3860.949951171875)
    reroute_499_1.mute = False
    reroute_499_1.name = 'Reroute.499'
    reroute_499_1.use_custom_color = False
    reroute_499_1.width = 16.0

    reroute_503_1 = test_group.nodes.new('NodeReroute')
    reroute_503_1.parent = test_group.nodes.get('Frame.012')
    reroute_503_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_503_1.hide = False
    reroute_503_1.label = 'DO NOT TOUCH'
    reroute_503_1.location = (-708.5927734375, 1179.050048828125)
    reroute_503_1.mute = False
    reroute_503_1.name = 'Reroute.503'
    reroute_503_1.use_custom_color = False
    reroute_503_1.width = 16.0

    reroute_502_1 = test_group.nodes.new('NodeReroute')
    reroute_502_1.parent = test_group.nodes.get('Frame.012')
    reroute_502_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_502_1.hide = False
    reroute_502_1.label = 'DO NOT TOUCH'
    reroute_502_1.location = (-728.5927734375, -60.949951171875)
    reroute_502_1.mute = False
    reroute_502_1.name = 'Reroute.502'
    reroute_502_1.use_custom_color = False
    reroute_502_1.width = 16.0

    reroute_505_1 = test_group.nodes.new('NodeReroute')
    reroute_505_1.parent = test_group.nodes.get('Frame.012')
    reroute_505_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_505_1.hide = False
    reroute_505_1.label = 'DO NOT TOUCH'
    reroute_505_1.location = (-768.5927734375, -2540.949951171875)
    reroute_505_1.mute = False
    reroute_505_1.name = 'Reroute.505'
    reroute_505_1.use_custom_color = False
    reroute_505_1.width = 16.0

    reroute_504_1 = test_group.nodes.new('NodeReroute')
    reroute_504_1.parent = test_group.nodes.get('Frame.012')
    reroute_504_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_504_1.hide = False
    reroute_504_1.label = 'DO NOT TOUCH'
    reroute_504_1.location = (-748.5927734375, -1300.949951171875)
    reroute_504_1.mute = False
    reroute_504_1.name = 'Reroute.504'
    reroute_504_1.use_custom_color = False
    reroute_504_1.width = 16.0

    reroute_506_1 = test_group.nodes.new('NodeReroute')
    reroute_506_1.parent = test_group.nodes.get('Frame.012')
    reroute_506_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_506_1.hide = False
    reroute_506_1.label = 'DO NOT TOUCH'
    reroute_506_1.location = (-788.5927734375, -3780.949951171875)
    reroute_506_1.mute = False
    reroute_506_1.name = 'Reroute.506'
    reroute_506_1.use_custom_color = False
    reroute_506_1.width = 16.0

    reroute_507_1 = test_group.nodes.new('NodeReroute')
    reroute_507_1.parent = test_group.nodes.get('Frame.012')
    reroute_507_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_507_1.hide = False
    reroute_507_1.label = 'DO NOT TOUCH'
    reroute_507_1.location = (-808.5927734375, -5020.9501953125)
    reroute_507_1.mute = False
    reroute_507_1.name = 'Reroute.507'
    reroute_507_1.use_custom_color = False
    reroute_507_1.width = 16.0

    reroute_472_1 = test_group.nodes.new('NodeReroute')
    reroute_472_1.parent = test_group.nodes.get('Frame.012')
    reroute_472_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_472_1.hide = False
    reroute_472_1.label = 'DO NOT TOUCH'
    reroute_472_1.location = (-188.5927734375, 1839.050048828125)
    reroute_472_1.mute = False
    reroute_472_1.name = 'Reroute.472'
    reroute_472_1.use_custom_color = False
    reroute_472_1.width = 16.0

    reroute_495_1 = test_group.nodes.new('NodeReroute')
    reroute_495_1.parent = test_group.nodes.get('Frame.012')
    reroute_495_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_495_1.hide = False
    reroute_495_1.label = 'DO NOT TOUCH'
    reroute_495_1.location = (-1048.5927734375, -5200.9501953125)
    reroute_495_1.mute = False
    reroute_495_1.name = 'Reroute.495'
    reroute_495_1.use_custom_color = False
    reroute_495_1.width = 16.0

    reroute_514_1 = test_group.nodes.new('NodeReroute')
    reroute_514_1.parent = test_group.nodes.get('Frame.012')
    reroute_514_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_514_1.hide = False
    reroute_514_1.label = 'DO NOT TOUCH'
    reroute_514_1.location = (-1068.5927734375, -5220.9501953125)
    reroute_514_1.mute = False
    reroute_514_1.name = 'Reroute.514'
    reroute_514_1.use_custom_color = False
    reroute_514_1.width = 16.0

    reroute_515_1 = test_group.nodes.new('NodeReroute')
    reroute_515_1.parent = test_group.nodes.get('Frame.012')
    reroute_515_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_515_1.hide = False
    reroute_515_1.label = 'DO NOT TOUCH'
    reroute_515_1.location = (-1088.5927734375, -5240.9501953125)
    reroute_515_1.mute = False
    reroute_515_1.name = 'Reroute.515'
    reroute_515_1.use_custom_color = False
    reroute_515_1.width = 16.0

    reroute_516_1 = test_group.nodes.new('NodeReroute')
    reroute_516_1.parent = test_group.nodes.get('Frame.012')
    reroute_516_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_516_1.hide = False
    reroute_516_1.label = 'DO NOT TOUCH'
    reroute_516_1.location = (-1108.5927734375, -5260.9501953125)
    reroute_516_1.mute = False
    reroute_516_1.name = 'Reroute.516'
    reroute_516_1.use_custom_color = False
    reroute_516_1.width = 16.0

    reroute_517_1 = test_group.nodes.new('NodeReroute')
    reroute_517_1.parent = test_group.nodes.get('Frame.012')
    reroute_517_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_517_1.hide = False
    reroute_517_1.label = 'DO NOT TOUCH'
    reroute_517_1.location = (-1128.5927734375, -5280.9501953125)
    reroute_517_1.mute = False
    reroute_517_1.name = 'Reroute.517'
    reroute_517_1.use_custom_color = False
    reroute_517_1.width = 16.0

    reroute_524_1 = test_group.nodes.new('NodeReroute')
    reroute_524_1.parent = test_group.nodes.get('Frame.012')
    reroute_524_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_524_1.hide = False
    reroute_524_1.label = 'DO NOT TOUCH'
    reroute_524_1.location = (-1048.5927734375, 1519.050048828125)
    reroute_524_1.mute = False
    reroute_524_1.name = 'Reroute.524'
    reroute_524_1.use_custom_color = False
    reroute_524_1.width = 16.0

    reroute_519_1 = test_group.nodes.new('NodeReroute')
    reroute_519_1.parent = test_group.nodes.get('Frame.012')
    reroute_519_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_519_1.hide = False
    reroute_519_1.label = 'DO NOT TOUCH'
    reroute_519_1.location = (-1068.5927734375, 259.050048828125)
    reroute_519_1.mute = False
    reroute_519_1.name = 'Reroute.519'
    reroute_519_1.use_custom_color = False
    reroute_519_1.width = 16.0

    reroute_520_1 = test_group.nodes.new('NodeReroute')
    reroute_520_1.parent = test_group.nodes.get('Frame.012')
    reroute_520_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_520_1.hide = False
    reroute_520_1.label = 'DO NOT TOUCH'
    reroute_520_1.location = (-1088.5927734375, -980.949951171875)
    reroute_520_1.mute = False
    reroute_520_1.name = 'Reroute.520'
    reroute_520_1.use_custom_color = False
    reroute_520_1.width = 16.0

    reroute_522_1 = test_group.nodes.new('NodeReroute')
    reroute_522_1.parent = test_group.nodes.get('Frame.012')
    reroute_522_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_522_1.hide = False
    reroute_522_1.label = 'DO NOT TOUCH'
    reroute_522_1.location = (-1128.5927734375, -3460.949951171875)
    reroute_522_1.mute = False
    reroute_522_1.name = 'Reroute.522'
    reroute_522_1.use_custom_color = False
    reroute_522_1.width = 16.0

    reroute_521_1 = test_group.nodes.new('NodeReroute')
    reroute_521_1.parent = test_group.nodes.get('Frame.012')
    reroute_521_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_521_1.hide = False
    reroute_521_1.label = 'DO NOT TOUCH'
    reroute_521_1.location = (-1108.5927734375, -2220.949951171875)
    reroute_521_1.mute = False
    reroute_521_1.name = 'Reroute.521'
    reroute_521_1.use_custom_color = False
    reroute_521_1.width = 16.0

    reroute_448_1 = test_group.nodes.new('NodeReroute')
    reroute_448_1.parent = test_group.nodes.get('Frame.012')
    reroute_448_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_448_1.hide = False
    reroute_448_1.label = 'DO NOT TOUCH'
    reroute_448_1.location = (-168.5927734375, -940.949951171875)
    reroute_448_1.mute = False
    reroute_448_1.name = 'Reroute.448'
    reroute_448_1.use_custom_color = False
    reroute_448_1.width = 16.0

    reroute_459_1 = test_group.nodes.new('NodeReroute')
    reroute_459_1.parent = test_group.nodes.get('Frame.012')
    reroute_459_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    reroute_459_1.hide = False
    reroute_459_1.label = 'DO NOT TOUCH'
    reroute_459_1.location = (-168.5927734375, -4180.9501953125)
    reroute_459_1.mute = False
    reroute_459_1.name = 'Reroute.459'
    reroute_459_1.use_custom_color = False
    reroute_459_1.width = 16.0

    combine_xyz_031_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_031_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_031_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_031_1.hide = False
    combine_xyz_031_1.label = 'DO NOT TOUCH'
    combine_xyz_031_1.location = (-628.5927734375, 1639.050048828125)
    combine_xyz_031_1.mute = False
    combine_xyz_031_1.name = 'Combine XYZ.031'
    combine_xyz_031_1.use_custom_color = False
    combine_xyz_031_1.width = 127.601318359375
    combine_xyz_031_1.inputs[0].default_value = 0.0
    combine_xyz_031_1.inputs[1].default_value = 0.0
    combine_xyz_031_1.inputs[2].default_value = 0.0
    combine_xyz_031_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_030_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_030_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_030_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_030_1.hide = False
    combine_xyz_030_1.label = 'DO NOT TOUCH'
    combine_xyz_030_1.location = (-28.5927734375, 1819.050048828125)
    combine_xyz_030_1.mute = False
    combine_xyz_030_1.name = 'Combine XYZ.030'
    combine_xyz_030_1.use_custom_color = False
    combine_xyz_030_1.width = 125.422119140625
    combine_xyz_030_1.inputs[0].default_value = 0.0
    combine_xyz_030_1.inputs[1].default_value = 0.0
    combine_xyz_030_1.inputs[2].default_value = 0.0
    combine_xyz_030_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_1.hide = False
    combine_xyz_1.label = 'DO NOT TOUCH'
    combine_xyz_1.location = (431.4072265625, 1979.050048828125)
    combine_xyz_1.mute = False
    combine_xyz_1.name = 'Combine XYZ'
    combine_xyz_1.use_custom_color = False
    combine_xyz_1.width = 116.125
    combine_xyz_1.inputs[0].default_value = 0.0
    combine_xyz_1.inputs[1].default_value = 0.0
    combine_xyz_1.inputs[2].default_value = 0.0
    combine_xyz_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_012_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_012_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_012_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_012_1.hide = False
    combine_xyz_012_1.label = 'DO NOT TOUCH'
    combine_xyz_012_1.location = (1171.4072265625, 2199.050048828125)
    combine_xyz_012_1.mute = False
    combine_xyz_012_1.name = 'Combine XYZ.012'
    combine_xyz_012_1.use_custom_color = False
    combine_xyz_012_1.width = 123.68994140625
    combine_xyz_012_1.inputs[0].default_value = -1.6666669845581055
    combine_xyz_012_1.inputs[1].default_value = 5.2083330154418945
    combine_xyz_012_1.inputs[2].default_value = 0.6499999761581421
    combine_xyz_012_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_013_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_013_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_013_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_013_1.hide = False
    combine_xyz_013_1.label = 'DO NOT TOUCH'
    combine_xyz_013_1.location = (1171.4072265625, 2079.050048828125)
    combine_xyz_013_1.mute = False
    combine_xyz_013_1.name = 'Combine XYZ.013'
    combine_xyz_013_1.use_custom_color = False
    combine_xyz_013_1.width = 123.616455078125
    combine_xyz_013_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_013_1.inputs[1].default_value = 0.0
    combine_xyz_013_1.inputs[2].default_value = 0.0
    combine_xyz_013_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_020_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_020_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_020_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_020_1.hide = False
    combine_xyz_020_1.label = 'DO NOT TOUCH'
    combine_xyz_020_1.location = (1171.4072265625, 1959.050048828125)
    combine_xyz_020_1.mute = False
    combine_xyz_020_1.name = 'Combine XYZ.020'
    combine_xyz_020_1.use_custom_color = False
    combine_xyz_020_1.width = 124.859375
    combine_xyz_020_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_020_1.inputs[1].default_value = 0.0
    combine_xyz_020_1.inputs[2].default_value = 0.0
    combine_xyz_020_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_001_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_001_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_001_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_001_1.hide = False
    combine_xyz_001_1.label = 'DO NOT TOUCH'
    combine_xyz_001_1.location = (431.4072265625, 1859.050048828125)
    combine_xyz_001_1.mute = False
    combine_xyz_001_1.name = 'Combine XYZ.001'
    combine_xyz_001_1.use_custom_color = False
    combine_xyz_001_1.width = 117.023681640625
    combine_xyz_001_1.inputs[0].default_value = 0.0
    combine_xyz_001_1.inputs[1].default_value = 0.0
    combine_xyz_001_1.inputs[2].default_value = 0.0
    combine_xyz_001_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_032_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_032_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_032_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_032_1.hide = False
    combine_xyz_032_1.label = 'DO NOT TOUCH'
    combine_xyz_032_1.location = (-968.5927734375, 1219.050048828125)
    combine_xyz_032_1.mute = False
    combine_xyz_032_1.name = 'Combine XYZ.032'
    combine_xyz_032_1.use_custom_color = False
    combine_xyz_032_1.width = 125.2945556640625
    combine_xyz_032_1.inputs[0].default_value = 0.0
    combine_xyz_032_1.inputs[1].default_value = 0.0
    combine_xyz_032_1.inputs[2].default_value = 0.0
    combine_xyz_032_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_014_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_014_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_014_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_014_1.hide = False
    combine_xyz_014_1.label = 'DO NOT TOUCH'
    combine_xyz_014_1.location = (1171.4072265625, 959.050048828125)
    combine_xyz_014_1.mute = False
    combine_xyz_014_1.name = 'Combine XYZ.014'
    combine_xyz_014_1.use_custom_color = False
    combine_xyz_014_1.width = 121.84521484375
    combine_xyz_014_1.inputs[0].default_value = -1.6666669845581055
    combine_xyz_014_1.inputs[1].default_value = 5.2083330154418945
    combine_xyz_014_1.inputs[2].default_value = 0.6499999761581421
    combine_xyz_014_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_015_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_015_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_015_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_015_1.hide = False
    combine_xyz_015_1.label = 'DO NOT TOUCH'
    combine_xyz_015_1.location = (1171.4072265625, 839.050048828125)
    combine_xyz_015_1.mute = False
    combine_xyz_015_1.name = 'Combine XYZ.015'
    combine_xyz_015_1.use_custom_color = False
    combine_xyz_015_1.width = 121.845458984375
    combine_xyz_015_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_015_1.inputs[1].default_value = 0.0
    combine_xyz_015_1.inputs[2].default_value = 0.0
    combine_xyz_015_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_021_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_021_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_021_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_021_1.hide = False
    combine_xyz_021_1.label = 'DO NOT TOUCH'
    combine_xyz_021_1.location = (1171.4072265625, 719.050048828125)
    combine_xyz_021_1.mute = False
    combine_xyz_021_1.name = 'Combine XYZ.021'
    combine_xyz_021_1.use_custom_color = False
    combine_xyz_021_1.width = 121.8447265625
    combine_xyz_021_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_021_1.inputs[1].default_value = 0.0
    combine_xyz_021_1.inputs[2].default_value = 0.0
    combine_xyz_021_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_002_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_002_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_002_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_002_1.hide = False
    combine_xyz_002_1.label = 'DO NOT TOUCH'
    combine_xyz_002_1.location = (431.4072265625, 739.050048828125)
    combine_xyz_002_1.mute = False
    combine_xyz_002_1.name = 'Combine XYZ.002'
    combine_xyz_002_1.use_custom_color = False
    combine_xyz_002_1.width = 124.23828125
    combine_xyz_002_1.inputs[0].default_value = 0.0
    combine_xyz_002_1.inputs[1].default_value = 0.0
    combine_xyz_002_1.inputs[2].default_value = 0.0
    combine_xyz_002_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_003_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_003_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_003_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_003_1.hide = False
    combine_xyz_003_1.label = 'DO NOT TOUCH'
    combine_xyz_003_1.location = (431.4072265625, 619.050048828125)
    combine_xyz_003_1.mute = False
    combine_xyz_003_1.name = 'Combine XYZ.003'
    combine_xyz_003_1.use_custom_color = False
    combine_xyz_003_1.width = 124.238037109375
    combine_xyz_003_1.inputs[0].default_value = 0.0
    combine_xyz_003_1.inputs[1].default_value = 0.0
    combine_xyz_003_1.inputs[2].default_value = 0.0
    combine_xyz_003_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_033_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_033_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_033_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_033_1.hide = False
    combine_xyz_033_1.label = 'DO NOT TOUCH'
    combine_xyz_033_1.location = (-28.5927734375, 579.050048828125)
    combine_xyz_033_1.mute = False
    combine_xyz_033_1.name = 'Combine XYZ.033'
    combine_xyz_033_1.use_custom_color = False
    combine_xyz_033_1.width = 117.468994140625
    combine_xyz_033_1.inputs[0].default_value = 0.0
    combine_xyz_033_1.inputs[1].default_value = 0.0
    combine_xyz_033_1.inputs[2].default_value = 0.0
    combine_xyz_033_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_038_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_038_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_038_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_038_1.hide = False
    combine_xyz_038_1.label = 'DO NOT TOUCH'
    combine_xyz_038_1.location = (-628.5927734375, 399.050048828125)
    combine_xyz_038_1.mute = False
    combine_xyz_038_1.name = 'Combine XYZ.038'
    combine_xyz_038_1.use_custom_color = False
    combine_xyz_038_1.width = 122.51708984375
    combine_xyz_038_1.inputs[0].default_value = 0.0
    combine_xyz_038_1.inputs[1].default_value = 0.0
    combine_xyz_038_1.inputs[2].default_value = 0.0
    combine_xyz_038_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_043_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_043_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_043_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_043_1.hide = False
    combine_xyz_043_1.label = 'DO NOT TOUCH'
    combine_xyz_043_1.location = (-968.5927734375, -20.949951171875)
    combine_xyz_043_1.mute = False
    combine_xyz_043_1.name = 'Combine XYZ.043'
    combine_xyz_043_1.use_custom_color = False
    combine_xyz_043_1.width = 127.3455810546875
    combine_xyz_043_1.inputs[0].default_value = 0.0
    combine_xyz_043_1.inputs[1].default_value = 0.0
    combine_xyz_043_1.inputs[2].default_value = 0.0
    combine_xyz_043_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_016_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_016_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_016_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_016_1.hide = False
    combine_xyz_016_1.label = 'DO NOT TOUCH'
    combine_xyz_016_1.location = (1171.4072265625, -280.949951171875)
    combine_xyz_016_1.mute = False
    combine_xyz_016_1.name = 'Combine XYZ.016'
    combine_xyz_016_1.use_custom_color = False
    combine_xyz_016_1.width = 127.345703125
    combine_xyz_016_1.inputs[0].default_value = -1.6666669845581055
    combine_xyz_016_1.inputs[1].default_value = 5.2083330154418945
    combine_xyz_016_1.inputs[2].default_value = 0.6499999761581421
    combine_xyz_016_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_017_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_017_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_017_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_017_1.hide = False
    combine_xyz_017_1.label = 'DO NOT TOUCH'
    combine_xyz_017_1.location = (1171.4072265625, -400.949951171875)
    combine_xyz_017_1.mute = False
    combine_xyz_017_1.name = 'Combine XYZ.017'
    combine_xyz_017_1.use_custom_color = False
    combine_xyz_017_1.width = 127.967529296875
    combine_xyz_017_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_017_1.inputs[1].default_value = 0.0
    combine_xyz_017_1.inputs[2].default_value = 0.0
    combine_xyz_017_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_022_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_022_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_022_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_022_1.hide = False
    combine_xyz_022_1.label = 'DO NOT TOUCH'
    combine_xyz_022_1.location = (1171.4072265625, -520.949951171875)
    combine_xyz_022_1.mute = False
    combine_xyz_022_1.name = 'Combine XYZ.022'
    combine_xyz_022_1.use_custom_color = False
    combine_xyz_022_1.width = 128.5888671875
    combine_xyz_022_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_022_1.inputs[1].default_value = 0.0
    combine_xyz_022_1.inputs[2].default_value = 0.0
    combine_xyz_022_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_005_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_005_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_005_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_005_1.hide = False
    combine_xyz_005_1.label = 'DO NOT TOUCH'
    combine_xyz_005_1.location = (431.4072265625, -500.949951171875)
    combine_xyz_005_1.mute = False
    combine_xyz_005_1.name = 'Combine XYZ.005'
    combine_xyz_005_1.use_custom_color = False
    combine_xyz_005_1.width = 125.53173828125
    combine_xyz_005_1.inputs[0].default_value = 0.0
    combine_xyz_005_1.inputs[1].default_value = 0.0
    combine_xyz_005_1.inputs[2].default_value = 0.0
    combine_xyz_005_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_004_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_004_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_004_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_004_1.hide = False
    combine_xyz_004_1.label = 'DO NOT TOUCH'
    combine_xyz_004_1.location = (431.4072265625, -620.949951171875)
    combine_xyz_004_1.mute = False
    combine_xyz_004_1.name = 'Combine XYZ.004'
    combine_xyz_004_1.use_custom_color = False
    combine_xyz_004_1.width = 124.860107421875
    combine_xyz_004_1.inputs[0].default_value = 0.0
    combine_xyz_004_1.inputs[1].default_value = 0.0
    combine_xyz_004_1.inputs[2].default_value = 0.0
    combine_xyz_004_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_034_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_034_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_034_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_034_1.hide = False
    combine_xyz_034_1.label = 'DO NOT TOUCH'
    combine_xyz_034_1.location = (-28.5927734375, -660.949951171875)
    combine_xyz_034_1.mute = False
    combine_xyz_034_1.name = 'Combine XYZ.034'
    combine_xyz_034_1.use_custom_color = False
    combine_xyz_034_1.width = 124.85986328125
    combine_xyz_034_1.inputs[0].default_value = 0.0
    combine_xyz_034_1.inputs[1].default_value = 0.0
    combine_xyz_034_1.inputs[2].default_value = 0.0
    combine_xyz_034_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_039_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_039_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_039_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_039_1.hide = False
    combine_xyz_039_1.label = 'DO NOT TOUCH'
    combine_xyz_039_1.location = (-628.5927734375, -840.949951171875)
    combine_xyz_039_1.mute = False
    combine_xyz_039_1.name = 'Combine XYZ.039'
    combine_xyz_039_1.use_custom_color = False
    combine_xyz_039_1.width = 124.696533203125
    combine_xyz_039_1.inputs[0].default_value = 0.0
    combine_xyz_039_1.inputs[1].default_value = 0.0
    combine_xyz_039_1.inputs[2].default_value = 0.0
    combine_xyz_039_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_044_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_044_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_044_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_044_1.hide = False
    combine_xyz_044_1.label = 'DO NOT TOUCH'
    combine_xyz_044_1.location = (-968.5927734375, -1260.949951171875)
    combine_xyz_044_1.mute = False
    combine_xyz_044_1.name = 'Combine XYZ.044'
    combine_xyz_044_1.use_custom_color = False
    combine_xyz_044_1.width = 121.9874267578125
    combine_xyz_044_1.inputs[0].default_value = 0.0
    combine_xyz_044_1.inputs[1].default_value = 0.0
    combine_xyz_044_1.inputs[2].default_value = 0.0
    combine_xyz_044_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_018_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_018_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_018_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_018_1.hide = False
    combine_xyz_018_1.label = 'DO NOT TOUCH'
    combine_xyz_018_1.location = (1171.4072265625, -1500.949951171875)
    combine_xyz_018_1.mute = False
    combine_xyz_018_1.name = 'Combine XYZ.018'
    combine_xyz_018_1.use_custom_color = False
    combine_xyz_018_1.width = 135.3369140625
    combine_xyz_018_1.inputs[0].default_value = -1.6666669845581055
    combine_xyz_018_1.inputs[1].default_value = 5.2083330154418945
    combine_xyz_018_1.inputs[2].default_value = 0.6499999761581421
    combine_xyz_018_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_019_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_019_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_019_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_019_1.hide = False
    combine_xyz_019_1.label = 'DO NOT TOUCH'
    combine_xyz_019_1.location = (1171.4072265625, -1620.949951171875)
    combine_xyz_019_1.mute = False
    combine_xyz_019_1.name = 'Combine XYZ.019'
    combine_xyz_019_1.use_custom_color = False
    combine_xyz_019_1.width = 134.5517578125
    combine_xyz_019_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_019_1.inputs[1].default_value = 0.0
    combine_xyz_019_1.inputs[2].default_value = 0.0
    combine_xyz_019_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_023_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_023_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_023_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_023_1.hide = False
    combine_xyz_023_1.label = 'DO NOT TOUCH'
    combine_xyz_023_1.location = (1171.4072265625, -1740.949951171875)
    combine_xyz_023_1.mute = False
    combine_xyz_023_1.name = 'Combine XYZ.023'
    combine_xyz_023_1.use_custom_color = False
    combine_xyz_023_1.width = 132.196044921875
    combine_xyz_023_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_023_1.inputs[1].default_value = 0.0
    combine_xyz_023_1.inputs[2].default_value = 0.0
    combine_xyz_023_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_007_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_007_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_007_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_007_1.hide = False
    combine_xyz_007_1.label = 'DO NOT TOUCH'
    combine_xyz_007_1.location = (431.4072265625, -1740.949951171875)
    combine_xyz_007_1.mute = False
    combine_xyz_007_1.name = 'Combine XYZ.007'
    combine_xyz_007_1.use_custom_color = False
    combine_xyz_007_1.width = 123.516357421875
    combine_xyz_007_1.inputs[0].default_value = 0.0
    combine_xyz_007_1.inputs[1].default_value = 0.0
    combine_xyz_007_1.inputs[2].default_value = 0.0
    combine_xyz_007_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_006_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_006_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_006_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_006_1.hide = False
    combine_xyz_006_1.label = 'DO NOT TOUCH'
    combine_xyz_006_1.location = (431.4072265625, -1860.949951171875)
    combine_xyz_006_1.mute = False
    combine_xyz_006_1.name = 'Combine XYZ.006'
    combine_xyz_006_1.use_custom_color = False
    combine_xyz_006_1.width = 124.860107421875
    combine_xyz_006_1.inputs[0].default_value = 0.0
    combine_xyz_006_1.inputs[1].default_value = 0.0
    combine_xyz_006_1.inputs[2].default_value = 0.0
    combine_xyz_006_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_035_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_035_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_035_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_035_1.hide = False
    combine_xyz_035_1.label = 'DO NOT TOUCH'
    combine_xyz_035_1.location = (-48.5927734375, -1900.949951171875)
    combine_xyz_035_1.mute = False
    combine_xyz_035_1.name = 'Combine XYZ.035'
    combine_xyz_035_1.use_custom_color = False
    combine_xyz_035_1.width = 122.844482421875
    combine_xyz_035_1.inputs[0].default_value = 0.0
    combine_xyz_035_1.inputs[1].default_value = 0.0
    combine_xyz_035_1.inputs[2].default_value = 0.0
    combine_xyz_035_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_040_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_040_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_040_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_040_1.hide = False
    combine_xyz_040_1.label = 'DO NOT TOUCH'
    combine_xyz_040_1.location = (-628.5927734375, -2080.949951171875)
    combine_xyz_040_1.mute = False
    combine_xyz_040_1.name = 'Combine XYZ.040'
    combine_xyz_040_1.use_custom_color = False
    combine_xyz_040_1.width = 129.021484375
    combine_xyz_040_1.inputs[0].default_value = 0.0
    combine_xyz_040_1.inputs[1].default_value = 0.0
    combine_xyz_040_1.inputs[2].default_value = 0.0
    combine_xyz_040_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_045_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_045_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_045_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_045_1.hide = False
    combine_xyz_045_1.label = 'DO NOT TOUCH'
    combine_xyz_045_1.location = (-968.5927734375, -2500.949951171875)
    combine_xyz_045_1.mute = False
    combine_xyz_045_1.name = 'Combine XYZ.045'
    combine_xyz_045_1.use_custom_color = False
    combine_xyz_045_1.width = 118.6451416015625
    combine_xyz_045_1.inputs[0].default_value = 0.0
    combine_xyz_045_1.inputs[1].default_value = 0.0
    combine_xyz_045_1.inputs[2].default_value = 0.0
    combine_xyz_045_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_024_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_024_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_024_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_024_1.hide = False
    combine_xyz_024_1.label = 'DO NOT TOUCH'
    combine_xyz_024_1.location = (1171.4072265625, -2760.949951171875)
    combine_xyz_024_1.mute = False
    combine_xyz_024_1.name = 'Combine XYZ.024'
    combine_xyz_024_1.use_custom_color = False
    combine_xyz_024_1.width = 131.23486328125
    combine_xyz_024_1.inputs[0].default_value = -1.6666669845581055
    combine_xyz_024_1.inputs[1].default_value = 5.2083330154418945
    combine_xyz_024_1.inputs[2].default_value = 0.6499999761581421
    combine_xyz_024_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_025_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_025_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_025_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_025_1.hide = False
    combine_xyz_025_1.label = 'DO NOT TOUCH'
    combine_xyz_025_1.location = (1171.4072265625, -2880.949951171875)
    combine_xyz_025_1.mute = False
    combine_xyz_025_1.name = 'Combine XYZ.025'
    combine_xyz_025_1.use_custom_color = False
    combine_xyz_025_1.width = 129.781982421875
    combine_xyz_025_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_025_1.inputs[1].default_value = 0.0
    combine_xyz_025_1.inputs[2].default_value = 0.0
    combine_xyz_025_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_026_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_026_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_026_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_026_1.hide = False
    combine_xyz_026_1.label = 'DO NOT TOUCH'
    combine_xyz_026_1.location = (1171.4072265625, -3000.949951171875)
    combine_xyz_026_1.mute = False
    combine_xyz_026_1.name = 'Combine XYZ.026'
    combine_xyz_026_1.use_custom_color = False
    combine_xyz_026_1.width = 127.602783203125
    combine_xyz_026_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_026_1.inputs[1].default_value = 0.0
    combine_xyz_026_1.inputs[2].default_value = 0.0
    combine_xyz_026_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_008_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_008_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_008_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_008_1.hide = False
    combine_xyz_008_1.label = 'DO NOT TOUCH'
    combine_xyz_008_1.location = (431.4072265625, -2980.949951171875)
    combine_xyz_008_1.mute = False
    combine_xyz_008_1.name = 'Combine XYZ.008'
    combine_xyz_008_1.use_custom_color = False
    combine_xyz_008_1.width = 127.346923828125
    combine_xyz_008_1.inputs[0].default_value = 0.0
    combine_xyz_008_1.inputs[1].default_value = 0.0
    combine_xyz_008_1.inputs[2].default_value = 0.0
    combine_xyz_008_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_009_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_009_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_009_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_009_1.hide = False
    combine_xyz_009_1.label = 'DO NOT TOUCH'
    combine_xyz_009_1.location = (431.4072265625, -3100.949951171875)
    combine_xyz_009_1.mute = False
    combine_xyz_009_1.name = 'Combine XYZ.009'
    combine_xyz_009_1.use_custom_color = False
    combine_xyz_009_1.width = 126.103759765625
    combine_xyz_009_1.inputs[0].default_value = 0.0
    combine_xyz_009_1.inputs[1].default_value = 0.0
    combine_xyz_009_1.inputs[2].default_value = 0.0
    combine_xyz_009_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_036_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_036_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_036_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_036_1.hide = False
    combine_xyz_036_1.label = 'DO NOT TOUCH'
    combine_xyz_036_1.location = (-68.5927734375, -3140.949951171875)
    combine_xyz_036_1.mute = False
    combine_xyz_036_1.name = 'Combine XYZ.036'
    combine_xyz_036_1.use_custom_color = False
    combine_xyz_036_1.width = 123.617919921875
    combine_xyz_036_1.inputs[0].default_value = 0.0
    combine_xyz_036_1.inputs[1].default_value = 0.0
    combine_xyz_036_1.inputs[2].default_value = 0.0
    combine_xyz_036_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_041_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_041_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_041_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_041_1.hide = False
    combine_xyz_041_1.label = 'DO NOT TOUCH'
    combine_xyz_041_1.location = (-628.5927734375, -3320.949951171875)
    combine_xyz_041_1.mute = False
    combine_xyz_041_1.name = 'Combine XYZ.041'
    combine_xyz_041_1.use_custom_color = False
    combine_xyz_041_1.width = 119.526123046875
    combine_xyz_041_1.inputs[0].default_value = 0.0
    combine_xyz_041_1.inputs[1].default_value = 0.0
    combine_xyz_041_1.inputs[2].default_value = 0.0
    combine_xyz_041_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_046_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_046_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_046_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_046_1.hide = False
    combine_xyz_046_1.label = 'DO NOT TOUCH'
    combine_xyz_046_1.location = (-968.5927734375, -3740.949951171875)
    combine_xyz_046_1.mute = False
    combine_xyz_046_1.name = 'Combine XYZ.046'
    combine_xyz_046_1.use_custom_color = False
    combine_xyz_046_1.width = 120.1575927734375
    combine_xyz_046_1.inputs[0].default_value = 0.0
    combine_xyz_046_1.inputs[1].default_value = 0.0
    combine_xyz_046_1.inputs[2].default_value = 0.0
    combine_xyz_046_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_028_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_028_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_028_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_028_1.hide = False
    combine_xyz_028_1.label = 'DO NOT TOUCH'
    combine_xyz_028_1.location = (1171.4072265625, -4000.949951171875)
    combine_xyz_028_1.mute = False
    combine_xyz_028_1.name = 'Combine XYZ.028'
    combine_xyz_028_1.use_custom_color = False
    combine_xyz_028_1.width = 132.688232421875
    combine_xyz_028_1.inputs[0].default_value = -1.6666669845581055
    combine_xyz_028_1.inputs[1].default_value = 5.2083330154418945
    combine_xyz_028_1.inputs[2].default_value = 0.6499999761581421
    combine_xyz_028_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_029_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_029_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_029_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_029_1.hide = False
    combine_xyz_029_1.label = 'DO NOT TOUCH'
    combine_xyz_029_1.location = (1171.4072265625, -4120.9501953125)
    combine_xyz_029_1.mute = False
    combine_xyz_029_1.name = 'Combine XYZ.029'
    combine_xyz_029_1.use_custom_color = False
    combine_xyz_029_1.width = 132.68798828125
    combine_xyz_029_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_029_1.inputs[1].default_value = 0.0
    combine_xyz_029_1.inputs[2].default_value = 0.0
    combine_xyz_029_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_027_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_027_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_027_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_027_1.hide = False
    combine_xyz_027_1.label = 'DO NOT TOUCH'
    combine_xyz_027_1.location = (1171.4072265625, -4240.9501953125)
    combine_xyz_027_1.mute = False
    combine_xyz_027_1.name = 'Combine XYZ.027'
    combine_xyz_027_1.use_custom_color = False
    combine_xyz_027_1.width = 133.41455078125
    combine_xyz_027_1.inputs[0].default_value = 0.23999999463558197
    combine_xyz_027_1.inputs[1].default_value = 0.0
    combine_xyz_027_1.inputs[2].default_value = 0.0
    combine_xyz_027_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_010_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_010_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_010_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_010_1.hide = False
    combine_xyz_010_1.label = 'DO NOT TOUCH'
    combine_xyz_010_1.location = (431.4072265625, -4220.9501953125)
    combine_xyz_010_1.mute = False
    combine_xyz_010_1.name = 'Combine XYZ.010'
    combine_xyz_010_1.use_custom_color = False
    combine_xyz_010_1.width = 125.42431640625
    combine_xyz_010_1.inputs[0].default_value = 0.0
    combine_xyz_010_1.inputs[1].default_value = 0.0
    combine_xyz_010_1.inputs[2].default_value = 0.0
    combine_xyz_010_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_011_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_011_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_011_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_011_1.hide = False
    combine_xyz_011_1.label = 'DO NOT TOUCH'
    combine_xyz_011_1.location = (431.4072265625, -4340.9501953125)
    combine_xyz_011_1.mute = False
    combine_xyz_011_1.name = 'Combine XYZ.011'
    combine_xyz_011_1.use_custom_color = False
    combine_xyz_011_1.width = 124.69775390625
    combine_xyz_011_1.inputs[0].default_value = 0.0
    combine_xyz_011_1.inputs[1].default_value = 0.0
    combine_xyz_011_1.inputs[2].default_value = 0.0
    combine_xyz_011_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_037_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_037_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_037_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_037_1.hide = False
    combine_xyz_037_1.label = 'DO NOT TOUCH'
    combine_xyz_037_1.location = (-68.5927734375, -4380.9501953125)
    combine_xyz_037_1.mute = False
    combine_xyz_037_1.name = 'Combine XYZ.037'
    combine_xyz_037_1.use_custom_color = False
    combine_xyz_037_1.width = 126.725830078125
    combine_xyz_037_1.inputs[0].default_value = 0.0
    combine_xyz_037_1.inputs[1].default_value = 0.0
    combine_xyz_037_1.inputs[2].default_value = 0.0
    combine_xyz_037_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_042_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_042_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_042_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_042_1.hide = False
    combine_xyz_042_1.label = 'DO NOT TOUCH'
    combine_xyz_042_1.location = (-628.5927734375, -4560.9501953125)
    combine_xyz_042_1.mute = False
    combine_xyz_042_1.name = 'Combine XYZ.042'
    combine_xyz_042_1.use_custom_color = False
    combine_xyz_042_1.width = 125.42431640625
    combine_xyz_042_1.inputs[0].default_value = 0.0
    combine_xyz_042_1.inputs[1].default_value = 0.0
    combine_xyz_042_1.inputs[2].default_value = 0.0
    combine_xyz_042_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    combine_xyz_047_1 = test_group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz_047_1.parent = test_group.nodes.get('Frame.012')
    combine_xyz_047_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    combine_xyz_047_1.hide = False
    combine_xyz_047_1.label = 'DO NOT TOUCH'
    combine_xyz_047_1.location = (-968.5927734375, -4980.9501953125)
    combine_xyz_047_1.mute = False
    combine_xyz_047_1.name = 'Combine XYZ.047'
    combine_xyz_047_1.use_custom_color = False
    combine_xyz_047_1.width = 115.6029052734375
    combine_xyz_047_1.inputs[0].default_value = 0.0
    combine_xyz_047_1.inputs[1].default_value = 0.0
    combine_xyz_047_1.inputs[2].default_value = 0.0
    combine_xyz_047_1.outputs[0].default_value = (0.0, 0.0, 0.0)

    attribute_1 = test_group.nodes.new('ShaderNodeAttribute')
    attribute_1.parent = test_group.nodes.get('Frame.012')
    attribute_1.attribute_name = 'slots'
    attribute_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    attribute_1.hide = True
    attribute_1.location = (2020.0, 2400.0)
    attribute_1.mute = False
    attribute_1.name = 'Attribute'
    attribute_1.use_custom_color = False
    attribute_1.width = 140.0

    math_004_1 = test_group.nodes.new('ShaderNodeMath')
    math_004_1.parent = test_group.nodes.get('Frame.012')
    math_004_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_004_1.hide = True
    math_004_1.label = 'Cloth_Secondary_Slot'
    math_004_1.location = (2473.43701171875, 1164.050048828125)
    math_004_1.mute = False
    math_004_1.name = 'Math.004'
    math_004_1.operation = 'MULTIPLY'
    math_004_1.use_clamp = False
    math_004_1.use_custom_color = False
    math_004_1.width = 140.0
    math_004_1.inputs[0].default_value = 0.5
    math_004_1.inputs[1].default_value = 0.5
    math_004_1.inputs[2].default_value = 0.0
    math_004_1.outputs[0].default_value = 0.0

    math_007_1 = test_group.nodes.new('ShaderNodeMath')
    math_007_1.parent = test_group.nodes.get('Frame.012')
    math_007_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_007_1.hide = True
    math_007_1.label = 'Suit_Primary_Slot'
    math_007_1.location = (2473.43701171875, 1104.050048828125)
    math_007_1.mute = False
    math_007_1.name = 'Math.007'
    math_007_1.operation = 'MULTIPLY'
    math_007_1.use_clamp = False
    math_007_1.use_custom_color = False
    math_007_1.width = 140.0
    math_007_1.inputs[0].default_value = 0.5
    math_007_1.inputs[1].default_value = 0.5
    math_007_1.inputs[2].default_value = 0.0
    math_007_1.outputs[0].default_value = 0.0

    separate_rgb_1 = test_group.nodes.new('ShaderNodeSeparateRGB')
    separate_rgb_1.parent = test_group.nodes.get('Frame.012')
    separate_rgb_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    separate_rgb_1.hide = True
    separate_rgb_1.location = (2123.4365234375, 1139.050048828125)
    separate_rgb_1.mute = False
    separate_rgb_1.name = 'Separate RGB'
    separate_rgb_1.use_custom_color = False
    separate_rgb_1.width = 140.0
    separate_rgb_1.inputs[0].default_value = (0.800000011920929, 0.800000011920929, 0.800000011920929, 1.0)
    separate_rgb_1.outputs[0].default_value = 0.0
    separate_rgb_1.outputs[1].default_value = 0.0
    separate_rgb_1.outputs[2].default_value = 0.0

    math_001_1 = test_group.nodes.new('ShaderNodeMath')
    math_001_1.parent = test_group.nodes.get('Frame.012')
    math_001_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_001_1.hide = True
    math_001_1.label = 'Cloth_Primary_Slot'
    math_001_1.location = (2303.4365234375, 1209.050048828125)
    math_001_1.mute = False
    math_001_1.name = 'Math.001'
    math_001_1.operation = 'GREATER_THAN'
    math_001_1.use_clamp = False
    math_001_1.use_custom_color = False
    math_001_1.width = 140.0
    math_001_1.inputs[0].default_value = 0.5
    math_001_1.inputs[1].default_value = 0.9980000257492065
    math_001_1.inputs[2].default_value = 0.0
    math_001_1.outputs[0].default_value = 0.0

    math_006_1 = test_group.nodes.new('ShaderNodeMath')
    math_006_1.parent = test_group.nodes.get('Frame.012')
    math_006_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_006_1.hide = True
    math_006_1.label = ' '
    math_006_1.location = (2303.4365234375, 1089.050048828125)
    math_006_1.mute = False
    math_006_1.name = 'Math.006'
    math_006_1.operation = 'GREATER_THAN'
    math_006_1.use_clamp = False
    math_006_1.use_custom_color = False
    math_006_1.width = 140.0
    math_006_1.inputs[0].default_value = 0.5
    math_006_1.inputs[1].default_value = 0.33329999446868896
    math_006_1.inputs[2].default_value = 0.0
    math_006_1.outputs[0].default_value = 0.0

    math_008_1 = test_group.nodes.new('ShaderNodeMath')
    math_008_1.parent = test_group.nodes.get('Frame.012')
    math_008_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_008_1.hide = True
    math_008_1.label = 'Suit_Secondary_Slot'
    math_008_1.location = (2303.4365234375, 1059.050048828125)
    math_008_1.mute = False
    math_008_1.name = 'Math.008'
    math_008_1.operation = 'GREATER_THAN'
    math_008_1.use_clamp = False
    math_008_1.use_custom_color = False
    math_008_1.width = 140.0
    math_008_1.inputs[0].default_value = 0.5
    math_008_1.inputs[1].default_value = 0.9980000257492065
    math_008_1.inputs[2].default_value = 0.0
    math_008_1.outputs[0].default_value = 0.0

    math_1 = test_group.nodes.new('ShaderNodeMath')
    math_1.parent = test_group.nodes.get('Frame.012')
    math_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_1.hide = True
    math_1.label = 'Armor_Primary_Slot'
    math_1.location = (2303.4365234375, 1239.050048828125)
    math_1.mute = False
    math_1.name = 'Math'
    math_1.operation = 'GREATER_THAN'
    math_1.use_clamp = False
    math_1.use_custom_color = False
    math_1.width = 140.0
    math_1.inputs[0].default_value = 0.5
    math_1.inputs[1].default_value = 0.33329999446868896
    math_1.inputs[2].default_value = 0.0
    math_1.outputs[0].default_value = 0.0

    math_002_1 = test_group.nodes.new('ShaderNodeMath')
    math_002_1.parent = test_group.nodes.get('Frame.012')
    math_002_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_002_1.hide = True
    math_002_1.label = ' '
    math_002_1.location = (2303.4365234375, 1179.050048828125)
    math_002_1.mute = False
    math_002_1.name = 'Math.002'
    math_002_1.operation = 'LESS_THAN'
    math_002_1.use_clamp = False
    math_002_1.use_custom_color = False
    math_002_1.width = 140.0
    math_002_1.inputs[0].default_value = 0.5
    math_002_1.inputs[1].default_value = 0.33329999446868896
    math_002_1.inputs[2].default_value = 0.0
    math_002_1.outputs[0].default_value = 0.0

    math_003_1 = test_group.nodes.new('ShaderNodeMath')
    math_003_1.parent = test_group.nodes.get('Frame.012')
    math_003_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_003_1.hide = True
    math_003_1.label = ' '
    math_003_1.location = (2303.4365234375, 1149.050048828125)
    math_003_1.mute = False
    math_003_1.name = 'Math.003'
    math_003_1.operation = 'GREATER_THAN'
    math_003_1.use_clamp = False
    math_003_1.use_custom_color = False
    math_003_1.width = 140.0
    math_003_1.inputs[0].default_value = 0.5
    math_003_1.inputs[1].default_value = 0.09
    math_003_1.inputs[2].default_value = 0.0
    math_003_1.outputs[0].default_value = 0.0

    math_005_1 = test_group.nodes.new('ShaderNodeMath')
    math_005_1.parent = test_group.nodes.get('Frame.012')
    math_005_1.color = (0.6079999804496765, 0.6079999804496765, 0.6079999804496765)
    math_005_1.hide = True
    math_005_1.label = ' '
    math_005_1.location = (2303.4365234375, 1119.050048828125)
    math_005_1.mute = False
    math_005_1.name = 'Math.005'
    math_005_1.operation = 'LESS_THAN'
    math_005_1.use_clamp = False
    math_005_1.use_custom_color = False
    math_005_1.width = 140.0
    math_005_1.inputs[0].default_value = 0.5
    math_005_1.inputs[1].default_value = 0.6665999889373779
    math_005_1.inputs[2].default_value = 0.0
    math_005_1.outputs[0].default_value = 0.0

    # LINKS
    test_group.links.new(separate_rgb_1.outputs[0], math_1.inputs[0])
    test_group.links.new(separate_rgb_1.outputs[0], math_001_1.inputs[0])
    test_group.links.new(separate_rgb_1.outputs[1], math_002_1.inputs[0])
    test_group.links.new(separate_rgb_1.outputs[1], math_003_1.inputs[0])
    test_group.links.new(math_002_1.outputs[0], math_004_1.inputs[0])
    test_group.links.new(math_003_1.outputs[0], math_004_1.inputs[1])
    test_group.links.new(separate_rgb_1.outputs[1], math_005_1.inputs[0])
    test_group.links.new(separate_rgb_1.outputs[1], math_006_1.inputs[0])
    test_group.links.new(math_005_1.outputs[0], math_007_1.inputs[0])
    test_group.links.new(math_006_1.outputs[0], math_007_1.inputs[1])
    test_group.links.new(separate_rgb_1.outputs[1], math_008_1.inputs[0])
    test_group.links.new(mix_1.outputs[0], mix_001_1.inputs[1])
    test_group.links.new(mix_001_1.outputs[0], mix_002_1.inputs[1])
    test_group.links.new(mix_002_1.outputs[0], mix_003_1.inputs[1])
    test_group.links.new(mix_003_1.outputs[0], mix_004_1.inputs[1])
    test_group.links.new(mix_004_1.outputs[0], reroute_337_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_001_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_002_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_003_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_004_1.inputs[0])
    test_group.links.new(mix_005_1.outputs[0], mix_006_1.inputs[1])
    test_group.links.new(mix_006_1.outputs[0], mix_007_1.inputs[1])
    test_group.links.new(mix_007_1.outputs[0], mix_008_1.inputs[1])
    test_group.links.new(mix_008_1.outputs[0], mix_009_1.inputs[1])
    test_group.links.new(mix_009_1.outputs[0], reroute_338_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_005_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_006_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_007_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_008_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_009_1.inputs[0])
    test_group.links.new(mix_010_1.outputs[0], mix_011_1.inputs[1])
    test_group.links.new(mix_011_1.outputs[0], mix_012_1.inputs[1])
    test_group.links.new(mix_012_1.outputs[0], mix_013_1.inputs[1])
    test_group.links.new(mix_013_1.outputs[0], mix_014_1.inputs[1])
    test_group.links.new(mix_014_1.outputs[0], reroute_339_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_010_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_011_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_012_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_013_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_014_1.inputs[0])
    test_group.links.new(mix_015_1.outputs[0], mix_016_1.inputs[1])
    test_group.links.new(mix_016_1.outputs[0], mix_017_1.inputs[1])
    test_group.links.new(mix_017_1.outputs[0], mix_018_1.inputs[1])
    test_group.links.new(mix_018_1.outputs[0], mix_019_1.inputs[1])
    test_group.links.new(mix_019_1.outputs[0], reroute_340_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_015_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_016_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_017_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_018_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_019_1.inputs[0])
    test_group.links.new(mix_020_1.outputs[0], mix_021_1.inputs[1])
    test_group.links.new(mix_021_1.outputs[0], mix_022_1.inputs[1])
    test_group.links.new(mix_022_1.outputs[0], mix_023_1.inputs[1])
    test_group.links.new(mix_023_1.outputs[0], mix_024_1.inputs[1])
    test_group.links.new(mix_024_1.outputs[0], reroute_341_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_020_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_021_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_022_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_023_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_024_1.inputs[0])
    test_group.links.new(mix_025_1.outputs[0], mix_026_1.inputs[1])
    test_group.links.new(mix_026_1.outputs[0], mix_027_1.inputs[1])
    test_group.links.new(mix_027_1.outputs[0], mix_028_1.inputs[1])
    test_group.links.new(mix_028_1.outputs[0], mix_029_1.inputs[1])
    test_group.links.new(mix_029_1.outputs[0], reroute_342_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_025_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_026_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_027_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_028_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_029_1.inputs[0])
    test_group.links.new(mix_030_1.outputs[0], mix_031_1.inputs[1])
    test_group.links.new(mix_031_1.outputs[0], mix_032_1.inputs[1])
    test_group.links.new(mix_032_1.outputs[0], mix_033_1.inputs[1])
    test_group.links.new(mix_033_1.outputs[0], mix_034_1.inputs[1])
    test_group.links.new(mix_034_1.outputs[0], reroute_343_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_030_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_031_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_032_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_033_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_034_1.inputs[0])
    test_group.links.new(mix_035_1.outputs[0], mix_036_1.inputs[1])
    test_group.links.new(mix_036_1.outputs[0], mix_037_1.inputs[1])
    test_group.links.new(mix_037_1.outputs[0], mix_038_1.inputs[1])
    test_group.links.new(mix_038_1.outputs[0], mix_039_1.inputs[1])
    test_group.links.new(mix_039_1.outputs[0], reroute_344_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_035_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_036_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_037_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_038_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_039_1.inputs[0])
    test_group.links.new(mix_040_1.outputs[0], mix_041_1.inputs[1])
    test_group.links.new(mix_041_1.outputs[0], mix_042_1.inputs[1])
    test_group.links.new(mix_042_1.outputs[0], mix_043_1.inputs[1])
    test_group.links.new(mix_043_1.outputs[0], mix_044_1.inputs[1])
    test_group.links.new(mix_044_1.outputs[0], reroute_345_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_040_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_041_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_042_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_043_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_044_1.inputs[0])
    test_group.links.new(mix_045_1.outputs[0], mix_046_1.inputs[1])
    test_group.links.new(mix_046_1.outputs[0], mix_047_1.inputs[1])
    test_group.links.new(mix_047_1.outputs[0], mix_048_1.inputs[1])
    test_group.links.new(mix_048_1.outputs[0], mix_049_1.inputs[1])
    test_group.links.new(mix_049_1.outputs[0], reroute_346_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_045_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_046_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_047_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_048_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_049_1.inputs[0])
    test_group.links.new(mix_050_1.outputs[0], mix_051_1.inputs[1])
    test_group.links.new(mix_051_1.outputs[0], mix_052_1.inputs[1])
    test_group.links.new(mix_052_1.outputs[0], mix_053_1.inputs[1])
    test_group.links.new(mix_053_1.outputs[0], mix_054_1.inputs[1])
    test_group.links.new(mix_054_1.outputs[0], reroute_347_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_050_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_051_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_052_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_053_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_054_1.inputs[0])
    test_group.links.new(armor_detail_normal_transform_1.outputs[0], reroute_291_1.inputs[0])
    test_group.links.new(cloth_detail_normal_transform_1.outputs[0], reroute_320_1.inputs[0])
    test_group.links.new(armor_primary_detail_normal_map_1.outputs[0], reroute_011_1.inputs[0])
    test_group.links.new(armor_secondary_detail_normal_map_1.outputs[0], reroute_039_1.inputs[0])
    test_group.links.new(cloth_primary_detail_normal_map_1.outputs[0], reroute_067_1.inputs[0])
    test_group.links.new(cloth_secondary_detail_normal_map_1.outputs[0], reroute_096_1.inputs[0])
    test_group.links.new(suit_primary_detail_normal_map_1.outputs[0], reroute_124_1.inputs[0])
    test_group.links.new(suit_secondary_detail_normal_map_1.outputs[0], reroute_194_1.inputs[0])
    test_group.links.new(armor_detail_diffuse_transform_1.outputs[0], reroute_288_1.inputs[0])
    test_group.links.new(cloth_detail_diffuse_transform_1.outputs[0], reroute_319_1.inputs[0])
    test_group.links.new(armor_primary_wear_remap_x_1.outputs[0], reroute_007_1.inputs[0])
    test_group.links.new(armor_primary_wear_remap_y_1.outputs[0], reroute_008_1.inputs[0])
    test_group.links.new(armor_primary_wear_remap_z_1.outputs[0], reroute_009_1.inputs[0])
    test_group.links.new(armor_primary_wear_remap_w_1.outputs[0], reroute_010_1.inputs[0])
    test_group.links.new(armor_secondary_wear_remap_x_1.outputs[0], reroute_035_1.inputs[0])
    test_group.links.new(armor_secondary_wear_remap_y_1.outputs[0], reroute_036_1.inputs[0])
    test_group.links.new(armor_secondary_wear_remap_z_1.outputs[0], reroute_037_1.inputs[0])
    test_group.links.new(armor_secondary_wear_remap_w_1.outputs[0], reroute_038_1.inputs[0])
    test_group.links.new(cloth_primary_wear_remap_x_1.outputs[0], reroute_063_1.inputs[0])
    test_group.links.new(cloth_primary_wear_remap_y_1.outputs[0], reroute_064_1.inputs[0])
    test_group.links.new(cloth_primary_wear_remap_z_1.outputs[0], reroute_065_1.inputs[0])
    test_group.links.new(cloth_primary_wear_remap_w_1.outputs[0], reroute_066_1.inputs[0])
    test_group.links.new(cloth_secondary_wear_remap_x_1.outputs[0], reroute_091_1.inputs[0])
    test_group.links.new(cloth_secondary_wear_remap_y_1.outputs[0], reroute_092_1.inputs[0])
    test_group.links.new(cloth_secondary_wear_remap_z_1.outputs[0], reroute_093_1.inputs[0])
    test_group.links.new(cloth_secondary_wear_remap_w_1.outputs[0], reroute_094_1.inputs[0])
    test_group.links.new(suit_primary_wear_remap_x_1.outputs[0], reroute_119_1.inputs[0])
    test_group.links.new(suit_primary_wear_remap_y_1.outputs[0], reroute_120_1.inputs[0])
    test_group.links.new(suit_primary_wear_remap_z_1.outputs[0], reroute_121_1.inputs[0])
    test_group.links.new(suit_primary_wear_remap_w_1.outputs[0], reroute_122_1.inputs[0])
    test_group.links.new(suit_secondary_wear_remap_x_1.outputs[0], reroute_183_1.inputs[0])
    test_group.links.new(suit_secondary_wear_remap_y_1.outputs[0], reroute_184_1.inputs[0])
    test_group.links.new(suit_secondary_wear_remap_z_1.outputs[0], reroute_185_1.inputs[0])
    test_group.links.new(suit_secondary_wear_remap_w_1.outputs[0], reroute_186_1.inputs[0])
    test_group.links.new(armor_primary_detail_diffuse_blend_1.outputs[0], reroute_012_1.inputs[0])
    test_group.links.new(armor_primary_detail_normal_blend_1.outputs[0], reroute_013_1.inputs[0])
    test_group.links.new(armor_secondary_detail_diffuse_blend_1.outputs[0], reroute_040_1.inputs[0])
    test_group.links.new(armor_secondary_detail_normal_blend_1.outputs[0], reroute_041_1.inputs[0])
    test_group.links.new(cloth_primary_detail_diffuse_blend_1.outputs[0], reroute_068_1.inputs[0])
    test_group.links.new(cloth_primary_detail_normal_blend_1.outputs[0], reroute_069_1.inputs[0])
    test_group.links.new(cloth_secondary_detail_diffuse_blend_1.outputs[0], reroute_095_1.inputs[0])
    test_group.links.new(cloth_secondary_detail_normal_blend_1.outputs[0], reroute_097_1.inputs[0])
    test_group.links.new(suit_primary_detail_diffuse_blend_1.outputs[0], reroute_123_1.inputs[0])
    test_group.links.new(suit_primary_detail_normal_blend_1.outputs[0], reroute_125_1.inputs[0])
    test_group.links.new(suit_secondary_detail_diffuse_blend_1.outputs[0], reroute_187_1.inputs[0])
    test_group.links.new(suit_secondary_detail_normal_blend_1.outputs[0], reroute_188_1.inputs[0])
    test_group.links.new(armor_primary_detail_roughness_blend_1.outputs[0], reroute_014_1.inputs[0])
    test_group.links.new(armor_secondary_detail_roughness_blend_1.outputs[0], reroute_042_1.inputs[0])
    test_group.links.new(cloth_primary_detail_roughness_blend_1.outputs[0], reroute_070_1.inputs[0])
    test_group.links.new(cloth_secondary_detail_roughness_blend_1.outputs[0], reroute_098_1.inputs[0])
    test_group.links.new(suit_primary_detail_roughness_blend_1.outputs[0], reroute_126_1.inputs[0])
    test_group.links.new(suit_secondary_detail_roughness_blend_1.outputs[0], reroute_189_1.inputs[0])
    test_group.links.new(mix_055_1.outputs[0], mix_056_1.inputs[1])
    test_group.links.new(mix_056_1.outputs[0], mix_057_1.inputs[1])
    test_group.links.new(mix_057_1.outputs[0], mix_058_1.inputs[1])
    test_group.links.new(mix_058_1.outputs[0], mix_059_1.inputs[1])
    test_group.links.new(mix_059_1.outputs[0], reroute_348_1.inputs[0])
    test_group.links.new(armor_primary_roughness_remap_x_1.outputs[0], reroute_001_1.inputs[0])
    test_group.links.new(armor_primary_roughness_remap_y_1.outputs[0], reroute_002_1.inputs[0])
    test_group.links.new(armor_primary_roughness_remap_z_1.outputs[0], reroute_003_1.inputs[0])
    test_group.links.new(armor_primary_roughness_remap_w_1.outputs[0], reroute_004_1.inputs[0])
    test_group.links.new(armor_secondary_roughness_remap_x_1.outputs[0], reroute_029_1.inputs[0])
    test_group.links.new(armor_secondary_roughness_remap_y_1.outputs[0], reroute_030_1.inputs[0])
    test_group.links.new(armor_secondary_roughness_remap_z_1.outputs[0], reroute_031_1.inputs[0])
    test_group.links.new(armor_secondary_roughness_remap_w_1.outputs[0], reroute_032_1.inputs[0])
    test_group.links.new(cloth_primary_roughness_remap_x_1.outputs[0], reroute_057_1.inputs[0])
    test_group.links.new(cloth_primary_roughness_remap_y_1.outputs[0], reroute_058_1.inputs[0])
    test_group.links.new(cloth_primary_roughness_remap_z_1.outputs[0], reroute_059_1.inputs[0])
    test_group.links.new(cloth_primary_roughness_remap_w_1.outputs[0], reroute_060_1.inputs[0])
    test_group.links.new(cloth_secondary_roughness_remap_x_1.outputs[0], reroute_085_1.inputs[0])
    test_group.links.new(cloth_secondary_roughness_remap_y_1.outputs[0], reroute_086_1.inputs[0])
    test_group.links.new(cloth_secondary_roughness_remap_z_1.outputs[0], reroute_087_1.inputs[0])
    test_group.links.new(cloth_secondary_roughness_remap_w_1.outputs[0], reroute_088_1.inputs[0])
    test_group.links.new(suit_primary_roughness_remap_x_1.outputs[0], reroute_113_1.inputs[0])
    test_group.links.new(suit_primary_roughness_remap_y_1.outputs[0], reroute_114_1.inputs[0])
    test_group.links.new(suit_primary_roughness_remap_z_1.outputs[0], reroute_115_1.inputs[0])
    test_group.links.new(suit_primary_roughness_remap_w_1.outputs[0], reroute_116_1.inputs[0])
    test_group.links.new(suit_secondary_roughness_remap_x_1.outputs[0], reroute_177_1.inputs[0])
    test_group.links.new(suit_secondary_roughness_remap_y_1.outputs[0], reroute_182_1.inputs[0])
    test_group.links.new(suit_secondary_roughness_remap_z_1.outputs[0], reroute_181_1.inputs[0])
    test_group.links.new(suit_secondary_roughness_remap_w_1.outputs[0], reroute_180_1.inputs[0])
    test_group.links.new(armor_primary_detail_diffuse_map_1.outputs[0], reroute_005_1.inputs[0])
    test_group.links.new(armor_secondary_detail_diffuse_map_1.outputs[0], reroute_033_1.inputs[0])
    test_group.links.new(cloth_primary_detail_diffuse_map_1.outputs[0], reroute_061_1.inputs[0])
    test_group.links.new(cloth_secondary_detail_diffuse_map_1.outputs[0], reroute_089_1.inputs[0])
    test_group.links.new(suit_primary_detail_diffuse_map_1.outputs[0], reroute_117_1.inputs[0])
    test_group.links.new(suit_secondary_detail_diffuse_map_1.outputs[0], reroute_179_1.inputs[0])
    test_group.links.new(worn_armor_primary_roughness_remap_x_1.outputs[0], reroute_020_1.inputs[0])
    test_group.links.new(worn_armor_primary_roughness_remap_y_1.outputs[0], reroute_021_1.inputs[0])
    test_group.links.new(worn_armor_primary_roughness_remap_z_1.outputs[0], reroute_022_1.inputs[0])
    test_group.links.new(worn_armor_primary_roughness_remap_w_1.outputs[0], reroute_023_1.inputs[0])
    test_group.links.new(worn_armor_secondary_roughness_remap_x_1.outputs[0], reroute_048_1.inputs[0])
    test_group.links.new(worn_armor_secondary_roughness_remap_y_1.outputs[0], reroute_049_1.inputs[0])
    test_group.links.new(worn_armor_secondary_roughness_remap_z_1.outputs[0], reroute_050_1.inputs[0])
    test_group.links.new(worn_armor_secondary_roughness_remap_w_1.outputs[0], reroute_051_1.inputs[0])
    test_group.links.new(worn_cloth_primary_roughness_remap_x_1.outputs[0], reroute_076_1.inputs[0])
    test_group.links.new(worn_cloth_primary_roughness_remap_y_1.outputs[0], reroute_077_1.inputs[0])
    test_group.links.new(worn_cloth_primary_roughness_remap_z_1.outputs[0], reroute_078_1.inputs[0])
    test_group.links.new(worn_cloth_primary_roughness_remap_w_1.outputs[0], reroute_079_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_roughness_remap_x_1.outputs[0], reroute_104_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_roughness_remap_y_1.outputs[0], reroute_105_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_roughness_remap_z_1.outputs[0], reroute_106_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_roughness_remap_w_1.outputs[0], reroute_107_1.inputs[0])
    test_group.links.new(worn_suit_primary_roughness_remap_x_1.outputs[0], reroute_132_1.inputs[0])
    test_group.links.new(worn_suit_primary_roughness_remap_y_1.outputs[0], reroute_133_1.inputs[0])
    test_group.links.new(worn_suit_primary_roughness_remap_z_1.outputs[0], reroute_134_1.inputs[0])
    test_group.links.new(worn_suit_primary_roughness_remap_w_1.outputs[0], reroute_135_1.inputs[0])
    test_group.links.new(worn_suit_secondary_roughness_remap_x_1.outputs[0], reroute_196_1.inputs[0])
    test_group.links.new(worn_suit_secondary_roughness_remap_y_1.outputs[0], reroute_197_1.inputs[0])
    test_group.links.new(worn_suit_secondary_roughness_remap_z_1.outputs[0], reroute_198_1.inputs[0])
    test_group.links.new(worn_suit_secondary_roughness_remap_w_1.outputs[0], reroute_199_1.inputs[0])
    test_group.links.new(armor_primary_detail_diffuse_map_1.outputs[1], reroute_006_1.inputs[0])
    test_group.links.new(armor_secondary_detail_diffuse_map_1.outputs[1], reroute_034_1.inputs[0])
    test_group.links.new(cloth_primary_detail_diffuse_map_1.outputs[1], reroute_062_1.inputs[0])
    test_group.links.new(cloth_secondary_detail_diffuse_map_1.outputs[1], reroute_090_1.inputs[0])
    test_group.links.new(suit_primary_detail_diffuse_map_1.outputs[1], reroute_118_1.inputs[0])
    test_group.links.new(suit_secondary_detail_diffuse_map_1.outputs[1], reroute_178_1.inputs[0])
    test_group.links.new(armor_primary_dye_color_1.outputs[0], reroute_1.inputs[0])
    test_group.links.new(armor_secondary_dye_color_1.outputs[0], reroute_028_1.inputs[0])
    test_group.links.new(cloth_primary_dye_color_1.outputs[0], reroute_056_1.inputs[0])
    test_group.links.new(cloth_secondary_dye_color_1.outputs[0], reroute_084_1.inputs[0])
    test_group.links.new(suit_primary_dye_color_1.outputs[0], reroute_112_1.inputs[0])
    test_group.links.new(suit_secondary_dye_color_1.outputs[0], reroute_176_1.inputs[0])
    test_group.links.new(worn_armor_primary_dye_color_1.outputs[0], reroute_019_1.inputs[0])
    test_group.links.new(worn_armor_secondary_dye_color_1.outputs[0], reroute_047_1.inputs[0])
    test_group.links.new(worn_cloth_primary_dye_color_1.outputs[0], reroute_075_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_dye_color_1.outputs[0], reroute_103_1.inputs[0])
    test_group.links.new(worn_suit_primary_dye_color_1.outputs[0], reroute_131_1.inputs[0])
    test_group.links.new(worn_suit_secondary_dye_color_1.outputs[0], reroute_195_1.inputs[0])
    test_group.links.new(math_1.outputs[0], mix_055_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_056_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_057_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_058_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_059_1.inputs[0])
    test_group.links.new(armor_primary_transmission_1.outputs[0], reroute_017_1.inputs[0])
    test_group.links.new(armor_primary_iridescence_1.outputs[0], reroute_016_1.inputs[0])
    test_group.links.new(armor_primary_metalness_1.outputs[0], reroute_015_1.inputs[0])
    test_group.links.new(armor_secondary_metalness_1.outputs[0], reroute_043_1.inputs[0])
    test_group.links.new(armor_secondary_iridescence_1.outputs[0], reroute_044_1.inputs[0])
    test_group.links.new(armor_secondary_transmission_1.outputs[0], reroute_045_1.inputs[0])
    test_group.links.new(cloth_primary_metalness_1.outputs[0], reroute_071_1.inputs[0])
    test_group.links.new(cloth_primary_iridescence_1.outputs[0], reroute_072_1.inputs[0])
    test_group.links.new(cloth_primary_transmission_1.outputs[0], reroute_073_1.inputs[0])
    test_group.links.new(cloth_secondary_metalness_1.outputs[0], reroute_099_1.inputs[0])
    test_group.links.new(cloth_secondary_iridescence_1.outputs[0], reroute_100_1.inputs[0])
    test_group.links.new(cloth_secondary_transmission_1.outputs[0], reroute_101_1.inputs[0])
    test_group.links.new(suit_primary_metalness_1.outputs[0], reroute_127_1.inputs[0])
    test_group.links.new(suit_primary_iridescence_1.outputs[0], reroute_128_1.inputs[0])
    test_group.links.new(suit_primary_transmission_1.outputs[0], reroute_129_1.inputs[0])
    test_group.links.new(suit_secondary_metalness_1.outputs[0], reroute_190_1.inputs[0])
    test_group.links.new(suit_secondary_iridescence_1.outputs[0], reroute_191_1.inputs[0])
    test_group.links.new(worn_armor_primary_metalness_1.outputs[0], reroute_027_1.inputs[0])
    test_group.links.new(worn_armor_secondary_metalness_1.outputs[0], reroute_055_1.inputs[0])
    test_group.links.new(worn_cloth_primary_metalness_1.outputs[0], reroute_083_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_metalness_1.outputs[0], reroute_111_1.inputs[0])
    test_group.links.new(worn_suit_primary_metalness_1.outputs[0], reroute_139_1.inputs[0])
    test_group.links.new(worn_suit_secondary_metalness_1.outputs[0], reroute_203_1.inputs[0])
    test_group.links.new(armor_primary_emission_color_1.outputs[0], reroute_018_1.inputs[0])
    test_group.links.new(armor_secondary_emission_color_1.outputs[0], reroute_046_1.inputs[0])
    test_group.links.new(cloth_primary_emission_color_1.outputs[0], reroute_074_1.inputs[0])
    test_group.links.new(cloth_secondary_emission_color_1.outputs[0], reroute_102_1.inputs[0])
    test_group.links.new(suit_primary_emission_color_1.outputs[0], reroute_130_1.inputs[0])
    test_group.links.new(suit_secondary_emission_color_1.outputs[0], reroute_193_1.inputs[0])
    test_group.links.new(reroute_001_1.outputs[0], combine_xyz_012_1.inputs[0])
    test_group.links.new(reroute_002_1.outputs[0], combine_xyz_012_1.inputs[1])
    test_group.links.new(reroute_003_1.outputs[0], combine_xyz_012_1.inputs[2])
    test_group.links.new(reroute_004_1.outputs[0], combine_xyz_013_1.inputs[0])
    test_group.links.new(mix_064_1.outputs[0], mix_060_1.inputs[1])
    test_group.links.new(mix_060_1.outputs[0], mix_061_1.inputs[1])
    test_group.links.new(mix_061_1.outputs[0], mix_062_1.inputs[1])
    test_group.links.new(mix_062_1.outputs[0], mix_063_1.inputs[1])
    test_group.links.new(math_1.outputs[0], mix_064_1.inputs[0])
    test_group.links.new(math_001_1.outputs[0], mix_060_1.inputs[0])
    test_group.links.new(math_004_1.outputs[0], mix_061_1.inputs[0])
    test_group.links.new(math_007_1.outputs[0], mix_062_1.inputs[0])
    test_group.links.new(math_008_1.outputs[0], mix_063_1.inputs[0])
    test_group.links.new(worn_armor_primary_detail_diffuse_blend_1.outputs[0], reroute_024_1.inputs[0])
    test_group.links.new(worn_armor_primary_detail_normal_blend_1.outputs[0], reroute_025_1.inputs[0])
    test_group.links.new(worn_armor_primary_detail_roughness_blend_1.outputs[0], reroute_026_1.inputs[0])
    test_group.links.new(worn_armor_secondary_detail_diffuse_blend_1.outputs[0], reroute_052_1.inputs[0])
    test_group.links.new(worn_armor_secondary_detail_normal_blend_1.outputs[0], reroute_053_1.inputs[0])
    test_group.links.new(worn_armor_secondary_detail_roughness_blend_1.outputs[0], reroute_054_1.inputs[0])
    test_group.links.new(worn_cloth_primary_detail_diffuse_blend_1.outputs[0], reroute_080_1.inputs[0])
    test_group.links.new(worn_cloth_primary_detail_normal_blend_1.outputs[0], reroute_081_1.inputs[0])
    test_group.links.new(worn_cloth_primary_detail_roughness_blend_1.outputs[0], reroute_082_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_detail_diffuse_blend_1.outputs[0], reroute_108_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_detail_normal_blend_1.outputs[0], reroute_109_1.inputs[0])
    test_group.links.new(worn_cloth_secondary_detail_roughness_blend_1.outputs[0], reroute_110_1.inputs[0])
    test_group.links.new(worn_suit_primary_detail_diffuse_blend_1.outputs[0], reroute_136_1.inputs[0])
    test_group.links.new(worn_suit_primary_detail_normal_blend_1.outputs[0], reroute_137_1.inputs[0])
    test_group.links.new(worn_suit_primary_detail_roughness_blend_1.outputs[0], reroute_138_1.inputs[0])
    test_group.links.new(worn_suit_secondary_detail_diffuse_blend_1.outputs[0], reroute_200_1.inputs[0])
    test_group.links.new(worn_suit_secondary_detail_normal_blend_1.outputs[0], reroute_201_1.inputs[0])
    test_group.links.new(worn_suit_secondary_detail_roughness_blend_1.outputs[0], reroute_202_1.inputs[0])
    test_group.links.new(reroute_029_1.outputs[0], combine_xyz_014_1.inputs[0])
    test_group.links.new(reroute_030_1.outputs[0], combine_xyz_014_1.inputs[1])
    test_group.links.new(reroute_031_1.outputs[0], combine_xyz_014_1.inputs[2])
    test_group.links.new(reroute_032_1.outputs[0], combine_xyz_015_1.inputs[0])
    test_group.links.new(reroute_057_1.outputs[0], combine_xyz_016_1.inputs[0])
    test_group.links.new(reroute_058_1.outputs[0], combine_xyz_016_1.inputs[1])
    test_group.links.new(reroute_059_1.outputs[0], combine_xyz_016_1.inputs[2])
    test_group.links.new(reroute_060_1.outputs[0], combine_xyz_017_1.inputs[0])
    test_group.links.new(reroute_085_1.outputs[0], combine_xyz_018_1.inputs[0])
    test_group.links.new(reroute_086_1.outputs[0], combine_xyz_018_1.inputs[1])
    test_group.links.new(reroute_087_1.outputs[0], combine_xyz_018_1.inputs[2])
    test_group.links.new(reroute_088_1.outputs[0], combine_xyz_019_1.inputs[0])
    test_group.links.new(reroute_090_1.outputs[0], combine_xyz_019_1.inputs[1])
    test_group.links.new(reroute_078_1.outputs[0], reroute_258_1.inputs[0])
    test_group.links.new(reroute_079_1.outputs[0], reroute_259_1.inputs[0])
    test_group.links.new(reroute_077_1.outputs[0], reroute_257_1.inputs[0])
    test_group.links.new(reroute_076_1.outputs[0], reroute_256_1.inputs[0])
    test_group.links.new(reroute_062_1.outputs[0], combine_xyz_017_1.inputs[1])
    test_group.links.new(reroute_034_1.outputs[0], combine_xyz_015_1.inputs[1])
    test_group.links.new(reroute_048_1.outputs[0], reroute_248_1.inputs[0])
    test_group.links.new(reroute_049_1.outputs[0], reroute_250_1.inputs[0])
    test_group.links.new(reroute_050_1.outputs[0], reroute_251_1.inputs[0])
    test_group.links.new(reroute_051_1.outputs[0], reroute_252_1.inputs[0])
    test_group.links.new(reroute_006_1.outputs[0], combine_xyz_013_1.inputs[1])
    test_group.links.new(reroute_020_1.outputs[0], reroute_158_1.inputs[0])
    test_group.links.new(reroute_021_1.outputs[0], reroute_160_1.inputs[0])
    test_group.links.new(reroute_022_1.outputs[0], reroute_161_1.inputs[0])
    test_group.links.new(reroute_023_1.outputs[0], reroute_162_1.inputs[0])
    test_group.links.new(reroute_113_1.outputs[0], combine_xyz_024_1.inputs[0])
    test_group.links.new(reroute_114_1.outputs[0], combine_xyz_024_1.inputs[1])
    test_group.links.new(reroute_115_1.outputs[0], combine_xyz_024_1.inputs[2])
    test_group.links.new(reroute_116_1.outputs[0], combine_xyz_025_1.inputs[0])
    test_group.links.new(reroute_118_1.outputs[0], combine_xyz_025_1.inputs[1])
    test_group.links.new(reroute_177_1.outputs[0], combine_xyz_028_1.inputs[0])
    test_group.links.new(reroute_182_1.outputs[0], combine_xyz_028_1.inputs[1])
    test_group.links.new(reroute_181_1.outputs[0], combine_xyz_028_1.inputs[2])
    test_group.links.new(reroute_180_1.outputs[0], combine_xyz_029_1.inputs[0])
    test_group.links.new(reroute_178_1.outputs[0], combine_xyz_029_1.inputs[1])
    test_group.links.new(reroute_1.outputs[0], reroute_140_1.inputs[0])
    test_group.links.new(reroute_140_1.outputs[0], reroute_141_1.inputs[0])
    test_group.links.new(reroute_143_1.outputs[0], reroute_142_1.inputs[0])
    test_group.links.new(reroute_028_1.outputs[0], reroute_143_1.inputs[0])
    test_group.links.new(reroute_056_1.outputs[0], reroute_144_1.inputs[0])
    test_group.links.new(reroute_144_1.outputs[0], reroute_145_1.inputs[0])
    test_group.links.new(reroute_084_1.outputs[0], reroute_146_1.inputs[0])
    test_group.links.new(reroute_146_1.outputs[0], reroute_147_1.inputs[0])
    test_group.links.new(reroute_112_1.outputs[0], reroute_148_1.inputs[0])
    test_group.links.new(reroute_148_1.outputs[0], reroute_149_1.inputs[0])
    test_group.links.new(reroute_176_1.outputs[0], reroute_150_1.inputs[0])
    test_group.links.new(reroute_150_1.outputs[0], reroute_151_1.inputs[0])
    test_group.links.new(reroute_141_1.outputs[0], reroute_154_1.inputs[0])
    test_group.links.new(reroute_142_1.outputs[0], reroute_153_1.inputs[0])
    test_group.links.new(reroute_145_1.outputs[0], reroute_155_1.inputs[0])
    test_group.links.new(reroute_147_1.outputs[0], reroute_156_1.inputs[0])
    test_group.links.new(reroute_149_1.outputs[0], reroute_157_1.inputs[0])
    test_group.links.new(reroute_151_1.outputs[0], reroute_152_1.inputs[0])
    test_group.links.new(reroute_154_1.outputs[0], mix_1.inputs[1])
    test_group.links.new(reroute_153_1.outputs[0], mix_1.inputs[2])
    test_group.links.new(reroute_155_1.outputs[0], mix_001_1.inputs[2])
    test_group.links.new(reroute_156_1.outputs[0], mix_002_1.inputs[2])
    test_group.links.new(reroute_157_1.outputs[0], mix_003_1.inputs[2])
    test_group.links.new(reroute_152_1.outputs[0], mix_004_1.inputs[2])
    test_group.links.new(reroute_158_1.outputs[0], reroute_159_1.inputs[0])
    test_group.links.new(reroute_159_1.outputs[0], combine_xyz_013_1.inputs[2])
    test_group.links.new(reroute_160_1.outputs[0], reroute_163_1.inputs[0])
    test_group.links.new(reroute_163_1.outputs[0], combine_xyz_020_1.inputs[0])
    test_group.links.new(reroute_161_1.outputs[0], reroute_164_1.inputs[0])
    test_group.links.new(reroute_164_1.outputs[0], combine_xyz_020_1.inputs[1])
    test_group.links.new(reroute_162_1.outputs[0], reroute_165_1.inputs[0])
    test_group.links.new(reroute_165_1.outputs[0], combine_xyz_020_1.inputs[2])
    test_group.links.new(reroute_205_1.outputs[0], reroute_171_1.inputs[0])
    test_group.links.new(reroute_204_1.outputs[0], reroute_170_1.inputs[0])
    test_group.links.new(reroute_175_1.outputs[0], reroute_169_1.inputs[0])
    test_group.links.new(reroute_174_1.outputs[0], reroute_168_1.inputs[0])
    test_group.links.new(reroute_173_1.outputs[0], reroute_167_1.inputs[0])
    test_group.links.new(reroute_172_1.outputs[0], reroute_166_1.inputs[0])
    test_group.links.new(combine_xyz_012_1.outputs[0], reroute_172_1.inputs[0])
    test_group.links.new(combine_xyz_014_1.outputs[0], reroute_173_1.inputs[0])
    test_group.links.new(combine_xyz_016_1.outputs[0], reroute_174_1.inputs[0])
    test_group.links.new(combine_xyz_018_1.outputs[0], reroute_175_1.inputs[0])
    test_group.links.new(combine_xyz_024_1.outputs[0], reroute_204_1.inputs[0])
    test_group.links.new(combine_xyz_028_1.outputs[0], reroute_205_1.inputs[0])
    test_group.links.new(reroute_166_1.outputs[0], reroute_206_1.inputs[0])
    test_group.links.new(reroute_167_1.outputs[0], reroute_211_1.inputs[0])
    test_group.links.new(reroute_168_1.outputs[0], reroute_207_1.inputs[0])
    test_group.links.new(reroute_169_1.outputs[0], reroute_208_1.inputs[0])
    test_group.links.new(reroute_170_1.outputs[0], reroute_209_1.inputs[0])
    test_group.links.new(reroute_171_1.outputs[0], reroute_210_1.inputs[0])
    test_group.links.new(reroute_206_1.outputs[0], mix_020_1.inputs[1])
    test_group.links.new(reroute_211_1.outputs[0], mix_020_1.inputs[2])
    test_group.links.new(reroute_207_1.outputs[0], mix_021_1.inputs[2])
    test_group.links.new(reroute_208_1.outputs[0], mix_022_1.inputs[2])
    test_group.links.new(reroute_209_1.outputs[0], mix_023_1.inputs[2])
    test_group.links.new(reroute_210_1.outputs[0], mix_024_1.inputs[2])
    test_group.links.new(reroute_219_1.outputs[0], reroute_213_1.inputs[0])
    test_group.links.new(reroute_220_1.outputs[0], reroute_214_1.inputs[0])
    test_group.links.new(reroute_221_1.outputs[0], reroute_215_1.inputs[0])
    test_group.links.new(reroute_222_1.outputs[0], reroute_216_1.inputs[0])
    test_group.links.new(reroute_218_1.outputs[0], reroute_212_1.inputs[0])
    test_group.links.new(reroute_223_1.outputs[0], reroute_217_1.inputs[0])
    test_group.links.new(combine_xyz_013_1.outputs[0], reroute_219_1.inputs[0])
    test_group.links.new(combine_xyz_015_1.outputs[0], reroute_220_1.inputs[0])
    test_group.links.new(combine_xyz_017_1.outputs[0], reroute_221_1.inputs[0])
    test_group.links.new(combine_xyz_019_1.outputs[0], reroute_222_1.inputs[0])
    test_group.links.new(combine_xyz_025_1.outputs[0], reroute_218_1.inputs[0])
    test_group.links.new(combine_xyz_029_1.outputs[0], reroute_223_1.inputs[0])
    test_group.links.new(reroute_213_1.outputs[0], reroute_229_1.inputs[0])
    test_group.links.new(reroute_214_1.outputs[0], reroute_225_1.inputs[0])
    test_group.links.new(reroute_215_1.outputs[0], reroute_226_1.inputs[0])
    test_group.links.new(reroute_216_1.outputs[0], reroute_227_1.inputs[0])
    test_group.links.new(reroute_212_1.outputs[0], reroute_224_1.inputs[0])
    test_group.links.new(reroute_217_1.outputs[0], reroute_228_1.inputs[0])
    test_group.links.new(reroute_229_1.outputs[0], mix_025_1.inputs[1])
    test_group.links.new(reroute_225_1.outputs[0], mix_025_1.inputs[2])
    test_group.links.new(reroute_226_1.outputs[0], mix_026_1.inputs[2])
    test_group.links.new(reroute_227_1.outputs[0], mix_027_1.inputs[2])
    test_group.links.new(reroute_224_1.outputs[0], mix_028_1.inputs[2])
    test_group.links.new(reroute_228_1.outputs[0], mix_029_1.inputs[2])
    test_group.links.new(reroute_236_1.outputs[0], reroute_230_1.inputs[0])
    test_group.links.new(reroute_237_1.outputs[0], reroute_231_1.inputs[0])
    test_group.links.new(reroute_238_1.outputs[0], reroute_232_1.inputs[0])
    test_group.links.new(reroute_239_1.outputs[0], reroute_233_1.inputs[0])
    test_group.links.new(reroute_240_1.outputs[0], reroute_234_1.inputs[0])
    test_group.links.new(reroute_241_1.outputs[0], reroute_235_1.inputs[0])
    test_group.links.new(combine_xyz_020_1.outputs[0], reroute_241_1.inputs[0])
    test_group.links.new(combine_xyz_021_1.outputs[0], reroute_240_1.inputs[0])
    test_group.links.new(combine_xyz_022_1.outputs[0], reroute_239_1.inputs[0])
    test_group.links.new(combine_xyz_023_1.outputs[0], reroute_238_1.inputs[0])
    test_group.links.new(combine_xyz_026_1.outputs[0], reroute_237_1.inputs[0])
    test_group.links.new(combine_xyz_027_1.outputs[0], reroute_236_1.inputs[0])
    test_group.links.new(reroute_235_1.outputs[0], reroute_245_1.inputs[0])
    test_group.links.new(reroute_234_1.outputs[0], reroute_244_1.inputs[0])
    test_group.links.new(reroute_233_1.outputs[0], reroute_243_1.inputs[0])
    test_group.links.new(reroute_232_1.outputs[0], reroute_242_1.inputs[0])
    test_group.links.new(reroute_231_1.outputs[0], reroute_246_1.inputs[0])
    test_group.links.new(reroute_230_1.outputs[0], reroute_247_1.inputs[0])
    test_group.links.new(reroute_245_1.outputs[0], mix_030_1.inputs[1])
    test_group.links.new(reroute_244_1.outputs[0], mix_030_1.inputs[2])
    test_group.links.new(reroute_243_1.outputs[0], mix_031_1.inputs[2])
    test_group.links.new(reroute_242_1.outputs[0], mix_032_1.inputs[2])
    test_group.links.new(reroute_246_1.outputs[0], mix_033_1.inputs[2])
    test_group.links.new(reroute_247_1.outputs[0], mix_034_1.inputs[2])
    test_group.links.new(reroute_248_1.outputs[0], reroute_249_1.inputs[0])
    test_group.links.new(reroute_249_1.outputs[0], combine_xyz_015_1.inputs[2])
    test_group.links.new(reroute_250_1.outputs[0], reroute_253_1.inputs[0])
    test_group.links.new(reroute_251_1.outputs[0], reroute_254_1.inputs[0])
    test_group.links.new(reroute_252_1.outputs[0], reroute_255_1.inputs[0])
    test_group.links.new(reroute_253_1.outputs[0], combine_xyz_021_1.inputs[0])
    test_group.links.new(reroute_254_1.outputs[0], combine_xyz_021_1.inputs[1])
    test_group.links.new(reroute_255_1.outputs[0], combine_xyz_021_1.inputs[2])
    test_group.links.new(reroute_256_1.outputs[0], reroute_263_1.inputs[0])
    test_group.links.new(reroute_257_1.outputs[0], reroute_262_1.inputs[0])
    test_group.links.new(reroute_258_1.outputs[0], reroute_261_1.inputs[0])
    test_group.links.new(reroute_259_1.outputs[0], reroute_260_1.inputs[0])
    test_group.links.new(reroute_260_1.outputs[0], combine_xyz_022_1.inputs[2])
    test_group.links.new(reroute_261_1.outputs[0], combine_xyz_022_1.inputs[1])
    test_group.links.new(reroute_262_1.outputs[0], combine_xyz_022_1.inputs[0])
    test_group.links.new(reroute_263_1.outputs[0], combine_xyz_017_1.inputs[2])
    test_group.links.new(reroute_268_1.outputs[0], reroute_267_1.inputs[0])
    test_group.links.new(reroute_269_1.outputs[0], reroute_265_1.inputs[0])
    test_group.links.new(reroute_270_1.outputs[0], reroute_264_1.inputs[0])
    test_group.links.new(reroute_271_1.outputs[0], reroute_266_1.inputs[0])
    test_group.links.new(reroute_266_1.outputs[0], combine_xyz_023_1.inputs[2])
    test_group.links.new(reroute_264_1.outputs[0], combine_xyz_023_1.inputs[1])
    test_group.links.new(reroute_265_1.outputs[0], combine_xyz_023_1.inputs[0])
    test_group.links.new(reroute_267_1.outputs[0], combine_xyz_019_1.inputs[2])
    test_group.links.new(reroute_104_1.outputs[0], reroute_268_1.inputs[0])
    test_group.links.new(reroute_105_1.outputs[0], reroute_269_1.inputs[0])
    test_group.links.new(reroute_106_1.outputs[0], reroute_270_1.inputs[0])
    test_group.links.new(reroute_107_1.outputs[0], reroute_271_1.inputs[0])
    test_group.links.new(reroute_273_1.outputs[0], reroute_279_1.inputs[0])
    test_group.links.new(reroute_274_1.outputs[0], reroute_278_1.inputs[0])
    test_group.links.new(reroute_275_1.outputs[0], reroute_277_1.inputs[0])
    test_group.links.new(reroute_276_1.outputs[0], reroute_272_1.inputs[0])
    test_group.links.new(reroute_132_1.outputs[0], reroute_273_1.inputs[0])
    test_group.links.new(reroute_133_1.outputs[0], reroute_274_1.inputs[0])
    test_group.links.new(reroute_134_1.outputs[0], reroute_275_1.inputs[0])
    test_group.links.new(reroute_135_1.outputs[0], reroute_276_1.inputs[0])
    test_group.links.new(reroute_272_1.outputs[0], combine_xyz_026_1.inputs[2])
    test_group.links.new(reroute_277_1.outputs[0], combine_xyz_026_1.inputs[1])
    test_group.links.new(reroute_278_1.outputs[0], combine_xyz_026_1.inputs[0])
    test_group.links.new(reroute_279_1.outputs[0], combine_xyz_025_1.inputs[2])
    test_group.links.new(reroute_280_1.outputs[0], reroute_287_1.inputs[0])
    test_group.links.new(reroute_281_1.outputs[0], reroute_286_1.inputs[0])
    test_group.links.new(reroute_282_1.outputs[0], reroute_285_1.inputs[0])
    test_group.links.new(reroute_283_1.outputs[0], reroute_284_1.inputs[0])
    test_group.links.new(reroute_284_1.outputs[0], combine_xyz_027_1.inputs[2])
    test_group.links.new(reroute_285_1.outputs[0], combine_xyz_027_1.inputs[1])
    test_group.links.new(reroute_286_1.outputs[0], combine_xyz_027_1.inputs[0])
    test_group.links.new(reroute_287_1.outputs[0], combine_xyz_029_1.inputs[2])
    test_group.links.new(reroute_196_1.outputs[0], reroute_280_1.inputs[0])
    test_group.links.new(reroute_197_1.outputs[0], reroute_281_1.inputs[0])
    test_group.links.new(reroute_198_1.outputs[0], reroute_282_1.inputs[0])
    test_group.links.new(reroute_199_1.outputs[0], reroute_283_1.inputs[0])
    test_group.links.new(reroute_288_1.outputs[0], reroute_289_1.inputs[0])
    test_group.links.new(reroute_289_1.outputs[0], armor_primary_detail_diffuse_map_1.inputs[0])
    test_group.links.new(reroute_288_1.outputs[0], reroute_290_1.inputs[0])
    test_group.links.new(reroute_290_1.outputs[0], armor_secondary_detail_diffuse_map_1.inputs[0])
    test_group.links.new(reroute_291_1.outputs[0], reroute_292_1.inputs[0])
    test_group.links.new(reroute_292_1.outputs[0], armor_primary_detail_normal_map_1.inputs[0])
    test_group.links.new(reroute_291_1.outputs[0], reroute_293_1.inputs[0])
    test_group.links.new(reroute_293_1.outputs[0], armor_secondary_detail_normal_map_1.inputs[0])
    test_group.links.new(reroute_295_1.outputs[0], armor_detail_normal_transform_1.inputs[0])
    test_group.links.new(reroute_295_1.outputs[0], reroute_296_1.inputs[0])
    test_group.links.new(reroute_296_1.outputs[0], armor_detail_diffuse_transform_1.inputs[0])
    test_group.links.new(reroute_298_1.outputs[0], cloth_detail_normal_transform_1.inputs[0])
    test_group.links.new(reroute_299_1.outputs[0], cloth_detail_diffuse_transform_1.inputs[0])
    test_group.links.new(uv_map_1.outputs[0], reroute_300_1.inputs[0])
    test_group.links.new(reroute_300_1.outputs[0], reroute_299_1.inputs[0])
    test_group.links.new(reroute_300_1.outputs[0], reroute_298_1.inputs[0])
    test_group.links.new(reroute_299_1.outputs[0], reroute_295_1.inputs[0])
    test_group.links.new(reroute_298_1.outputs[0], reroute_301_1.inputs[0])
    test_group.links.new(reroute_301_1.outputs[0], suit_detail_diffuse_transform_1.inputs[0])
    test_group.links.new(reroute_301_1.outputs[0], reroute_302_1.inputs[0])
    test_group.links.new(reroute_302_1.outputs[0], suit_detail_normal_transform_1.inputs[0])
    test_group.links.new(reroute_294_1.outputs[0], reroute_307_1.inputs[0])
    test_group.links.new(reroute_297_1.outputs[0], reroute_308_1.inputs[0])
    test_group.links.new(reroute_303_1.outputs[0], reroute_309_1.inputs[0])
    test_group.links.new(reroute_304_1.outputs[0], reroute_310_1.inputs[0])
    test_group.links.new(reroute_305_1.outputs[0], reroute_311_1.inputs[0])
    test_group.links.new(reroute_306_1.outputs[0], reroute_312_1.inputs[0])
    test_group.links.new(reroute_307_1.outputs[0], mix_005_1.inputs[1])
    test_group.links.new(reroute_308_1.outputs[0], mix_005_1.inputs[2])
    test_group.links.new(reroute_309_1.outputs[0], mix_006_1.inputs[2])
    test_group.links.new(reroute_310_1.outputs[0], mix_007_1.inputs[2])
    test_group.links.new(reroute_311_1.outputs[0], mix_008_1.inputs[2])
    test_group.links.new(reroute_312_1.outputs[0], mix_009_1.inputs[2])
    test_group.links.new(reroute_318_1.outputs[0], reroute_306_1.inputs[0])
    test_group.links.new(reroute_317_1.outputs[0], reroute_305_1.inputs[0])
    test_group.links.new(reroute_316_1.outputs[0], reroute_304_1.inputs[0])
    test_group.links.new(reroute_315_1.outputs[0], reroute_303_1.inputs[0])
    test_group.links.new(reroute_314_1.outputs[0], reroute_297_1.inputs[0])
    test_group.links.new(reroute_313_1.outputs[0], reroute_294_1.inputs[0])
    test_group.links.new(reroute_019_1.outputs[0], reroute_313_1.inputs[0])
    test_group.links.new(reroute_047_1.outputs[0], reroute_314_1.inputs[0])
    test_group.links.new(reroute_075_1.outputs[0], reroute_315_1.inputs[0])
    test_group.links.new(reroute_103_1.outputs[0], reroute_316_1.inputs[0])
    test_group.links.new(reroute_131_1.outputs[0], reroute_317_1.inputs[0])
    test_group.links.new(reroute_195_1.outputs[0], reroute_318_1.inputs[0])
    test_group.links.new(reroute_319_1.outputs[0], reroute_323_1.inputs[0])
    test_group.links.new(reroute_320_1.outputs[0], reroute_321_1.inputs[0])
    test_group.links.new(reroute_321_1.outputs[0], cloth_primary_detail_normal_map_1.inputs[0])
    test_group.links.new(reroute_320_1.outputs[0], reroute_322_1.inputs[0])
    test_group.links.new(reroute_322_1.outputs[0], cloth_secondary_detail_normal_map_1.inputs[0])
    test_group.links.new(reroute_323_1.outputs[0], cloth_primary_detail_diffuse_map_1.inputs[0])
    test_group.links.new(reroute_319_1.outputs[0], reroute_324_1.inputs[0])
    test_group.links.new(reroute_324_1.outputs[0], cloth_secondary_detail_diffuse_map_1.inputs[0])
    test_group.links.new(reroute_328_1.outputs[0], reroute_327_1.inputs[0])
    test_group.links.new(reroute_325_1.outputs[0], reroute_330_1.inputs[0])
    test_group.links.new(reroute_325_1.outputs[0], reroute_326_1.inputs[0])
    test_group.links.new(reroute_328_1.outputs[0], reroute_329_1.inputs[0])
    test_group.links.new(suit_detail_normal_transform_1.outputs[0], reroute_325_1.inputs[0])
    test_group.links.new(suit_detail_diffuse_transform_1.outputs[0], reroute_328_1.inputs[0])
    test_group.links.new(reroute_330_1.outputs[0], suit_primary_detail_normal_map_1.inputs[0])
    test_group.links.new(reroute_326_1.outputs[0], suit_secondary_detail_normal_map_1.inputs[0])
    test_group.links.new(reroute_329_1.outputs[0], suit_secondary_detail_diffuse_map_1.inputs[0])
    test_group.links.new(reroute_327_1.outputs[0], suit_primary_detail_diffuse_map_1.inputs[0])
    test_group.links.new(reroute_007_1.outputs[0], combine_xyz_1.inputs[0])
    test_group.links.new(reroute_008_1.outputs[0], combine_xyz_1.inputs[1])
    test_group.links.new(reroute_009_1.outputs[0], combine_xyz_1.inputs[2])
    test_group.links.new(reroute_010_1.outputs[0], combine_xyz_001_1.inputs[0])
    test_group.links.new(combine_xyz_1.outputs[0], reroute_331_1.inputs[0])
    test_group.links.new(combine_xyz_001_1.outputs[0], reroute_332_1.inputs[0])
    test_group.links.new(reroute_331_1.outputs[0], reroute_333_1.inputs[0])
    test_group.links.new(reroute_332_1.outputs[0], reroute_334_1.inputs[0])
    test_group.links.new(reroute_333_1.outputs[0], reroute_335_1.inputs[0])
    test_group.links.new(reroute_334_1.outputs[0], reroute_336_1.inputs[0])
    test_group.links.new(reroute_335_1.outputs[0], mix_010_1.inputs[1])
    test_group.links.new(reroute_336_1.outputs[0], mix_015_1.inputs[1])
    test_group.links.new(mix_063_1.outputs[0], reroute_349_1.inputs[0])
    test_group.links.new(reroute_337_1.outputs[0], reroute_350_1.inputs[0])
    test_group.links.new(reroute_338_1.outputs[0], reroute_351_1.inputs[0])
    test_group.links.new(reroute_339_1.outputs[0], reroute_352_1.inputs[0])
    test_group.links.new(reroute_340_1.outputs[0], reroute_353_1.inputs[0])
    test_group.links.new(reroute_341_1.outputs[0], reroute_354_1.inputs[0])
    test_group.links.new(reroute_342_1.outputs[0], reroute_355_1.inputs[0])
    test_group.links.new(reroute_343_1.outputs[0], reroute_356_1.inputs[0])
    test_group.links.new(reroute_344_1.outputs[0], reroute_357_1.inputs[0])
    test_group.links.new(reroute_345_1.outputs[0], reroute_358_1.inputs[0])
    test_group.links.new(reroute_346_1.outputs[0], reroute_359_1.inputs[0])
    test_group.links.new(reroute_347_1.outputs[0], reroute_360_1.inputs[0])
    test_group.links.new(reroute_348_1.outputs[0], reroute_361_1.inputs[0])
    test_group.links.new(reroute_349_1.outputs[0], reroute_362_1.inputs[0])
    test_group.links.new(reroute_350_1.outputs[0], group_output_1.inputs[0])
    test_group.links.new(reroute_351_1.outputs[0], group_output_1.inputs[1])
    test_group.links.new(reroute_352_1.outputs[0], group_output_1.inputs[2])
    test_group.links.new(reroute_353_1.outputs[0], group_output_1.inputs[3])
    test_group.links.new(reroute_354_1.outputs[0], group_output_1.inputs[4])
    test_group.links.new(reroute_355_1.outputs[0], group_output_1.inputs[5])
    test_group.links.new(reroute_356_1.outputs[0], group_output_1.inputs[6])
    test_group.links.new(reroute_357_1.outputs[0], group_output_1.inputs[8])
    test_group.links.new(reroute_358_1.outputs[0], group_output_1.inputs[9])
    test_group.links.new(reroute_359_1.outputs[0], group_output_1.inputs[11])
    test_group.links.new(reroute_360_1.outputs[0], group_output_1.inputs[12])
    test_group.links.new(reroute_361_1.outputs[0], group_output_1.inputs[10])
    test_group.links.new(reroute_362_1.outputs[0], group_output_1.inputs[7])
    test_group.links.new(reroute_035_1.outputs[0], combine_xyz_002_1.inputs[0])
    test_group.links.new(reroute_036_1.outputs[0], combine_xyz_002_1.inputs[1])
    test_group.links.new(reroute_037_1.outputs[0], combine_xyz_002_1.inputs[2])
    test_group.links.new(reroute_038_1.outputs[0], combine_xyz_003_1.inputs[0])
    test_group.links.new(reroute_063_1.outputs[0], combine_xyz_005_1.inputs[0])
    test_group.links.new(reroute_064_1.outputs[0], combine_xyz_005_1.inputs[1])
    test_group.links.new(reroute_065_1.outputs[0], combine_xyz_005_1.inputs[2])
    test_group.links.new(reroute_066_1.outputs[0], combine_xyz_004_1.inputs[0])
    test_group.links.new(reroute_091_1.outputs[0], combine_xyz_007_1.inputs[0])
    test_group.links.new(reroute_092_1.outputs[0], combine_xyz_007_1.inputs[1])
    test_group.links.new(reroute_093_1.outputs[0], combine_xyz_007_1.inputs[2])
    test_group.links.new(reroute_094_1.outputs[0], combine_xyz_006_1.inputs[0])
    test_group.links.new(reroute_119_1.outputs[0], combine_xyz_008_1.inputs[0])
    test_group.links.new(reroute_120_1.outputs[0], combine_xyz_008_1.inputs[1])
    test_group.links.new(reroute_121_1.outputs[0], combine_xyz_008_1.inputs[2])
    test_group.links.new(reroute_122_1.outputs[0], combine_xyz_009_1.inputs[0])
    test_group.links.new(reroute_183_1.outputs[0], combine_xyz_010_1.inputs[0])
    test_group.links.new(reroute_184_1.outputs[0], combine_xyz_010_1.inputs[1])
    test_group.links.new(reroute_185_1.outputs[0], combine_xyz_010_1.inputs[2])
    test_group.links.new(reroute_186_1.outputs[0], combine_xyz_011_1.inputs[0])
    test_group.links.new(armor_secondary_fuzz_1.outputs[0], reroute_364_1.inputs[0])
    test_group.links.new(cloth_primary_fuzz_1.outputs[0], reroute_365_1.inputs[0])
    test_group.links.new(cloth_secondary_fuzz_1.outputs[0], reroute_366_1.inputs[0])
    test_group.links.new(suit_primary_fuzz_1.outputs[0], reroute_367_1.inputs[0])
    test_group.links.new(suit_secondary_transmission_1.outputs[0], reroute_192_1.inputs[0])
    test_group.links.new(suit_secondary_fuzz_1.outputs[0], reroute_368_1.inputs[0])
    test_group.links.new(reroute_012_1.outputs[0], combine_xyz_030_1.inputs[0])
    test_group.links.new(reroute_013_1.outputs[0], combine_xyz_030_1.inputs[1])
    test_group.links.new(reroute_014_1.outputs[0], combine_xyz_030_1.inputs[2])
    test_group.links.new(reroute_024_1.outputs[0], combine_xyz_032_1.inputs[0])
    test_group.links.new(reroute_025_1.outputs[0], combine_xyz_032_1.inputs[1])
    test_group.links.new(reroute_026_1.outputs[0], combine_xyz_032_1.inputs[2])
    test_group.links.new(reroute_027_1.outputs[0], reroute_371_1.inputs[0])
    test_group.links.new(reroute_015_1.outputs[0], reroute_372_1.inputs[0])
    test_group.links.new(reroute_016_1.outputs[0], combine_xyz_031_1.inputs[0])
    test_group.links.new(reroute_363_1.outputs[0], combine_xyz_031_1.inputs[1])
    test_group.links.new(reroute_017_1.outputs[0], combine_xyz_031_1.inputs[2])
    test_group.links.new(reroute_371_1.outputs[0], reroute_373_1.inputs[0])
    test_group.links.new(reroute_372_1.outputs[0], reroute_374_1.inputs[0])
    test_group.links.new(reroute_373_1.outputs[0], combine_xyz_001_1.inputs[2])
    test_group.links.new(reroute_374_1.outputs[0], combine_xyz_001_1.inputs[1])
    test_group.links.new(combine_xyz_002_1.outputs[0], reroute_380_1.inputs[0])
    test_group.links.new(reroute_380_1.outputs[0], reroute_375_1.inputs[0])
    test_group.links.new(reroute_375_1.outputs[0], reroute_381_1.inputs[0])
    test_group.links.new(reroute_376_1.outputs[0], reroute_382_1.inputs[0])
    test_group.links.new(reroute_377_1.outputs[0], reroute_383_1.inputs[0])
    test_group.links.new(reroute_378_1.outputs[0], reroute_384_1.inputs[0])
    test_group.links.new(reroute_379_1.outputs[0], reroute_385_1.inputs[0])
    test_group.links.new(reroute_043_1.outputs[0], reroute_386_1.inputs[0])
    test_group.links.new(reroute_055_1.outputs[0], reroute_389_1.inputs[0])
    test_group.links.new(reroute_386_1.outputs[0], reroute_387_1.inputs[0])
    test_group.links.new(reroute_387_1.outputs[0], combine_xyz_003_1.inputs[1])
    test_group.links.new(reroute_388_1.outputs[0], combine_xyz_003_1.inputs[2])
    test_group.links.new(reroute_389_1.outputs[0], reroute_388_1.inputs[0])
    test_group.links.new(reroute_071_1.outputs[0], reroute_390_1.inputs[0])
    test_group.links.new(reroute_083_1.outputs[0], reroute_393_1.inputs[0])
    test_group.links.new(reroute_390_1.outputs[0], reroute_391_1.inputs[0])
    test_group.links.new(reroute_391_1.outputs[0], combine_xyz_004_1.inputs[1])
    test_group.links.new(reroute_392_1.outputs[0], combine_xyz_004_1.inputs[2])
    test_group.links.new(reroute_393_1.outputs[0], reroute_392_1.inputs[0])
    test_group.links.new(reroute_099_1.outputs[0], reroute_394_1.inputs[0])
    test_group.links.new(reroute_111_1.outputs[0], reroute_397_1.inputs[0])
    test_group.links.new(reroute_394_1.outputs[0], reroute_395_1.inputs[0])
    test_group.links.new(reroute_395_1.outputs[0], combine_xyz_006_1.inputs[1])
    test_group.links.new(reroute_396_1.outputs[0], combine_xyz_006_1.inputs[2])
    test_group.links.new(reroute_397_1.outputs[0], reroute_396_1.inputs[0])
    test_group.links.new(reroute_127_1.outputs[0], reroute_398_1.inputs[0])
    test_group.links.new(reroute_139_1.outputs[0], reroute_401_1.inputs[0])
    test_group.links.new(reroute_398_1.outputs[0], reroute_399_1.inputs[0])
    test_group.links.new(reroute_399_1.outputs[0], combine_xyz_009_1.inputs[1])
    test_group.links.new(reroute_400_1.outputs[0], combine_xyz_009_1.inputs[2])
    test_group.links.new(reroute_401_1.outputs[0], reroute_400_1.inputs[0])
    test_group.links.new(reroute_190_1.outputs[0], reroute_402_1.inputs[0])
    test_group.links.new(reroute_203_1.outputs[0], reroute_405_1.inputs[0])
    test_group.links.new(reroute_402_1.outputs[0], reroute_403_1.inputs[0])
    test_group.links.new(reroute_403_1.outputs[0], combine_xyz_011_1.inputs[1])
    test_group.links.new(reroute_404_1.outputs[0], combine_xyz_011_1.inputs[2])
    test_group.links.new(reroute_405_1.outputs[0], reroute_404_1.inputs[0])
    test_group.links.new(combine_xyz_005_1.outputs[0], reroute_406_1.inputs[0])
    test_group.links.new(reroute_406_1.outputs[0], reroute_376_1.inputs[0])
    test_group.links.new(combine_xyz_007_1.outputs[0], reroute_407_1.inputs[0])
    test_group.links.new(reroute_407_1.outputs[0], reroute_377_1.inputs[0])
    test_group.links.new(combine_xyz_008_1.outputs[0], reroute_408_1.inputs[0])
    test_group.links.new(combine_xyz_010_1.outputs[0], reroute_409_1.inputs[0])
    test_group.links.new(reroute_408_1.outputs[0], reroute_378_1.inputs[0])
    test_group.links.new(reroute_409_1.outputs[0], reroute_379_1.inputs[0])
    test_group.links.new(reroute_410_1.outputs[0], reroute_415_1.inputs[0])
    test_group.links.new(reroute_411_1.outputs[0], reroute_416_1.inputs[0])
    test_group.links.new(reroute_412_1.outputs[0], reroute_417_1.inputs[0])
    test_group.links.new(reroute_413_1.outputs[0], reroute_418_1.inputs[0])
    test_group.links.new(combine_xyz_003_1.outputs[0], reroute_420_1.inputs[0])
    test_group.links.new(reroute_420_1.outputs[0], reroute_410_1.inputs[0])
    test_group.links.new(reroute_421_1.outputs[0], reroute_411_1.inputs[0])
    test_group.links.new(combine_xyz_004_1.outputs[0], reroute_421_1.inputs[0])
    test_group.links.new(reroute_424_1.outputs[0], reroute_414_1.inputs[0])
    test_group.links.new(reroute_423_1.outputs[0], reroute_413_1.inputs[0])
    test_group.links.new(reroute_422_1.outputs[0], reroute_412_1.inputs[0])
    test_group.links.new(reroute_414_1.outputs[0], reroute_419_1.inputs[0])
    test_group.links.new(combine_xyz_006_1.outputs[0], reroute_422_1.inputs[0])
    test_group.links.new(combine_xyz_009_1.outputs[0], reroute_423_1.inputs[0])
    test_group.links.new(combine_xyz_011_1.outputs[0], reroute_424_1.inputs[0])
    test_group.links.new(reroute_381_1.outputs[0], mix_010_1.inputs[2])
    test_group.links.new(reroute_382_1.outputs[0], mix_011_1.inputs[2])
    test_group.links.new(reroute_383_1.outputs[0], mix_012_1.inputs[2])
    test_group.links.new(reroute_384_1.outputs[0], mix_013_1.inputs[2])
    test_group.links.new(reroute_385_1.outputs[0], mix_014_1.inputs[2])
    test_group.links.new(reroute_415_1.outputs[0], mix_015_1.inputs[2])
    test_group.links.new(reroute_416_1.outputs[0], mix_016_1.inputs[2])
    test_group.links.new(reroute_417_1.outputs[0], mix_017_1.inputs[2])
    test_group.links.new(reroute_418_1.outputs[0], mix_018_1.inputs[2])
    test_group.links.new(reroute_419_1.outputs[0], mix_019_1.inputs[2])
    test_group.links.new(combine_xyz_030_1.outputs[0], reroute_425_1.inputs[0])
    test_group.links.new(reroute_425_1.outputs[0], reroute_426_1.inputs[0])
    test_group.links.new(reroute_426_1.outputs[0], reroute_432_1.inputs[0])
    test_group.links.new(reroute_427_1.outputs[0], reroute_433_1.inputs[0])
    test_group.links.new(reroute_428_1.outputs[0], reroute_434_1.inputs[0])
    test_group.links.new(reroute_429_1.outputs[0], reroute_435_1.inputs[0])
    test_group.links.new(reroute_430_1.outputs[0], reroute_436_1.inputs[0])
    test_group.links.new(reroute_431_1.outputs[0], reroute_437_1.inputs[0])
    test_group.links.new(reroute_441_1.outputs[0], reroute_431_1.inputs[0])
    test_group.links.new(reroute_440_1.outputs[0], reroute_430_1.inputs[0])
    test_group.links.new(reroute_439_1.outputs[0], reroute_429_1.inputs[0])
    test_group.links.new(reroute_438_1.outputs[0], reroute_428_1.inputs[0])
    test_group.links.new(reroute_442_1.outputs[0], reroute_427_1.inputs[0])
    test_group.links.new(reroute_040_1.outputs[0], combine_xyz_033_1.inputs[0])
    test_group.links.new(reroute_041_1.outputs[0], combine_xyz_033_1.inputs[1])
    test_group.links.new(reroute_042_1.outputs[0], combine_xyz_033_1.inputs[2])
    test_group.links.new(combine_xyz_033_1.outputs[0], reroute_442_1.inputs[0])
    test_group.links.new(reroute_068_1.outputs[0], combine_xyz_034_1.inputs[0])
    test_group.links.new(reroute_069_1.outputs[0], combine_xyz_034_1.inputs[1])
    test_group.links.new(reroute_070_1.outputs[0], combine_xyz_034_1.inputs[2])
    test_group.links.new(combine_xyz_034_1.outputs[0], reroute_438_1.inputs[0])
    test_group.links.new(reroute_095_1.outputs[0], combine_xyz_035_1.inputs[0])
    test_group.links.new(reroute_097_1.outputs[0], combine_xyz_035_1.inputs[1])
    test_group.links.new(reroute_098_1.outputs[0], combine_xyz_035_1.inputs[2])
    test_group.links.new(combine_xyz_035_1.outputs[0], reroute_439_1.inputs[0])
    test_group.links.new(reroute_123_1.outputs[0], combine_xyz_036_1.inputs[0])
    test_group.links.new(reroute_125_1.outputs[0], combine_xyz_036_1.inputs[1])
    test_group.links.new(reroute_126_1.outputs[0], combine_xyz_036_1.inputs[2])
    test_group.links.new(combine_xyz_036_1.outputs[0], reroute_440_1.inputs[0])
    test_group.links.new(reroute_187_1.outputs[0], combine_xyz_037_1.inputs[0])
    test_group.links.new(reroute_188_1.outputs[0], combine_xyz_037_1.inputs[1])
    test_group.links.new(reroute_189_1.outputs[0], combine_xyz_037_1.inputs[2])
    test_group.links.new(combine_xyz_037_1.outputs[0], reroute_441_1.inputs[0])
    test_group.links.new(reroute_432_1.outputs[0], mix_040_1.inputs[1])
    test_group.links.new(reroute_433_1.outputs[0], mix_040_1.inputs[2])
    test_group.links.new(reroute_434_1.outputs[0], mix_041_1.inputs[2])
    test_group.links.new(reroute_435_1.outputs[0], mix_042_1.inputs[2])
    test_group.links.new(reroute_436_1.outputs[0], mix_043_1.inputs[2])
    test_group.links.new(reroute_437_1.outputs[0], mix_044_1.inputs[2])
    test_group.links.new(reroute_459_1.outputs[0], reroute_448_1.inputs[0])
    test_group.links.new(reroute_458_1.outputs[0], reroute_447_1.inputs[0])
    test_group.links.new(reroute_457_1.outputs[0], reroute_446_1.inputs[0])
    test_group.links.new(reroute_456_1.outputs[0], reroute_445_1.inputs[0])
    test_group.links.new(reroute_455_1.outputs[0], reroute_444_1.inputs[0])
    test_group.links.new(reroute_460_1.outputs[0], reroute_443_1.inputs[0])
    test_group.links.new(reroute_005_1.outputs[0], reroute_460_1.inputs[0])
    test_group.links.new(reroute_033_1.outputs[0], reroute_455_1.inputs[0])
    test_group.links.new(reroute_061_1.outputs[0], reroute_456_1.inputs[0])
    test_group.links.new(reroute_089_1.outputs[0], reroute_457_1.inputs[0])
    test_group.links.new(reroute_117_1.outputs[0], reroute_458_1.inputs[0])
    test_group.links.new(reroute_179_1.outputs[0], reroute_459_1.inputs[0])
    test_group.links.new(reroute_443_1.outputs[0], reroute_466_1.inputs[0])
    test_group.links.new(reroute_444_1.outputs[0], reroute_461_1.inputs[0])
    test_group.links.new(reroute_445_1.outputs[0], reroute_462_1.inputs[0])
    test_group.links.new(reroute_446_1.outputs[0], reroute_463_1.inputs[0])
    test_group.links.new(reroute_447_1.outputs[0], reroute_464_1.inputs[0])
    test_group.links.new(reroute_448_1.outputs[0], reroute_465_1.inputs[0])
    test_group.links.new(reroute_466_1.outputs[0], mix_064_1.inputs[1])
    test_group.links.new(reroute_461_1.outputs[0], mix_064_1.inputs[2])
    test_group.links.new(reroute_462_1.outputs[0], mix_060_1.inputs[2])
    test_group.links.new(reroute_463_1.outputs[0], mix_061_1.inputs[2])
    test_group.links.new(reroute_464_1.outputs[0], mix_062_1.inputs[2])
    test_group.links.new(reroute_465_1.outputs[0], mix_063_1.inputs[2])
    test_group.links.new(reroute_471_1.outputs[0], reroute_453_1.inputs[0])
    test_group.links.new(reroute_470_1.outputs[0], reroute_452_1.inputs[0])
    test_group.links.new(reroute_469_1.outputs[0], reroute_451_1.inputs[0])
    test_group.links.new(reroute_468_1.outputs[0], reroute_450_1.inputs[0])
    test_group.links.new(reroute_467_1.outputs[0], reroute_449_1.inputs[0])
    test_group.links.new(reroute_472_1.outputs[0], reroute_454_1.inputs[0])
    test_group.links.new(reroute_454_1.outputs[0], reroute_478_1.inputs[0])
    test_group.links.new(reroute_449_1.outputs[0], reroute_473_1.inputs[0])
    test_group.links.new(reroute_450_1.outputs[0], reroute_474_1.inputs[0])
    test_group.links.new(reroute_451_1.outputs[0], reroute_475_1.inputs[0])
    test_group.links.new(reroute_452_1.outputs[0], reroute_476_1.inputs[0])
    test_group.links.new(reroute_453_1.outputs[0], reroute_477_1.inputs[0])
    test_group.links.new(reroute_478_1.outputs[0], mix_035_1.inputs[1])
    test_group.links.new(reroute_473_1.outputs[0], mix_035_1.inputs[2])
    test_group.links.new(reroute_474_1.outputs[0], mix_036_1.inputs[2])
    test_group.links.new(reroute_475_1.outputs[0], mix_037_1.inputs[2])
    test_group.links.new(reroute_476_1.outputs[0], mix_038_1.inputs[2])
    test_group.links.new(reroute_477_1.outputs[0], mix_039_1.inputs[2])
    test_group.links.new(reroute_011_1.outputs[0], reroute_472_1.inputs[0])
    test_group.links.new(reroute_039_1.outputs[0], reroute_467_1.inputs[0])
    test_group.links.new(reroute_067_1.outputs[0], reroute_468_1.inputs[0])
    test_group.links.new(reroute_096_1.outputs[0], reroute_469_1.inputs[0])
    test_group.links.new(reroute_124_1.outputs[0], reroute_470_1.inputs[0])
    test_group.links.new(reroute_194_1.outputs[0], reroute_471_1.inputs[0])
    test_group.links.new(reroute_489_1.outputs[0], reroute_484_1.inputs[0])
    test_group.links.new(reroute_488_1.outputs[0], reroute_483_1.inputs[0])
    test_group.links.new(reroute_487_1.outputs[0], reroute_482_1.inputs[0])
    test_group.links.new(reroute_486_1.outputs[0], reroute_481_1.inputs[0])
    test_group.links.new(reroute_485_1.outputs[0], reroute_480_1.inputs[0])
    test_group.links.new(reroute_370_1.outputs[0], reroute_369_1.inputs[0])
    test_group.links.new(combine_xyz_031_1.outputs[0], reroute_370_1.inputs[0])
    test_group.links.new(reroute_044_1.outputs[0], combine_xyz_038_1.inputs[0])
    test_group.links.new(reroute_364_1.outputs[0], combine_xyz_038_1.inputs[1])
    test_group.links.new(reroute_045_1.outputs[0], combine_xyz_038_1.inputs[2])
    test_group.links.new(combine_xyz_038_1.outputs[0], reroute_485_1.inputs[0])
    test_group.links.new(reroute_072_1.outputs[0], combine_xyz_039_1.inputs[0])
    test_group.links.new(reroute_365_1.outputs[0], combine_xyz_039_1.inputs[1])
    test_group.links.new(reroute_073_1.outputs[0], combine_xyz_039_1.inputs[2])
    test_group.links.new(combine_xyz_039_1.outputs[0], reroute_486_1.inputs[0])
    test_group.links.new(reroute_100_1.outputs[0], combine_xyz_040_1.inputs[0])
    test_group.links.new(reroute_366_1.outputs[0], combine_xyz_040_1.inputs[1])
    test_group.links.new(reroute_101_1.outputs[0], combine_xyz_040_1.inputs[2])
    test_group.links.new(combine_xyz_040_1.outputs[0], reroute_487_1.inputs[0])
    test_group.links.new(reroute_128_1.outputs[0], combine_xyz_041_1.inputs[0])
    test_group.links.new(reroute_367_1.outputs[0], combine_xyz_041_1.inputs[1])
    test_group.links.new(reroute_129_1.outputs[0], combine_xyz_041_1.inputs[2])
    test_group.links.new(combine_xyz_041_1.outputs[0], reroute_488_1.inputs[0])
    test_group.links.new(combine_xyz_042_1.outputs[0], reroute_489_1.inputs[0])
    test_group.links.new(reroute_191_1.outputs[0], combine_xyz_042_1.inputs[0])
    test_group.links.new(reroute_368_1.outputs[0], combine_xyz_042_1.inputs[1])
    test_group.links.new(reroute_192_1.outputs[0], combine_xyz_042_1.inputs[2])
    test_group.links.new(reroute_369_1.outputs[0], reroute_479_1.inputs[0])
    test_group.links.new(reroute_480_1.outputs[0], reroute_490_1.inputs[0])
    test_group.links.new(reroute_481_1.outputs[0], reroute_491_1.inputs[0])
    test_group.links.new(reroute_482_1.outputs[0], reroute_492_1.inputs[0])
    test_group.links.new(reroute_483_1.outputs[0], reroute_493_1.inputs[0])
    test_group.links.new(reroute_484_1.outputs[0], reroute_494_1.inputs[0])
    test_group.links.new(reroute_479_1.outputs[0], mix_045_1.inputs[1])
    test_group.links.new(reroute_490_1.outputs[0], mix_045_1.inputs[2])
    test_group.links.new(reroute_491_1.outputs[0], mix_046_1.inputs[2])
    test_group.links.new(reroute_492_1.outputs[0], mix_047_1.inputs[2])
    test_group.links.new(reroute_493_1.outputs[0], mix_048_1.inputs[2])
    test_group.links.new(reroute_494_1.outputs[0], mix_049_1.inputs[2])
    test_group.links.new(reroute_052_1.outputs[0], combine_xyz_043_1.inputs[0])
    test_group.links.new(reroute_053_1.outputs[0], combine_xyz_043_1.inputs[1])
    test_group.links.new(reroute_054_1.outputs[0], combine_xyz_043_1.inputs[2])
    test_group.links.new(reroute_080_1.outputs[0], combine_xyz_044_1.inputs[0])
    test_group.links.new(reroute_081_1.outputs[0], combine_xyz_044_1.inputs[1])
    test_group.links.new(reroute_082_1.outputs[0], combine_xyz_044_1.inputs[2])
    test_group.links.new(reroute_108_1.outputs[0], combine_xyz_045_1.inputs[0])
    test_group.links.new(reroute_109_1.outputs[0], combine_xyz_045_1.inputs[1])
    test_group.links.new(reroute_110_1.outputs[0], combine_xyz_045_1.inputs[2])
    test_group.links.new(reroute_136_1.outputs[0], combine_xyz_046_1.inputs[0])
    test_group.links.new(reroute_137_1.outputs[0], combine_xyz_046_1.inputs[1])
    test_group.links.new(reroute_138_1.outputs[0], combine_xyz_046_1.inputs[2])
    test_group.links.new(reroute_200_1.outputs[0], combine_xyz_047_1.inputs[0])
    test_group.links.new(reroute_201_1.outputs[0], combine_xyz_047_1.inputs[1])
    test_group.links.new(reroute_202_1.outputs[0], combine_xyz_047_1.inputs[2])
    test_group.links.new(reroute_503_1.outputs[0], reroute_500_1.inputs[0])
    test_group.links.new(reroute_502_1.outputs[0], reroute_501_1.inputs[0])
    test_group.links.new(reroute_504_1.outputs[0], reroute_496_1.inputs[0])
    test_group.links.new(reroute_505_1.outputs[0], reroute_497_1.inputs[0])
    test_group.links.new(reroute_506_1.outputs[0], reroute_498_1.inputs[0])
    test_group.links.new(reroute_507_1.outputs[0], reroute_499_1.inputs[0])
    test_group.links.new(reroute_500_1.outputs[0], reroute_509_1.inputs[0])
    test_group.links.new(reroute_501_1.outputs[0], reroute_508_1.inputs[0])
    test_group.links.new(reroute_496_1.outputs[0], reroute_510_1.inputs[0])
    test_group.links.new(reroute_497_1.outputs[0], reroute_511_1.inputs[0])
    test_group.links.new(reroute_498_1.outputs[0], reroute_512_1.inputs[0])
    test_group.links.new(reroute_499_1.outputs[0], reroute_513_1.inputs[0])
    test_group.links.new(reroute_509_1.outputs[0], mix_055_1.inputs[1])
    test_group.links.new(reroute_508_1.outputs[0], mix_055_1.inputs[2])
    test_group.links.new(reroute_510_1.outputs[0], mix_056_1.inputs[2])
    test_group.links.new(reroute_511_1.outputs[0], mix_057_1.inputs[2])
    test_group.links.new(reroute_512_1.outputs[0], mix_058_1.inputs[2])
    test_group.links.new(reroute_513_1.outputs[0], mix_059_1.inputs[2])
    test_group.links.new(combine_xyz_032_1.outputs[0], reroute_503_1.inputs[0])
    test_group.links.new(combine_xyz_043_1.outputs[0], reroute_502_1.inputs[0])
    test_group.links.new(combine_xyz_044_1.outputs[0], reroute_504_1.inputs[0])
    test_group.links.new(combine_xyz_045_1.outputs[0], reroute_505_1.inputs[0])
    test_group.links.new(combine_xyz_046_1.outputs[0], reroute_506_1.inputs[0])
    test_group.links.new(combine_xyz_047_1.outputs[0], reroute_507_1.inputs[0])
    test_group.links.new(reroute_524_1.outputs[0], reroute_495_1.inputs[0])
    test_group.links.new(reroute_519_1.outputs[0], reroute_514_1.inputs[0])
    test_group.links.new(reroute_520_1.outputs[0], reroute_515_1.inputs[0])
    test_group.links.new(reroute_521_1.outputs[0], reroute_516_1.inputs[0])
    test_group.links.new(reroute_522_1.outputs[0], reroute_517_1.inputs[0])
    test_group.links.new(reroute_523_1.outputs[0], reroute_518_1.inputs[0])
    test_group.links.new(reroute_495_1.outputs[0], reroute_530_1.inputs[0])
    test_group.links.new(reroute_514_1.outputs[0], reroute_529_1.inputs[0])
    test_group.links.new(reroute_515_1.outputs[0], reroute_528_1.inputs[0])
    test_group.links.new(reroute_516_1.outputs[0], reroute_527_1.inputs[0])
    test_group.links.new(reroute_517_1.outputs[0], reroute_526_1.inputs[0])
    test_group.links.new(reroute_518_1.outputs[0], reroute_525_1.inputs[0])
    test_group.links.new(reroute_530_1.outputs[0], mix_050_1.inputs[1])
    test_group.links.new(reroute_529_1.outputs[0], mix_050_1.inputs[2])
    test_group.links.new(reroute_528_1.outputs[0], mix_051_1.inputs[2])
    test_group.links.new(reroute_527_1.outputs[0], mix_052_1.inputs[2])
    test_group.links.new(reroute_526_1.outputs[0], mix_053_1.inputs[2])
    test_group.links.new(reroute_525_1.outputs[0], mix_054_1.inputs[2])
    test_group.links.new(reroute_018_1.outputs[0], reroute_524_1.inputs[0])
    test_group.links.new(reroute_046_1.outputs[0], reroute_519_1.inputs[0])
    test_group.links.new(reroute_074_1.outputs[0], reroute_520_1.inputs[0])
    test_group.links.new(reroute_102_1.outputs[0], reroute_521_1.inputs[0])
    test_group.links.new(reroute_130_1.outputs[0], reroute_522_1.inputs[0])
    test_group.links.new(reroute_193_1.outputs[0], reroute_523_1.inputs[0])
    test_group.links.new(armor_primary_fuzz_1.outputs[0], reroute_363_1.inputs[0])
    test_group.links.new(group_input_1.outputs[0], math_021_1.inputs[0])
    test_group.links.new(math_021_1.outputs[0], math_022_1.inputs[0])
    test_group.links.new(math_021_1.outputs[0], math_023_1.inputs[0])
    test_group.links.new(math_023_1.outputs[0], math_024_1.inputs[0])
    test_group.links.new(math_022_1.outputs[0], combine_rgb_1.inputs[0])
    test_group.links.new(math_024_1.outputs[0], combine_rgb_1.inputs[1])
    test_group.links.new(mix_071_1.outputs[0], separate_rgb_1.inputs[0])
    test_group.links.new(combine_rgb_1.outputs[0], mix_071_1.inputs[2])
    test_group.links.new(math_021_1.outputs[0], mix_071_1.inputs[0])
    test_group.links.new(attribute_1.outputs[0], mix_071_1.inputs[1])
    # -----------------------------------------------------------------------------------------
    # Armor Transforms
    armor_detail_diffuse_transform_1.inputs[1].default_value = (DiffTrans1.Z, DiffTrans1.W, 0.0)  # position
    armor_detail_diffuse_transform_1.inputs[3].default_value = (DiffTrans1.X, DiffTrans1.Y, 1.0)  # scale
    armor_detail_normal_transform_1.inputs[1].default_value = (NormTrans1.Z, NormTrans1.W, 1.0)  # scale
    armor_detail_normal_transform_1.inputs[3].default_value = (NormTrans1.X, NormTrans1.Y, 0.0)  # position

    # Armor Primary
    armor_primary_dye_color_1.outputs[0].default_value = (CPrime1.X, CPrime1.Y, CPrime1.Z, 1.0)
    armor_primary_roughness_remap_x_1.outputs[0].default_value = PrimeRoughMap1.X
    armor_primary_roughness_remap_y_1.outputs[0].default_value = PrimeRoughMap1.Y
    armor_primary_roughness_remap_z_1.outputs[0].default_value = PrimeRoughMap1.Z
    armor_primary_roughness_remap_w_1.outputs[0].default_value = PrimeRoughMap1.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/DiffMap1"), check_existing=False)
    DetailDiffuse01 = bpy.data.images.get("DiffMap1")
    DetailDiffuse01.colorspace_settings.name = "sRGB"
    DetailDiffuse01.alpha_mode = "CHANNEL_PACKED"
    armor_primary_detail_diffuse_map_1.image = DetailDiffuse01

    armor_primary_wear_remap_x_1.outputs[0].default_value = PrimeWearMap1.X
    armor_primary_wear_remap_y_1.outputs[0].default_value = PrimeWearMap1.Y
    armor_primary_wear_remap_z_1.outputs[0].default_value = PrimeWearMap1.Z
    armor_primary_wear_remap_w_1.outputs[0].default_value = PrimeWearMap1.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/NormMap1"), check_existing=False)
    DetailNormal01 = bpy.data.images.get("NormMap1")
    DetailNormal01.colorspace_settings.name = "Non-Color"
    DetailNormal01.alpha_mode = "CHANNEL_PACKED"
    armor_primary_detail_normal_map_1.image = DetailNormal01

    armor_primary_detail_diffuse_blend_1.outputs[0].default_value = PrimeMatParams1.X
    armor_primary_detail_normal_blend_1.outputs[0].default_value = PrimeMatParams1.Y
    armor_primary_detail_roughness_blend_1.outputs[0].default_value = PrimeMatParams1.Z
    armor_primary_metalness_1.outputs[0].default_value = PrimeMatParams1.W
    armor_primary_iridescence_1.outputs[0].default_value = PrimeAdvMatParams1.X
    armor_primary_fuzz_1.outputs[0].default_value = PrimeAdvMatParams1.Y
    armor_primary_transmission_1.outputs[0].default_value = PrimeAdvMatParams1.Z

    worn_armor_primary_dye_color_1.outputs[0].default_value = (CPrimeWear1.X, CPrimeWear1.Y, CPrimeWear1.Z, 1.0)
    worn_armor_primary_roughness_remap_x_1.outputs[0].default_value = PrimeWornRoughMap1.X
    worn_armor_primary_roughness_remap_y_1.outputs[0].default_value = PrimeWornRoughMap1.Y
    worn_armor_primary_roughness_remap_z_1.outputs[0].default_value = PrimeWornRoughMap1.Z
    worn_armor_primary_roughness_remap_w_1.outputs[0].default_value = PrimeWornRoughMap1.W
    worn_armor_primary_detail_diffuse_blend_1.outputs[0].default_value = PrimeWornMatParams1.X
    worn_armor_primary_detail_normal_blend_1.outputs[0].default_value = PrimeWornMatParams1.Y
    worn_armor_primary_detail_roughness_blend_1.outputs[0].default_value = PrimeWornMatParams1.Z
    worn_armor_primary_metalness_1.outputs[0].default_value = PrimeWornMatParams1.W

    armor_primary_emission_color_1.outputs[0].default_value = (CPrimeEmit1.X, CPrimeEmit1.Y, CPrimeEmit1.Z, 1.0)

    # Armor Secondary
    armor_secondary_dye_color_1.outputs[0].default_value = (CSecon1.X, CSecon1.Y, CSecon1.Z, 1.0)
    armor_secondary_roughness_remap_x_1.outputs[0].default_value = SeconRoughMap1.X
    armor_secondary_roughness_remap_y_1.outputs[0].default_value = SeconRoughMap1.Y
    armor_secondary_roughness_remap_z_1.outputs[0].default_value = SeconRoughMap1.Z
    armor_secondary_roughness_remap_w_1.outputs[0].default_value = SeconRoughMap1.W

    bpy.data.images.load(os.path.join(riplocation,"OUTPUTPATH/DiffMap1"), check_existing=False)
    DetailDiffuse02 = bpy.data.images.get("DiffMap1")
    DetailDiffuse02.colorspace_settings.name = "sRGB"
    DetailDiffuse02.alpha_mode = "CHANNEL_PACKED"
    armor_secondary_detail_diffuse_map_1.image = DetailDiffuse02

    armor_secondary_wear_remap_x_1.outputs[0].default_value = SeconWearMap1.X
    armor_secondary_wear_remap_y_1.outputs[0].default_value = SeconWearMap1.Y
    armor_secondary_wear_remap_z_1.outputs[0].default_value = SeconWearMap1.Z
    armor_secondary_wear_remap_w_1.outputs[0].default_value = SeconWearMap1.W

    bpy.data.images.load(os.path.join(riplocation,"OUTPUTPATH/NormMap1"), check_existing=False)
    DetailNormal02 = bpy.data.images.get("NormMap1")
    DetailNormal02.colorspace_settings.name = "Non-Color"
    DetailNormal02.alpha_mode = "CHANNEL_PACKED"
    armor_secondary_detail_normal_map_1.image = DetailNormal02

    armor_secondary_detail_diffuse_blend_1.outputs[0].default_value = SeconMatParams1.X
    armor_secondary_detail_normal_blend_1.outputs[0].default_value = SeconMatParams1.Y
    armor_secondary_detail_roughness_blend_1.outputs[0].default_value = SeconMatParams1.Z
    armor_secondary_metalness_1.outputs[0].default_value = SeconMatParams1.W
    armor_secondary_iridescence_1.outputs[0].default_value = SeconAdvMatParams1.X
    armor_secondary_fuzz_1.outputs[0].default_value = SeconAdvMatParams1.Y
    armor_secondary_transmission_1.outputs[0].default_value = SeconAdvMatParams1.Z

    worn_armor_secondary_dye_color_1.outputs[0].default_value = (CSeconWear1.X, CSeconWear1.Y, CSeconWear1.Z, 1.0)
    worn_armor_secondary_roughness_remap_x_1.outputs[0].default_value = SeconWornRoughMap1.X
    worn_armor_secondary_roughness_remap_y_1.outputs[0].default_value = SeconWornRoughMap1.Y
    worn_armor_secondary_roughness_remap_z_1.outputs[0].default_value = SeconWornRoughMap1.Z
    worn_armor_secondary_roughness_remap_w_1.outputs[0].default_value = SeconWornRoughMap1.W
    worn_armor_secondary_detail_diffuse_blend_1.outputs[0].default_value = SeconWornMatParams1.X
    worn_armor_secondary_detail_normal_blend_1.outputs[0].default_value = SeconWornMatParams1.Y
    worn_armor_secondary_detail_roughness_blend_1.outputs[0].default_value = SeconWornMatParams1.Z
    worn_armor_secondary_metalness_1.outputs[0].default_value = SeconWornMatParams1.W

    armor_secondary_emission_color_1.outputs[0].default_value = (CSeconEmit1.X, CSeconEmit1.Y, CSeconEmit1.Z, 1.0)

    # Cloth Transforms
    cloth_detail_diffuse_transform_1.inputs[1].default_value = (DiffTrans2.Z, DiffTrans2.W, 1.0) # scale
    cloth_detail_diffuse_transform_1.inputs[3].default_value = (DiffTrans2.X, DiffTrans2.Y, 0.0) # position
    cloth_detail_normal_transform_1.inputs[1].default_value = (NormTrans2.Z, NormTrans2.W, 1.0) # scale
    cloth_detail_normal_transform_1.inputs[3].default_value = (NormTrans2.X, NormTrans2.Y, 0.0) # position

    # Cloth Primary
    cloth_primary_dye_color_1.outputs[0].default_value = (CPrime2.X, CPrime2.Y, CPrime2.Z, 1.0)
    cloth_primary_roughness_remap_x_1.outputs[0].default_value = PrimeRoughMap2.X
    cloth_primary_roughness_remap_y_1.outputs[0].default_value = PrimeRoughMap2.Y
    cloth_primary_roughness_remap_z_1.outputs[0].default_value = PrimeRoughMap2.Z
    cloth_primary_roughness_remap_w_1.outputs[0].default_value = PrimeRoughMap2.W

    bpy.data.images.load(os.path.join(riplocation,"OUTPUTPATH/DiffMap2"), check_existing=False)
    DetailDiffuse03 = bpy.data.images.get("DiffMap2")
    DetailDiffuse03.colorspace_settings.name = "sRGB"
    DetailDiffuse03.alpha_mode = "CHANNEL_PACKED"
    cloth_primary_detail_diffuse_map_1.image = DetailDiffuse03

    cloth_primary_wear_remap_x_1.outputs[0].default_value = PrimeWearMap2.X
    cloth_primary_wear_remap_y_1.outputs[0].default_value = PrimeWearMap2.Y
    cloth_primary_wear_remap_z_1.outputs[0].default_value = PrimeWearMap2.Z
    cloth_primary_wear_remap_w_1.outputs[0].default_value = PrimeWearMap2.W

    bpy.data.images.load(os.path.join(riplocation,"OUTPUTPATH/NormMap2"), check_existing=False)
    DetailNormal03 = bpy.data.images.get("NormMap2")
    DetailNormal03.colorspace_settings.name = "Non-Color"
    DetailNormal03.alpha_mode = "CHANNEL_PACKED"
    cloth_primary_detail_normal_map_1.image = DetailNormal03

    cloth_primary_detail_diffuse_blend_1.outputs[0].default_value = PrimeMatParams2.X
    cloth_primary_detail_normal_blend_1.outputs[0].default_value = PrimeMatParams2.Y
    cloth_primary_detail_roughness_blend_1.outputs[0].default_value = PrimeMatParams2.Z
    cloth_primary_metalness_1.outputs[0].default_value = PrimeMatParams2.W
    cloth_primary_iridescence_1.outputs[0].default_value = PrimeAdvMatParams2.X
    cloth_primary_fuzz_1.outputs[0].default_value = PrimeAdvMatParams2.Y
    cloth_primary_transmission_1.outputs[0].default_value = PrimeAdvMatParams2.Z

    worn_cloth_primary_dye_color_1.outputs[0].default_value = (CPrimeWear2.X, CPrimeWear2.Y, CPrimeWear2.Z, 1.0)
    worn_cloth_primary_roughness_remap_x_1.outputs[0].default_value = PrimeWornRoughMap2.X
    worn_cloth_primary_roughness_remap_y_1.outputs[0].default_value = PrimeWornRoughMap2.Y
    worn_cloth_primary_roughness_remap_z_1.outputs[0].default_value = PrimeWornRoughMap2.Z
    worn_cloth_primary_roughness_remap_w_1.outputs[0].default_value = PrimeWornRoughMap2.W
    worn_cloth_primary_detail_diffuse_blend_1.outputs[0].default_value = PrimeWornMatParams1.X
    worn_cloth_primary_detail_normal_blend_1.outputs[0].default_value = PrimeWornMatParams1.Y
    worn_cloth_primary_detail_roughness_blend_1.outputs[0].default_value = PrimeWornMatParams1.Z
    worn_cloth_primary_metalness_1.outputs[0].default_value = PrimeWornMatParams1.W

    cloth_primary_emission_color_1.outputs[0].default_value = (CPrimeEmit2.X, CPrimeEmit2.Y, CPrimeEmit2.Z, 1.0)

    # Cloth Secondary
    cloth_secondary_dye_color_1.outputs[0].default_value = (CSecon2.X, CSecon2.Y, CSecon2.Z, 1.0)
    cloth_secondary_roughness_remap_x_1.outputs[0].default_value = SeconRoughMap2.X
    cloth_secondary_roughness_remap_y_1.outputs[0].default_value = SeconRoughMap2.Y
    cloth_secondary_roughness_remap_z_1.outputs[0].default_value = SeconRoughMap2.Z
    cloth_secondary_roughness_remap_w_1.outputs[0].default_value = SeconRoughMap2.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/DiffMap2"), check_existing=False)
    DetailDiffuse04 = bpy.data.images.get("DiffMap2")
    DetailDiffuse04.colorspace_settings.name = "sRGB"
    DetailDiffuse04.alpha_mode = "CHANNEL_PACKED"
    cloth_secondary_detail_diffuse_map_1.image = DetailDiffuse04

    cloth_secondary_wear_remap_x_1.outputs[0].default_value = SeconWearMap2.X
    cloth_secondary_wear_remap_y_1.outputs[0].default_value = SeconWearMap2.Y
    cloth_secondary_wear_remap_z_1.outputs[0].default_value = SeconWearMap2.Z
    cloth_secondary_wear_remap_w_1.outputs[0].default_value = SeconWearMap2.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/NormMap2"), check_existing=False)
    DetailNormal04 = bpy.data.images.get("NormMap2")
    DetailNormal04.colorspace_settings.name = "Non-Color"
    DetailNormal04.alpha_mode = "CHANNEL_PACKED"
    cloth_secondary_detail_normal_map_1.image = DetailNormal04

    cloth_secondary_detail_diffuse_blend_1.outputs[0].default_value = SeconMatParams2.X
    cloth_secondary_detail_normal_blend_1.outputs[0].default_value = SeconMatParams2.Y
    cloth_secondary_detail_roughness_blend_1.outputs[0].default_value = SeconMatParams2.Z
    cloth_secondary_metalness_1.outputs[0].default_value = SeconMatParams2.W
    cloth_secondary_iridescence_1.outputs[0].default_value = SeconAdvMatParams2.X
    cloth_secondary_fuzz_1.outputs[0].default_value = SeconAdvMatParams2.Y
    cloth_secondary_transmission_1.outputs[0].default_value = SeconAdvMatParams2.Z

    worn_cloth_secondary_dye_color_1.outputs[0].default_value = (CSeconWear2.X, CSeconWear2.Y, CSeconWear2.Z, 1.0)
    worn_cloth_secondary_roughness_remap_x_1.outputs[0].default_value = SeconWornRoughMap2.X
    worn_cloth_secondary_roughness_remap_y_1.outputs[0].default_value = SeconWornRoughMap2.Y
    worn_cloth_secondary_roughness_remap_z_1.outputs[0].default_value = SeconWornRoughMap2.Z
    worn_cloth_secondary_roughness_remap_w_1.outputs[0].default_value = SeconWornRoughMap2.W
    worn_cloth_secondary_detail_diffuse_blend_1.outputs[0].default_value = SeconWornMatParams2.X
    worn_cloth_secondary_detail_normal_blend_1.outputs[0].default_value = SeconWornMatParams2.Y
    worn_cloth_secondary_detail_roughness_blend_1.outputs[0].default_value = SeconWornMatParams2.Z
    worn_cloth_secondary_metalness_1.outputs[0].default_value = SeconWornMatParams2.W

    cloth_secondary_emission_color_1.outputs[0].default_value = (CSeconEmit2.X, CSeconEmit2.Y, CSeconEmit2.Z, 1.0)

    # Suit Transform
    suit_detail_diffuse_transform_1.inputs[1].default_value = (DiffTrans3.Z, DiffTrans3.W, 1.0)  # scale
    suit_detail_diffuse_transform_1.inputs[3].default_value = (DiffTrans3.X, DiffTrans3.Y, 0.0)  # position
    suit_detail_normal_transform_1.inputs[1].default_value = (NormTrans3.Z, NormTrans3.W, 1.0)  # scale
    suit_detail_normal_transform_1.inputs[3].default_value = (NormTrans3.X, NormTrans3.Y, 0.0)  # position

    # Suit Primary
    suit_primary_dye_color_1.outputs[0].default_value = (CPrime3.X, CPrime3.Y, CPrime3.Z, 1.0)
    suit_primary_roughness_remap_x_1.outputs[0].default_value = PrimeRoughMap3.X
    suit_primary_roughness_remap_y_1.outputs[0].default_value = PrimeRoughMap3.Y
    suit_primary_roughness_remap_z_1.outputs[0].default_value = PrimeRoughMap3.Z
    suit_primary_roughness_remap_w_1.outputs[0].default_value = PrimeRoughMap3.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/DiffMap3"), check_existing=False)
    DetailDiffuse05 = bpy.data.images.get("DiffMap3")
    DetailDiffuse05.colorspace_settings.name = "sRGB"
    DetailDiffuse05.alpha_mode = "CHANNEL_PACKED"
    suit_primary_detail_diffuse_map_1.image = DetailDiffuse05

    suit_primary_wear_remap_x_1.outputs[0].default_value = PrimeWearMap3.X
    suit_primary_wear_remap_y_1.outputs[0].default_value = PrimeWearMap3.Y
    suit_primary_wear_remap_z_1.outputs[0].default_value = PrimeWearMap3.Z
    suit_primary_wear_remap_w_1.outputs[0].default_value = PrimeWearMap3.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/NormMap3"), check_existing=False)
    DetailNormal05 = bpy.data.images.get("NormMap3")
    DetailNormal05.colorspace_settings.name = "Non-Color"
    DetailNormal05.alpha_mode = "CHANNEL_PACKED"
    suit_primary_detail_normal_map_1.image = DetailNormal05

    suit_primary_detail_diffuse_blend_1.outputs[0].default_value = PrimeMatParams3.X
    suit_primary_detail_normal_blend_1.outputs[0].default_value = PrimeMatParams3.Y
    suit_primary_detail_roughness_blend_1.outputs[0].default_value = PrimeMatParams3.Z
    suit_primary_metalness_1.outputs[0].default_value = PrimeMatParams3.W
    suit_primary_iridescence_1.outputs[0].default_value = PrimeAdvMatParams3.X
    suit_primary_fuzz_1.outputs[0].default_value = PrimeAdvMatParams3.Y
    suit_primary_transmission_1.outputs[0].default_value = PrimeAdvMatParams3.Z

    worn_suit_primary_dye_color_1.outputs[0].default_value = (CPrimeWear3.X, CPrimeWear3.Y, CPrimeWear3.Z, 1.0)
    worn_suit_primary_roughness_remap_x_1.outputs[0].default_value = PrimeWornRoughMap3.X
    worn_suit_primary_roughness_remap_y_1.outputs[0].default_value = PrimeWornRoughMap3.Y
    worn_suit_primary_roughness_remap_z_1.outputs[0].default_value = PrimeWornRoughMap3.Z
    worn_suit_primary_roughness_remap_w_1.outputs[0].default_value = PrimeWornRoughMap3.W
    worn_suit_primary_detail_diffuse_blend_1.outputs[0].default_value = PrimeWornMatParams3.X
    worn_suit_primary_detail_normal_blend_1.outputs[0].default_value = PrimeWornMatParams3.Y
    worn_suit_primary_detail_roughness_blend_1.outputs[0].default_value = PrimeWornMatParams3.Z
    worn_suit_primary_metalness_1.outputs[0].default_value = PrimeWornMatParams3.W

    suit_primary_emission_color_1.outputs[0].default_value = (CPrimeEmit3.X, CPrimeEmit3.Y, CPrimeEmit3.Z, 1.0)

    # Suit Secondary
    suit_secondary_dye_color_1.outputs[0].default_value = (CSecon3.X, CSecon3.Y, CSecon3.Z, 1.0)
    suit_secondary_roughness_remap_x_1.outputs[0].default_value = SeconRoughMap3.X
    suit_secondary_roughness_remap_y_1.outputs[0].default_value = SeconRoughMap3.Y
    suit_secondary_roughness_remap_z_1.outputs[0].default_value = SeconRoughMap3.Z
    suit_secondary_roughness_remap_w_1.outputs[0].default_value = SeconRoughMap3.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/DiffMap3"), check_existing=False)
    DetailDiffuse06 = bpy.data.images.get("DiffMap3")
    DetailDiffuse06.colorspace_settings.name = "sRGB"
    DetailDiffuse06.alpha_mode = "CHANNEL_PACKED"
    suit_secondary_detail_diffuse_map_1.image = DetailDiffuse06

    suit_secondary_wear_remap_x_1.outputs[0].default_value = SeconWearMap3.X
    suit_secondary_wear_remap_y_1.outputs[0].default_value = SeconWearMap3.Y
    suit_secondary_wear_remap_z_1.outputs[0].default_value = SeconWearMap3.Z
    suit_secondary_wear_remap_w_1.outputs[0].default_value = SeconWearMap3.W

    bpy.data.images.load(os.path.join(riplocation, "OUTPUTPATH/NormMap3"), check_existing=False)
    DetailNormal06 = bpy.data.images.get("NormMap3")
    DetailNormal06.colorspace_settings.name = "Non-Color"
    DetailNormal06.alpha_mode = "CHANNEL_PACKED"
    suit_secondary_detail_normal_map_1.image = DetailNormal06

    suit_secondary_detail_diffuse_blend_1.outputs[0].default_value = SeconMatParams3.X
    suit_secondary_detail_normal_blend_1.outputs[0].default_value = SeconMatParams3.Y
    suit_secondary_detail_roughness_blend_1.outputs[0].default_value = SeconMatParams3.Z
    suit_secondary_metalness_1.outputs[0].default_value = SeconMatParams3.W
    suit_secondary_iridescence_1.outputs[0].default_value = SeconAdvMatParams3.X
    suit_secondary_fuzz_1.outputs[0].default_value = SeconAdvMatParams3.Y
    suit_secondary_transmission_1.outputs[0].default_value = SeconAdvMatParams3.Z

    worn_suit_secondary_dye_color_1.outputs[0].default_value = (CSeconWear3.X, CSeconWear3.Y, CSeconWear3.Z, 1.0)
    worn_suit_secondary_roughness_remap_x_1.outputs[0].default_value = SeconWornRoughMap3.X
    worn_suit_secondary_roughness_remap_y_1.outputs[0].default_value = SeconWornRoughMap3.Y
    worn_suit_secondary_roughness_remap_z_1.outputs[0].default_value = SeconWornRoughMap3.Z
    worn_suit_secondary_roughness_remap_w_1.outputs[0].default_value = SeconWornRoughMap3.W
    worn_suit_secondary_detail_diffuse_blend_1.outputs[0].default_value = SeconWornMatParams3.X
    worn_suit_secondary_detail_normal_blend_1.outputs[0].default_value = SeconWornMatParams3.Y
    worn_suit_secondary_detail_roughness_blend_1.outputs[0].default_value = SeconWornMatParams3.Z
    worn_suit_secondary_metalness_1.outputs[0].default_value = SeconWornMatParams3.W

    suit_secondary_emission_color_1.outputs[0].default_value = (CSeconEmit3.X, CSeconEmit3.Y, CSeconEmit3.Z, 1.0)

    return test_group


class NODE_OT_TEST(bpy.types.Operator):
    bl_label = "Add D2 Weapons/Armor Shader"
    bl_idname = "node.test_operator"

    def execute(self, context):
        # BIOS Change Nodegroup name dependent on name of shader ripped from API
        custom_node_name = "SHADERNAMEENUM"
        global RIP_LOCATION
        my_group = create_test_group(self, context, custom_node_name, RIP_LOCATION)
        test_node = context.view_layer.objects.active.active_material.node_tree.nodes.new('ShaderNodeGroup')
        test_node.node_tree = bpy.data.node_groups[my_group.name]
        test_node.use_custom_color = True
        test_node.color = (0.101, 0.170, 0.297)

        return {'FINISHED'}


def register():
    global RIP_LOCATION
    RIP_LOCATION = os.path.abspath(bpy.context.space_data.text.filepath)
    bpy.utils.register_class(NODE_PT_MAINPANEL)
    bpy.utils.register_class(NODE_OT_TEST)


def unregister():
    bpy.utils.unregister_class(NODE_PT_MAINPANEL)
    bpy.utils.unregister_class(NODE_OT_TEST)


if __name__ == "__main__":
    register()
