# Music User Service

Microservizio .NET 9 responsabile della gestione degli utenti, dell'autenticazione e dell'autorizzazione. Emette token JWT utilizzati dagli altri microservizi per identificare l'utente autenticato. Consuma eventi Kafka dal LibraryService per aggiornare il contatore di canzoni dell'utente.

---

## Composizione del progetto

Il servizio è strutturato in 5 progetti secondo l'architettura a layer:

| Progetto | Responsabilità |
|---|---|
| `Music.User.WebApi` | Controller REST, configurazione Swagger, consumer Kafka |
| `Music.User.Business` | Logica applicativa, generazione token JWT, hashing password |
| `Music.User.Repository` | Accesso al database tramite Entity Framework Core |
| `Music.User.ClientHttp` | Client HTTP verso LibraryService e CatalogueService |
| `Music.User.Shared` | DTO e modelli condivisi |

---

## Cosa fa

Il UserService gestisce il ciclo di vita degli utenti e l'autenticazione.

### Endpoint esposti

- `POST /User/Register` — registra un nuovo utente
- `POST /User/Login` — effettua il login e restituisce un token JWT
- `GET /User/GetCanzoniUtente` — restituisce le ultime 5 canzoni aggiunte alla libreria
- `GET /User/GetCanzoniPopolari` — restituisce canzoni popolari dell'ultimo artista aggiunto

### Flusso di registrazione

1. L'utente invia nome, cognome, data di nascita, username, email e password
2. Il Business verifica che l'email non sia già registrata
3. La password viene hashata con PBKDF2 + SHA256 + salt casuale
4. L'utente viene salvato nel database
5. Il UserService chiama il LibraryService via HTTP per creare automaticamente la libreria dell'utente

### Flusso di login

1. L'utente invia email e password
2. Il Business recupera l'utente dal database
3. Verifica la password confrontando gli hash
4. Genera e restituisce un token JWT valido 1 ora

### Aggiornamento contatore canzoni (Kafka)

Il UserService è in ascolto sul topic Kafka `song-added-to-library` e `song-removed-from-library`. Ogni volta che un utente aggiunge o rimuove una canzone dalla libreria, il contatore `NumeroCanzoni` nel profilo utente viene aggiornato automaticamente.

---

## Comunicazioni

- **HTTP in uscita**: chiama il LibraryService tramite `Music.Library.ClientHttp` e il CatalogueService tramite `Music.Catalogue.ClientHttp`
- **Kafka (consumer)**: ascolta `song-added-to-library` e `song-removed-from-library`
- **JWT**: emette token per l'autenticazione degli altri microservizi

---

## Tecnologie

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core con PostgreSQL
- Kafka (tramite `Utility.Kafka.2025`)
- JWT Bearer Authentication
- PBKDF2 + SHA256 per l'hashing delle password
- Swagger / OpenAPI


## Come eseguire in locale

Il modo consigliato è tramite Docker Compose dal repository [Music_Compose](https://github.com/lucatam05/Music_Compose).
Swagger sarà disponibile su `http://localhost:5003/swagger`.
