---
name: csharp-testing
description: How to write and run the xUnit tests for Rustlight's pure C# Core (tests/Rustlight.Core.Tests). Use when adding or changing game logic in UnityProject/Assets/Rustlight/Scripts/Core, or when reviewing test quality.
---

<!-- Adapted from the csharp-testing skill of ECC (https://github.com/affaan-m/ECC, MIT). See .claude/THIRD_PARTY_NOTICES.md. -->

# Testing Rustlight's Core

Game rules live in `UnityProject/Assets/Rustlight/Scripts/Core` (no Unity references). The test project
`tests/Rustlight.Core.Tests` compiles those files directly, so every Core change can be tested without the Editor.
Runtime MonoBehaviours are not covered here: test them in Unity (Play Mode) or move the logic into Core.

## Stack

- **xUnit** with the built-in `Assert` (no FluentAssertions, no mocking library: keep dependencies minimal).
- Hand-written fakes for interfaces, e.g. `FixedRandom : IRandom` in `CoreTests.cs`.
- Test project targets `net8.0` but pins `LangVersion 9.0` so tests and Core use the same syntax as Unity.
  Do not use `required`, collection expressions `[..]`, file-scoped namespaces or raw string literals.

## Running

```bash
dotnet test tests/Rustlight.Core.Tests
dotnet test tests/Rustlight.Core.Tests --filter "FullyQualifiedName~InventoryTests"
```

## Writing tests

- One test class per Core area (`StatsTests`, `InventoryTests`, ...). Name tests by behaviour:
  `TryAdd_ChangesNothing_WhenItDoesNotFit`.
- Arrange-Act-Assert, one logical assertion per test. Build fresh state in each test; xUnit creates a new
  class instance per test, so constructor setup is safe.
- Make randomness and time injectable (`IRandom`, explicit timestamps) so results are deterministic.
- Floats: compare with precision, `Assert.Equal(18f, value, 3)`.
- Use `[Theory]` + `[InlineData]` for tables of inputs (damage formulas, level curve, prices).
- Cover the failure paths that protect the player: all-or-nothing operations leave state unchanged,
  over-capacity inventories keep items, invalid ids are rejected, currency never goes negative.
- **Save compatibility:** when `SaveData` changes, add a test that loads an old-format JSON
  (`System.Text.Json` with `IncludeFields = true` mirrors `JsonUtility`'s field-based format) and checks the
  migration result.
- DLC and achievements: test both owned and not-owned states, and removal of a DLC after its content was used.

```csharp
public class WalletTests
{
    [Fact]
    public void TrySpendScrap_ChangesNothing_WhenBalanceIsTooLow()
    {
        // Arrange
        var wallet = new Wallet(new WalletData { scrap = 10 });

        // Act
        bool spent = wallet.TrySpendScrap(25);

        // Assert
        Assert.False(spent);
        Assert.Equal(10, wallet.Scrap);
    }
}
```

## Anti-patterns

| Anti-pattern | Fix |
|---|---|
| Testing private fields or call order | Assert on public state and events |
| Real randomness (`System.Random` without seed) | Inject `IRandom` |
| Shared static state between tests | Build state inside each test |
| Asserting on log text or `ToString()` | Assert on typed values |
| Logic only reachable from a MonoBehaviour | Move it to Core, then test it |
