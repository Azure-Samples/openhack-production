FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Build
COPY . ./
RUN dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

# Set Development environment
ENV ASPNETCORE_ENVIRONMENT Development

# Run LinkyLink
ENTRYPOINT ["dotnet", "LinkyLink.dll"]