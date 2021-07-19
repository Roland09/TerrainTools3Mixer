# Terrain Tools 3 Mixer
 
Mixer for Unity's Terrain Tools 3 is a free and Open Source extension for the Unity Terrain Tools version 3. You can stack and order the provided tools via drag & drop and change the size and opacity of the brushes quickly. During painting the tool stack is painted one after the other.

That way you can create a stack and use the tool as a road painter, path painter, river and lake bed painter, mountain andvances ans so on.

This is an update to the predecessor Path Paint Tool which supports now Unity 2021. The name got changed to "Mixer" because it'll become more over time with Unity's addition of more Terrain Tools like Twist and filters like Noise.
 
## Introduction

Let's see in a video how it looks like in action, please click on the picture to see the video on youtube. In the example I use the Mixer for creating a bike path from scratch

[![Terrain Tools 3 Mixer Example](https://img.youtube.com/vi/lzw_TlfGHaE/0.jpg)](https://www.youtube.com/watch?v=lzw_TlfGHaE)


# Installation

In Unity 2020+ or 2021+ open package manager and install via git url:

	https://github.com/Roland09/TerrainTools3Mixer.git?path=/Assets/TerrainTools/Mixer
	
For older Unity versions I recommend [Path Paint Tool](https://github.com/Roland09/PathPaintTool).

## Requirements

	* Unity 2020+ and Unity 2021.1 ... support for Unity's experimental version of their terrain tools
	* Unity 2021.2 ... support for Unity's release version of their terrain tools
	
## Changes compared to Path Paint Tool

The code was adapted to work with Unity 2020+ and 2021+. 

The UI has become more compact. The individual Terrain Tools are quickly accessible and the Brush Size and Opacity can be quickly compared and changed: 
To the right of the Terrain Tool name is the slider for the brush size and to the right of that is the slider for the brush opacity, i. e. strength.

Example:

![mixer-ui](https://user-images.githubusercontent.com/10963432/126138546-553bec24-f4d2-4763-9a2a-7bbd3ba08afd.gif)


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

## More Feature Visuals

Multi-Tile Terrain Road &ath Painting

![multi-tile-with-vspro](https://user-images.githubusercontent.com/10963432/126135199-3c0f4bd6-dc68-4e25-a810-3ddf4c4194bd.gif)

![pathpainttool-motocross-track](https://user-images.githubusercontent.com/10963432/126135118-c5797f67-8560-42aa-9b75-e4bd6e52d894.gif)

![motocross-track](https://user-images.githubusercontent.com/10963432/126135185-805c6772-605e-4c2e-85ca-e70685ed80d5.jpg)

Vegetation Studio Pro Integration

![vspro](https://user-images.githubusercontent.com/10963432/126135149-318667ca-b8cc-42e6-89bb-96654b227f9f.gif)

![golf](https://user-images.githubusercontent.com/10963432/126135162-bf6e0904-c5d4-4e07-8bf2-11ccbe21a585.gif)

Mountain Advancement

![mountain-advance](https://user-images.githubusercontent.com/10963432/126135191-83827068-6d1d-46ee-bfd2-306adee4fd44.gif)

Resize / Rotate via keyboard shortcuts

![resize-rotate](https://user-images.githubusercontent.com/10963432/126135133-93467a19-f891-4a4b-9a11-75a1829002b1.gif)

Undo over Multi-Tile Terrain

![undo tiles](https://user-images.githubusercontent.com/10963432/126135147-c3203421-97a3-465b-97fd-14db42dee83a.gif)

Underlay Paint for e. g. River Beds

![underlay-paint](https://user-images.githubusercontent.com/10963432/126135140-3c0c3fbf-a534-43a6-96fa-2586da46f307.gif)



## Notes

The demo unitypackage is provided to get you started with a tiled and textured terrain.

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