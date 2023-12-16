FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EndtermAuth/EndtermAuth.csproj", "EndtermAuth/"]
RUN dotnet restore "EndtermAuth/EndtermAuth.csproj"
COPY . .
WORKDIR "/src/EndtermAuth"
RUN dotnet build "EndtermAuth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EndtermAuth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EndtermAuth.dll"]
