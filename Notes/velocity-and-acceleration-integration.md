# Velocity and acceleration integration (why gravity accumulates)

## The three quantities

Classical mechanics defines motion through three related quantities:

| Quantity | Meaning | Typical unit |
|---|---|---|
| Position | Where something is | meters (m) |
| Velocity | Rate of change of position | meters/second (m/s) |
| Acceleration | Rate of change of velocity | meters/second² (m/s²) |

Each one is the *derivative* of the one above it, and the *integral* of the one below it. Acceleration tells you how fast velocity is changing; velocity tells you how fast position is changing.

Gravity is an **acceleration**, not a velocity and not a distance. `Physics.gravity` is a `Vector3`, defaulting to `(0, -9.81, 0)` — Earth's approximate surface gravity in m/s², pointing down. That default is configurable in Project Settings → Physics, but the shape of the logic below applies regardless of the value.

## Why a falling object doesn't fall at constant speed

A common misconception: "falling" means moving down at some fixed speed. In reality, an object in freefall keeps speeding up — that's what acceleration means. A ball dropped from a tall building is moving slowly right after release and much faster just before it lands.

To reproduce this in code, gravity (the acceleration) must be **added into a running velocity value** every frame, rather than used directly as a position offset:

```csharp
yVelocity += Physics.gravity.y * Time.deltaTime;
```

Read this as: "this frame's tiny velocity change equals acceleration × elapsed time, and it accumulates onto whatever velocity already existed." Frame 1 adds a small downward velocity. Frame 2 adds another small increment *on top of* frame 1's result. After N frames, `yVelocity` has grown roughly proportional to elapsed time — matching the real physics of freefall, where speed increases the longer something falls.

This pattern — repeatedly adding small increments of a rate into an accumulator — is called **numerical integration** (specifically, semi-implicit/symplectic Euler integration in most game engines' physics steps). It's an approximation of calculus-style continuous integration, done in discrete per-frame steps. It's precise enough for games; it is not exactly how continuous real-world physics works, but converges to a very close approximation as frame rate increases.

## Applying velocity to position

Once `yVelocity` (a rate, m/s) is known for the frame, it's converted into an actual position change the same way any rate is: multiply by `Time.deltaTime` (see [[frame-rate-independence-and-deltatime]]) to get this frame's distance, then hand it to the mover:

```csharp
Vector3 move = ...;
move.y = yVelocity;
controller.Move(move * speed * Time.deltaTime);
```

Note `yVelocity` here is not itself re-multiplied by `speed` — it's already a real velocity value (m/s) computed from gravity, unlike the horizontal input axes which are raw `[-1, 1]` sticks needing a `speed` scalar to become m/s in the first place. Only `Time.deltaTime` is needed to turn it into this-frame displacement.

## Why yVelocity resets instead of growing forever

Left unchecked, `yVelocity` would keep accumulating negative value indefinitely, even long after the character has landed — because nothing tells the loop "we've hit the ground, stop accumulating." That's the job of the grounded check, covered in [[charactercontroller-grounding-stability]].

## See also

- [[frame-rate-independence-and-deltatime]] — the deltaTime multiplication used at both the accumulation step and the final movement step.
- [[charactercontroller-grounding-stability]] — resetting yVelocity in a way that keeps CharacterController's ground detection reliable.
