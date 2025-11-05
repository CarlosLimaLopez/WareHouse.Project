# Stage 1: Base image for running the ASP.NET Core app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and dependencies
COPY ["WareHouse.API.Query/WareHouse.API.Query.csproj", "WareHouse.API.Query/"]
COPY ["WareHouse.Services/WareHouse.Services.csproj", "WareHouse.Services/"]

# Copy the rest of the source code
COPY . .

# Restore NuGet packages and publish the app
WORKDIR "/src/WareHouse.API.Query"
RUN dotnet restore "WareHouse.API.Query.csproj"
RUN dotnet publish "WareHouse.API.Query.csproj" -c Release -o /app/publish

# Stage 3: Final image with only published output
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WareHouse.API.Query.dll"]
