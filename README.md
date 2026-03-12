# Enhanzer Pre-Screening Assignment (Books CRUD)

Simple full-stack web app to manage a list of books (Create, Read, Update, Delete).

## Tech
- Frontend: Angular (standalone components)
- Backend: ASP.NET Core Minimal API (C#)
- Storage: In-memory list (resets on restart)

## Book model
`id`, `title`, `author`, `isbn`, `publicationDate`

## Run (local)

### Backend (API)
Prereq: .NET 8 SDK

```bash
cd backend/BooksApi
dotnet restore
dotnet run
```

API: `http://localhost:5000/api/books`  

### Frontend (Angular)
Prereq: Node.js + npm, Angular CLI (`npm i -g @angular/cli`)

```bash
cd frontend/books-ui
npm install
npm start
```

UI: `http://localhost:4200`

## Endpoints
- `GET /api/books` (list)
- `GET /api/books/{id}` (get by id)
- `POST /api/books` (create)
- `PUT /api/books/{id}` (update)
- `DELETE /api/books/{id}` (delete)


