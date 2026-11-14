# Versioni Free e Pro

Divisione prevista delle funzioni tra la versione free (open source, MIT) e la versione pro
(closed source, a pagamento).

Il confine è tracciato sulle **funzioni**, non sul tipo di account: nella free il laboratorio
usa il proprio Client ID, quindi un blocco sugli account aziendali nel codice MIT si
aggirerebbe facilmente. Il valore della pro sta nelle acquisizioni aziendali senza la
password dell'utente, nei backend in sola lettura e nella reportistica forense.

| Funzione | Free | Pro |
|---|:---:|:---:|
| **Autenticazione** | | |
| IMAP generico con utente e password | ✅ | ✅ |
| Gmail con password per le app | ✅ | ✅ |
| OAuth Microsoft personale (Outlook.com / Hotmail) con login dell'utente | ✅ | ✅ |
| OAuth Google personale (Gmail) con login dell'utente | ✅ | ✅ |
| OAuth M365 / Workspace con login dell'utente | ✅¹ | ✅ |
| Login con device code (l'utente si autentica dal proprio dispositivo) | ✅ | ✅ |
| Client ID Microsoft predefinito (solo account personali) | ✅ | ✅ |
| Client ID / `client_secret.json` del laboratorio | ✅ | ✅ |
| Più profili "app" salvati (cambio progetto Google quando si arriva a 100 utenti) | — | ✅ |
| **Acquisizione aziendale senza la password dell'utente** | | |
| M365 senza utente (client credentials, `IMAP.AccessAsApp`) | — | ✅ |
| Workspace con account di servizio e delega a livello di dominio | — | ✅ |
| Più caselle in un'unica sessione (lista utenti / tutto il tenant) | — | ✅ |
| **Backend di acquisizione** | | |
| IMAP (cartelle aperte in sola lettura) | ✅ | ✅ |
| Microsoft Graph in sola lettura (`Mail.Read`, anche con IMAP disabilitato, Recoverable Items) | — | ✅ |
| Gmail API in sola lettura (`gmail.readonly`, etichette) | — | ✅ |
| **Forense e catena di custodia** | | |
| `.eml` con data originale, MD5 per file, SHA-1 dello ZIP | ✅ | ✅ |
| Filtri di ricerca, ripresa dopo interruzione, verifica dei file scaricati | ✅ | ✅ |
| Log di audit (metodo di autenticazione, Client ID, account, scope, orari) | ✅ | ✅ |
| Promemoria per la revoca dell'accesso a fine acquisizione | ✅ | ✅ |
| Report di acquisizione PDF firmato, da allegare alla perizia | — | ✅ |
| Hash aggiuntivi (SHA-256) e marca temporale certificata | — | ✅ |
| Documentazione di validazione dello strumento | — | ✅ |
| Visualizzatore integrato a fine acquisizione, in sola lettura sullo ZIP ([dettagli](#visualizzatore-integrato-pro)) | — | ✅ |
| **Distribuzione e supporto** | | |
| Codice sorgente MIT | ✅ | — (closed source) |
| Eseguibile firmato digitalmente | — | ✅ |
| Supporto e aggiornamenti prioritari quando cambiano le API | — | ✅ |

¹ Tecnicamente funziona con il Client ID del laboratorio e il tenant impostato su `common`
o `organizations`; il tenant aziendale può però richiedere il consenso dell'amministratore.

## Visualizzatore integrato (pro)

A download terminato, un visualizzatore apre l'acquisizione per una prima revisione senza
estrarre nulla e senza strumenti esterni. Spunto per funzioni e grafica:
[STViewer](https://github.com/RandyAybar/STViewer) (Rust/Tauri, licenza MIT o Apache-2.0).
Lo prendiamo come riferimento, non come dipendenza: il progetto è molto giovane e scrive
indici e log accanto all'archivio aperto, cosa da evitare su una prova.

Implementazione prevista in .NET con MimeKit (già usato dal progetto).

**Vincoli forensi**

- Legge i `.eml` direttamente dallo ZIP, in sola lettura: niente estrazione, niente file
  scritti accanto alla prova. Prima dell'apertura ricalcola lo SHA-1 dello ZIP e lo confronta
  con quello del log di acquisizione.
- HTML mostrato con contenuti remoti e script bloccati: aprire un'immagine remota
  scaricherebbe i pixel di tracciamento e avviserebbe il mittente della lettura.
- Per ogni messaggio mostra l'MD5 registrato durante l'acquisizione.

**Schermate**

- Elenco messaggi per cartella: mittente, oggetto, data, dimensione, allegati; ordinabile
  e con ricerca su mittente, destinatari, oggetto, testo e intervallo di date.
- **Message**: corpo (testo o HTML protetto) e allegati, apribili o esportabili.
- **Conversation**: thread ricostruito da `Message-ID`, `In-Reply-To` e `References`.
- **Headers**, sul modello della schermata di STViewer:
  - *Overview*: From, To/Cc, data nel fuso dell'esaminatore **e** con l'orologio del
    mittente (offset originale del campo `Date`), oggetto, client usato (`X-Mailer` /
    `User-Agent`), Message-ID, In-Reply-To, References.
  - *Security*: esiti SPF, DKIM, DMARC (da `Authentication-Results` /
    `ARC-Authentication-Results`) e punteggio antispam se presente (es. `X-MS-Exchange-…-SCL`),
    come badge colorati.
  - *Delivery path*: i campi `Received` letti dal basso verso l'alto e mostrati come
    timeline dal server di invio alla casella: host, IP, protocollo (`SMTP`, `ESMTPS`,
    `ESMTPSA`, `LMTP`…), versione TLS quando dichiarata, orario di ogni passaggio, ritardo
    rispetto al precedente e tempo totale di transito. Evidenzia i passaggi senza TLS e gli
    orari incoerenti (un hop "prima" del precedente), utili per individuare intestazioni
    alterate.
  - *All headers*: elenco completo e grezzo, copiabile.
- Esportazione di un messaggio o di una selezione (EML, PDF per la perizia) con l'hash del
  file esportato nel log.

## Stato della versione free

| Funzione | Stato |
|---|---|
| OAuth Microsoft: browser e device code | Implementato |
| OAuth Google con `client_secret.json` del laboratorio | Implementato |
| Log di audit dell'accesso OAuth | Implementato |
| Revoca dell'accesso a fine acquisizione (automatica su Google, guidata su Microsoft) | Implementato |
| Client ID Microsoft predefinito (forzato sul tenant `consumers`) | Implementato |
