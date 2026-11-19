# Configurare il progetto Unity (sul tuo computer)

Gli script sono pronti, ma il progetto Unity va creato con l'Editor. Per la build Steam basta un PC Windows.

## 1. Crea il progetto
1. Installa **Unity Hub** e **Unity 6.3 LTS** (o la LTS più recente). Su Windows la build per Windows è già inclusa; aggiungi il modulo **Linux Build Support (Mono/IL2CPP)** se vuoi una build nativa per Steam Deck.
2. In Unity Hub: New project → template **Universal 3D** (URP), nome `RustlightTmp`, in una cartella qualsiasi.
3. Chiudi Unity. Copia le cartelle `Assets/`, `Packages/` e `ProjectSettings/` del progetto appena creato
   dentro `UnityProject/` di questo repository, **unendole** a quelle esistenti (non sovrascrivere `Assets/Rustlight`).
4. In Unity Hub: Add → seleziona la cartella `UnityProject/` e aprila.

## 2. Pacchetti
Da Window → Package Manager → Unity Registry:
- **Input System**: già presente nel template URP. In Player Settings → Active Input Handling scegli "Input System Package".
- **AI Navigation**: per generare la NavMesh su cui si muovono i nemici.
- **TextMeshPro / uGUI**: già inclusi in Unity 6.

**Steamworks.NET**: Package Manager → "+" → *Install package from git URL*, con l'URL indicato nel README di
https://github.com/rlabrecque/Steamworks.NET (sezione "Installation", scegli l'ultima release).
Poi, in Player Settings → Scripting Define Symbols, controlla che ci sia `STEAMWORKS_NET`, e se manca aggiungilo.
Senza questo passaggio il gioco funziona lo stesso, ma senza Steam: niente DLC reali né obiettivi.

## 3. Scena di prova
1. GameObject vuoto `Game` con il componente `GameServices`, più un AudioMixer con i parametri esposti `MasterVolume`, `MusicVolume` e `SfxVolume`.
2. Un Plane come terreno, con `NavMeshSurface` (AI Navigation) → **Bake**.
3. Giocatore: una Capsule con i componenti `CharacterController`, `Health`, `PlayerController` e `PlayerCombat`. Più un Empty `Respawn` da collegare a `respawnPoint`.
4. Main Camera con `FollowCamera` (target = giocatore).
5. Nemico: una Capsule con `NavMeshAgent`, `Health`, `Enemy` e un Collider, sul layer `Enemy` (da impostare anche in `PlayerCombat.enemyLayers`).
6. Canvas (Screen Space Overlay, Canvas Scaler "Scale With Screen Size", 1920×1080, Match 0,5) con un EventSystem:
   - HUD con `HudView` (barra della vita = Image di tipo Filled).
   - Menu di pausa con `PauseMenu`. Dentro: le opzioni (`SettingsView`), i comandi (una `RebindRow` per azione) e i DLC (`DlcView`, con un prefab riga `DlcRowView`).
7. Premi Play: WASD per muoverti, clic sinistro o Spazio per attaccare verso il cursore, Esc per la pausa. Con un gamepad: stick sinistro, A, Start.

## 4. Test con Steam
- Avvia il client Steam e accedi. Il file `UnityProject/steam_appid.txt` contiene 480 (Spacewar, l'app di test pubblica di Valve), quindi Steam si inizializza anche nell'Editor.
- Gli obiettivi e i DLC veri funzionano solo con il tuo App ID. Fino ad allora usa `editorOwnedDlc` su `GameServices` per simulare i DLC.

## 5. Test della logica (senza Unity)
```bash
dotnet test tests/Rustlight.Core.Tests
```
