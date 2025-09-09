FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN groupadd -r appgroup && useradd -r -g appgroup appuser
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ChuBank.Api/ChuBank.Api.csproj", "ChuBank.Api/"]
COPY ["ChuBank.Application/ChuBank.Application.csproj", "ChuBank.Application/"]
COPY ["ChuBank.Infrastructure/ChuBank.Infrastructure.csproj", "ChuBank.Infrastructure/"]
COPY ["ChuBank.Domain/ChuBank.Domain.csproj", "ChuBank.Domain/"]
RUN dotnet restore "ChuBank.Api/ChuBank.Api.csproj"
COPY . .
WORKDIR "/src/ChuBank.Api"
RUN dotnet build "ChuBank.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChuBank.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish --chown=appuser:appgroup /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "ChuBank.Api.dll"]