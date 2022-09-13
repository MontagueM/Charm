import bpy
import json
import mathutils
import os

def assemble_mat():
    #key = name (str), value = most recent node
    variable_dict = {}
    
    material = bpy.data.materials.new(name="{Name}")
    material.use_nodes = True
    matnodes = material.node_tree.nodes
    link = material.node_tree.links.new
    
    def updateVariable(variable, newNode, inputIdx, outputIdx):
        link(variable_dict[variable].outputs[outputIdx], newNode[inputIdx])
        variable_dict[variable] = newNode
    
    #cbuffer helpers
    def addFloat4(var_name, x, y, z, w):
        variable_dict[var_name + ".x"] = x
        variable_dict[var_name + ".y"] = y
        variable_dict[var_name + ".z"] = z
        variable_dict[var_name + ".w"] = w
    
    def addFloat3(var_name, x, y, z):
        variable_dict[var_name + ".x"] = x
        variable_dict[var_name + ".y"] = y
        variable_dict[var_name + ".z"] = z
    
    def addFloat2(var_name, x, y):
        variable_dict[var_name + ".x"] = x
        variable_dict[var_name + ".y"] = y
    
    def addFloat(var_name, x):
        variable_dict[var_name + ".x"] = x
        
    principled_node = matnodes.get('Principled BDSF')
    
    #Texture: ShaderNodeTexImage
    #Color (4dim vector): ShaderNodeRGB
    #Texture Coordinate: ShaderNodeTexCoord
    #Math: ShaderNodeMath
    #Clamp: ShaderNodeClamp
    #Value: ShaderNodeValue
    
    ### REPLACE WITH STATIC VARIABLES ###
    
    i = 0
    for var in variable_dict.keys():
        varNode = matnodes.new("ShaderNodeValue")
        varNode.location = (-370.0, 200.0 + (float(i)*-1.1)*50)
        varNode.label = var
        varNode.outputs[0].default_value = variable_dict[var]
        varNode.hide = True
        variable_dict[var] = varNode
        i += 1
        print(var)

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