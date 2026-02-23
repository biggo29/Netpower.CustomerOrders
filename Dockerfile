# ============================================================
# Stage 1: Base runtime image
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ============================================================
# Stage 2: Build
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files first (layer caching for NuGet restore)
COPY ["Netpower.CustomerOrders.Api/Netpower.CustomerOrders.Api.csproj",                           "Netpower.CustomerOrders.Api/"]
COPY ["Netpower.CustomerOrders.Application/Netpower.CustomerOrders.Application.csproj",           "Netpower.CustomerOrders.Application/"]
COPY ["Netpower.CustomerOrders.Domain/Netpower.CustomerOrders.Domain.csproj",                     "Netpower.CustomerOrders.Domain/"]
COPY ["Netpower.CustomerOrders.Infrastructure/Netpower.CustomerOrders.Infrastructure.csproj",     "Netpower.CustomerOrders.Infrastructure/"]

RUN dotnet restore "Netpower.CustomerOrders.Api/Netpower.CustomerOrders.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Netpower.CustomerOrders.Api"
RUN dotnet build "Netpower.CustomerOrders.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ============================================================
# Stage 3: Publish
# ============================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Netpower.CustomerOrders.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ============================================================
# Stage 4: Final runtime image
# ============================================================
FROM base AS final
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "Netpower.CustomerOrders.Api.dll"]