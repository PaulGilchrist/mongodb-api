#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# docker build --rm -f "Dockerfile" -t paulgilchrist/mongodb-api:latest .
# docker push paulgilchrist/mongodb-api
# Don't let GitHub Action build this project on AMD64 if you want to run it on ARM64.  Build and publish it yourself

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["mongodb-api/mongodb-api.csproj", "mongodb-api/"]
RUN dotnet restore "mongodb-api/mongodb-api.csproj"
COPY . .
WORKDIR "/src/mongodb-api"
RUN dotnet build "mongodb-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mongodb-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mongodb-api.dll"]

# docker run -d -p 8081:80 -e ConnectionString=mongodb://localhost:27017 -e DatabaseName=mongotest -e ContactsCollectionName=contacts -e QueueHostName=localhost paulgilchrist/mongodb-api
# docker rm -f <containerID>
