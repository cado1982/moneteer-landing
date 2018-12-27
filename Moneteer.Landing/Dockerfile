FROM microsoft/dotnet:2.1-sdk AS base
WORKDIR /app
EXPOSE 4500

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Moneteer.Landing.V2/Moneteer.Landing.V2.csproj Moneteer.Landing.V2/
RUN dotnet restore Moneteer.Landing.V2/Moneteer.Landing.V2.csproj
COPY . .
WORKDIR /src/Moneteer.Landing.V2
RUN dotnet build Moneteer.Landing.V2.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Moneteer.Landing.V2.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Moneteer.Landing.V2.dll"]