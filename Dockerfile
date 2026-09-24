# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first
COPY ["RAGA/RAGA.csproj", "RAGA/"]
COPY ["RAGA.Application/RAGA.Application.csproj", "RAGA.Application/"]
COPY ["RAGA.Domain/RAGA.Domain.csproj", "RAGA.Domain/"]
COPY ["RAGA.Infrastructure/RAGA.Infrastructure.csproj", "RAGA.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "RAGA/RAGA.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/RAGA"
RUN dotnet build "RAGA.csproj" -c Release -o /app/build


# -----------------------------
# Publish stage
# -----------------------------
FROM build AS publish

RUN dotnet publish "RAGA.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# -----------------------------
# Runtime stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "RAGA.dll"]