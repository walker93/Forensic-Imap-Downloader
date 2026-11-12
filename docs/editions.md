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
| **Distribuzione e supporto** | | |
| Codice sorgente MIT | ✅ | — (closed source) |
| Eseguibile firmato digitalmente | — | ✅ |
| Supporto e aggiornamenti prioritari quando cambiano le API | — | ✅ |

¹ Tecnicamente funziona con il Client ID del laboratorio e il tenant impostato su `common`
o `organizations`; il tenant aziendale può però richiedere il consenso dell'amministratore.

## Stato della versione free

| Funzione | Stato |
|---|---|
| OAuth Microsoft: browser e device code | Implementato |
| OAuth Google con `client_secret.json` del laboratorio | Implementato |
| Log di audit dell'accesso OAuth | Implementato |
| Revoca dell'accesso a fine acquisizione (automatica su Google, guidata su Microsoft) | Implementato |
| Client ID Microsoft predefinito | Da fare: serve la registrazione dell'app (vedi [oauth-setup.md](oauth-setup.md)), poi va inserito in `OAuthAppSettings.DefaultMicrosoftClientId` |
