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
    
    def addFloat(var_name, x):
        variable_dict[var_name + ".x"] = x
        
    principled_node = matnodes.get('Principled BDSF')
    
    #Texture: ShaderNodeTexImage
    #Color (4dim vector): ShaderNodeRGB
    #Texture Coordinate: ShaderNodeTexCoord
    #Math: ShaderNodeMath
    #Clamp: ShaderNodeClamp
    #Value: ShaderNodeValue
    
    ### CBUFFERS ###
    #static float4 cb0[10]

    addFloat4('cb0[0]', 1, 2, 0, 0)
    addFloat4('cb0[1]', 1.25, 0.33425003, 0.12500003, 1.5)
    addFloat4('cb0[2]', -1.2214149, -0.17149174, -0.013661109, -1.4)
    addFloat4('cb0[3]', 0.9, 1, 1, 1.225)
    addFloat4('cb0[4]', -0.83695656, -0.87820184, -0.87820184, -0.975)
    addFloat4('cb0[5]', 0.14658621, 0.13658907, 0.10114445, 0.35)
    addFloat4('cb0[6]', 2, -2, 0, 0)
    addFloat4('cb0[7]', 1.9921875, -1, 0.39999998, 0)
    addFloat4('cb0[8]', 2.5, 5, 0, 0)
    addFloat4('cb0[9]', 3.984375, -2, 0.897024, 0)

    ### Function Definition ###
    ### v[n] vars ###
    addFloat4('v0', 1.0, 1.0, 1.0, 1.0)

    addFloat4('v1', 1.0, 1.0, 1.0, 1.0)

    addFloat4('v2', 1.0, 1.0, 1.0, 1.0)

    addFloat4('v3', 1.0, 1.0, 1.0, 1.0)

    addFloat3('v4', 1.0, 1.0, 1.0)

    addFloat4('v5', 1.0, 1.0, 1.0, 1.0)

    addFloat('v6', 1.0)
    
    i = 0
    for var in variable_dict.keys:
        varNode = matnodes.new("ShaderNodeValue")
        varNode.location = (-370.0, 200.0 + (float(i)*-1.1)*50)
        varNode.label = var
        varNode.value = variable_dict[var]
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