# CharacterController grounding stability (the -1f trick)

## What CharacterController.isGrounded actually is

`CharacterController` is Unity's capsule-shaped, kinematic mover — it does not participate in Unity's Rigidbody/physics simulation. It moves only when `CharacterController.Move()` (or `SimpleMove()`) is explicitly called, and it performs its own collision sweep at that moment.

`isGrounded` is **not** a live, continuously-updated sensor. Per the official Scripting API, it reflects whether the controller was touching the ground **during the most recent `Move()` call** — it's a snapshot from the last movement, not the current instant. If a frame passes with no meaningful downward motion, or the sweep just barely misses the ground due to floating point precision, `isGrounded` can read `false` even though the character looks like it's standing on solid ground.

Source: [CharacterController.isGrounded — Unity Scripting API](https://docs.unity3d.com/ScriptReference/CharacterController-isGrounded.html)

## Why a velocity of exactly 0 causes flicker

Consider the naive version:

```csharp
yVelocity = controller.isGrounded ? 0f : yVelocity + Physics.gravity.y * Time.deltaTime;
```

While grounded, this feeds `Move()` a Y component of exactly `0` every frame — no downward motion at all. On flat ground this often looks fine. But on a slope, a seam between floor pieces, or just from float-precision noise in the collision sweep, a zero vertical input can mean the controller's sweep doesn't register a firm enough hit to keep `isGrounded` true on the next check. The result: `isGrounded` flickers `true`/`false` frame to frame, which in turn makes gravity re-trigger intermittently — visible as jitter, tiny stutter-steps, or an unreliable jump/ground state.

## The fix: a small constant downward push

```csharp
yVelocity = controller.isGrounded ? -1f : yVelocity + Physics.gravity.y * Time.deltaTime;
```

Instead of zero, feed a small constant negative value (commonly `-1` or `-2`) into `Move()` every frame while grounded. This isn't a physically meaningful velocity — it's not modeling anything real about gravity's magnitude. It exists purely so the controller's sweep always has a firm, consistent downward motion to test against the ground each frame, keeping `isGrounded` stable instead of borderline.

This is a **community-established workaround**, not an official Unity API feature or formally documented pattern — searching Unity's official docs turns up no page describing it by name. It shows up repeatedly across tutorials and forum threads addressing exactly this jitter because `CharacterController`'s ground-check design makes it necessary in practice.

## Why this resets the freefall accumulation too

This same ternary also solves the problem raised in [[velocity-and-acceleration-integration]] — without a reset, `yVelocity` would keep growing more negative forever, even while standing still on the ground. Snapping back to `-1f` on every grounded frame discards the accumulated freefall velocity the instant the character lands, so the next time the character walks off a ledge, gravity starts accumulating fresh from a small baseline rather than from whatever large negative value built up during the last fall.

## See also

- [[velocity-and-acceleration-integration]] — how yVelocity builds up during freefall in the first place.
- [[frame-rate-independence-and-deltatime]] — why the accumulation step needs deltaTime scaling.
