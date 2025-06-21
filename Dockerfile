# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ProductCatalogAPIClean.csproj", "."]
RUN dotnet restore "ProductCatalogAPIClean.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "ProductCatalogAPIClean.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductCatalogAPIClean.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalogAPIClean.dll"]