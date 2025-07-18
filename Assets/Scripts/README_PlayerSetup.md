# Player Movement Setup Guide

## Overview
This guide explains how to set up a player character that can move to mouse click positions on the ground.

## Required Components for Player GameObject

### 1. **Transform Component** (Automatically added)
- **Purpose**: Defines position, rotation, and scale
- **Settings**: Default values work fine

### 2. **Mesh Renderer** (For visual representation)
- **Purpose**: Renders the player model
- **Setup**: 
  - Assign a mesh (cube, sphere, or character model)
  - Set material for visual appearance

### 3. **Mesh Filter** (For visual representation)
- **Purpose**: Holds the mesh data
- **Setup**: Assign the same mesh as the Mesh Renderer

### 4. **Collider Component** (Required for physics)
- **Purpose**: Defines the physical boundary for collision detection
- **Recommended**: Capsule Collider for character movement
- **Settings**:
  - Height: 2 (for humanoid character)
  - Radius: 0.5
  - Center: (0, 1, 0) - slightly above ground

### 5. **Rigidbody Component** (Optional but recommended)
- **Purpose**: Handles physics simulation
- **Settings**:
  - Use Gravity: ✅ (checked)
  - Is Kinematic: ❌ (unchecked)
  - Constraints: Freeze Rotation on X and Z axes
  - Mass: 1
  - Drag: 0
  - Angular Drag: 0.05

### 6. **PlayerMovement Script** (Custom script)
- **Purpose**: Handles mouse click movement logic
- **Settings**:
  - Move Speed: 5 (adjust as needed)
  - Stopping Distance: 0.1
  - Ground Layer: Set to your ground layer
  - Max Raycast Distance: 100

## Required Components for Camera

### 1. **Camera Component** (Automatically added)
- **Purpose**: Renders the game view
- **Settings**:
  - Tag: "MainCamera" (important!)
  - Projection: Perspective
  - Field of View: 60
  - Near Clip Plane: 0.3
  - Far Clip Plane: 1000

### 2. **CameraFollow Script** (Custom script)
- **Purpose**: Makes camera follow the player
- **Settings**:
  - Target: Assign player transform
  - Offset: (0, 10, -10) - adjust for desired camera angle
  - Smooth Speed: 5
  - Look At Target: ✅ (checked)

## Required Components for Ground

### 1. **Mesh Renderer & Mesh Filter**
- **Purpose**: Visual representation of the ground
- **Setup**: Use a plane or quad mesh

### 2. **Collider Component**
- **Purpose**: Allows mouse raycast to detect ground
- **Recommended**: Box Collider
- **Settings**: Size to match your ground mesh

### 3. **Layer Setup**
- **Purpose**: Identifies ground for raycast detection
- **Setup**: 
  - Create a new layer called "Ground"
  - Assign ground objects to this layer
  - Set the Ground Layer in PlayerMovement script

## Step-by-Step Setup Instructions

### 1. Create Player GameObject
```
1. Right-click in Hierarchy → Create Empty
2. Rename to "Player"
3. Add Tag: "Player"
4. Add components:
   - Mesh Filter (assign cube/sphere)
   - Mesh Renderer (assign material)
   - Capsule Collider
   - Rigidbody (configure as above)
   - PlayerMovement script
```

### 2. Create Camera Setup
```
1. Select Main Camera in Hierarchy
2. Ensure tag is "MainCamera"
3. Add CameraFollow script
4. Assign Player as Target in inspector
```

### 3. Create Ground
```
1. Right-click in Hierarchy → 3D Object → Plane
2. Rename to "Ground"
3. Set Layer to "Ground"
4. Scale as needed for your game area
```

### 4. Configure Scripts
```
1. Select Player GameObject
2. In PlayerMovement component:
   - Set Ground Layer to "Ground"
   - Adjust Move Speed as needed
3. Select Camera
4. In CameraFollow component:
   - Adjust Offset for desired camera angle
   - Adjust Smooth Speed as needed
```

## Testing the Setup

1. **Enter Play Mode**
2. **Click anywhere on the ground**
3. **Player should move to clicked position**
4. **Camera should follow player smoothly**

## Troubleshooting

### Player doesn't move
- Check if ground has correct layer
- Verify PlayerMovement script is attached
- Ensure camera has "MainCamera" tag
- Check console for error messages

### Camera doesn't follow
- Verify CameraFollow script is attached to camera
- Check if target is assigned in inspector
- Ensure player has "Player" tag

### Movement feels wrong
- Adjust Move Speed in PlayerMovement
- Modify Smooth Speed in CameraFollow
- Check Rigidbody constraints
- Adjust Stopping Distance

## Advanced Features

### Adding Animations
- Add Animator component to player
- Create animation controller
- Modify PlayerMovement to trigger animations

### Adding Sound Effects
- Add AudioSource component to player
- Play footstep sounds during movement
- Add click sound when setting target

### Visual Feedback
- Add particle effects at target position
- Show path line to target
- Add UI indicator for movement

## Performance Tips

- Use object pooling for visual effects
- Optimize ground mesh for large areas
- Consider using NavMesh for complex pathfinding
- Use LOD (Level of Detail) for distant objects 