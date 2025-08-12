# JokesApp

## Configuración del Proyecto

### Requisitos Previos
- .NET 7.0 o superior
- SQL Server (o Docker para SQL Server)
- Una cuenta de Google Cloud Platform para OAuth

### Configuración de Desarrollo

1. Clona el repositorio
```bash
git clone https://github.com/GustavoRu/JokesApp.git
cd JokesApp
```

2. Configura los User Secrets para desarrollo local:
```bash
cd src
dotnet user-secrets init
```

3. Configura los siguientes secretos:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "tu-connection-string"
dotnet user-secrets set "Jwt:Key" "tu-jwt-key-de-al-menos-32-caracteres"
dotnet user-secrets set "Google:ClientId" "tu-google-client-id"
dotnet user-secrets set "Google:ClientSecret" "tu-google-client-secret"
```

### Configuración de Producción

Para producción, configura las siguientes variables de entorno:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Google__ClientId`
- `Google__ClientSecret`

### Ejecutar el Proyecto

1. Restaura los paquetes NuGet:
```bash
dotnet restore
```

2. Ejecuta las migraciones:
```bash
dotnet ef database update
```

3. Inicia el proyecto:
```bash
dotnet run
```

## Seguridad

Este proyecto utiliza:
- User Secrets para desarrollo local
- Variables de entorno para producción
- JWT para autenticación
- Google OAuth para login externo

NO comitees información sensible en los archivos de configuración. Usa los mecanismos de secretos apropiados mencionados arriba.