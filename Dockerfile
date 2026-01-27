# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RentADad.Platform.sln .
COPY src/RentADad.Api/RentADad.Api.csproj src/RentADad.Api/
COPY src/RentADad.Application/RentADad.Application.csproj src/RentADad.Application/
COPY src/RentADad.Domain/RentADad.Domain.csproj src/RentADad.Domain/
COPY src/RentADad.Infrastructure/RentADad.Infrastructure.csproj src/RentADad.Infrastructure/
RUN dotnet restore RentADad.Platform.sln

COPY src/ src/
RUN dotnet publish src/RentADad.Api/RentADad.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","RentADad.Api.dll"]
