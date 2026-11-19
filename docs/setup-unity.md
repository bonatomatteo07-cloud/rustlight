# Configurare il progetto Unity (sul tuo computer)

Gli script sono pronti, ma il progetto Unity va creato con l'Editor. Per la build iOS serve un **Mac con Xcode**.

## 1. Crea il progetto
1. Installa **Unity Hub** e **Unity 6.3 LTS** (o la LTS più recente) con il modulo **iOS Build Support**.
2. In Unity Hub: New project → template **Universal 3D** (URP), nome `RustlightTmp`, in una cartella qualsiasi.
3. Chiudi Unity. Copia le cartelle `Assets/`, `Packages/` e `ProjectSettings/` del progetto appena creato
   dentro `UnityProject/` di questo repository, **unendole** a quelle esistenti (non sovrascrivere `Assets/Rustlight`).
4. In Unity Hub: Add → seleziona la cartella `UnityProject/` e aprila.

## 2. Pacchetti (Window → Package Manager → Unity Registry)
- **In-App Purchasing** (versione 5.x): obbligatorio, altrimenti `IAPManager` non compila.
- **Input System**: già presente nel template URP. Se Unity chiede di riavviare, accetta.
- **AI Navigation**: per generare la NavMesh su cui si muovono i nemici.
- **TextMeshPro / uGUI**: già inclusi in Unity 6.

## 3. Scena di prova
1. GameObject vuoto `Game` con i componenti `GameServices` e `IAPManager`.
2. Un Plane come terreno, con `NavMeshSurface` (AI Navigation) → **Bake**.
3. Giocatore: una Capsule con i componenti `CharacterController`, `Health`, `PlayerController` e `PlayerCombat`. Più un Empty `Respawn` da collegare a `respawnPoint`.
4. Main Camera con `FollowCamera` (target = giocatore).
5. Nemico: una Capsule con `NavMeshAgent`, `Health`, `Enemy` e un Collider, sul layer `Enemy` (da impostare anche in `PlayerCombat.enemyLayers`).
6. Canvas (Screen Space Overlay, Canvas Scaler "Scale With Screen Size" 1170×2532):
   - HUD con `HudView` (barra della vita = Image di tipo Filled).
   - Controlli touch: **On-Screen Stick** (Control Path `<Gamepad>/leftStick`) e **On-Screen Button** (`<Gamepad>/buttonSouth`).
   - Pannello negozio con `ShopView`, più un prefab riga con `ShopItemView`.
7. Premi Play: WASD per muoverti, Spazio per attaccare.

## 4. Test degli acquisti
- Nell'Editor Unity IAP usa uno store finto: puoi provare l'intero flusso senza Apple.
- Su iPhone: build iOS → apri il progetto Xcode → aggiungi la capability **In-App Purchase** → esegui sul dispositivo con un account Sandbox.

## 5. Test della logica (senza Unity)
```bash
dotnet test tests/Rustlight.Core.Tests
```
