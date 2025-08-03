# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# WORKDIR /app
# EXPOSE 8080
# EXPOSE 443

# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /src
# COPY ["src/BackendApi.csproj", "src/"]
# RUN dotnet restore "src/BackendApi.csproj"
# COPY . .
# WORKDIR "/src/src"
# RUN dotnet build "BackendApi.csproj" -c Release -o /app/build

# FROM build AS publish
# RUN dotnet publish "BackendApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "BackendApi.dll"] 