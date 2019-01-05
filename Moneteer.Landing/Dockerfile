FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Moneteer.Landing.csproj .
RUN dotnet restore "Moneteer.Landing.csproj"
COPY . .
RUN dotnet build "Moneteer.Landing.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Moneteer.Landing.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Moneteer.Landing.dll"]