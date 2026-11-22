# Rustlight (titolo provvisorio)

Avventura/RPG d'azione in 3D, ambientata in un mondo post-apocalittico, per PC su Steam (anche Steam Deck): gioco a pagamento con DLC. Motore: Unity 6 (URP).

- Design del gioco: [docs/GDD.md](docs/GDD.md)
- Steam, DLC, obiettivi e checklist di pubblicazione: [docs/steam.md](docs/steam.md)
- Configurare il progetto Unity: [docs/setup-unity.md](docs/setup-unity.md)

## Struttura
```
UnityProject/Assets/Rustlight/Scripts/
  Core/      logica di gioco in C# puro (nessuna dipendenza da Unity, coperta da test)
  Runtime/   MonoBehaviour: Steam (Steamworks.NET), salvataggi, input rimappabile, giocatore, nemici, UI
tests/       test xUnit della logica Core (dotnet test)
docs/        design e guide
.claude/     configurazione Claude Code: agente csharp-reviewer, skill csharp-testing e dotnet-patterns,
             regole C# (adattati da ECC per Unity 6 / C# 9, vedi .claude/THIRD_PARTY_NOTICES.md)
```

## Stato
- Core: compilato e testato (`dotnet test tests/Rustlight.Core.Tests`).
- Runtime: scritto ma **non ancora compilato in Unity**. Va aperto nell'Editor seguendo `docs/setup-unity.md`.
