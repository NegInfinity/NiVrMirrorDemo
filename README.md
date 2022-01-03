This is unsupported and use at your own risk.

You can see a demo here:  
[![thumbnail](https://img.youtube.com/vi/QX_Klm8KoNU/0.jpg)](https://youtu.be/QX_Klm8KoNU)

I've created this when I was looking for a VR mirror in unity. I haven't found one, and eventually made my own.
This is tested on Virtual Desktop + Oculujs 1 and requires motion controllers with sticks to move in the demo scenes.
The demo functions in Single Pass instanced mode, but requires two more passes to render the reflection.

To use you'd need a plane with a VRPortalMaterial attached, and a VRPortalRenderer attached to it and configured.
VRPortalRenderer drives the render cam, and it REQUIRES position and rotation from for left and right eye from the
vr device. See VRMirrorDemoInputActions.inputactions for an example.

The files you really need are Coord.cs, VRPortalRenderer.cs, VRPortalShader.shader. The rest is used by the demo and 
can be replaced by your own material.

This works on internal render only, but could be likely adapted to HDRP or URP without much trouble. You'd only need to replace the shader.

To see how it works, run the demo scenes and tear them apart to see how they're configured.

This script can work to drive a mirror or a portal camera with depth, similar to portals from Prey. That requires an "eye" object to be assigned.

Have fun.

Victor "NegInfinity" Eremin, 2022 Jan 03. 
