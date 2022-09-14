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