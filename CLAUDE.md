# cold_embrace — Claude Instructions

## Project context

This is a Unity learning project. The user (Vlad) is using it to learn Unity hands-on.
When explaining things, favor clear explanations of the *why* alongside the *what*.
Prefer simple, readable C# over clever abstractions — this is a learning context, not production code.

## Unity version & render pipeline

- **Unity 6** (6000.5.1f1)
- **Universal Render Pipeline (URP)** 17.5.0 — do not suggest Built-in RP or HDRP APIs
- Renderer assets live in `Assets/Settings/`

## Key packages in use

| Package | Version |
|---|---|
| Input System (new) | 1.19.0 |
| AI Navigation (NavMesh) | 2.0.13 |
| Timeline | 1.8.12 |
| Visual Scripting | 1.9.11 |
| URP | 17.5.0 |
| UI Toolkit / uGUI | 2.5.0 |

Always use the **new Input System** (`UnityEngine.InputSystem`) — never `Input.GetKey` / legacy Input Manager.

## Project structure

```
Assets/
  Scenes/          # Unity scenes
  Settings/        # URP render pipeline & post-processing assets
  TutorialInfo/    # Template tutorial scripts (can be ignored/deleted)
```

New scripts, prefabs, and assets should go under `Assets/` in logically named subfolders
(e.g. `Assets/Scripts/`, `Assets/Prefabs/`, `Assets/Materials/`).

## Design principles

Apply in this priority order:

1. **YAGNI** — only build what the current task requires. If it can be done in 5 lines instead of 10, do it in 5. No speculative abstractions, base classes, events, or ScriptableObjects added "for later."
2. **KISS** — when two approaches both satisfy YAGNI, pick the simpler one.
3. **SOLID** — Single Responsibility is usually worth it; avoid premature Open/Closed or DI abstractions.

## Unity documentation

For non-trivial Unity-specific API questions (method signatures, package APIs like URP/Input System/NavMesh, Unity 6-specific behavior), check the official docs rather than answering from memory. Do not look up general C# fundamentals or universally-known Unity basics (MonoBehaviour lifecycle, `[SerializeField]`, etc.). Use judgment.

When answering a question about a specific Unity feature, append a single relevant docs link at the end of the response:
> **Docs:** [Page title](url)

## Coding conventions

- C# scripts must have `using UnityEngine;` at the top (and other namespaces as needed).
- Every MonoBehaviour file name must match the class name exactly.
- Prefer `[SerializeField] private` over `public` for inspector-exposed fields.
- Keep `Start` / `Awake` / `Update` lean — extract logic into named methods.
- No comments that just restate what the code does; comments only for non-obvious decisions.

## What to avoid

- Do not suggest `FindObjectOfType` in hot paths — cache references instead.
- Do not use `OnGUI` for UI — use UI Toolkit or uGUI Canvas.
- Do not add `.meta` files manually; Unity manages them.

## Misc

- The project has no git repo yet. Don't suggest git commands unless the user sets one up.
- IDE: Rider is configured (`com.unity.ide.rider`).
