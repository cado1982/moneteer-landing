#Build Stage
FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# Setup NodeJs
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_6.x | bash - && \
    apt-get install -y build-essential nodejs
# End setup

COPY Moneteer.Landing.csproj .
RUN dotnet restore

COPY package.json .
RUN npm install

COPY Styles Styles/
COPY gulpfile.js .
RUN npm run gulp-build

COPY . .

RUN dotnet publish -c Release -o /publish --no-restore

#Runtime Image Stage
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /public
COPY --from=build /publish .
ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Moneteer.Landing.dll"]