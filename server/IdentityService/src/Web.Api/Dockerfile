FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["IdentityService/Directory.Packages.props", "IdentityService/"]
COPY ["IdentityService/src/Web.Api/Web.Api.csproj", "IdentityService/src/Web.Api/"]
COPY ["IdentityService/src/Application/Application.csproj", "IdentityService/src/Application/"]
COPY ["IdentityService/src/Domain/Domain.csproj", "IdentityService/src/Domain/"]
COPY ["IdentityService/src/Infrastructure/Infrastructure.csproj", "IdentityService/src/Infrastructure/"]
COPY ["Messaging.Contracts/Messaging.Contracts.csproj", "Messaging.Contracts/"]
COPY ["Identity.Contracts/Identity.Contracts.csproj", "Identity.Contracts/"]

RUN dotnet restore "IdentityService/src/Web.Api/Web.Api.csproj"

COPY . .

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/IdentityService/src/Web.Api"
RUN dotnet publish "Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.Api.dll"]
