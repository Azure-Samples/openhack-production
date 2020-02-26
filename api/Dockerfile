FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY src/LinkyLink/*.csproj ./src/LinkyLink/
COPY tests/LinkyLink.Tests/*.csproj ./tests/LinkyLink.Tests/
COPY tests/LinkyLink.Integration.Tests/*.csproj ./tests/LinkyLink.Integration.Tests/
RUN dotnet restore

# Build
COPY . ./ 
RUN dotnet build -c Release --no-restore

# ---------------------------------------------------------

FROM build-env AS unit-tests
WORKDIR /app/tests/LinkyLink.Tests
RUN dotnet test *.csproj

FROM build-env AS publish
WORKDIR /app/src/LinkyLink
RUN dotnet publish -c Release -o out --no-build --no-restore

# ---------------------------------------------------------

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=publish /app/src/LinkyLink/out .

# Set Development environment
ENV ASPNETCORE_ENVIRONMENT Development

# Run LinkyLink
ENTRYPOINT ["dotnet", "LinkyLink.dll"]
