FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ShopEZ_Shared_Lib/ ShopEZ_Shared_Lib/
COPY ShopEZ_ProductService/ShopEZ_ProductService.csproj ShopEZ_ProductService/
RUN dotnet restore ShopEZ_ProductService/ShopEZ_ProductService.csproj

COPY ShopEZ_ProductService/ ShopEZ_ProductService/
RUN dotnet publish ShopEZ_ProductService/ShopEZ_ProductService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/images
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "ShopEZ_ProductService.dll"]