# FuelTrack API

API ASP.NET Core 10 preparada para Railway y MySQL. La aplicación aplica las
migraciones EF Core al arrancar, usa JWT y separa pedidos, perfil y pagos por
usuario.

## Desarrollo local

Requisitos:

- .NET SDK 10
- MySQL 8

Copia `.env.example` a una configuración local no versionada o define las
variables en la terminal. En desarrollo, si no defines `MYSQL*`, la API intenta
usar `localhost:3306`, usuario `root` y base `fueltrack`.

```powershell
dotnet restore
dotnet run
```

La base debe existir; las tablas se crean automáticamente mediante la migración
inicial. Swagger está disponible en `/swagger` únicamente en Development, salvo
que `ENABLE_SWAGGER=true`.

## Despliegue en Railway

1. Crea un proyecto Railway y agrega un servicio MySQL vacío.
2. Agrega este repositorio como servicio de API. Railway detectará el
   `Dockerfile` y `railway.json`.
3. Configura en la API:

```env
MYSQLHOST=${{MySQL.MYSQLHOST}}
MYSQLPORT=${{MySQL.MYSQLPORT}}
MYSQLUSER=${{MySQL.MYSQLUSER}}
MYSQLPASSWORD=${{MySQL.MYSQLPASSWORD}}
MYSQLDATABASE=${{MySQL.MYSQLDATABASE}}
JWT_SECRET=<secreto-aleatorio-de-64-caracteres-o-mas>
JWT_ISSUER=FuelTrack.Api
JWT_AUDIENCE=FuelTrack.Web
JWT_EXPIRATION_MINUTES=120
ALLOWED_ORIGINS=https://front-38m.pages.dev
ASPNETCORE_ENVIRONMENT=Production
ENABLE_SWAGGER=false
```

No configures `PORT`: Railway lo proporciona. La API escucha en
`0.0.0.0:$PORT`. Después del despliegue, genera un dominio público y comprueba
`https://back-production-02fc.up.railway.app/health`.

El dominio público actual de la API es:

```text
https://back-production-02fc.up.railway.app
```

El proxy público actual de MySQL es `reseau.proxy.rlwy.net:42341`. No se
configura en el frontend ni se guarda en el código de conexión: dentro de
Railway la API debe seguir usando las referencias `MYSQL*` del servicio MySQL.

Para generar `JWT_SECRET` en PowerShell:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

## Seguridad y datos

- No hay datos iniciales ni cuenta demo.
- Las contraseñas se almacenan con PBKDF2-SHA256.
- Los métodos de pago son superficiales: solo se guardan marca, titular,
  vencimiento y últimos cuatro dígitos.
- Los avatares JPEG, PNG o WEBP se almacenan en MySQL con límite de 2 MB.
- El frontend nunca se conecta directamente a MySQL.
