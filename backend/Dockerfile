FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Create directories for application data
RUN mkdir -p /app/Data

# Create startup script
COPY startup.sh /app/startup.sh
RUN chmod +x /app/startup.sh

EXPOSE 8080
ENTRYPOINT ["/app/startup.sh"]