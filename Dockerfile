FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/SquadEstoque.Web/SquadEstoque.Web.csproj src/SquadEstoque.Web/
RUN dotnet restore src/SquadEstoque.Web/SquadEstoque.Web.csproj

COPY src/SquadEstoque.Web/ src/SquadEstoque.Web/
WORKDIR /source/src/SquadEstoque.Web
RUN dotnet publish --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SquadEstoque.Web.dll"]
