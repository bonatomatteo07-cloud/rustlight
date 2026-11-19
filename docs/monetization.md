# Monetizzazione e App Store

## Catalogo (`Core/Monetization/StoreCatalog.cs`)
| ID prodotto (suffisso) | Tipo | Contenuto | Prezzo indicativo |
|---|---|---|---|
| `.cores_small` | Consumabile | 100 Nuclei | 0,99 € |
| `.cores_medium` | Consumabile | 550 Nuclei | 4,99 € |
| `.cores_large` | Consumabile | 1200 Nuclei | 9,99 € |
| `.starter_pack` | Non consumabile | 300 Nuclei, tuta "Predone delle dune", +10 slot d'inventario | 2,99 € |
| `.supporter_pack` | Non consumabile | tuta "Avanguardia", drone "Aureola", +20 slot | 7,99 € |

Il prefisso è `com.yourstudio.rustlight`: **sostituiscilo** in `StoreCatalog.BundlePrefix` con il tuo
bundle id, e crea i prodotti in App Store Connect con gli **stessi identici ID**.

I prezzi mostrati nel gioco arrivano sempre dall'App Store, già localizzati: non sono scritti nel codice.

Con i Nuclei si comprano cosmetici (es. "Neon del vecchio mondo", 400 Nuclei). Niente che renda il personaggio più forte.

## Come funziona un acquisto (`Runtime/Services/IAPManager.cs`, Unity IAP 5)
1. `PurchaseProduct` apre la schermata di pagamento di Apple.
2. `OnPurchasePending`: `PurchaseGrantService.Grant` accredita il contenuto.
3. Il gioco salva su disco.
4. Solo se il salvataggio è riuscito, chiama `ConfirmPurchase`. Altrimenti l'App Store riconsegna
   l'acquisto al prossimo avvio: **nessun acquisto va perso**.
5. Gli acquisti duplicati vengono ignorati: i consumabili per ID transazione, i non consumabili per ID prodotto.
6. **Ripristina acquisti**: c'è un pulsante nel negozio (`ShopView`), obbligatorio per la revisione Apple.
7. **"Chiedi di acquistare"** (Family Sharing): il negozio mostra "In attesa di approvazione" e accredita solo dopo l'approvazione.

Nota: se il giocatore reinstalla l'app perdendo il salvataggio, il ripristino ridà anche i 300 Nuclei
dello Starter Pack. È una scelta voluta: il giocatore non perde ciò che ha pagato.

## Checklist App Store
- [ ] Account Apple Developer (99 $/anno) e contratto "Paid Apps" firmato in App Store Connect, con dati bancari e fiscali (senza il contratto gli acquisti non funzionano).
- [ ] App creata in App Store Connect con il bundle id definitivo.
- [ ] I 5 prodotti creati con nome, descrizione, prezzo e screenshot di revisione.
- [ ] Account Sandbox per i test (Utenti e accessi → Sandbox) e test sull'iPhone.
- [ ] Privacy: compilare le "Privacy Nutrition Labels" e aggiungere un link alla privacy policy.
- [ ] Nessuna loot box. Se un giorno le aggiungi, Apple obbliga a mostrare le probabilità prima dell'acquisto.
- [ ] Facoltativo, ma consigliato prima del lancio: validare gli acquisti lato server con `order.Info.Apple?.jwsRepresentation` (App Store Server API).
