FROM microsoft/dotnet:3.0-sdk AS build
WORKDIR /app

COPY consumer.csproj .
RUN dotnet restore consumer.csproj

COPY . .
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:3.0-runtime AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "consumer.dll"]