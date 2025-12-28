# DocuMind API Documentation

**Project Name:** DocuMind  
**Version:** v1  
**Base URL:** `{server_url}/api` (e.g., `http://localhost:5000/api` for development)

---

## 1. General Information

The DocuMind API provides an AI-powered document management and analysis system. It allows users to upload documents, manage chat sessions, and interact with an AI assistant to extract insights from their files.

- **Authentication:** JWT (JSON Web Token) Bearer Token.
- **Content-Type:** `application/json` (except for file uploads which use `multipart/form-data`).
- **Date & Time Format:** ISO 8601 (e.g., `2023-12-28T14:30:00Z`).
- **Error Handling:** Unified `ApiResponse<T>` structure for standardized success and error responses.

---

## 2. Authentication

The API uses **JWT (JSON Web Token)** for securing endpoints. Clients must authenticate via the `/api/Auth/login` endpoint to receive an `access_token`. This token must be included in the `Authorization` header of subsequent requests.

**Authorization Header Format:**
```http
Authorization: Bearer <your_access_token>
```

### Token Lifecycle
- **Access Token:** Short-lived token used to access protected resources.
- **Expiration:** Check `expiresAt` in the login response for validity duration.

---

## 3. API Endpoints Documentation

### 3.1 Authentication

#### **Login**
Authenticates a user and returns a JWT token.

- **Endpoint:** `POST /Auth/login`
- **Auth Required:** No

**Request Body:** `LoginDto`
```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "fullName": "John Doe",
    "email": "user@example.com",
    "role": "User",
    "token": "eyJhbGciOiJIUzI1...",
    "expiresAt": "2023-12-29T14:30:00Z"
  }
}
```

#### **Register**
Registers a new user account.

- **Endpoint:** `POST /Auth/register`
- **Auth Required:** No

**Request Body:** `RegisterDto`
```json
{
  "fullName": "John Doe",
  "email": "user@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

**Success Response (200 OK):**
Same structure as Login.

#### **Change Password**
Updates the authenticated user's password.

- **Endpoint:** `POST /Auth/change-password`
- **Auth Required:** Yes

**Request Body:** `ChangePasswordDto`
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!",
  "confirmNewPassword": "NewPassword123!"
}
```

---

### 3.2 Chat Management

#### **Create Chat Session**
Starts a new chat session.

- **Endpoint:** `POST /Chat/create-chat`
- **Auth Required:** Yes

**Request Body:** `CreateSessionDto`
```json
{
  "title": "Project Requirements Analysis"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 101,
    "title": "Project Requirements Analysis",
    "createdAt": "2023-12-28T10:00:00Z",
    "lastActiveAt": "2023-12-28T10:00:00Z",
    "messageCount": 0,
    "messages": [],
    "documents": []
  }
}
```

#### **Get All Sessions**
Retrieves all chat sessions for the user.

- **Endpoint:** `GET /Chat/sessions`
- **Auth Required:** Yes

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 101,
      "title": "Analysis",
      ...
    }
  ]
}
```

#### **Send Message**
Sends a message to the AI agent within a specific session.

- **Endpoint:** `POST /Chat/sessions/{sessionId}/messages`
- **Auth Required:** Yes

**Request Body:** `SendMessageDto`
```json
{
  "content": "Summarize the uploaded document."
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userMessage": {
      "id": 501,
      "content": "Summarize the uploaded document.",
      "isUser": true,
      "timestamp": "2023-12-28T10:05:00Z"
    },
    "botMessage": {
      "id": 502,
      "content": "The document discusses...",
      "isUser": false,
      "timestamp": "2023-12-28T10:05:05Z"
    },
    "processingTimeMs": 1500
  }
}
```

#### **Get Session Messages**
Retrieves message history for a session.

- **Endpoint:** `GET /Chat/sessions/{sessionId}/messages`
- **Auth Required:** Yes

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": [
    { "id": 501, "content": "Hi", "isUser": true, ... },
    { "id": 502, "content": "Hello!", "isUser": false, ... }
  ]
}
```

---

### 3.3 Document Management

#### **Upload Document**
Uploads a document to a chat session for analysis.

- **Endpoint:** `POST /Document/sessions/{sessionId}/upload`
- **Auth Required:** Yes
- **Content-Type:** `multipart/form-data`

**Request:**
- `file`: (Binary) The file to upload.

**Success Response (200 OK):**
*Note: This endpoint returns the data object directly, not wrapped in `ApiResponse`.*
```json
{
  "id": 201,
  "fileName": "requirements.pdf",
  "fileSize": 102400,
  "status": 1,
  "statusDisplay": "Processed",
  "createdAt": "2023-12-28T10:01:00Z",
  "processedAt": "2023-12-28T10:01:05Z"
}
```

#### **Get Document Status**
Checks the processing status of a document.

- **Endpoint:** `GET /Document/{id}/status`
- **Auth Required:** Yes

**Success Response (200 OK):**
*Note: This endpoint returns the data object directly.*
```json
{
  "id": 201,
  "status": 1,
  "statusDisplay": "Processed",
  ...
}
```

---

### 3.4 User Profile & Dashboard

#### **Get Profile**
- **Endpoint:** `GET /User/profile`
- **Auth Required:** Yes

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "fullName": "John Doe",
    "email": "user@example.com",
    "role": "User",
    "totalDocuments": 5,
    "totalChats": 3
  }
}
```

#### **Get Dashboard**
Retrieves aggregated statistics and recent activity.

- **Endpoint:** `GET /user/dashboard`
- **Auth Required:** Yes

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "statistics": {
      "totalDocuments": 10,
      "totalChats": 5,
      "statusCounts": {
        "Uploaded": 1,
        "Processed": 9
      }
    },
    "recentDocuments": [...],
    "recentChats": [...]
  }
}
```

---

## 4. Data Models

### **User Types**
**LoginDto**
- `Email` (string, required): User's email address.
- `Password` (string, required): User's password.

**RegisterDto**
- `FullName` (string, required): Full name (min 2 chars).
- `Email` (string, required): Valid email address.
- `Password` (string, required): Min 6 chars.
- `ConfirmPassword` (string): Must match Password.

### **Chat Types**
**CreateSessionDto**
- `Title` (string, optional): Title of the chat session (min 3 chars).

**SendMessageDto**
- `Content` (string, required): The message text to send to AI (max 2000 chars).

**SessionDto**
- `Id` (int): Unique session identifier.
- `Title` (string): Session title.
- `Messages` (List<MessageDto>): List of messages in the session.
- `Documents` (List<DocumentItemDto>): List of attached documents.

**MessageDto**
- `id` (int): Message ID.
- `content` (string): Message text.
- `isUser` (bool): `true` if sent by user, `false` if AI response.
- `timestamp` (DateTime): Time message was sent.

### **Document Types**
**DocumentItemDto**
- `Id` (int): Document ID.
- `FileName` (string): Name of the file.
- `Status` (enum): Processing status (0=Pending, 1=Processed, etc.).

---

## 5. Error Handling

The API uses a standardized error format for most endpoints (`ApiResponse<T>`).

**Error Response Structure (400 Bad Request):**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "The Email field is not a valid e-mail address.",
    "Password must be at least 6 characters."
  ]
}
```

**Common HTTP Status Codes:**
- `200 OK`: Request succeeded.
- `400 Bad Request`: Validation error or business logic failure.
- `401 Unauthorized`: Missing or invalid JWT token.
- `403 Forbidden`: Insufficient permissions.
- `500 Internal Server Error`: Unexpected server error.

---

## 6. Security Considerations

- **JWT Authentication:** All private endpoints require a valid Bearer token. Tokens should be stored securely on the client (e.g., `SecureStorage` in mobile, `HttpOnly` cookies in web).
- **File Validation:** The `UploadDocument` endpoint accepts specific file types (PDF, etc.) and enforces size limits (implied by `IFormFile` constraints and server config).
- **Input Sanitization:** All inputs are validated using Data Annotations (e.g., `[EmailAddress]`, `[StringLength]`).

---

## 7. Example Use Cases

### **Scenario: User Analyzes a PDF**

1.  **Login:**
    - User sends `POST /api/Auth/login`.
    - Client stores `token` from response.

2.  **Create Session:**
    - User sends `POST /api/Chat/create-chat` with `{"title": "Invoice Analysis"}`.
    - Server returns new `sessionId` (e.g., 55).

3.  **Upload Document:**
    - User sends `POST /api/Document/sessions/55/upload` with the PDF file.
    - Server processes the file and returns document info.

4.  **Chat with PDF:**
    - User sends `POST /api/Chat/sessions/55/messages` with `{"content": "What is the total amount?"}`.
    - Server performs RAG (Retrieval-Augmented Generation) and responds with the answer.
