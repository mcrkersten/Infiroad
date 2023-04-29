import maya.cmds as cmds
import json

# Load the JSON data
with open("C:\Repository\Infiroad\Assets\[Project]\[GENERATOR] Settings\MeshTasks\PointFiles\grandstand_Modern_Right.txt", 'r') as f:
    json_data = json.load(f)

# Loop through each layer in the JSON data
for layer in json_data['layers']:
    # Loop through each path in the layer
    for path in layer['paths']:
        # Get the points for the path
        points = path['points']
        
        # Create a list of Maya vertices based on the JSON points
        vertices = [(point[0], 0, point[1]) for point in points]
        
        # Create a polygon shape based on the vertices
        poly = cmds.polyCreateFacet( p=vertices )[0]
        
        # Extrude the polygon along the Y-axis
        cmds.polyExtrudeFacet(poly, translation=(0, 1, 0))
