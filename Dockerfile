FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SurveyApp.Server/SurveyApp.Server.csproj SurveyApp.Server/
RUN dotnet restore SurveyApp.Server/SurveyApp.Server.csproj

COPY . .
RUN dotnet publish SurveyApp.Server/SurveyApp.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SurveyApp.Server.dll"]
