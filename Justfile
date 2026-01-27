default:
  @just --list

restore:
  dotnet restore .\RentADad.Platform.sln

build:
  dotnet build .\RentADad.Platform.sln -v:m

test:
  dotnet test .\src\RentADad.Tests\RentADad.Tests.csproj -v:n

format:
  dotnet format

docker-up:
  docker compose up -d

docker-down:
  docker compose down

migrate:
  dotnet run --project .\src\RentADad.Api\RentADad.Api.csproj -- --apply-migrations-only

seed-demo:
  dotnet run --project .\src\RentADad.Api\RentADad.Api.csproj -- --seed-demo

run:
  dotnet run --project .\src\RentADad.Api\RentADad.Api.csproj
