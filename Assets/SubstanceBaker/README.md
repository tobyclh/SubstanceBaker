# Substance Baker
#### Plugin to bake substance aka procedural material into ordinary materials.
Batch edit substances?  
Get confused and frustrated by all the API settings documents?
Want to keep the parameter settings just to yourself?
We heard you.


### Features
Bake - Turn procedural material into ordinary material  
GUI - Unity like GUI, but more  
Batch tools  - edit multiple procedural materials at a time  
Custom shader - Use your own shader with easy setup
Hot swipe setting profile - Add and save your own settings 


### Includes 
Ready to use setting profile - for Standard & Standard (specular) shader
Properly commented source code - Add function as you see fit, or contact us for support
User manual - You will be fine without one, but just in case

### Basics
1. Open Windows/SubstanceBaker
2. Create a profile, or use one of those included  
#####   using Substance Database?
- Bumped Specular profile - Achieve identical look for Substance Database material, but doesn't work with other PBR workflow.
- Standard profile - Some settings will be lost, but works nicely with the rest of the lighting system
#####   using Substance Source?
- Standard profile - Default shader
######  Of course, you can always use your own shader
3. Drop the prefered profile to the main panel
4. Select substance(s)
5. Apply settings if required
6. Happy Baking


### User Manual
#### Apply Settings
Apply all settings found iin "Substance Settings" section
- Change platform settings, resolution etc on screen, no coding needed.  
- Adjust common fields in multiple procedural material 

#### Baking  
Create a new material independent to the procedural material (e.g. not affected by the procedural material anymore)  
-  Shader : Which shader for the material
-  Material folder : Where to save the generated material
-  Remove Substance : Delete the procedural material from Unity Project

###### Below is needed ONLY if you plan to use a different shader 


-  Generate All Maps : Generate all maps that the procedural material has, take up more space but could be helpful if you use another shader
- Remap Alpha : Remap Alpha channel to fit some shader (e.g. standard shader), See [detail](https://docs.unity3d.com/ScriptReference/SubstanceImporter.ExportBitmaps.html)
- Map Names : Required only if you plan to use a different shader, Maps name from substance Material to your shader