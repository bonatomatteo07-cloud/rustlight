# Rustlight: documento di design (GDD)

> "Rustlight" è un titolo provvisorio: prima di usarlo verifica che sia libero su Steam e come marchio.

## In una frase
Un recuperatore e il suo drone esplorano le rovine di un mondo spento per riaccendere il generatore
del loro Bunker. È un'avventura/RPG d'azione in 3D per PC (Steam e Steam Deck), venduta a prezzo
pieno, con circa 10 ore di gioco nel primo capitolo.

## Pilastri
1. **Esplorare paga**: ogni zona nasconde rottami, equipaggiamento e scorciatoie.
2. **Combattimento leggibile**: con mouse e tastiera si attacca verso il cursore, con il gamepad c'è la mira assistita. Le abilità arriveranno più avanti.
3. **Progressione visibile**: livello, equipaggiamento e Bunker crescono in modo chiaro.
4. **Si paga una volta**: il gioco base è completo. I DLC aggiungono capitoli e cosmetici, mai statistiche. Niente microtransazioni e niente loot box.

## Ciclo di gioco
- **Momento per momento**: muoviti, avvicinati, attacca, raccogli il bottino.
- **Sessione**: dal Bunker accetti una missione, entri in una zona, la completi, torni e la riscuoti, poi equipaggi il nuovo bottino.
- **Lungo termine**: livelli (massimo 30), equipaggiamento più raro, nuove zone e potenziamenti del Bunker.

## Ambientazione e stile
- Mondo post-apocalittico sci-fi: deserti di ruggine, città sommerse dalla sabbia, impianti radioattivi.
- Stile **low-poly stilizzato**, con palette calda (ruggine, ocra) e accenti freddi (neon ciano) per il loot e l'interfaccia. Gira bene anche su Steam Deck e richiede poca arte.
- Visuale a tre quarti dall'alto (`FollowCamera`).

## Contenuti del primo capitolo (MVP)
| Zona | Contenuto |
|---|---|
| Bunker (hub) | Negozio, armadietto per i cosmetici, bacheca delle missioni |
| Discarica | Zona tutorial con predoni base e la missione "Primi passi" |
| Deposito | Combattimento più denso e la missione "Ripulire il deposito" |

- **Nemici**: predone da mischia (`Enemy`) e, in seguito, un predone a distanza e un mini-boss.
- **Oggetti**: 7 oggetti iniziali (`GameContent.Items`) negli slot Arma, Armatura e Gadget.
- **Missioni**: 3 iniziali (`GameContent.Quests`), di tipo Uccidi, Raccogli e Raggiungi.

## Sistemi (già implementati in `Scripts/Core`, coperti da test)
| Sistema | File | Note |
|---|---|---|
| Statistiche | `Stats/StatBlock.cs` | (base + piatti) × (1 + percentuali), indipendente dall'ordine |
| Livelli | `Progression/LevelCurve.cs` | XP al livello successivo = 100 · livello^1,5 |
| Danni | `Combat/DamageCalculator.cs` | attacco × 100/(100+armatura); critico ×1,5; minimo 1 |
| Inventario | `Items/Inventory.cs` | 20 slot, oggetti impilabili, aggiunte "tutto o niente" |
| Equipaggiamento | `Items/Equipment.cs` | scambio sicuro e statistiche sempre allineate |
| Valute | `Economy/Wallet.cs` | Rottami (comuni) e Nuclei (rari, dalle missioni): entrambi si guadagnano solo giocando |
| Cosmetici | `Economy/CosmeticShop.cs` | tute e skin del drone, solo estetici |
| DLC | `Dlc/Dlc.cs` | quello che un DLC sblocca si ricalcola dal possesso su Steam, non viene salvato |
| Obiettivi | `Achievements/Achievements.cs` | registrati nel salvataggio e inviati a Steam appena possibile |
| Opzioni | `Settings/GameSettings.cs` | risoluzione, finestra, V-Sync, qualità, volumi, tasti |
| Missioni | `Quests/Quests.cs` | ricompense riscattabili una sola volta |
| Salvataggi | `Save/SaveData.cs` | versionati e con migrazione; scrittura atomica con backup (`SaveService`) |

## Modello di business
Vedi [steam.md](steam.md).

## Prossimi passi di design
- Abilità attive del drone (scudo, impulso EMP) con ricarica.
- Potenziamenti del Bunker che consumano Rottami: la principale fonte di spesa della valuta gratuita.
- Capitolo II "La Città Sommersa" come primo DLC di contenuto.
- Porting su iPhone in futuro: la logica Core è già indipendente dalla piattaforma.
