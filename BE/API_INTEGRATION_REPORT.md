# DocuMind API Integration Report

**Date:** 2025-12-28  
**Project:** DocuMind AI Backend  
**Version:** 1.0  
**Target Audience:** Frontend Engineers, Mobile Developers, System Integrators

---

## 1. Executive Summary

This document serves as the technical integration guide for the **DocuMind** backend system, an intelligent document analysis platform powered by Generative AI (RAG - Retrieval-Augmented Generation).

The system allows clients to upload complex documents (PDFs), manage chat sessions, and interact with an AI agent to extract insights, summaries, and answers based on the uploaded content. This API is designed for **RESTful** consumption and employs **Clean Architecture** principles to ensure scalability and maintainability.

**Key Capabilities:**
*   **Secure Authentication:** JWT-based identity management.
*   **Document Processing:** Asynchronous ingestion and vectorization of documents.
*   **Contextual AI Chat:** Conversation-aware Q&A using RAG.
*   **User Dashboarding:** Aggregated metrics and history tracking.

---

## 2. System Architecture Overview

The DocuMind backend is built on **ASP.NET Core** and follows a modular **Clean Architecture** pattern, separating concerns into distinct layers:

### Logical Architecture
1.  **API Layer (`DocuMind.API`):** The entry point. Handles HTTP requests, validation (Data Annotations), and routes traffic to the Application layer.
2.  **Application Layer (`DocuMind.Application`):** Contains business logic, DTOs, and Service Interfaces. This layer orchestrates the flow between the API and Infrastructure.
3.  **Infrastructure Layer (`DocuMind.Infrastructure`):** Implements interfaces. Handles database connections (EF Core), AI Service integrations (Google Gemini), and background job processing (**Hangfire**).
4.  **Core Layer (`DocuMind.Core`):** Domain entities and enterprise rules.

### High-Level Data Flow
1.  **Upload:** User uploads a file -> API creates a record -> **Hangfire** triggers a background job to process/embed the file -> File status updates to `Processed`.
2.  **Chat:** User sends a query -> System retrieves relevant document chunks (Vector Search) -> Augments prompt with context -> Sends to LLM -> Returns response to user.

---

## 3. Authentication & Security Model

The system utilizes **JSON Web Tokens (JWT)** for stateless authentication.

### Authentication Mechanism
*   **Protocol:** Bearer Token Usage.
*   **Token Type:** Access Token (Standard JWT).
*   **Transport Security:** HTTPS is mandatory for all production traffic.

### Login Flow
1.  **Credential Exchange:** Client sends `email` and `password` to `/api/Auth/login`.
2.  **Token Generation:** Server validates credentials and issues a signed JWT `access_token` containing user claims (`NameIdentifier`, `Email`, `Role`).
3.  **Client Storage:** Client should securely store this token (e.g., `SecureStorage` on Mobile, `HttpOnly` Cookies or Memory on Web). Do **not** store in `localStorage` if possible to avoid XSS risks.
4.  **Authenticated Requests:** For every subsequent request, add the header:
    ```http
    Authorization: Bearer <your_access_token>
    ```

### Token Lifecycle
*   **Expiration:** Tokens have a set expiration time (returned in `expiresAt`).
*   **Handling 401s:** If an API call returns `401 Unauthorized`, the client must redirect the user to the login screen or attempt a refresh (if refresh flow is implemented).

---

## 4. Availability & Prerequisites

### Environment Configuration
*   **Base URL (Dev):** `http://localhost:5000/api` (or user-configured port)
*   **Base URL (Prod):** `https://api.documind.com/api`

### Standard Headers
All POST/PUT requests must include:
```http
Content-Type: application/json
Accept: application/json
```
*Exception: File uploads use `multipart/form-data`.*

---

## 5. Core Integration Flows

### Flow A: User Onboarding
**Purpose:** Create a new user account and gain access.
1.  **Call:** `POST /Auth/register` with `RegisterDto`.
2.  **Response:** Success message.
3.  **Action:** Redirect user to Login immediately.

### Flow B: Document Analysis (The "Wait-and-Chat" Pattern)
**Purpose:** Upload a document and prepare it for AI analysis. This is an asynchronous process.

1.  **Create Session (Optional but recommended):**
    *   Call `POST /Chat/create-chat` to start a context.
    *   Store `sessionId` (e.g., `101`).

2.  **Upload File:**
    *   Call `POST /Document/sessions/{sessionId}/upload` with the file.
    *   **Store:** `documentId` from the response.
    *   **Observation:** The response status will likely be `0` (Pending) or `1` (Processing).

3.  **Poll for Status (Crucial):**
    *   The file is not ready for chat immediately.
    *   **Loop:** Call `GET /Document/{documentId}/status` every 2-3 seconds.
    *   **Stop Condition:** When `status` becomes `Processed` (or `Failed`).
    *   *UX Tip:* Show a progress spinner during this phase.

### Flow C: Messaging the AI
**Purpose:** Ask questions about the processed document.
1.  **Pre-requisite:** Document status must be `Processed`.
2.  **Send Message:**
    *   Call `POST /Chat/sessions/{sessionId}/messages` with `content`.
3.  **Display Response:**
    *   The API returns both the `userMessage` (echo) and `botMessage`.
    *   Append both to the chat UI list.
    *   `processingTimeMs` is provided for performance auditing.

---

## 6. API Endpoints Reference (Condensed)

| Method | Endpoint | Purpose | Auth |
| :--- | :--- | :--- | :--- |
| **Auth** | | | |
| `POST` | `/Auth/login` | Obtain JWT token. | No |
| `POST` | `/Auth/register` | Create account. | No |
| `POST` | `/Auth/change-password` | Update credentials. | **Yes** |
| **Chat** | | | |
| `POST` | `/Chat/create-chat` | Initialize new chat context. | **Yes** |
| `GET` | `/Chat/sessions` | List history. | **Yes** |
| `POST` | `/sessions/{id}/messages` | Send query to AI. | **Yes** |
| `GET` | `/sessions/{id}/messages` | Load chat history. | **Yes** |
| **Document**| | | |
| `POST` | `/sessions/{id}/upload` | Upload PDF/Doc. | **Yes** |
| `GET` | `/{id}/status` | Check processing state. | **Yes** |
| **User** | | | |
| `GET` | `/User/dashboard` | Get stats & recent activity. | **Yes** |
| `GET` | `/User/profile` | Get user details. | **Yes** |

---

## 7. Data Models Overview

### Key DTOs
*   **`SessionDto`**: Represents a chat room. Contains a list of `Messages` and attached `Documents`. *Frontend should cache lists of sessions.*
*   **`MessageDto`**: A single bubble in the chat. Contains `content`, `timestamp`, and `isUser` boolean.
*   **`DocumentItemDto`**: Metadata about a file. Crucially contains the `Status` enum.

### Mapping Strategy
*   **FE State:** Map `SessionDto` directly to your Chat Room View Model.
*   **Immutability:** Chat history (`MessageDto` lists) is generally append-only.

---

## 8. Error Handling & Client Responsibilities

The API returns a unified structure `ApiResponse<T>`.

**Standard Error Response (400/500):**
```json
{
  "success": false,
  "message": "Friendly error message",
  "errors": ["Detailed validation error 1", "Detailed validation error 2"]
}
```

**Client Requirements:**
1.  Check `success` boolean.
2.  If `false`, display `message` to the user (toast/alert).
3.  If `errors` list exists, highlight specific form fields (e.g., "Invalid Email").

---

## 9. Performance & Guidelines

*   **File Size:** Recommended max 10MB per file (server configurable).
*   **Polling Interval:** Do not poll `/status` faster than 1s intervals to avoid rate limiting.
*   **Image Generation:** Not currently supported; text-only RAG.
*   **Dashboard:** Cached or computed on-the-fly; avoid refreshing `/dashboard` on every page focus.

---

## 10. Security & Compliance

*   **Role-Based Access Control (RBAC):** Endpoints protect data based on `NameIdentifier` (UserId). Users cannot access other users' chats or documents.
*   **Input Sanitization:** All text inputs are sanitized to prevent SQLi and XSS.
*   **File Type Validator:** Only allow specific extensions (e.g., .pdf, .txt) on the client side before upload to save bandwidth, though the server doubles-checks.

---

## 11. Example End-to-End Scenario

**Scenario:** A user logs in and summarizes a contract.

**Step 1: Login**
`POST /Auth/login` -> Returns `Token: "abc..."`

**Step 2: Init Session**
`POST /Chat/create-chat`
Body: `{ "title": "Contract Review" }`
Returns: `{ "id": 55, ... }`

**Step 3: Upload**
`POST /Document/sessions/55/upload`
Form-Data: `File: <contract.pdf>`
Returns: `{ "id": 200, "status": "Pending" }`

**Step 4: Poll Loop**
`GET /Document/200/status` -> "Pending"
`GET /Document/200/status` -> "Processing"
`GET /Document/200/status` -> "Processed" (Loop ends)

**Step 5: Query**
`POST /Chat/sessions/55/messages`
Body: `{ "content": "Summarize the liability clause." }`
Header: `Authorization: Bearer abc...`

**Step 6: Render**
Client receives AI response and renders it in the chat bubble.

---

## 12. Appendix

### HTTP Status Codes
*   `200`: Success.
*   `400`: Validation Error (Check `errors` array).
*   `401`: Token missing or expired (Re-login).
*   `403`: Valid token but no permission.
*   `500`: Server panic (Report to BE team).

### Document Status Enum
*   `0`: Pending
*   `1`: Processed
*   `2`: Failed
