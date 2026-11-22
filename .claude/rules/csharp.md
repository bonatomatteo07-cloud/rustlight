---
paths:
  - "**/*.cs"
  - "**/*.asmdef"
  - "**/*.csproj"
---

<!-- Adapted from the C# rules of ECC (https://github.com/affaan-m/ECC, MIT). See .claude/THIRD_PARTY_NOTICES.md. -->

# C# rules for Rustlight

- C# 9 only (Unity 6). No `required`, records, collection expressions, file-scoped namespaces, raw strings.
- Nullable reference types are off; handle real null paths explicitly.
- `Scripts/Core` must not reference `UnityEngine` or any Unity package. New game rules go there, with tests in
  `tests/Rustlight.Core.Tests`.
- `sealed` classes by default, explicit access modifiers, `_camelCase` private fields, PascalCase public API.
  Serialized save fields stay public lowercase fields (`JsonUtility`); renaming them needs a save migration.
- Player actions that can fail are all-or-nothing `TryX` methods returning `bool`; no swallowed exceptions.
- On `UnityEngine.Object` use `== null`, never `?.`/`??`/`is null`. No allocations, `GetComponent` or LINQ per frame.
- No secrets in source; no `BinaryFormatter`.
- After changing Core, run `dotnet test tests/Rustlight.Core.Tests`. Runtime code is only verified in the Unity Editor:
  say so when it was not compiled.
