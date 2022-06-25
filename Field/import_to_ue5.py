import unreal
import os
import json


class CharmImporter:
    def __init__(self, folder_path: str) -> None:
        self.folder_path = folder_path
        self.config = json.load(open(self.folder_path + "/info.cfg"))
        self.content_path = self.config["UnrealInteropPath"]

def import_entity(self):
        self.make_materials()
        self.import_entity_mesh()
        self.assign_materials()

    def assign_materials(self) -> None:
        # Identify entity mesh
        mesh = unreal.load_asset(f"/Game/{self.content_path}/{self.config['MeshName']}")

        # Check material slots and compare names from config
        mesh_materials = mesh.get_editor_property("materials")
        material_slot_name_dict = {x: unreal.load_asset(f"/Game/{self.content_path}/Materials/M_{y}") for x, y in self.config["Parts"].items()}
        new_mesh_materials = []
        for skeletal_material in mesh_materials:
            slot_name = skeletal_material.get_editor_property("material_slot_name").__str__()
            slot_name = '_'.join(slot_name.split('_')[:-1])
            if slot_name in material_slot_name_dict.keys():
                if material_slot_name_dict[slot_name] != None:
                    skeletal_material.set_editor_property("material_interface", material_slot_name_dict[slot_name])
            new_mesh_materials.append(skeletal_material)
        print(new_mesh_materials)
        mesh.set_editor_property("materials", new_mesh_materials)

    def import_entity_mesh(self) -> None:
        task = unreal.AssetImportTask()
        task.set_editor_property("automated", True)
        task.set_editor_property("destination_path", f"/Game/{self.content_path}/")
        task.set_editor_property("filename", f"{self.folder_path}/{self.config['MeshName']}.fbx")
        task.set_editor_property("replace_existing", True)
        task.set_editor_property("save", True)

        options = unreal.FbxImportUI()
        options.set_editor_property('import_mesh', True)
        options.set_editor_property('import_textures', False)
        options.set_editor_property('import_materials', False)
        options.set_editor_property('import_as_skeletal', True)
        options.static_mesh_import_data.set_editor_property('convert_scene', False)
        options.static_mesh_import_data.set_editor_property('combine_meshes', False)
        options.static_mesh_import_data.set_editor_property('generate_lightmap_u_vs', False)
        options.static_mesh_import_data.set_editor_property('auto_generate_collision', False)
        options.static_mesh_import_data.set_editor_property("vertex_color_import_option", unreal.VertexColorImportOption.REPLACE)
        options.static_mesh_import_data.set_editor_property("build_nanite", False)  # todo add nanite option
        task.set_editor_property("options", options)

        unreal.AssetToolsHelpers.get_asset_tools().import_asset_tasks([task])

    def make_materials(self) -> None:
        # Get all materials we need
        materials = list(self.config["Materials"].keys())

        # Check if materials exist already
        existing_materials = [x.split('/')[-1].split('.')[0][2:] for x in unreal.EditorAssetLibrary.list_assets(f'/Game/{self.content_path}/Materials/', recursive=False) if unreal.EditorAssetLibrary.find_asset_data(x).asset_class == 'Material']
        materials_to_make = list(set(materials)-set(existing_materials))

        # If doesn't exist, make
        for mat in materials_to_make:
            material = self.make_material(mat)
            unreal.MaterialEditingLibrary.recompile_material(material)
            unreal.EditorAssetLibrary.save_loaded_asset(material)

    def make_material(self, matstr: str) -> unreal.Material:
        # Make base material
        material = unreal.AssetToolsHelpers.get_asset_tools().create_asset("M_" + matstr, f"/Game/{self.content_path}/Materials", unreal.Material, unreal.MaterialFactoryNew())

        if os.path.exists(f"{self.folder_path}/PS_{matstr}.usf"):
            # Add textures
            texture_samples = self.add_textures(material, matstr)

            # Add custom node
            custom_node = self.add_custom_node(material, texture_samples, matstr)

            # Set output, not using in-built custom expression system because I want to leave it open for manual control
            self.create_output(material, custom_node)
        else:
            material.set_editor_property("blend_mode", unreal.BlendMode.BLEND_MASKED)
            const = unreal.MaterialEditingLibrary.create_material_expression(material, unreal.MaterialExpressionConstant, -300, 0)
            unreal.MaterialEditingLibrary.connect_material_property(const, "", unreal.MaterialProperty.MP_OPACITY_MASK)

        return material

    def create_output(self, material: unreal.Material, custom_node: unreal.MaterialExpressionCustom) -> None:
        mat_att = unreal.MaterialEditingLibrary.create_material_expression(material, unreal.MaterialExpressionBreakMaterialAttributes, -300, 0)
        # Connect custom node to the new break
        unreal.MaterialEditingLibrary.connect_material_expressions(custom_node, '', mat_att, 'Attr')
        # Connect all outputs
        unreal.MaterialEditingLibrary.connect_material_property(mat_att, "BaseColor", unreal.MaterialProperty.MP_BASE_COLOR)
        unreal.MaterialEditingLibrary.connect_material_property(mat_att, "Metallic", unreal.MaterialProperty.MP_METALLIC)
        unreal.MaterialEditingLibrary.connect_material_property(mat_att, "Roughness", unreal.MaterialProperty.MP_ROUGHNESS)
        unreal.MaterialEditingLibrary.connect_material_property(mat_att, "OpacityMask", unreal.MaterialProperty.MP_OPACITY_MASK)
        unreal.MaterialEditingLibrary.connect_material_property(mat_att, "Normal", unreal.MaterialProperty.MP_NORMAL)
        unreal.MaterialEditingLibrary.connect_material_property(mat_att, "AmbientOcclusion", unreal.MaterialProperty.MP_AMBIENT_OCCLUSION)

    def add_custom_node(self, material: unreal.Material, texture_samples: list, matstr: str) -> unreal.MaterialExpressionCustom:
        # Get sorted list of textures
        sorted_texture_indices = list(sorted([int(x) for x in self.config["Materials"][matstr]["PS"].keys()]))
        sorted_texture_vars = [f"t{x}" for x in sorted_texture_indices]

        custom_node = unreal.MaterialEditingLibrary.create_material_expression(material, unreal.MaterialExpressionCustom, -500, 0)

        # Definitions

        # Check the material shader exists
        code = ""
        f = open(f"{self.folder_path}/PS_{matstr}.usf", "r").read()
        code = f

        # If the material is masked, change its blend mode for alpha + make it two-sided
        if "// masked" in f:
            material.set_editor_property("blend_mode", unreal.BlendMode.BLEND_MASKED)
            material.set_editor_property("two_sided", True)

        
        inputs = []
        for tvar in sorted_texture_vars:
            ci = unreal.CustomInput()
            ci.set_editor_property('input_name', tvar)
            inputs.append(ci)
        ci = unreal.CustomInput()
        ci.set_editor_property('input_name', 'tx')
        inputs.append(ci)

        custom_node.set_editor_property('code', code)
        custom_node.set_editor_property('inputs', inputs)
        custom_node.set_editor_property('output_type', unreal.CustomMaterialOutputType.CMOT_MATERIAL_ATTRIBUTES)

        for i, t in texture_samples.items():
            unreal.MaterialEditingLibrary.connect_material_expressions(t, 'RGBA', custom_node, f't{i}')
        texcoord = unreal.MaterialEditingLibrary.create_material_expression(material, unreal.MaterialExpressionTextureCoordinate, -500, 300)
        unreal.MaterialEditingLibrary.connect_material_expressions(texcoord, '', custom_node, 'tx')

        return custom_node

    def add_textures(self,  material: unreal.Material, matstr: str) -> dict:
        texture_samples = {}

        # Import texture list for the material

        tex_factory = unreal.TextureFactory()
        tex_factory.set_editor_property('supported_class', unreal.Texture2D)
        # Only pixel shader for now
        names = [f"{self.folder_path}/PS_{i}_{texstruct['Hash']}.dds" for i, texstruct in self.config["Materials"][matstr]["PS"].items()]
        srgbs = [texstruct['SRGB'] for i, texstruct in self.config["Materials"][matstr]["PS"].items()]
        task = unreal.AutomatedAssetImportData()
            
        task.set_editor_property('filenames', names)
        task.set_editor_property('destination_path', f'/Game/{self.content_path}/Textures')
        task.set_editor_property('replace_existing', False)  # dont do extra work if we dont need to
        
        textures = unreal.AssetToolsHelpers.get_asset_tools().import_assets_automated(task)
        unreal.EditorAssetLibrary.save_loaded_assets(textures)
        for i, t in enumerate(textures):
            t.set_editor_property('srgb', srgbs[i])
            if srgbs[i] == True:
                t.set_editor_property('compression_settings', unreal.TextureCompressionSettings.TC_DEFAULT)
            else:
                t.set_editor_property('compression_settings', unreal.TextureCompressionSettings.TC_VECTOR_DISPLACEMENTMAP)

        # Make texture samples
        for i, texstruct in self.config["Materials"][matstr]["PS"].items():
            i = int(i)
            texture_sample = unreal.MaterialEditingLibrary.create_material_expression(material, unreal.MaterialExpressionTextureSample, -1000, -500 + 250 * i)

            ts_TextureUePath = f"/Game/{self.content_path}/Textures/PS_{i}_{texstruct['Hash']}.PS_{i}_{texstruct['Hash']}"
            ts_LoadedTexture = unreal.EditorAssetLibrary.load_asset(ts_TextureUePath)
            texture_sample.set_editor_property('texture', ts_LoadedTexture)
            if texstruct['SRGB'] == True:
                texture_sample.set_editor_property("sampler_type", unreal.MaterialSamplerType.SAMPLERTYPE_COLOR)
            else:
                texture_sample.set_editor_property("sampler_type", unreal.MaterialSamplerType.SAMPLERTYPE_LINEAR_COLOR)
            texture_samples[i] = texture_sample

        return texture_samples


if __name__ == "__main__":
    importer = CharmImporter(os.path.dirname(os.path.realpath(__file__)))
    importer.import_entity()