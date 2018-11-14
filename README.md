## Group behavior modeling

### Simulation platform
### [ROS#](https://github.com/siemens/ros-sharp)

ROS# is a set of open source software libraries and tools in C# for communicating with ROS from .NET applications, in particular Unity.

### Pepper robot

<img src="docs/pepper-single.png" alt="one pepper" height="300px"/><img src="docs/pepper-two.png" alt="two peppers" height="300px"/>


### Setups & Installation

#### Unity (version >= 2018.2.6)
1. Copy [RosSharp](https://github.com/mingfeisun/ros-sharp/tree/master/Unity3D/Assets/RosSharp) into the Assets folder of Unity project 
2. In the Unity menu, go to *Edit* > *Project Settings* > *Player*
3. In the inspector panel, look under *Settings* > *Configuration*, set *Scripting Runtime Version* to *.Net 4.x Equivalent*

More details can be found [here](https://github.com/siemens/ros-sharp/wiki/User_Inst_Unity3DOnWindows)

#### ROS (version Kinetic)
1. Copy [ROS packages](https://github.com/mingfeisun/ros-sharp/tree/master/ROS) to *src* folder
2. Building: 
``` bash 
# generate build/ and devel/
catkin_make 
# source env
source devel/setup.bash
# running
```
