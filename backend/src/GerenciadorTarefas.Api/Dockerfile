FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend ./backend

RUN dotnet restore backend/GerenciadorTarefas.sln
RUN dotnet publish backend/src/GerenciadorTarefas.Api/GerenciadorTarefas.Api.csproj \
    -c Release \
    -o /app/publicar \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publicar .

EXPOSE 8080

ENTRYPOINT ["dotnet", "GerenciadorTarefas.Api.dll"]
