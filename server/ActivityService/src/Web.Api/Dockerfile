FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ActivityService/Directory.Packages.props", "ActivityService/"]
COPY ["ActivityService/Directory.Build.props", "ActivityService/"]

COPY ["ActivityService/src/Web.Api/Web.Api.csproj", "ActivityService/src/Web.Api/"]
COPY ["ActivityService/src/Application/Application.csproj", "ActivityService/src/Application/"]
COPY ["ActivityService/src/Domain/Domain.csproj", "ActivityService/src/Domain/"]
COPY ["ActivityService/src/Infrastructure/Infrastructure.csproj", "ActivityService/src/Infrastructure/"]
COPY ["Messaging.Contracts/Messaging.Contracts.csproj", "Messaging.Contracts/"]
COPY ["Identity.Contracts/Identity.Contracts.csproj", "Identity.Contracts/"]

RUN dotnet restore "ActivityService/src/Web.Api/Web.Api.csproj"

COPY . .

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/ActivityService/src/Web.Api"
RUN dotnet publish "Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.Api.dll"]
