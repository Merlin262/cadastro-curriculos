FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first so `dotnet restore` is cached as long as dependencies don't change.
COPY src/CadastroCurriculos.Domain/CadastroCurriculos.Domain.csproj src/CadastroCurriculos.Domain/
COPY src/CadastroCurriculos.Application/CadastroCurriculos.Application.csproj src/CadastroCurriculos.Application/
COPY src/CadastroCurriculos.Infrastructure/CadastroCurriculos.Infrastructure.csproj src/CadastroCurriculos.Infrastructure/
COPY src/CadastroCurriculos.Api/CadastroCurriculos.Api.csproj src/CadastroCurriculos.Api/
RUN dotnet restore src/CadastroCurriculos.Api/CadastroCurriculos.Api.csproj

COPY src/ src/
RUN dotnet publish src/CadastroCurriculos.Api/CadastroCurriculos.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CadastroCurriculos.Api.dll"]
