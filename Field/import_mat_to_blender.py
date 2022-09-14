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
        
    def addFloat4(var_name, x, y, z, w):
        registerFloat(var_name + ".x", x)
        registerFloat(var_name + ".y", y)
        registerFloat(var_name + ".z", z)
        registerFloat(var_name + ".w", w)
    
    def addFloat3(var_name, x, y, z):
        registerFloat(var_name + ".x", x)
        registerFloat(var_name + ".y", y)
        registerFloat(var_name + ".z", z)
    
    def addFloat2(var_name, x, y):
        registerFloat(var_name + ".x", x)
        registerFloat(var_name + ".y", y)
    
    def addFloat(var_name, x):
        registerFloat(var_name + ".x", x)  
    
    principled_node = matnodes.get('Principled BSDF')
    
    #Texture: ShaderNodeTexImage
    #Color (4dim vector): ShaderNodeRGB
    #Texture Coordinate: ShaderNodeTexCoord
    #Math: ShaderNodeMath
    #Clamp: ShaderNodeClamp
    #Value: ShaderNodeValue
    if True:
        #using this to keep the variables here in their own scope
        print("setting up texcoord")
        texcoord = matnodes.new("ShaderNodeTexCoord")
        texcoord.location = (-700, 180)
        splitNode = matnodes.new("ShaderNodeSeparateXYZ")
        splitNode.location = (-550, 205)
        link(texcoord.outputs[2], splitNode.inputs[0])        
        variable_dict['tx.x'] = splitNode.outputs[0]
        variable_dict['tx.y'] = splitNode.outputs[1]
        

    #### REPLACE WITH SCRIPT ####
    
    if True:        #Base Color (Albedo)
        combineRGB = matnodes.new("ShaderNodecombineColor")
        link(variable_dict['o0.x'], combineRGB.inputs[0])
        link(variable_dict['o0.y'], combineRGB.inputs[1])
        link(variable_dict['o0.z'], combineRGB.inputs[2])
        link(combineRGB.outputs[0], principled_node.inputs[0])
        
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
        
        normal_dot = matnodes.new("ShaderNodeVectorMath")l
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
        
        normal_post_1 = matnodes.new("ShaderNodeVectorMath")l
        normal_post_1.operation = 'MULTIPLY_ADD'
        link(full_recombine.outputs[0], normal_post_1.inputs[0])
        normal_post_1.inputs[1].default_value = [2.0, 2.0, 2.0]
        normal_post_1.inputs[2].default_value = [-1.5, -1.5, -1.5]
        
        normal_post_2 = matnodes.new("ShaderNodeVectorMath")l
        normal_post_2.operation = 'MULTIPLY_ADD'
        link(normal_post_1.outputs[0], normal_post_2.inputs[0])
        normal_post_2.inputs[1].default_value = [0.5, 0.5, 0.5]
        normal_post_2.inputs[2].default_value = [0.5, 0.5, 0.5]
        
        normal_post_norm = matnodes.new("ShaderNodeVectorMath")l
        normal_post_norm.operation = 'NORMALIZE'
        link(normal_post_2.outputs[0], normal_post_norm.inputs[0])
        
        link(normal_post_norm.outputs[0], principled_node.inputs[23])
        
    if True:
        link(variable_dict['o2.x'], principled_node.inputs[6])

def ShowMessageBox(message = "", title = "Message Box", icon = 'INFO'):

    def draw(self, context):
        self.layout.label(text=message)

    bpy.context.window_manager.popup_menu(draw, title = title, icon = icon)

if __name__ == "__main__":
    #Shows a message box with a message, custom title, and a specific icon
    ShowMessageBox("Importing Material {Name}", "This might take some time! (Especially on multiple imports)", 'ERROR')
    
    #To give the message box a chance to show up
    bpy.app.timers.register(assemble_mat, first_interval=0.3)
    
    #Deselect all objects just in case 
    bpy.ops.object.select_all(action='DESELECT')