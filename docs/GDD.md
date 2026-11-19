# Rustlight: documento di design (GDD)

> "Rustlight" è un titolo provvisorio: prima di usarlo verifica che sia libero sull'App Store e come marchio.

## In una frase
Un recuperatore e il suo drone esplorano le rovine di un mondo spento per riaccendere il generatore
del loro Bunker. È un'avventura/RPG d'azione in 3D, pensata per partite da 5–15 minuti su iPhone.

## Pilastri
1. **Esplorare paga**: ogni zona nasconde rottami, equipaggiamento e scorciatoie.
2. **Combattimento semplice e leggibile**: una mano sullo stick e un pulsante d'attacco con mira assistita. Le abilità arriveranno più avanti.
3. **Progressione visibile**: livello, equipaggiamento e Bunker crescono in modo chiaro.
4. **Equo verso chi non paga**: con soldi veri si comprano solo cosmetici, comodità e la valuta premium. Niente statistiche in vendita e niente loot box.

## Ciclo di gioco
- **Momento per momento**: muoviti, avvicinati, attacca, raccogli il bottino.
- **Sessione**: dal Bunker accetti una missione, entri in una zona, la completi, torni e la riscuoti, poi equipaggi il nuovo bottino.
- **Lungo termine**: livelli (massimo 30), equipaggiamento più raro, nuove zone e potenziamenti del Bunker.

## Ambientazione e stile
- Mondo post-apocalittico sci-fi: deserti di ruggine, città sommerse dalla sabbia, impianti radioattivi.
- Stile **low-poly stilizzato**, con palette calda (ruggine, ocra) e accenti freddi (neon ciano) per il loot e l'interfaccia. Regge bene su mobile e richiede poca arte.
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
| Valute | `Economy/Wallet.cs` | Rottami (si guadagnano giocando) e Nuclei (premium) |
| Cosmetici | `Economy/CosmeticShop.cs` | tute e skin del drone, solo estetici |
| Missioni | `Quests/Quests.cs` | ricompense riscattabili una sola volta |
| Salvataggi | `Save/SaveData.cs` | versionati e con migrazione; scrittura atomica con backup (`SaveService`) |
| Acquisti | `Monetization/*` | ogni acquisto viene accreditato una sola volta, anche al ripristino |

## Monetizzazione
Vedi [monetization.md](monetization.md).

## Prossimi passi di design
- Abilità attive del drone (scudo, impulso EMP) con ricarica.
- Potenziamenti del Bunker che consumano Rottami: la principale fonte di spesa della valuta gratuita.
- Abbonamento "Pass della Stagione" (solo dopo il lancio, se i dati lo giustificano).
