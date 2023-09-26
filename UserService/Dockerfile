FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./

# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release --property:PublishDir=out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App

# Install postgresql client
RUN apt-get update && apt-get install -y postgresql-client
COPY check_user_db.sh .

COPY --from=build-env /App/out .
#ENTRYPOINT ["dotnet", "UserService.dll"]