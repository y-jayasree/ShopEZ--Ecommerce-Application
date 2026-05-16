FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ShopEZ_Shared_Lib/ ShopEZ_Shared_Lib/
COPY ShopEZ_cartservice/ShopEZ_cartservice.csproj ShopEZ_cartservice/
RUN dotnet restore ShopEZ_cartservice/ShopEZ_cartservice.csproj

COPY ShopEZ_cartservice/ ShopEZ_cartservice/
RUN dotnet publish ShopEZ_cartservice/ShopEZ_cartservice.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "ShopEZ_cartservice.dll"]