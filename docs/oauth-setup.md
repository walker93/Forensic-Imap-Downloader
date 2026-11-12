# OAuth setup

Microsoft and Google no longer accept a plain password over IMAP for most accounts. To
sign in with OAuth the tool needs an **application registration** (a client ID) with each
provider. Registration is free and needs no verification or audit; you do it once per lab.

The client ID is not a secret. Tokens are kept in memory only: nothing that grants access
to the mailbox is written to disk or to `report.log`.

## Microsoft (Outlook.com, Hotmail, Microsoft 365)

1. Sign in to the [Azure portal](https://portal.azure.com) with an account that has a
   Microsoft Entra tenant (a free Azure account creates one).
2. **Microsoft Entra ID → App registrations → New registration**
   - *Supported account types*: **Personal Microsoft accounts only**, or *Accounts in any
     organizational directory and personal Microsoft accounts* to also acquire work accounts.
   - *Redirect URI*: platform **Public client/native (mobile & desktop)**, value `http://localhost`.
3. **Authentication → Advanced settings → Allow public client flows**: **Yes** (needed for
   the device-code sign-in).
4. **API permissions → Add a permission → APIs my organization uses → Office 365 Exchange
   Online → Delegated permissions → `IMAP.AccessAsUser.All`**.
5. Copy the **Application (client) ID** from the *Overview* page.

In the tool: **Auth → Microsoft OAuth → OAuth settings…**

| Field | Value |
|---|---|
| Client ID | The application (client) ID |
| Tenant | `consumers` for personal accounts; `common` for personal and work accounts; `organizations` or a tenant ID for work accounts only. Must be compatible with the *Supported account types* chosen above. |
| Device code | Tick to have the account owner sign in on their own device instead of in a browser on this workstation |

Work accounts: the tenant may require an administrator to consent to the application, and
IMAP must be enabled for the mailbox.

## Google (Gmail)

1. In the [Google Cloud console](https://console.cloud.google.com) create a project (no
   billing needed).
2. **Google Auth Platform → Audience**: user type **External**, publishing status **Testing**.
3. **Audience → Test users**: add the address of **every mailbox you are going to acquire**.
   Only test users can sign in.
4. **Data access → Add or remove scopes**: add `https://mail.google.com/`.
5. **Clients → Create client → Desktop app**, then download the JSON file
   (`client_secret_….json`).

In the tool: **Auth → Google OAuth → OAuth settings… → Browse**, select the JSON file, and
enter the mailbox address in **Username**.

Limits of an unverified Google project, none of which matter for a single acquisition:

- the sign-in shows a "Google hasn't verified this app" warning (**Continue**);
- at most 100 test users over the lifetime of the project: create a new project when it runs out;
- the grant expires after 7 days.

Gmail accounts with 2-Step Verification can alternatively use an **app password** with
**Auth → Password**, which needs no registration.

## What the sign-in leaves on the account

An OAuth sign-in is visible to the account owner: a security notification, and an entry in
the list of applications with access to the account. Record this in your notes.

When the download finishes the tool asks whether to end the access:

- **Google**: the grant is revoked at Google.
- **Microsoft**: public clients cannot revoke a grant; the tokens are discarded and the
  tool shows the page where the account owner removes the application
  (`https://account.live.com/consent/Manage` for personal accounts).

Either outcome is written to `report.log`, together with the sign-in itself: method,
client ID, signed-in account, granted scopes and token expiry.
