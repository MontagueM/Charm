import bpy
import json
import mathutils
import os

#Adapted from Monteven's UE5 import script


#Globally get all the objects in the scene
objects = bpy.data.objects
vis_objects = [ob for ob in bpy.context.view_layer.objects if ob.visible_get()]
scene = bpy.context.scene
BPY = bpy.ops

#####
Name = "MAP_HASH"
Filepath = "OUTPUT_DIR"
#####

info_name = Name + "_info.cfg" 
config = json.load(open(Filepath + f"/{info_name}"))
FileName = Filepath + "\\" + Name + ".fbx" 

original_statics = {} #original static objects

def assemble_map():
    print("Starting import on map: " + Name)
    
    #Grab all the objects currently in the scene
    oldobjects = bpy.data.objects.items()

    BPY.import_scene.fbx(filepath=FileName)
    #add_to_collection()
    
    #Now get all the objects in the scene after the import
    newobjects = bpy.data.objects.items()

    for ob in oldobjects:
        if ob[0] not in newobjects:
            #print("Removing: " + ob[0])
            newobjects.remove(ob) #Removes every object from the list that wasnt imported (makes things not instance multiple times?)

    print("Imported map: " + Name)
    print("Instancing...")

    static_names = {}
    for x in newobjects:
        obj_name = x[0][:8]
   
        if obj_name not in static_names.keys():
            static_names[obj_name] = []
        static_names[obj_name].append(x)

    for static, instances in config["Instances"].items():
        try:  # fix this
            parts = static_names[static]
        except:
            print(f"Failed on {static}")
        for part in parts:
            for instance in instances:

                ob_copy = part[1].copy()
                bpy.context.collection.objects.link(ob_copy) #makes the instances?

                location = [instance["Translation"][0], instance["Translation"][1], instance["Translation"][2]]
                quat = mathutils.Quaternion([instance["Rotation"][3], instance["Rotation"][0], instance["Rotation"][1], instance["Rotation"][2]])

                ob_copy.location = location
                ob_copy.rotation_mode = 'QUATERNION'
                ob_copy.rotation_quaternion = quat
               
                ob_copy.scale = [instance["Scale"]]*3
    
    print("Assigning materials...")
    assign_map_materials()

def assign_map_materials():
        materials = bpy.data.materials
        
        static_names = {}
        for x in objects: #find the original static objects and add them to static_names
            part = x.name.rpartition('.')
            obj_name = part[0]
            if obj_name not in static_names.keys():
                static_names[obj_name] = []
                original_statics[obj_name] = []

            static_names[obj_name].append(x)
            original_statics[obj_name].append(x)
            
        for staticname, matname in config["Parts"].items(): #assign the objects matching material from the config file
            if staticname in static_names.keys():
                obj = bpy.data.objects[str(staticname)]
                for slt in obj.material_slots:
                    if matname in materials.keys():
                        slt.material = materials[matname]
                        print(f"Existing material {matname} assigned to {staticname}")
                    else: 
                        if slt.material.name != matname:
                            slt.material.name = matname

        for obj in bpy.data.objects: #remove any duplicate materials that may have been created
            for slt in obj.material_slots:
                part = slt.name.rpartition('.')
                if part[2].isnumeric() and part[0] in materials:
                    slt.material = materials.get(part[0])
           

        #Get all the images in the directory and load them
        for img in os.listdir(Filepath + "/Textures/"):
            if img.endswith("TEX_EXT"):
                for i in bpy.data.images:
                    if i == img:
                        break
                bpy.data.images.load(Filepath + "/Textures/" + f"/{img}", check_existing = True)
                print(f"Loaded {img}")

        for mat, ps in config["Materials"].items():
            for ps, texnum in ps.items():
                for texnum, hash in texnum.items():
                    for hash, image in hash.items():
                        try:
                            matnodes = materials[mat].node_tree.nodes
                            texnode = matnodes.new('ShaderNodeTexImage')
                            texnode.hide = True
                            texnode.location = (-370.0, -200.0 + (float(texnum)*1.1)*50) #shitty offsetting

                            texture = bpy.data.images.get("PS_" + texnum + "_" + image[:8] + "TEX_EXT")
                            texture.colorspace_settings.name = "sRGB"
                            texture.alpha_mode = "CHANNEL_PACKED"
                            texnode.image = texture
                            
                            #assign a texture to material's diffuse just to help a little 
                            if int(texnum) == 0 and texnode.image == texture:
                                link_diffuse(materials[mat])
                            break
                        except:
                            continue
            continue
                       

def find_nodes_by_type(material, node_type):
    """ Return a list of all of the nodes in the material
        that match the node type.
        Return an empty list if the material doesn't use
        nodes or doesn't have a tree.
    """
    node_list = []
    if material.use_nodes and material.node_tree:
            for n in material.node_tree.nodes:
                if n.type == node_type:
                    node_list.append(n)
    return node_list

def link_diffuse(material):
    """ Find at least one image texture in the material
        and at least one Principled shader.
        If they both exist and neither have a link to
        the relevant alpha socket, connect them.
        There are many ways this can fail.
        if there's no image; if there's no principled
        shader; if the selected image/principled sockets
        are already in use.
        Returns false on any detected error.
        Does not try alternatives if there are multiple
        images or multiple principled shaders.
    """
    #print(f'processing material {material.name}')
    it_list = find_nodes_by_type(material, 'TEX_IMAGE')
    # if len(it_list) > 1:
    #     print(f'{material.name}: too many image textures. Trying the first one')
    s_list = find_nodes_by_type(material, 'BSDF_PRINCIPLED')
    if len(s_list) == 0:
        #print(f'{material.name}: no principled shader.')
        return False
    # if len(s_list) != 1:
    #     print(f'{material.name}: too many principled shaders. Trying the first one')
    image_node = it_list[0]
    shader_node = s_list[0]
    #print(f'{material.name}: Attempting to connect {image_node.name} diffuse to {shader_node.name}')
    image_socket = image_node.outputs['Color']
    shader_socket = shader_node.inputs['Base Color']
   
    material.node_tree.links.new(shader_socket, image_socket)
    #return True
                       

def cleanup():
    print(f"Cleaning up...")
    #Delete all the objects in original_statics
    for x in objects:
        if x.name in original_statics.keys():
            bpy.data.objects.remove(x)

    #Removes unused data such as duplicate images, materials, etc.
    for block in bpy.data.meshes:
        if block.users == 0:
            bpy.data.meshes.remove(block)

    for block in bpy.data.materials:
        if block.users == 0:
            bpy.data.materials.remove(block)

    for block in bpy.data.textures:
        if block.users == 0:
            bpy.data.textures.remove(block)

    for block in bpy.data.images:
        if block.users == 0:
            bpy.data.images.remove(block)
    print("Done cleaning up!")

def add_to_collection(): #Needs to be fixed
    obj = bpy.context.selected_objects
    #obj_old_coll = obj.users_collection #list of all collection the obj is in

    new_coll = bpy.data.collections.new(name="COLLECTION TEST") #create new coll in data
    bpy.context.scene.collection.children.link(new_coll) #add new coll to the scene
    new_coll.objects.link(obj) #link obj to scene

    #for ob in obj_old_coll: #unlink from all  precedent obj collections
        #ob.objects.unlink(obj)


if __name__ == "__main__":
    #Deselect all objects just in case 
    bpy.ops.object.select_all(action='DESELECT')

    assemble_map()
    assign_map_materials()
    cleanup()