import bpy
import json
import mathutils
import os
#MAYBE TO DO: Add color sampling so non-color textures that arent normals dont connect to the normal node?
#https://www.geeksforgeeks.org/find-most-used-colors-in-image-using-python/


#Adapted from Monteven's UE5 import script

#Globally gets all the objects in the scene
objects = bpy.data.objects
vis_objects = [ob for ob in bpy.context.view_layer.objects if ob.visible_get()]
scene = bpy.context.scene
BPY = bpy.ops

#Info
Name = "MAP_HASH"
Filepath = os.path.abspath(bpy.context.space_data.text.filepath+"/..") #"OUTPUT_DIR"
#

#Files to open
info_name = Name + "_info.cfg"
config = json.load(open(Filepath + f"\\{info_name}"))
FileName = Filepath + "\\" + Name + ".fbx" 
#

original_statics = {} #original static objects

def assemble_map():
    print("Starting import on map: " + Name)
    
    #Grab all the objects currently in the scene
    oldobjects = bpy.data.objects.items()

    BPY.import_scene.fbx(filepath=FileName) #Just imports the fbx, no special settings needed
    
    #Now grabs all the objects in the scene after the import
    newobjects = bpy.data.objects.items()

    for ob in oldobjects:
        if ob[0] not in newobjects:
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
                #Reminder that blender uses WXYZ, the order in the confing file is XYZW, so W is always first
                quat = mathutils.Quaternion([instance["Rotation"][3], instance["Rotation"][0], instance["Rotation"][1], instance["Rotation"][2]])

                ob_copy.location = location
                ob_copy.rotation_mode = 'QUATERNION'
                ob_copy.rotation_quaternion = quat
               
                ob_copy.scale = [instance["Scale"]]*3
    
    add_to_collection()
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
            bpy.data.images.load(Filepath + "/Textures/" + f"/{img}", check_existing = True)
            print(f"Loaded {img}")
    
    #New way of getting info from cfg, thank you Mont
    d = {x : [y["PS"].values()] for x, y in config["Materials"].items()}

    for k, mat in d.items():
        n = 0 #Kinda hacky, but it works
        matnodes = bpy.data.materials[k].node_tree.nodes
        matnodes['Principled BSDF'].inputs['Metallic'].default_value = 0 

        if not len(find_nodes_by_type(bpy.data.materials[k], 'TEX_IMAGE')) > 0: #

            #Check if the material already has a texture node with a specific name, avoids duplicates
            for info in mat[0]:
                current_image = "PS_" + str(n) + "_" + info["Hash"] + "TEX_EXT"
            
                if info["SRGB"]:
                    colorspace = "sRGB"
                else: 
                    colorspace = "Non-Color"

                texnode = matnodes.new('ShaderNodeTexImage')
                texnode.hide = True
                texnode.location = (-370.0, 200.0 + (float(n)*-1.1)*50) #shitty offsetting

                texture = bpy.data.images.get(current_image)

                if not texture: #Rare(?) case where the textures PS 'index' starts at 2 instead of 0
                    texture = bpy.data.images.get("PS_2" + "_" + info["Hash"] + "TEX_EXT")
                
                if texture:
                    texnode.label = texture.name
                    texture.colorspace_settings.name = colorspace
                    texture.alpha_mode = "CHANNEL_PACKED"
                    texnode.image = texture      #Assign the texture to the node

                    #assign a texture to material's diffuse and normal just to help a little 
                    if texture.colorspace_settings.name == "sRGB":     
                        link_diffuse(bpy.data.materials[k])
                    if texture.colorspace_settings.name == "Non-Color":
                        if int(n) == 0:
                            link_diffuse(bpy.data.materials[k])
                        else:
                            link_normal(bpy.data.materials[k], n)
                n += 1


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
    """ Finds at least one image texture in the material
        and at least one Principled shader.
        If they both exist and neither have a link to
        the relevant input socket, connect them.
        There are many ways this can fail.
        if there's no image; if there's no principled
        shader; if the selected image/principled sockets
        are already in use.
        Returns false on any detected error.
        Does not try alternatives if there are multiple
        images or multiple principled shaders.
    """
    it_list = find_nodes_by_type(material, 'TEX_IMAGE')
    s_list = find_nodes_by_type(material, 'BSDF_PRINCIPLED')
    if len(s_list) == 0:
        return False  
    image_node = it_list[0]
    shader_node = s_list[0]
    image_socket = image_node.outputs['Color']
    shader_socket = shader_node.inputs['Base Color']
    if shader_socket.is_linked:
        return
    material.node_tree.links.new(shader_socket, image_socket)


def link_normal(material, num = 0):
    it_list = find_nodes_by_type(material, 'TEX_IMAGE')
    s_list = find_nodes_by_type(material, 'NORMAL_MAP')
    if len(s_list) == 0:
        return False
    image_node = it_list[num]
    shader_node = s_list[0]
    if image_node.image.colorspace_settings.name == "Non-Color":
        image_socket = image_node.outputs['Color']
        shader_socket = shader_node.inputs['Color']
        if shader_socket.is_linked:
            return
        material.node_tree.links.new(shader_socket, image_socket)
                     
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

def add_to_collection():

    #make a collection for the objects
    bpy.data.collections.new(str(Name))
    bpy.context.scene.collection.children.link(bpy.data.collections[str(Name)])

    C = bpy.context
    # List of object references
    objs = C.selected_objects
    # Set target collection to a known collection 
    coll_target = C.scene.collection.children.get(str(Name))
    # If target found and object list not empty
    if coll_target and objs:
        # Loop through all objects
        for ob in objs:
            # Loop through all collections the obj is linked to
            for coll in ob.users_collection:
                # Unlink the object
                coll.objects.unlink(ob)
            # Link each object to the target collection
            coll_target.objects.link(ob)

if __name__ == "__main__":
    #Deselect all objects just in case 
    bpy.ops.object.select_all(action='DESELECT')
    assemble_map()
    cleanup()
