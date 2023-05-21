Project description
-------------------
This is a repository of a WIP Unity Asset Store asset. The project is intended to contain and showcase a component called `TouchManager`, which is meant to simplify the complexities of [touch detection in Unity](https://docs.unity3d.com/ScriptReference/Touch.html) behind message functions like `OnTriggerEnter()` or `OnTriggerExit()`.

Setting up the Unity project
----------------------------
Drop this GitHub project into an existing Unity project and you are good to go.

**Please note:** This is not a Unity project by itself!

How to use
----------
To use this, attach the `TouchManager` component to the Main Camera in the Scene. Once done, all GameObjects with colliders will detect the following events:

- `OnSwipeEnter(Touch t)`
- `OnSwipeStay(Touch t)`
- `OnSwipeExit(Touch t)`
- `OnSwipeEnter2D(Touch t)`
- `OnSwipeStay2D(Touch t)`
- `OnSwipeExit(Touch t)`
- `OnTouchTap(Touch t)`
- `OnTouchHold(Touch t)`
- `OnTouchUntap(Touch t)`
- `OnTouchTap2D(Touch t)`
- `OnTouchHold2D(Touch t)`
- `OnTouchUntap2D(Touch t)`

Each of these functions will receive the Touch object that caused the event.
