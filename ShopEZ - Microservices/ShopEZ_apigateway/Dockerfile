FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ShopEZ_Shared_Lib/ ShopEZ_Shared_Lib/
COPY ShopEZ_apigateway/ShopEZ_apigateway.csproj ShopEZ_apigateway/
RUN dotnet restore ShopEZ_apigateway/ShopEZ_apigateway.csproj

COPY ShopEZ_apigateway/ ShopEZ_apigateway/
RUN dotnet publish ShopEZ_apigateway/ShopEZ_apigateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "ShopEZ_apigateway.dll"]