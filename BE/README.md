# Backend Documentation

This directory contains the backend API for the DocuMind-AI application, built with .NET 9.

## Prerequisites

- [Docker](https://www.docker.com/products/docker-desktop) installed on your machine.
- [Docker Compose](https://docs.docker.com/compose/install/).

## Setup & Run (Recommended)

The easiest way to run the application is using Docker Compose, which handles configuration and networking automatically.

### 1. Run the application

Open a terminal in this directory (`BE`) and run:

```bash
docker-compose up --build
```

This will:
- Build the Docker image.
- Start the container with the `Development` environment.
- Mount your local `appsettings.Development.json` so the app can access your database connection strings.
- Map port `8080` to your host.

### 2. Access the API

- **Swagger UI**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Health Check**: [http://localhost:8080/health](http://localhost:8080/health)

---

## Manual Docker Run (Alternative)

If you prefer to run `docker` manually without Compose:

### 1. Build

```bash
docker build -t documind-api .
```

### 2. Run

You must pass the environment variable and volume mappings manually:

```bash
docker run -d -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -v ${PWD}/src/DocuMind.API/appsettings.Development.json:/app/appsettings.Development.json:ro \
  --add-host=host.docker.internal:host-gateway \
  --name documind-backend \
  documind-api
```
*(Note: On PowerShell, replace `${PWD}` with `$(Get-Location)` or use the absolute path)*

---

## Development (Local)

To run without Docker:

1.  Navigate to `src/DocuMind.API`.
2.  Run:
    ```bash
    dotnet run
    ```

## Troubleshooting

### Container exits immediately
- Check logs: `docker logs documind-backend`
- Ensure `ASPNETCORE_ENVIRONMENT` is set to `Development`.
- Verify `appsettings.Development.json` has the correct connection string. Use `Server=host.docker.internal;...` if your DB is on the host machine.

## Connect to API

### Authentication

The API uses JWT Bearer Authentication. You must first obtain a token to access protected endpoints.

**1. Login**

- **Endpoint**: `POST /api/Auth/login`
- **Body**:
  ```json
  {
    "email": "user@example.com",
    "password": "yourpassword"
  }
  ```
- **Response**:
  ```json
  {
    "data": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "..."
    },
    "success": true,
    "message": "Login successful"
  }
  ```

**2. Use Token**

For subsequent requests to protected endpoints (e.g., `/api/User/profile`), add the `Authorization` header:

```http
Authorization: Bearer <your_token_here>
```

```http
Authorization: Bearer <your_token_here>
```

### Chat

**1. Create a New Chat Session**

- **Endpoint**: `POST /api/Chat/create-chat`
- **Headers**: `Authorization: Bearer <token>`
- **Body**:
  ```json
  {
    "title": "My New Chat"
  }
  ```

**2. Send a Message**

- **Endpoint**: `POST /api/Chat/sessions/{sessionId}/messages`
- **Headers**: `Authorization: Bearer <token>`
- **Body**:
  ```json
  {
    "content": "Hello via API"
  }
  ```

### Document

**1. Upload a Document**

- **Endpoint**: `POST /api/Document/sessions/{sessionId}/upload`
- **Headers**: `Authorization: Bearer <token>`
- **Body** (`multipart/form-data`):
  - `file`: (Select file to upload)

### Swagger

You can also use the **Authorize** button in Swagger UI ([http://localhost:8080/swagger](http://localhost:8080/swagger)) to enter your token (prefix it with `Bearer ` if prompted, though usually Swagger handles the scheme).


