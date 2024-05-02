# 🌶️🧂 Paprika: Software rendering, with a bit of kick

This is an attempt to make a fast, simple, and semi-practical software renderer for fun. This project is an exercise in optimization and learning and is an interesting challenge in optimization and program structuring. As such, this repo is likely to be a mess for a while, but feel free to stick around and watch it blow up!

I want to see just how far I can push my CPU into being what it always dreamed of: A GPU. In C# no less!

There is a common misconception (perhaps a fading one now, though) that C# isn't a performance horse compared to statically-compiled counterparts. Benchmarks disagree and so do I; so let's make the JIT dance shall we?

I'll update this README with any important milestones I reach.


Current objectives are:

* [ ] Fix camera projection
* [ ] Implement frustrum culling
* [ ] Fix triangle-edge imprecision (gaps between triangles)
* [ ] Vectorize per-triangle raster functions
* [ ] Properly byte-align framebuffer accesses


Performance goals (single core, 1280x1024):

- Less than 10ms on ~34k tri 3D scan, raw raster: **REACHED** (now ~5ms on a ~333k tri 3D scan)
- Less than or equal to 8-10ms on ~1.6 million tri 3D scan, raw raster: Getting there at ~20ms-30ms per frame
- Less than or equal to 10-12ms on ~1.6 million tri 3D scan, basic albedo, vertex lighting: Undefined

