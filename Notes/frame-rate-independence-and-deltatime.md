# Frame-rate independence and Time.deltaTime

## The problem

A game loop runs `Update()` once per rendered frame. Frame rate is not constant — it varies with hardware, scene complexity, and background load. A machine rendering at 144 fps calls `Update()` roughly 2.4x more often per second than one rendering at 60 fps.

If per-frame code applies a fixed amount of change — move 1 unit, rotate 1 degree, subtract 1 from a timer — every call, that quantity ends up scaling with frame rate instead of real time. Movement speed, fall speed, animation pacing, anything computed per-frame without correction will differ between machines, or even on the same machine as frame rate fluctuates (e.g. a sudden GPU load spike).

This is the frame-rate dependence bug. It's the classic gotcha in any real-time loop, not Unity-specific — the same problem exists in raw OpenGL loops, custom game engines, or any per-tick update.

## Time.deltaTime

`Time.deltaTime` is the elapsed real time (in seconds) since the previous frame completed. It is read inside `Update()` and changes every call — small on a fast frame, larger on a slow one.

[[velocity-and-acceleration-integration|Velocity and acceleration]] are expressed in "per second" units (m/s, m/s²). To find out how much of that quantity applies to *this specific frame*, multiply by how many seconds this frame actually took:

```
amount_this_frame = rate_per_second * Time.deltaTime
```

This holds for any rate-like quantity:

```csharp
// Position change from a speed (units/sec)
transform.position += direction * speed * Time.deltaTime;

// Velocity change from an acceleration (units/sec²)
yVelocity += Physics.gravity.y * Time.deltaTime;

// Rotation change from an angular speed (deg/sec)
transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
```

Multiplying by `Time.deltaTime` converts a *per-second* rate into a *this-frame* delta. Skip that multiplication and the game runs faster on faster hardware — the exact bug this guards against.

## Rule of thumb

Any variable whose name or meaning includes "per second" — speed, velocity, acceleration, angular velocity — needs `* Time.deltaTime` at the point it's applied to a position, rotation, or another accumulating value, once per frame, inside `Update()`.

Values that are already "this frame's amount" (e.g. a value read directly from input, like a raw `Vector2` from `OnMove`) don't need it — they aren't rates, they're immediate reads.

## Update() vs FixedUpdate()

`Time.deltaTime` is for `Update()`, which runs once per rendered frame (variable interval). Physics code that runs in `FixedUpdate()` (fixed, constant interval, independent of rendering) uses `Time.fixedDeltaTime` instead — conceptually the same correction, applied because `FixedUpdate` has its own separate timestep.

`CharacterController.Move()` is typically called from `Update()`, since it isn't governed by Unity's built-in Rigidbody physics step, so `Time.deltaTime` is the right one for it.

## See also

- [[velocity-and-acceleration-integration]] — why gravity accumulates into velocity over multiple frames, using this same deltaTime pattern.
- [[charactercontroller-grounding-stability]] — a CharacterController-specific issue that interacts with per-frame velocity.
