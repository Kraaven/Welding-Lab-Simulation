# Welding Lab Simulation Internship Assignment
Name : ********
Goal : 
To create a Welding setup as per the given requirements.
Repository ; [Github](https://github.com/Kraaven/Welding-Lab-Simulation) --> [Release](https://github.com/Kraaven/Welding-Lab-Simulation/releases/tag/1.0.0) 
## Implementation
The project was created after viewing the video provided in the materials.

After watching the video, i started designing the experience. Since i have a Meta Quest 2 on hand, i wanted to create something that felt hand on.

I decided to explore the welding part of the application to its limit (given the time frame), and invested a lot of time into making a system i felt was atleast half decent.

In this application, the user will be able to align 2 different metal plates together, and weld them into a composite piece. 

Despite the time limit, i felt like i managed to create something usable.

*NOTE* : This project was made in Unity and runs on the OpenXR runtime. However, i tested and developed with a Meta Quest 2.

## Assets and Packages
- Unity Engine
- Blender 3D
- XR Interaction Toolkit
- A nice free Outline [package](https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488) in the Unity store
- A house model on [sketchfab](https://sketchfab.com/3d-models/abandoned-brick-building-4f5ce406ce4e42ee8c8713259818953e)
- The 3D models provided

## Approach
- Focused on giving the user freedom while still allowing precision
- There is a snap system implemented to ensure plates can be neatly aligned
- The welding process involves detecting two plates in proximity and confirming if one on of them is clipped (connected to electricity). Once welding begins, particle effects, sound, and haptics simulate the experience.
- There are checks to see the the user is welding in the correct direction
- Based on the weld Success, audio feedback is provided.
- Haptics were also used when the torch was lit

## What could be better? 
A lot of things, actually. A lot of which was cut out due to time.

- Using a grid spacial partitioning system to find snap points faster. This would have increased performance a lot better than direct for loops 
- Having a mirror on the right side of the doorway, allowing you to increase and decrease height for accessability
- A dedicated UI to spawn in metal plates on your choice
- A fix for the weird outline after welding
- Welding metal onto already welded metal (pretty easy)
- Gating the nozzle to the gas cylinder state (currently non interactable)
- Manipulating the random range on welding material scale instantiated based on the current output (pretty easy once the knob becomes interactable)
- A tutorial perhaps? (using text-to-speech or something)
- Play test with friends to see what else could be added or made easier
- Exporting your welded design? (either as a json file or as an fbx)
- Making this more realistic in some way. 
