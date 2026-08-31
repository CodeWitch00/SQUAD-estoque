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

RUN mkdir -p /app/data \
    && chown -R "$APP_UID:$APP_UID" /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "SquadEstoque.Web.dll"]
