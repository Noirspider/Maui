# Maui - E-commerce di Birre Artigianali

Maui è un'applicazione di e-commerce per la vendita di birre artigianali. L'applicazione è stata sviluppata utilizzando ASP.NET Core MVC e Entity Framework Core.

## Funzionalità

- **Carrello**: Gli utenti possono aggiungere prodotti al carrello, rimuoverli o aggiornare la quantità. Le informazioni del carrello sono memorizzate nella sessione dell'utente.
- **Ordini**: Gli utenti possono creare ordini a partire dal carrello. Gli ordini vengono salvati nel database e possono essere visualizzati nella cronologia degli ordini dell'utente.
- **Gestione dei prodotti**: L'applicazione supporta le operazioni CRUD sui prodotti. I prodotti possono essere creati, letti, aggiornati e cancellati dal database.

## Controller

Il controller `UserOrderController` gestisce le operazioni relative al carrello e agli ordini degli utenti. Ecco una breve descrizione dei metodi del controller:

- `FetchAddToCartSession(int id, int quantity)`: Aggiunge un prodotto al carrello dell'utente.
- `Cart()`: Mostra il carrello dell'utente.
- `RiepilogoOrdine(Ordine ordine)`: Crea un nuovo ordine a partire dal carrello dell'utente.
- `FetchRemoveFromCartSession(int id)`: Rimuove un prodotto dal carrello dell'utente.
- `RemoveFromCart(int idProdotto)`: Rimuove un prodotto dal carrello dell'utente.
- `UpdateQuantity(int idProdotto, int quantity)`: Aggiorna la quantità di un prodotto nel carrello dell'utente.
- `UserOrderHistory()`: Mostra la cronologia degli ordini dell'utente.

## Come eseguire l'applicazione

Per eseguire l'applicazione, è necessario avere installato .NET Core SDK. Dopo aver clonato il repository, navigare nella directory del progetto e eseguire i seguenti comandi:
dotnet restore
dotnet run


L'applicazione sarà disponibile all'indirizzo `http://localhost:5000`.

## Contribuire

Sei libero di contribuire a questo progetto. Per segnalare bug o richiedere nuove funzionalità, apri una issue. Se vuoi contribuire con del codice, apri una pull request.
