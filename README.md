# todo.users

## Table of Contents
1. [Overview](#overview)
2. [Installation](#installation)
3. [Usage](#usage)
4. [API Endpoints](#api-endpoints)
    - [Notification Controller](#notification-controller)
    - [Todo Controller](#todo-controller)
    - [User Controller](#user-controller)
5. [Contributing](#contributing)

## Overview
The `todo.users` repository provides the user actions service for the Todo application. This service manages user-related functionalities, including user registration, authentication, and interactions with the frontend via WebSocket for real-time updates. The service is built using .NET and integrates seamlessly with the `todo.storage` service for asynchronous processing.

### Technologies Used
- .NET 8
- WebSockets
- AWS SQS for posting storage requests to message queue
- AWS SNS for reading notifications as storage completes

## Installation
To set up the `todo.users` service locally, follow these steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/coder755/todo.users.git
   cd todo.users
   ```
2. Ensure you have .NET SDK installed. You can download it from [here](https://dotnet.microsoft.com/en-us/download).
3. Install the dependencies
4. Run the application
      ```bash
   dotnet run
   ```


## Usage
Once the service is running, you can interact with the API using [Swagger](http://localhost:5224/swagger/index.html) or tools like Postman or curl.

## API Endpoints

### Notification Controller
- **WebSocket Connection**:
    - This controller sets up the WebSocket connection for the frontend, allowing for real-time notifications based on topic events.

### Todo Controller
- **GET** `/api/todos`
    - Description: Retrieves all todos for the authenticated user.

- **POST** `/api/todos`
    - Description: Posts a new todo.

- **PATCH** `/api/todos/{id}/complete`
    - Description: Marks the specified todo as complete.

### User Controller
- **POST** `/api/users`
    - Description: Creates a new user. If `useQueue` is set to true, the service posts a message to a queue for asynchronous handling by the todo.storage service.

- **GET** `/api/users/{id}`
    - Description: Retrieves information for the specified user by ID.


## Contributing
Contributions are welcome! If you find a bug or want to suggest a feature, please open an issue or submit a pull request.