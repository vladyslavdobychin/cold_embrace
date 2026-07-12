# Core Shoot Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **For this project specifically:** the user is implementing this themselves in the Unity Editor and reviewing with Claude after each task, rather than having an agent write the files — see the note at the end of this document before choosing an execution mode.

**Goal:** Build the first playable slice of the isometric shooter — mouse-aim, hitscan attack, and a health-based dummy target that can be destroyed — validating the core "aim, shoot, something reacts" loop before any movement polish, real enemy, or level content.

**Architecture:** Four single-responsibility pieces on top of the existing `PlayerMovement`/`CameraFollow` setup: two Editor-only Physics Layers (`Ground`, `Player`), a reusable `Health` component, a `PlayerAim` script (mouse → ground raycast → rotation), and a `PlayerShooting` script (Attack input → forward raycast → `Health.TakeDamage`). No new abstractions beyond what each task needs — no death events, no projectile pooling, no AI.

**Tech Stack:** Unity 6 (6000.5.1f1), URP, new Input System (`UnityEngine.InputSystem`), C#.

## Global Constraints

- Unity 6 / URP only — no Built-in RP or HDRP APIs.
- New Input System only — no `Input.GetKey`/legacy Input Manager.
- `[SerializeField] private` for Inspector-exposed fields, not `public`.
- Cross-object references (camera, layers) are Inspector-assigned, not `Camera.main`/`FindObjectOfType`/tags — established convention from `PlayerMovement.cameraTransform`.
- File name matches class name exactly; every script under `Assets/Scripts/`.
- YAGNI/KISS/SOLID, YAGNI weighted heaviest — no speculative abstractions (e.g. no death-event system yet; see spec's Out of Scope).
- Testing for this slice is manual/Play-mode only, per the approved spec — the gameplay feel this increment validates isn't something a unit test can judge, so no fabricated automated tests are included below.

---

### Task 1: Physics Layers (`Ground`, `Player`)

Prerequisite Editor configuration — both `PlayerAim` (Task 3) and `PlayerShooting` (Task 4) need real Layers to point their `LayerMask` fields at.

**Files:**
- Modify: `ProjectSettings/TagManager.asset` (Unity writes this automatically when you add a Layer in the Editor — don't hand-edit it)
- Modify: `Assets/Scenes/SampleScene.unity` (floor and capsule GameObjects get reassigned layers)

**Interfaces:**
- Produces: a `Ground` layer (assigned to the floor GameObject) and a `Player` layer (assigned to the capsule), both selectable in any `LayerMask` field from here on.

- [ ] **Step 1: Add the two layers**

In the Editor: select any GameObject → Inspector → **Layer** dropdown (top-right, next to Tag) → **Add Layer...** → in the Tags & Layers window, type `Ground` into the first empty **User Layer** slot and `Player` into the next one.

- [ ] **Step 2: Assign the floor to `Ground`**

Select the floor GameObject in the Hierarchy → Inspector → **Layer** dropdown → choose `Ground`. If prompted "Change layer for children?", choose **No** (the floor has no children in this scene).

- [ ] **Step 3: Assign the capsule to `Player`**

Select the player capsule in the Hierarchy → Inspector → **Layer** dropdown → choose `Player`.

- [ ] **Step 4: Verify**

Reselect the floor — Inspector's Layer field should read `Ground`. Reselect the capsule — should read `Player`. No console errors on entering Play mode (layers alone don't change behavior yet).

- [ ] **Step 5: Commit**

```bash
git add ProjectSettings/TagManager.asset Assets/Scenes/SampleScene.unity
git commit -m "Add Ground and Player physics layers"
```

---

### Task 2: `Health` component + dummy target

**Files:**
- Create: `Assets/Scripts/Health.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (new dummy GameObject)

**Interfaces:**
- Produces: `public void TakeDamage(int amount)` on `Health` — Task 4 calls this directly after a successful raycast hit.

- [ ] **Step 1: Write `Health.cs`**

```csharp
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;

    private int currentHealth;

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage, {currentHealth}/{maxHealth} HP left");

        if (currentHealth <= 0)
        {
            Debug.Log($"{name} destroyed");
            Destroy(gameObject);
        }
    }
}
```

- [ ] **Step 2: Create the dummy GameObject**

Hierarchy → right-click → **3D Object → Capsule**. Rename to `Dummy`. Position it a few units in front of where the player capsule spawns (e.g. `(0, 1, 5)`) so it's visible without moving. Leave it on the `Default` layer — `PlayerShooting`'s mask only needs to *exclude* `Player`, not restrict to an allow-list, so `Default` is fine.

- [ ] **Step 3: Attach `Health` to the dummy**

Select `Dummy` → **Add Component** → `Health`. Leave `Max Health` at its default (30), or set whatever feels right.

- [ ] **Step 4: Verify**

Enter Play mode. No console errors. Select `Dummy` in the Hierarchy — Inspector shows the `Health (Script)` component with an editable `Max Health` field. (Full damage/destroy behavior isn't testable yet — nothing can call `TakeDamage` until Task 4 wires up shooting. That's expected, not a gap.)

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Health.cs Assets/Scripts/Health.cs.meta Assets/Scenes/SampleScene.unity
git commit -m "Add reusable Health component and dummy target"
```

---

### Task 3: `PlayerAim` (mouse-aim rotation)

**Files:**
- Create: `Assets/Scripts/PlayerAim.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (component attached to capsule, Inspector fields wired)

**Interfaces:**
- Consumes: `Ground` layer from Task 1.
- Produces: rotates this GameObject's `transform` to face the mouse cursor's projected ground position every frame. `PlayerShooting` (Task 4) reads `transform.forward` afterward — no direct code dependency, just relies on `PlayerAim` having already rotated the transform this frame.

- [ ] **Step 1: Write `PlayerAim.cs`**

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private LayerMask groundMask;

    void Update()
    {
        Ray ray = aimCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            Vector3 lookPoint = hit.point;
            lookPoint.y = transform.position.y;
            transform.LookAt(lookPoint);
        }
    }
}
```

- [ ] **Step 2: Attach and wire in the Inspector**

Select the player capsule → **Add Component** → `PlayerAim`. Drag `Main Camera` into the `Aim Camera` slot. Click the `Ground Mask` dropdown → uncheck `Everything` → check only `Ground`.

- [ ] **Step 3: Verify**

Enter Play mode. Move the mouse across the floor — the capsule should visibly rotate to face wherever the cursor is, independent of the movement keys. Move the mouse off the edge of the floor (pointing at empty space/sky) — the capsule should just hold its last facing, no console errors.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/PlayerAim.cs Assets/Scripts/PlayerAim.cs.meta Assets/Scenes/SampleScene.unity
git commit -m "Add mouse-aim rotation for player capsule"
```

---

### Task 4: `PlayerShooting` (hitscan attack) — full loop integration

**Files:**
- Create: `Assets/Scripts/PlayerShooting.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (component attached to capsule, Inspector fields wired)

**Interfaces:**
- Consumes: `Player` layer (Task 1, to exclude self-hits), `Health.TakeDamage(int amount)` (Task 2), existing `Attack` action already defined in the `Player` action map of `InputSystem_Actions` (no asset changes needed), `transform.forward` as rotated by `PlayerAim` (Task 3).
- Produces: nothing further tasks depend on — this is the integration point that completes the slice.

- [ ] **Step 1: Write `PlayerShooting.cs`**

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private float range = 50f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask hittableMask;

    void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;

        Vector3 origin = transform.position + Vector3.up;
        Debug.DrawRay(origin, transform.forward * range, Color.red, 1f);

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, range, hittableMask))
        {
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
```

`value.isPressed` guard matters here: `PlayerInput`'s Send Messages behavior can invoke `OnAttack` more than once per press/release cycle (started/performed/canceled), so without the guard a single click could fire multiple raycasts.

- [ ] **Step 2: Attach and wire in the Inspector**

Select the player capsule → **Add Component** → `PlayerShooting`. Set `Range` (default 50 is fine) and `Damage` (e.g. 10 — three hits kills the default 30-HP dummy). Click the `Hittable Mask` dropdown → `Everything` should already be checked by default — uncheck only `Player`, so the raycast can't hit the capsule's own collider.

- [ ] **Step 3: Verify — full loop**

Enter Play mode. Aim the capsule at the dummy (mouse over it). Press the Attack input (left mouse button by default binding). Expected:
- A red ray flashes in the Scene view from the capsule toward the dummy (visible if the Scene tab is active alongside Game view).
- Console logs `Dummy took 10 damage, 20/30 HP left` (or similar).
- After 3 hits, console logs `Dummy destroyed` and the dummy disappears from the scene.
- Firing at the floor, at empty space, or at something without `Health` produces no error and no log — silent no-op, as designed.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/PlayerShooting.cs Assets/Scripts/PlayerShooting.cs.meta Assets/Scenes/SampleScene.unity
git commit -m "Add hitscan shooting, completing the core shoot loop"
```

---

## Note on execution mode for this project

The two standard options below (Subagent-Driven, Inline Execution) both mean an agent writes the files directly. Earlier in this project the user asked to implement Unity work themselves in the Editor, with review after each piece — that preference should carry over here rather than defaulting to agent-authored code. See execution handoff message for the third option offered alongside the standard two.
