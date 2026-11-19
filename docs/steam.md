# Steam: modello di business e pubblicazione

## Modello
- **Gioco base a pagamento**: prezzo indicativo 14,99 €, con sconto lancio del 10–15%.
- **DLC** (`Core/Dlc/Dlc.cs`):

| DLC | Contenuto | Prezzo indicativo |
|---|---|---|
| Pacchetto Sostenitore | tuta "Avanguardia", drone "Aureola", +10 slot d'inventario | 4,99 € |
| Capitolo II: La Città Sommersa | nuova zona, tuta "Palombaro" | 7,99 € |

  Più avanti si può aggiungere la colonna sonora come DLC.
- **Niente microtransazioni**: i Nuclei si guadagnano solo giocando e servono per i cosmetici.
  Per vendere singoli oggetti su Steam servirebbe un server proprio, e i giocatori PC li accolgono male.

## Come funzionano i DLC nel codice
- All'avvio il gioco chiede a Steam quali DLC possiedi (`SteamApps.BIsDlcInstalled`). Se ne compri uno mentre giochi, lo sblocco arriva subito, senza riavviare.
- Quello che un DLC sblocca **non viene salvato**: se un DLC viene rimborsato, al prossimo avvio sparisce. Se indossavi la tuta, il gioco torna a quella base. Gli oggetti negli slot extra restano, ma finché non liberi spazio non puoi raccoglierne di nuovi.
- Il pulsante "Vedi nel negozio" apre la pagina del DLC nell'overlay di Steam.
- Nell'Editor puoi simulare il possesso di un DLC dal campo `editorOwnedDlc` di `GameServices`.

## Obiettivi Steam
API name da creare in Steamworks → Stats & Achievements:

| API name | Nome | Condizione |
|---|---|---|
| `ACH_FIRST_RAIDER` | Primo sangue | sconfiggi il primo predone |
| `ACH_DEPOT_CLEARED` | Deposito ripulito | completa "Ripulire il deposito" |
| `ACH_SCRAPPER` | Rottamaio | guadagna 1000 Rottami in totale |
| `ACH_LEVEL_10` | Sopravvissuto esperto | raggiungi il livello 10 |
| `ACH_RAIDER_100` | Flagello dei predoni | sconfiggi 100 predoni |

## Salvataggi nel cloud (Steam Auto-Cloud, senza codice)
In Steamworks → Application → Steam Cloud: imposta una quota (es. 10 MB, 10 file) e aggiungi una regola Auto-Cloud:
- Root: `WinAppDataLocalLow`
- Sottocartella: `<Company Name>/<Product Name>` (quelli di Player Settings)
- File: `save.json*` (così include anche il backup `.bak`)

`settings.json` resta fuori di proposito: risoluzione e tasti dipendono dal singolo PC.

## Checklist di pubblicazione
- [ ] Account Steamworks e **Steam Direct**: 100 $ per gioco, rimborsati dopo 1.000 $ di incassi. Servono dati fiscali e bancari.
- [ ] Sostituire 480 con il proprio App ID: in `GameServices.steamAppId` e in `UnityProject/steam_appid.txt` (quel file serve solo in sviluppo e non va incluso nella build).
- [ ] Creare i DLC in Steamworks e scrivere i loro App ID in `DlcCatalog`.
- [ ] Creare gli obiettivi con gli API name della tabella sopra.
- [ ] Pagina del negozio con capsule, screenshot, trailer e descrizione. La pagina "Coming Soon" deve restare pubblica **almeno 2 settimane** prima del lancio: è il periodo in cui si raccolgono le wishlist.
- [ ] Revisione della pagina e della build da parte di Valve (alcuni giorni lavorativi ciascuna).
- [ ] Build Windows x64 caricata con SteamPipe (`steamcmd` + script `.vdf`). Facoltativi: Linux, per lo Steam Deck nativo, e macOS.
- [ ] Steam Deck: comandi completi da gamepad, testo leggibile a 1280×800, nessun launcher esterno.
- [ ] Facoltativo: una demo gratuita per lo Steam Next Fest.
