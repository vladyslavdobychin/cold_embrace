# Core Shoot Loop — Design

## Purpose

Validate the central "aim, shoot, something reacts" loop for the isometric shooter MVP before investing in movement polish (dash/jump), a real enemy, or level content. This is the first of six planned increments; the others (movement polish, projectile upgrade, enemy AI, level greybox, UI/HUD) are deferred to their own future spec/plan/build cycles.

## Background

Project state going in: greyboxed floor, capsule character driven by `PlayerMovement` (CharacterController, new Input System, movement rotated 45° to align with the isometric camera), and `CameraFollow` (LateUpdate position-follow with a captured offset). No combat, no enemies, no aiming yet.

## Decisions

- **Aim style: mouse-aim.** Isometric shooters conventionally aim independently of movement direction — raycast from the camera through the mouse cursor onto the ground plane, rotate the character to face that point.
- **Attack style: hitscan, not projectile.** Instant raycast, no travelling bullet object. Proves the loop with the fewest new systems; a visible projectile is a deliberate later upgrade (its own increment), not part of this slice.
- **Target: static, health-based dummy.** Doesn't move or seek the player — isolates the shoot→feedback loop. Health-based (not simple destroy-on-first-hit or endless no-death) because the game needs *some* metric to know when to remove an enemy, and this dummy should exercise the same `Health` component real enemies will reuse later.

## Architecture

Four pieces, each with one responsibility. None of them need to know about each other's internals — they interact only through the `Transform`/`Collider`/`Health` each already exposes.

### `PlayerAim` (new script, on the player capsule)

- `[SerializeField] Camera aimCamera` — Inspector-assigned, not `Camera.main`. Matches the explicit-reference convention already used for `PlayerMovement.cameraTransform`; avoids the tag-lookup fragility of `Camera.main` (silent failure if the tag is ever wrong).
- `[SerializeField] LayerMask groundMask` — restricts the aim raycast to the floor layer only, so aiming can't accidentally hit the dummy or other scene geometry.
- `Update()`: build a ray via `aimCamera.ScreenPointToRay(Mouse.current.position.ReadValue<Vector2>())`, `Physics.Raycast` against `groundMask`. On hit, rotate `transform` to face the hit point, flattened to the character's own Y (no pitching up/down). On miss, keep the last facing — no-op, not an error.

### `PlayerShooting` (new script, on the player capsule)

- `[SerializeField] float range`, `[SerializeField] int damage`.
- `OnAttack(InputValue)` — Send Messages callback, same convention as `PlayerMovement.OnMove`. Wired to the `Attack` action that already exists in the `Player` action map of `InputSystem_Actions` — no changes needed to the Input Actions asset itself.
- On press: `Physics.Raycast` forward from `transform.forward`, using a `LayerMask` that excludes the player's own layer (see Edge Cases). If the hit collider has a `Health` component, call `TakeDamage(damage)`. Miss or hit something without `Health` → silent no-op.

### `Health` (new script, reusable component)

- `[SerializeField] int maxHealth`.
- `TakeDamage(int amount)` subtracts from current health; at zero or below, `Destroy(gameObject)`.
- No death event/callback in this slice — deferred until something besides "the object disappears" needs to react to a death (UI, score, spawner). Adding an event now would be speculative; the reusable component itself is the part worth building ahead of time, since Enemy AI (a later increment) will attach the same script as-is.

### Dummy target (GameObject)

- A static primitive (Capsule or Cube) with a Collider and a `Health` component. No custom script of its own — `Health` is the entire behavior.

## Data flow

```
Mouse position  → PlayerAim raycast (vs groundMask)   → rotate capsule transform
Attack pressed  → PlayerShooting raycast (vs !player)  → Health.TakeDamage() → maybe Destroy
```

## Edge cases

- **Aim raycast misses the ground** (cursor off the floor mesh): keep the last facing, no error.
- **Fire raycast misses everything**: silent no-op.
- **Fire raycast hits something without `Health`** (a wall, the floor): null-check `GetComponent<Health>()`, skip damage silently.
- **Self-hit risk**: a raycast starting at `transform.position` could immediately re-hit the capsule's own collider. Fix: put the player capsule on a dedicated `Player` layer, exclude that layer from `PlayerShooting`'s raycast mask.

## Testing

Manual playtest only for this slice — feel-driven validation, not something a unit test can judge:

- Capsule should track the mouse cursor smoothly across the floor.
- Pressing Attack should visibly decrease the dummy's health (logged) each hit, and destroy it at zero.
- Add `Debug.DrawRay` on both the aim direction and the fire raycast during development — near-zero cost, gives a visual confirmation in the Scene view that rays are firing where expected.
- No automated tests planned. `Health.TakeDamage`'s arithmetic is simple enough that a unit test would be optional polish, not a requirement for this increment.

## Out of scope (deferred to later increments)

- Movement polish: dash, jump
- Projectile upgrade (hitscan → visible travelling bullet)
- Enemy AI (movement/detection/basic behavior — reuses `Health` from this slice)
- Level greybox (a real sample location, beyond the current flat test floor)
- UI/HUD (health, ammo, any player-facing feedback beyond console logs and `Debug.DrawRay`)
