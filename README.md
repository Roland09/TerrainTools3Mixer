# Terrain Tools 3 Mixer
 
Mixer for Unity's Terrain Tools 3 is a free and Open Source extension for the Unity Terrain Tools version 3. You can stack and order the provided tools via drag & drop and change the size and opacity of the brushes quickly. During painting the tool stack is painted one after the other.

That way you can create a stack and use the tool as a road painter, path painter, river and lake bed painter, mountain andvances ans so on.

This is an update to the predecessor Path Paint Tool which supports now Unity 2021. The name got changed to "Mixer" because it'll become more over time with Unity's addition of more Terrain Tools like Twist and filters like Noise.
 
## Introduction

Let's see in a video how it looks like in action, please click on the picture to see the video on youtube. In the example I use the Mixer for creating a bike path from scratch

[![Terrain Tools 3 Mixer Example](https://img.youtube.com/vi/lzw_TlfGHaE/0.jpg)](https://www.youtube.com/watch?v=lzw_TlfGHaE)


# Installation

In Unity 2021.1 open package manager and install via git url:

	https://github.com/Roland09/TerrainTools3Mixer.git?path=/Assets/TerrainTools/Mixer
	
For older Unity versions I recommend [Path Paint Tool](https://github.com/Roland09/PathPaintTool).

## Requirements

	* Unity 2020+ and Unity 2021.1 ... support for Unity's experimental version of their terrain tools
	* Unity 2021.2 ... support for Unity's release version of their terrain tools
	
## Changes compared to Path Paint Tool

The code was adapted to work with Unity 2020+ and 2021+. 

The UI has become more compact. The individual Terrain Tools are quickly accessible and the Brush Size and Opacity can be quickly compared and changed: 
To the right of the Terrain Tool name is the slider for the brush size and to the right of that is the slider for the brush opacity, i. e. strength.

## Features

- Supported  Terrain Tools:

	Any combination of Paint, Path, Smooth, Ridge Erosion, Smudge, Underlay

 - Various Paint Modes
 
   * Paint Brush: Paint by dragging the mouse
   * Stroke: Create strokes by placing an anchor point and subsequently create strokes from the previous anchor point.
   * Automatic Waypoint creation and Spline manipulation are in development and will be implemented as soon as Unity releases their Spline Package
   
- Create roads, paths, plateaus, ramps, lake and river beds, mountain spurs
   
- Multi Tile Terrain

- Unity 2021.1 and 2021.2+ Support

- Vegetation Studio Pro Support

- Open Source, **FREE** for everyone, no DLL

## Integrations

 - Vegetation Studio Pro 

For Vegetation Studio Pro I recommend to use the include and exclude terrain texture rules for automatic vegetation placement.

## Notes

The demo unitypackage is provided to get you started with a tiled and textured terrain. Future updates will be done on the code alone.

## Credits

Full credit and a BIG THANK YOU(!!!) to the very skilled and most awesome developers at Unity who provided the Terrain Tool Samples for free for the community.

Demo Scene: 

World Creator 2 with which the creation of the demo terrain was possible within minutes. Most of all thank you to Yanik for providing the base terrain.

The textures for the terrain after importing into Unity are Creative Common textures which are freely available and can be used without restriction. 

Credit to these providers:

* CC0 Textures

	[https://cc0textures.com](https://cc0textures.com)

* cgbookcase

	[https://www.cgbookcase.com](https://www.cgbookcase.com)

## Roadmap

* Integrate the upcoming Terrain Tools 4
* Automatic Waypoint finder and shaping the terrain
* Spline creation, Spline saving and flexible adjustment
* Additional Terrain Tool support
* Presets & Quick Access Settings
* Paint and remove terrain details using Unity's detail painter
* ...