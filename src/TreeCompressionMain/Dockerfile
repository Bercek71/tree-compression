# Use .NET 9 runtime base image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base

# Ensure that we set the correct working directory
WORKDIR /app

# Use .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restoring dependencies
COPY ["src/TreeCompressionMain/TreeCompressionMain.csproj", "src/TreeCompressionMain/"]
COPY ["lib/MorphoDiTaBindings/MorphoDiTaBindings.csproj", "lib/MorphoDiTaBindings/"]

# Restore dependencies
RUN dotnet restore "src/TreeCompressionMain/TreeCompressionMain.csproj"

# Copy the entire source folder into the build container
COPY . .

# Set the working directory to the correct folder
WORKDIR "/src/src/TreeCompressionMain"

# Build the project
RUN dotnet build "TreeCompressionMain.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app to the /app/publish directory
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TreeCompressionMain.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image for running the application
FROM base AS final

# Set working directory
WORKDIR /app

# Copy the published files from the previous stage
COPY --from=publish /app/publish .

# Install necessary dependencies for the native shared library (for Linux-based containers)
RUN apt-get update && \
    apt-get install -y libcurl4-openssl-dev && \
    apt-get install -y libc6-dev && \
    apt-get install -y libstdc++6

# Copy the native library early to make sure it's in the right place

# Ensure the native library is in the correct path
#ENV LD_LIBRARY_PATH=/app:$LD_LIBRARY_PATH
# Set the entrypoint for the app
ENTRYPOINT ["dotnet", "TreeCompressionMain.dll"]