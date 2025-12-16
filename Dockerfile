FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ReportApi/ReportApi.csproj", "ReportApi/"]
RUN dotnet restore "ReportApi/ReportApi.csproj"

COPY . .
WORKDIR "/src/ReportApi"
RUN dotnet publish "ReportApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReportApi.dll"]
