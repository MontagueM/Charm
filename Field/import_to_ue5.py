import unreal
import os
import json


class CharmImporter:
    def __init__(self, folder_path: str, b_unique_folder: bool) -> None:
        self.folder_path = folder_path
        info_name = f"{__file__.split('/')[-1].split('_')[0]}_info.cfg"
        self.config = json.load(open(self.folder_path + f"/{info_name}"))
        if b_unique_folder:
            self.content_path = f"{self.config['UnrealInteropPath']}/{self.config['MeshName']}"
        else:
            self.content_path = f"{self.config['UnrealInteropPath']}"
        if not unreal.EditorAssetLibrary.does_directory_exist(self.content_path):
            unreal.EditorAssetLibrary.make_directory(self.content_path)

    def import_entity(self):
        self.make_materials()
        self.import_entity_mesh()
        self.assign_entity_materials()
        unreal.EditorAssetLibrary.save_directory(f"/Game/{self.content_path}/", False)

    def import_static(self):
        self.make_materials()
        self.import_static_mesh(combine=True)
        self.assign_static_materials()
        unreal.EditorAssetLibrary.save_directory(f"/Game/{self.content_path}/", False)

    def import_map(self):
        self.make_materials()
        self.import_static_mesh(combine=False)
        self.assign_map_materials()
        self.assemble_map()
        unreal.EditorAssetLibrary.save_directory(f"/Game/{self.content_path}/", False)
        
    def assemble_map(self) -> None:
        # Create new level asset
        unreal.EditorLevelLibrary.new_level(f'/Game/{self.content_path}/map_{self.config["MeshName"]}')
        
        static_names = {}
        for x in unreal.EditorAssetLibrary.list_assets(f'/Game/{self.content_path}/Statics/', recursive=False):
            if "Group" in x:
                name = x.split('/')[-1].split("_")[1]
            else:
                name = x.split('/')[-1].split(".")[0]
            if name not in static_names.keys():
                static_names[name] = []
            static_names[name].append(x)

        for static, instances in self.config["Instances"].items():
            try:  # fix this
                parts = static_names[static]
            except:
                print(f"Failed on {static}")
            for part in parts:
                sm = unreal.EditorAssetLibrary.load_asset(part)
                for instance in instances:
                    quat = unreal.Quat(instance["Rotation"][0], instance["Rotation"][1], instance["Rotation"][2], instance["Rotation"][3])
                    euler = quat.euler()
                    rotator = unreal.Rotator(-euler.x+180, -euler.y+180, -euler.z)
                    location = [-instance["Translation"][0]*100, instance["Translation"][1]*100, instance["Translation"][2]*100]
                    s = unreal.EditorLevelLibrary.spawn_actor_from_object(sm, location=location, rotation=rotator)  # l must be UE4 Object
                    s.set_actor_label(s.get_actor_label() + f"_{instance['Scale']}")
                    s.set_actor_relative_scale3d([instance["Scale"]]*3)


        # for i, a in enumerate(assets):
        #     name = a.split('.')[0].split('/')[-1].split(origin_folder)[-1][1:]
        #     # s = name.split('_')
        #     # print(s)
        #     # continue
        #     sm = unreal.EditorAssetLibrary.load_asset(a)
        #     # instance_component = unreal.HierarchicalInstancedStaticMeshComponent()
        #     # instance_component.set_editor_property("static_mesh", sm)
        #     try:
        #         data = helper[name]
        #     except KeyError:
        #         continue
        #     # transforms = unreal.Array(unreal.Transform)
        #     for d in data:
        #         r = d[1]
        #         l = d[0]
        #         l = [-l[0]*100, l[1]*100, l[2]*100]
        #         rotator = unreal.Rotator(-r[0], r[1], -r[2])
        #         # transform = rotator.transform()
        #         # transform.set_editor_property("translation", l)
        #         # transform.set_editor_property("scale3d", [d[2]]*3)
        #         # transforms.append(transform)
        #         s = unreal.EditorLevelLibrary.spawn_actor_from_object(sm, location=l, rotation=rotator)  # l must be UE4 Object
        #         s.set_actor_scale3d([d[2]]*3)
        # 
        #     # instance_component.add_instances(transforms, False)
        #     # unreal.EditorAssetLibrary.duplicate_asset(template_path + "HLODTemplate", f"/Game/{data_path}/actors/{name}")
        #     # actorbp = unreal.EditorAssetLibrary.load_asset(f"/Game/{data_path}/actors/{name}")
        #     # actor_spawn = unreal.EditorAssetLibrary.load_blueprint_class(f"/Game/{data_path}/actors/{name}")
        #     # actor = unreal.EditorLevelLibrary.spawn_actor_from_class(actor_spawn, location=[0, 0, 0])
        #     # actor.set_actor_label(name, True)
        # 
        #     # instance_component.attach_to_component(actor.root_component, ' ', unreal.AttachmentRule.KEEP_WORLD,
        #     #                                        unreal.AttachmentRule.KEEP_WORLD, unreal.AttachmentRule.KEEP_WORLD,
        #     #                                        False)
        #     # actor.set_editor_property('root_component', instance_component)
        unreal.EditorLevelLibrary.save_current_level()

    def assign_map_materials(self) -> None:
        for x in unreal.EditorAssetLibrary.list_assets(f'/Game/{self.content_path}/Statics/', recursive=False):
            # Identify static mesh
            mesh = unreal.load_asset(x)

            # Check material slots and compare names from config
            mesh_materials = mesh.get_editor_property("static_materials")
            material_slot_name_dict = {x: unreal.load_asset(f"/Game/{self.config['UnrealInteropPath']}/Materials/M_{y}") for x, y in self.config["Parts"].items()}
            new_mesh_materials = []
            for skeletal_material in mesh_materials:
                slot_name = skeletal_material.get_editor_property("material_slot_name").__str__()
                slot_name = '_'.join(slot_name.split('_')[:-1])
                if slot_name in material_slot_name_dict.keys():
                    if material_slot_name_dict[slot_name] != None:
                        skeletal_material.set_editor_property("material_interface", material_slot_name_dict[slot_name])
                new_mesh_materials.append(skeletal_material)
            print(new_mesh_materials)
            mesh.set_editor_property("static_materials", new_mesh_materials)
    
    def assign_static_materials(self) -> None:
        # Identify static mesh
        mesh = unreal.load_asset(f"/Game/{self.content_path}/{self.config['MeshName']}")

        # Check material slots and compare names from config
        mesh_materials = mesh.get_editor_property("static_materials")
        material_slot_name_dict = {x: unreal.load_asset(f"/Game/{self.config['UnrealInteropPath']}/Materials/M_{y}") for x, y in self.config["Parts"].items()}
        new_mesh_materials = []
        for skeletal_material in mesh_materials:
            slot_name = skeletal_material.get_editor_property("material_slot_name").__str__()
            slot_name = '_'.join(slot_name.split('_')[:-1])
            if slot_name in material_slot_name_dict.keys():
                if material_slot_name_dict[slot_name] != None:
                    skeletal_material.set_editor_property("material_interface", material_slot_name_dict[slot_name])
            new_mesh_materials.append(skeletal_material)
        print(new_mesh_materials)
        mesh.set_editor_property("static_materials", new_mesh_materials)

    def assign_entity_materials(self) -> None:
        # Identify entity mesh
        mesh = unreal.load_asset(f"/Game/{self.content_path}/{self.config['MeshName']}")

        # Check material slots and compare names from config
        mesh_materials = mesh.get_editor_property("materials")
        material_slot_name_dict = {x: unreal.load_asset(f"/Game/{self.config['UnrealInteropPath']}/Materials/M_{y}") for x, y in self.config["Parts"].items()}
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
        # todo fix this, not static mesh import data
        options.static_mesh_import_data.set_editor_property('convert_scene', False)
        options.static_mesh_import_data.set_editor_property('combine_meshes', False)
        options.static_mesh_import_data.set_editor_property('generate_lightmap_u_vs', False)
        options.static_mesh_import_data.set_editor_property('auto_generate_collision', False)
        options.static_mesh_import_data.set_editor_property("vertex_color_import_option", unreal.VertexColorImportOption.REPLACE)
        options.static_mesh_import_data.set_editor_property("build_nanite", False)  # todo add nanite option
        task.set_editor_property("options", options)

        unreal.AssetToolsHelpers.get_asset_tools().import_asset_tasks([task])
        
    def import_static_mesh(self, combine) -> None:
        task = unreal.AssetImportTask()
        task.set_editor_property("automated", True)
        task.set_editor_property("destination_path", f"/Game/{self.content_path}/Statics/")
        task.set_editor_property("filename", f"{self.folder_path}/{self.config['MeshName']}.fbx")
        task.set_editor_property("replace_existing", True)
        task.set_editor_property("save", True)
    
        options = unreal.FbxImportUI()
        options.set_editor_property('import_mesh', True)
        options.set_editor_property('import_textures', False)
        options.set_editor_property('import_materials', False)
        options.set_editor_property('import_as_skeletal', False)
        options.static_mesh_import_data.set_editor_property('convert_scene', False)
        options.static_mesh_import_data.set_editor_property('import_uniform_scale', 100.0)
        options.static_mesh_import_data.set_editor_property('combine_meshes', combine)
        options.static_mesh_import_data.set_editor_property('generate_lightmap_u_vs', False)
        options.static_mesh_import_data.set_editor_property('auto_generate_collision', False)
        options.static_mesh_import_data.set_editor_property('normal_import_method', unreal.FBXNormalImportMethod.FBXNIM_IMPORT_NORMALS)
        options.static_mesh_import_data.set_editor_property("vertex_color_import_option", unreal.VertexColorImportOption.REPLACE)
        options.static_mesh_import_data.set_editor_property("build_nanite", False)  # todo add nanite option
        task.set_editor_property("options", options)
    
        unreal.AssetToolsHelpers.get_asset_tools().import_asset_tasks([task])

    def make_materials(self) -> None:
        # Get all materials we need
        materials = list(self.config["Materials"].keys())

        # Check if materials exist already
        existing_materials = [x.split('/')[-1].split('.')[0][2:] for x in unreal.EditorAssetLibrary.list_assets(f'/Game/{self.config["UnrealInteropPath"]}/Materials/', recursive=False) if unreal.EditorAssetLibrary.find_asset_data(x).asset_class == 'Material']
        materials_to_make = list(set(materials)-set(existing_materials))

        # If doesn't exist, make
        for mat in materials_to_make:
            material = self.make_material(mat)
            unreal.MaterialEditingLibrary.recompile_material(material)

    def make_material(self, matstr: str) -> unreal.Material:
        # Make base material
        material = unreal.AssetToolsHelpers.get_asset_tools().create_asset("M_" + matstr, f"/Game/{self.config['UnrealInteropPath']}/Materials", unreal.Material, unreal.MaterialFactoryNew())

        if os.path.exists(f"{self.folder_path}/Shaders/PS_{matstr}.usf"):
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
        unreal.MaterialEditingLibrary.connect_material_property(mat_att, "EmissiveColor", unreal.MaterialProperty.MP_EMISSIVE_COLOR)
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
        code = open(f"{self.folder_path}/Shaders/PS_{matstr}.usf", "r").read()

        # If the material is masked, change its blend mode for alpha + make it two-sided
        if "// masked" in code:
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
        # Only pixel shader for now, todo replace .dds with the extension
        names = [f"{self.folder_path}/Textures/PS_{i}_{texstruct['Hash']}.dds" for i, texstruct in self.config["Materials"][matstr]["PS"].items()]
        srgbs = {int(i): texstruct['SRGB'] for i, texstruct in self.config["Materials"][matstr]["PS"].items()}
        import_tasks = []
        for name in names:
            asset_import_task = unreal.AssetImportTask()
            asset_import_task.set_editor_property('filename', name)
            asset_import_task.set_editor_property('destination_path', f'/Game/{self.content_path}/Textures')
            asset_import_task.set_editor_property('save', True)
            asset_import_task.set_editor_property('replace_existing', False)  # dont do extra work if we dont need to
            asset_import_task.set_editor_property('automated', True)
            import_tasks.append(asset_import_task)

        unreal.AssetToolsHelpers.get_asset_tools().import_asset_tasks(import_tasks)

        # Make texture samples
        for i, texstruct in self.config["Materials"][matstr]["PS"].items():
            i = int(i)
            texture_sample = unreal.MaterialEditingLibrary.create_material_expression(material, unreal.MaterialExpressionTextureSample, -1000, -500 + 250 * i)

            ts_TextureUePath = f"/Game/{self.content_path}/Textures/PS_{i}_{texstruct['Hash']}.PS_{i}_{texstruct['Hash']}"
            ts_LoadedTexture = unreal.EditorAssetLibrary.load_asset(ts_TextureUePath)
            if not ts_LoadedTexture:  # some cubemaps and 3d textures cannot be loaded for now
                continue
            ts_LoadedTexture.set_editor_property('srgb', srgbs[i])
            if srgbs[i] == True:
                ts_LoadedTexture.set_editor_property('compression_settings', unreal.TextureCompressionSettings.TC_DEFAULT)
            else:
                ts_LoadedTexture.set_editor_property('compression_settings', unreal.TextureCompressionSettings.TC_VECTOR_DISPLACEMENTMAP)

            texture_sample.set_editor_property('texture', ts_LoadedTexture)
            if texstruct['SRGB'] == True:
                texture_sample.set_editor_property("sampler_type", unreal.MaterialSamplerType.SAMPLERTYPE_COLOR)
            else:
                texture_sample.set_editor_property("sampler_type", unreal.MaterialSamplerType.SAMPLERTYPE_LINEAR_COLOR)
            texture_samples[i] = texture_sample

            unreal.EditorAssetLibrary.save_loaded_asset(ts_LoadedTexture)

        return texture_samples

    """
    Updates all materials used by this model to the latest .usfs found in the Shaders/ folder.
    Very useful for improving the material quality without much manual work.
    """
    def update_material_code(self) -> None:
        # Get all materials to update
        materials = list(self.config["Materials"].keys())

        # For each material, find the code node and update it
        mats = {unreal.EditorAssetLibrary.load_asset(f"/Game/{self.config['UnrealInteropPath']}/Materials/M_{matstr}"): matstr for matstr in materials}
        it = unreal.ObjectIterator()
        for x in it:
            if x.get_outer() in mats:
                if isinstance(x, unreal.MaterialExpressionCustom):
                    code = open(f"{self.folder_path}/Shaders/PS_{mats[x.get_outer()]}.usf", "r").read()
                    x.set_editor_property('code', code)
                    print(f"Updated material {mats[x.get_outer()]}")

        unreal.EditorAssetLibrary.save_directory(f"/Game/{self.content_path}/Materials/", False)


if __name__ == "__main__":
    importer = CharmImporter(os.path.dirname(os.path.realpath(__file__)), b_unique_folder=False)
    importer.import_entity()
    # importer.update_material_code()