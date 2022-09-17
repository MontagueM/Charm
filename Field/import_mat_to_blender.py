import bpy
import json
import mathutils
import os

def assemble_mat():
    #key = name (str), value = most recent node output
    variable_dict = {}
    
    material = bpy.data.materials.new(name="{Name}")
    material.use_nodes = True
    matnodes = material.node_tree.nodes
    link = material.node_tree.links.new
    
    def updateVariable(variable, newNode, inputIdx):
        link(variable_dict[variable], newNode.inputs[inputIdx])
        variable_dict[variable] = newNode
    
    #cbuffer helpers
    i = 0
    
    def registerFloat(var_name, x):
        nonlocal i
        varNode = matnodes.new("ShaderNodeValue")
        varNode.location = (-370.0, 200.0 + (float(i)*-1.1)*50)
        varNode.label = var_name
        varNode.outputs[0].default_value = x
        varNode.hide = True
        i += 1
        #print(var_name)
        variable_dict[var_name] = varNode.outputs[0]
        
#    def addFloat4(var_name, x, y, z, w):
#        registerFloat(var_name + ".x", x)
#        registerFloat(var_name + ".y", y)
#        registerFloat(var_name + ".z", z)
#        registerFloat(var_name + ".w", w)
#    
#    def addFloat3(var_name, x, y, z):
#        registerFloat(var_name + ".x", x)
#        registerFloat(var_name + ".y", y)
#        registerFloat(var_name + ".z", z)
#    
#    def addFloat2(var_name, x, y):
#        registerFloat(var_name + ".x", x)
#        registerFloat(var_name + ".y", y)
#    
#    def addFloat(var_name, x):
#        registerFloat(var_name + ".x", x)  
    
    principled_node = matnodes.get('Principled BSDF')
    
    #Texture: ShaderNodeTexImage
    #Color (4dim vector): ShaderNodeRGB
    #Texture Coordinate: ShaderNodeTexCoord
    #Math: ShaderNodeMath
    #Clamp: ShaderNodeClamp
    #Value: ShaderNodeValue
    if True:
        #using this to keep the variables here in their own scope
        print("setting up v3")
        texcoord = matnodes.new("ShaderNodeTexCoord")
        texcoord.location = (-700, 180)
        splitNode = matnodes.new("ShaderNodeSeparateXYZ")
        splitNode.location = (-550, 205)
        link(texcoord.outputs[2], splitNode.inputs[0])        
        variable_dict['v3.x'] = splitNode.outputs[0]
        variable_dict['v3.y'] = splitNode.outputs[1]
        variable_dict['v3.z'] = splitNode.outputs[0]
        variable_dict['v3.w'] = splitNode.outputs[1]
    
    if True:
        print("setting up v0, v1, v2, v6")
        geo = matnodes.new("ShaderNodeNewGeometry")
        geo.location = (-700, -120)
        splitpos = matnodes.new("ShaderNodeSeparateXYZ")
        link(geo.outputs[0], splitpos.inputs[0])
        variable_dict['v0.x'] = splitpos.outputs[0]
        variable_dict['v0.y'] = splitpos.outputs[1]
        variable_dict['v0.z'] = splitpos.outputs[2]
        variable_dict['v0.w'] = splitpos.outputs[0] #probably wrong
        
        splitnorm = matnodes.new("ShaderNodeSeparateXYZ")
        link(geo.outputs[1], splitpos.inputs[0])
        variable_dict['v1.x'] = splitnorm.outputs[0]
        variable_dict['v1.y'] = splitnorm.outputs[1]
        variable_dict['v1.z'] = splitnorm.outputs[2]
        variable_dict['v1.w'] = splitnorm.outputs[0] #probably wrong

        splittang = matnodes.new("ShaderNodeSeparateXYZ")
        link(geo.outputs[2], splitpos.inputs[0])
        variable_dict['v2.x'] = splittang.outputs[0]
        variable_dict['v2.y'] = splittang.outputs[1]
        variable_dict['v2.z'] = splittang.outputs[2]
        variable_dict['v2.w'] = splittang.outputs[0] #probably wrong
        
        invback = matnodes.new("ShaderNodeMath")
        invback.operation = 'SUBTRACT'
        invback.inputs[0].default_value = 1
        link(geo.outputs[6], invback.inputs[1])
        variable_dict['v6.x'] = invback.outputs[0]
        variable_dict['v6.y'] = invback.outputs[0] #probably wrong
        variable_dict['v6.z'] = invback.outputs[0] #probably wrong
        variable_dict['v6.w'] = invback.outputs[0] #probably wrong
    
    if True:
        print("setting up v4, v5")
        attribute = matnodes.new("ShaderNodeAttribute")
        attribute.attribute_name = 'colourLayerName'
        
        splitattr = matnodes.new("ShaderNodeSeparateXYZ")
        link(attribute.outputs[1], splitattr.inputs[0])
        variable_dict['v4.x'] = splitattr.outputs[0]
        variable_dict['v4.y'] = splitattr.outputs[1]
        variable_dict['v4.z'] = splitattr.outputs[2]
        variable_dict['v4.w'] = splitattr.outputs[0] #probably wrong
        
        variable_dict['v5.x'] = attribute.outputs[3]
        variable_dict['v5.y'] = attribute.outputs[3] #probably wrong
        variable_dict['v5.z'] = attribute.outputs[3] #probably wrong
        variable_dict['v5.w'] = attribute.outputs[3] #probably wrong
        
    ### REPLACE WITH SCRIPT ###

    if True:        #Base Color (Albedo)
        combineRGB = matnodes.new("ShaderNodeCombineColor")
        link(variable_dict['o0.x'], combineRGB.inputs[0])
        link(variable_dict['o0.y'], combineRGB.inputs[1])
        link(variable_dict['o0.z'], combineRGB.inputs[2])
        variable_dict['output_rgb_albedo'] = combineRGB.outputs[0]
        print("rgb")
        link(combineRGB.outputs[0], principled_node.inputs[0])
        link(combineRGB.outputs[0], principled_node.inputs[19])
        
    if True:        #Normal
        biasSubtract1 = matnodes.new("ShaderNodeMath")
        biasSubtract2 = matnodes.new("ShaderNodeMath")
        biasSubtract3 = matnodes.new("ShaderNodeMath")
        biasSubtract1.operation = 'SUBTRACT'
        biasSubtract2.operation = 'SUBTRACT'
        biasSubtract3.operation = 'SUBTRACT'
        
        link(variable_dict['o1.x'], biasSubtract1.inputs[0])
        link(variable_dict['o1.y'], biasSubtract2.inputs[0])
        link(variable_dict['o1.z'], biasSubtract3.inputs[0])        
        
        biasSubtract1.inputs[1].default_value = 0.5
        biasSubtract2.inputs[1].default_value = 0.5
        biasSubtract3.inputs[1].default_value = 0.5
        
        combineNormal = matnodes.new("ShaderNodeCombineXYZ")
        link(biasSubtract1.outputs[0], combineNormal.inputs[0])
        link(biasSubtract2.outputs[0], combineNormal.inputs[1])
        link(biasSubtract3.outputs[0], combineNormal.inputs[2])
        
        normalLengthNode = matnodes.new("ShaderNodeVectorMath")
        normalLengthNode.operation = 'LENGTH'
        link(combineNormal.outputs[0], normalLengthNode.inputs[0])
        variable_dict['normal_length_output'] = normalLengthNode.outputs[0]
        
        normalLengthInvert = matnodes.new("ShaderNodeMath")
        normalLengthInvert.operation = 'DIVIDE'
        normalLengthInvert.inputs[0].default_value = 1.0
        link(normalLengthNode.outputs[0], normalLengthInvert.inputs[1])
        
        normal_in_world_space = matnodes.new("ShaderNodeVectorMath")
        normal_in_world_space.operation = 'SCALE'
        link(combineNormal.outputs[0], normal_in_world_space.inputs[0])
        link(normalLengthInvert.outputs[0], normal_in_world_space.inputs[1])
        
        resplit = matnodes.new("ShaderNodeSeparateXYZ")
        link(normal_in_world_space.outputs[0], resplit.inputs[0])
        recombine = matnodes.new("ShaderNodeCombineXYZ")
        link(resplit.outputs[0], recombine.inputs[0])
        link(resplit.outputs[1], recombine.inputs[1])
        
        normal_dot = matnodes.new("ShaderNodeVectorMath")
        normal_dot.operation = 'DOT_PRODUCT'
        link(recombine.outputs[0], normal_dot.inputs[0])
        link(recombine.outputs[0], normal_dot.inputs[1])
        
        normal_saturate = matnodes.new("ShaderNodeClamp")
        #existing defaults are fine
        link(normal_dot.outputs[0], normal_saturate.inputs[0])
        
        normal_inverse = matnodes.new("ShaderNodeMath")
        normal_inverse.operation = 'SUBTRACT'
        normal_inverse.inputs[0].default_value = 1
        link(normal_saturate.outputs[0], normal_inverse.inputs[1])
        
        normal_sqrt = matnodes.new("ShaderNodeMath")
        normal_sqrt.operation = 'SQRT'
        link(normal_inverse.outputs[0], normal_sqrt.inputs[0])
        
        full_recombine = matnodes.new("ShaderNodeCombineXYZ")
        link(resplit.outputs[0], full_recombine.inputs[0])
        link(resplit.outputs[1], full_recombine.inputs[1])
        link(normal_sqrt.outputs[0], full_recombine.inputs[2])
        
        normal_post_1 = matnodes.new("ShaderNodeVectorMath")
        normal_post_1.operation = 'MULTIPLY_ADD'
        link(full_recombine.outputs[0], normal_post_1.inputs[0])
        normal_post_1.inputs[1].default_value = [2.0, 2.0, 2.0]
        normal_post_1.inputs[2].default_value = [-1.5, -1.5, -1.5]
        
        normal_post_2 = matnodes.new("ShaderNodeVectorMath")
        normal_post_2.operation = 'MULTIPLY_ADD'
        link(normal_post_1.outputs[0], normal_post_2.inputs[0])
        normal_post_2.inputs[1].default_value = [0.5, 0.5, 0.5]
        normal_post_2.inputs[2].default_value = [0.5, 0.5, 0.5]
        
        normal_post_norm = matnodes.new("ShaderNodeVectorMath")
        normal_post_norm.operation = 'NORMALIZE'
        link(normal_post_2.outputs[0], normal_post_norm.inputs[0])
        print("normal")
        link(normal_post_norm.outputs[0], principled_node.inputs[22])
    
    if True:        #Roughness
        smoothness_subtract = matnodes.new("ShaderNodeMath")
        smoothness_subtract.operation = 'SUBTRACT'
        link(variable_dict['o2.y'], smoothness_subtract.inputs[0])
        smoothness_subtract.inputs[1].default_value = 0.375
        
        smoothness_multiply = matnodes.new("ShaderNodeMath")
        smoothness_multiply.operation = 'MULTIPLY'
        link(smoothness_subtract.outputs[0], smoothness_multiply.inputs[0])
        smoothness_multiply.inputs[1].default_value = 8
        
        rough_saturate = matnodes.new("ShaderNodeClamp")
        #existing defaults are fine
        link(smoothness_multiply.outputs[0], rough_saturate.inputs[0])
        
        smoothness_invert = matnodes.new("ShaderNodeMath")
        smoothness_invert.operation = 'SUBTRACT'
        link(rough_saturate.outputs[0], smoothness_invert.inputs[1])
        smoothness_invert.inputs[0].default_value = 1
        print("smoothness")
        link(smoothness_subtract.outputs[0], principled_node.inputs[9])
        
        
    if True:        #RT2
        link(variable_dict['o2.x'], principled_node.inputs[6])
        
        emissive_subtract = matnodes.new("ShaderNodeMath")
        emissive_subtract.operation = 'SUBTRACT'
        link(variable_dict['o2.y'], emissive_subtract.inputs[1])
        emissive_subtract.inputs[1].default_value = 0.5
        
        emissive_multiply = matnodes.new("ShaderNodeMath")
        emissive_multiply.operation = 'MULTIPLY'
        link(emissive_subtract.outputs[0], emissive_multiply.inputs[0])
        emissive_multiply.inputs[1].default_value = 10
        
        emissive_scale= matnodes.new("ShaderNodeVectorMath")
        emissive_scale.operation = 'SCALE'
        link(variable_dict['output_rgb_albedo'], emissive_scale.inputs[0])
        link(emissive_multiply.outputs[0], emissive_scale.inputs[1])
        print("emissive")
        link(emissive_scale.outputs[0], principled_node.inputs[20])
        
        #AO is ignored single blender does that on its own
        
def setupEngine():
    bpy.context.scene.render.engine = 'BLENDER_EEVEE'
    bpy.context.scene.eevee.use_gtao = True


def ShowMessageBox(message = "", title = "Message Box", icon = 'INFO'):

    def draw(self, context):
        self.layout.label(text=message)

    bpy.context.window_manager.popup_menu(draw, title = title, icon = icon)

if __name__ == "__main__":
    #Shows a message box with a message, custom title, and a specific icon
    ShowMessageBox("Importing Material {Name}", "This might take some time! (Especially on multiple imports)", 'ERROR')
    
    #To give the message box a chance to show up
    setupEngine()
    bpy.app.timers.register(assemble_mat, first_interval=0.3)
    
    #Deselect all objects just in case 
    bpy.ops.object.select_all(action='DESELECT')