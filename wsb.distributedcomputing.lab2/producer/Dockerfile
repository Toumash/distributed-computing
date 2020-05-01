FROM microsoft/dotnet:3.0-sdk AS build
WORKDIR /app

COPY producer.csproj .
RUN dotnet restore producer.csproj

COPY . .
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:3.0-runtime AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "producer.dll"]