---
name: dotnet-patterns
description: C# conventions for Rustlight (Unity 6, C# 9) — Core/Runtime split, immutability, guard clauses, results instead of exceptions, events, Unity-safe performance. Use when writing or reviewing C# in UnityProject/.
---

<!-- Adapted from the dotnet-patterns skill of ECC (https://github.com/affaan-m/ECC, MIT). See .claude/THIRD_PARTY_NOTICES.md. -->

# C# patterns for Rustlight

## 1. Core vs Runtime

- `Scripts/Core` (`Rustlight.Core.asmdef`, `noEngineReferences: true`): rules, numbers, state, saves. Pure C#,
  deterministic, unit tested.
- `Scripts/Runtime`: MonoBehaviours and services that read input, call Steam, draw UI and forward to Core.
  Keep them thin. If a MonoBehaviour contains a formula or a rule, it belongs in Core.
- Core talks to the outside through small interfaces (`IRandom`, platform/achievement sinks) that Runtime implements.

## 2. Language level

C# 9 only (Unity). Allowed: `is`/`switch` patterns, `?.`/`??` on plain C# objects, target-typed `new()`,
expression-bodied members. Not allowed: `required`, collection expressions, file-scoped namespaces, primary
constructors on classes, raw strings. Avoid `record` and `init` too: Unity needs an `IsExternalInit` shim for them.

## 3. Explicit and sealed

- Explicit access modifiers; `sealed` on every class not designed for inheritance.
- `private readonly` for dependencies; validate constructor arguments with guard clauses:

```csharp
public Wallet(WalletData data)
{
    _data = data ?? throw new ArgumentNullException(nameof(data));
}
```

- Expose collections as `IReadOnlyList<T>`; mutate only through methods that keep invariants.

## 4. Expected failures are results, not exceptions

Player actions that can legitimately fail (buy, equip, add to inventory) return `bool` via `TryX`, or a small
result enum, and leave state untouched on failure. Throw only for programming errors (bad arguments, missing
content definitions).

```csharp
// Core/Economy/Wallet.cs
public bool TrySpendScrap(long amount)
{
    if (amount < 0 || _data.scrap < amount) return false;
    _data.scrap -= amount;
    Changed?.Invoke();
    return true;
}
```

## 5. Guard clauses over nesting

Return early; keep the happy path unindented. Methods over ~50 lines get split.

## 6. Events

- Core raises plain `event Action`/`event Action<T>` after state changes; UI subscribes.
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`. Never leave a destroyed view subscribed.

## 7. Serialized data

- `[Serializable]` save classes use public lowercase fields because `JsonUtility` only reads fields.
  Do not convert them to properties and do not rename them without a migration step in `SaveData`.
- Bump `SaveData.CurrentVersion` when the shape changes and upgrade old saves in the migration step;
  old saves must still load.

## 8. Unity-safe code

- Never use `?.`, `??` or `is null` on `UnityEngine.Object`; use `== null`.
- Cache components and `Camera.main` in `Awake`; no `GetComponent`/`Find`/LINQ/allocations per frame.
- Movement scaled by `Time.deltaTime`; physics in `FixedUpdate`.
- No `async void` except Unity event callbacks; Unity APIs only on the main thread.
- No `BinaryFormatter`. No secrets in source (`steam_appid.txt` holds only the public app id).

## Anti-patterns

| Anti-pattern | Fix |
|---|---|
| Game rule inside a MonoBehaviour | Move to Core and test it |
| `catch (Exception) { }` | Log with context and fall back safely |
| Magic strings for ids/actions | Constants or content tables |
| Mutable `static` state | Instance owned by `GameServices` |
| Public setters on game state | Methods that enforce invariants |
| Partial success on player actions | All-or-nothing `TryX` |
