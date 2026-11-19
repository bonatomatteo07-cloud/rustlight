# Rustlight (titolo provvisorio)

Avventura/RPG d'azione in 3D, ambientata in un mondo post-apocalittico, per iPhone (App Store), con acquisti in-app. Motore: Unity 6 (URP).

- Design del gioco: [docs/GDD.md](docs/GDD.md)
- Acquisti e checklist App Store: [docs/monetization.md](docs/monetization.md)
- Configurare il progetto Unity: [docs/setup-unity.md](docs/setup-unity.md)

## Struttura
```
UnityProject/Assets/Rustlight/Scripts/
  Core/      logica di gioco in C# puro (nessuna dipendenza da Unity, coperta da test)
  Runtime/   MonoBehaviour: salvataggi, acquisti (Unity IAP 5), giocatore, nemici, UI
tests/       test xUnit della logica Core (dotnet test)
docs/        design e guide
```

## Stato
- Core: compilato e testato (`dotnet test tests/Rustlight.Core.Tests`).
- Runtime: scritto ma **non ancora compilato in Unity**. Va aperto nell'Editor seguendo `docs/setup-unity.md`.
