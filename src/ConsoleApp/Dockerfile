# Use a base image with both .NET and required C libraries
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install required C libraries and development tools
RUN apt-get update && apt-get install -y \
    libc6 \
    libpthread-stubs0-dev \
    libstdc++6 \
    libpcre2-8-0 \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Copy your C# and C project files
COPY ["src/ConsoleApp/ConsoleApp.csproj", "src/ConsoleApp/"]
COPY ["lib/MorphoDiTaBindings/MorphoDiTaBindings.csproj", "lib/MorphoDiTaBindings/"]
COPY ["lib/UDPipeBindings/UDPipeBindings.csproj", "lib/UDPipeBindings/"]

# Copy the .so file (for C bindings)
#COPY lib/MorphoDiTaBindings/morphodita_csharp.so /app/bin/Debug/net9.0/

# Restore and build the application
RUN dotnet restore "src/ConsoleApp/ConsoleApp.csproj"
COPY . .
WORKDIR "/src/src/ConsoleApp"
RUN dotnet build "ConsoleApp.csproj" -c Debug -o /app/build

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install required C libraries in the final image
RUN apt-get update && apt-get install -y \
    libc6 \
    libpthread-stubs0-dev \
    libstdc++6 \
    libpcre2-8-0 \
    && rm -rf /var/lib/apt/lists/*

# Copy the .so file into the final image
COPY lib/MorphoDiTaBindings/morphodita_csharp.so /usr/local/lib/

# Set the library path to the directory containing your .so file
ENV LD_LIBRARY_PATH=/usr/local/lib:$LD_LIBRARY_PATH

# Copy everything from the build stage
COPY --from=build /app/build .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "ConsoleApp.dll"]