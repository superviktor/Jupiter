#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Jupiter.Api/Jupiter.Api.csproj", "Jupiter.Api/"]
RUN dotnet restore "Jupiter.Api/Jupiter.Api.csproj"
COPY . .
WORKDIR "/src/Jupiter.Api"
RUN dotnet build "Jupiter.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jupiter.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jupiter.Api.dll"]