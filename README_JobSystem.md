# Unity Job System Demo

This demo showcases the performance advantages of the Unity Job System when handling a large number of objects.

## How to use:
1. Create a new Scene.
2. Create an empty GameObject and name it `JobSystemManager`.
3. Attach the `JobSystemDemo` script to it.
4. Assign a prefab to the `Prefab` field (e.g., `Assets/ZNS3D/Prefabs/Pizza.prefab`).
5. Set `Spawn Count` to 40,000.
6. Press Play.

## Comparison:
- **With Job System (useJobSystem = true):** The movement of 40,000 objects is multi-threaded, significantly reducing the load on the Main Thread.
- **Without Job System (useJobSystem = false):** The movement is performed in a standard `for` loop on the Main Thread, which will likely cause a significant drop in FPS.

## Key Components:
- `IJobParallelForTransform`: A job type designed specifically for moving Unity Transforms in parallel.
- `TransformAccessArray`: A specialized container that allows Jobs to access and modify Transforms safely from multiple threads.
- `JobHandle`: Used to schedule and manage the execution of jobs.
