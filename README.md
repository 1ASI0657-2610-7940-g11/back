# FuelTrack Backend

## Overview

FuelTrack Backend is a monolithic RESTful API developed with ASP.NET Core. It provides the backend services required by the FuelTrack platform, including authentication, orders, payments, and other business functionalities.

The application is containerized using Docker and deployed on Render.

---

## Technologies

- ASP.NET Core 7
- C#
- Docker
- Swagger / OpenAPI
- Render
- GitHub

---

## Deployment

The backend was deployed using Render as a Docker-based Web Service.

### Deployment Steps

1. Push the backend source code to GitHub.
2. Create a new **Web Service** in Render.
3. Connect the GitHub repository.
4. Select the repository:

   ```text
   1ASI0657-2610-7940-g11/back
   ```

5. Configure the service:

   ```text
   Name: back
   Runtime: Docker
   Branch: main
   Instance Type: Free
   ```

6. Render automatically detects the `Dockerfile`.
7. Click **Deploy Web Service**.
8. Wait for the build and deployment process to finish.
9. Verify the deployment through the Render logs.
10. Access the deployed API and Swagger documentation.

---

## Deployment Evidence

The deployment was completed successfully in Render.

The logs confirmed:

```text
Application started.
Hosting environment: Production
Content root path: /app
Your service is live
```

---

## Live Backend URL

```text
https://back-7eu8.onrender.com
```

Backend:

https://back-7eu8.onrender.com
