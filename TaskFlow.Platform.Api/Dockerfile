FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Create wwwroot directory and set permissions before switching to non-root user
RUN mkdir -p /app/wwwroot && \
    chmod -R 777 /app/wwwroot && \
    # Ensure future subdirectories inherit these permissions \
    chmod g+s /app/wwwroot
USER $APP_UID
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskFLow.Platform.Api/TaskFLow.Platform.Api.csproj", "TaskFLow.Platform.Api/"]
RUN dotnet restore "TaskFLow.Platform.Api/TaskFLow.Platform.Api.csproj"
COPY . .
WORKDIR "/src/TaskFLow.Platform.Api"
RUN dotnet build "TaskFLow.Platform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TaskFLow.Platform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFLow.Platform.Api.dll"]
