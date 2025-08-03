# Take Home API Scaffolding

Este proyecto es un scaffolding para APIs REST en .NET 8 con Entity Framework Core y arquitectura limpia.

## Estructura del Proyecto

```
/
├── src/                       # Código fuente de la aplicación
│   ├── Controllers/           # Controladores de la API
│   ├── Models/                # Modelos/Entidades de dominio
│   ├── Data/                  # Capa de acceso a datos
│   │   ├── Repositories/      # Implementaciones de repositorios
│   │   ├── DbContext/         # Contextos de Entity Framework
│   │   └── Configurations/    # Configuraciones de Entity Framework
│   ├── Services/              # Servicios de negocio
│   ├── DTOs/                  # Objetos de transferencia de datos
│   ├── Program.cs             # Punto de entrada
│   └── appsettings.json       # Configuración
├── tests/                     # Pruebas
│   └── BackendApiTests/       # Proyecto de pruebas
│       ├── UnitTests/         # Pruebas unitarias
│       └── IntegrationTests/  # Pruebas de integración
├── TakeHomeScaffolding.sln    # Archivo de solución
├── Dockerfile                 # Configuración para Docker
├── docker-compose.yml         # Configuración de Docker Compose
└── .gitignore                 # Exclusiones para Git
```

## Tecnologías Utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- FluentValidation
- xUnit para pruebas
- Swagger/OpenAPI
- Docker & Docker Compose

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (opcional)

## Instrucciones de Inicio

### Desarrollo Local

1. Clone el repositorio
   ```
   git clone <url-del-repositorio>
   cd TakeHomeScaffolding
   ```

2. Restaure los paquetes
   ```
   dotnet restore
   ```

3. Ejecute la aplicación
   ```
   cd src
   dotnet run
   ```

4. Navegue a https://localhost:7091/swagger para ver la documentación de la API

### Usando Docker

1. Construya y ejecute los contenedores
   ```
   docker-compose up --build
   ```

2. Navegue a http://localhost:5104/swagger para ver la documentación de la API

   > La base de datos SQL Server estará disponible en localhost:14334

## Configuración de Base de Datos

La aplicación está configurada para utilizar SQL Server. La cadena de conexión predeterminada es:

```
Server=localhost,14334;Database=TakeHomeDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;
```

En el entorno Docker, la cadena de conexión se configura automáticamente para usar el contenedor de SQL Server.

## Pruebas

```
dotnet test
```

## Licencia

Este proyecto está licenciado bajo la licencia MIT. 