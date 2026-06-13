# Use .NET 9.0 SDK Nightly build as the base image
FROM mcr.microsoft.com/dotnet/nightly/sdk:9.0

# Install Git
RUN apt-get update && apt-get install -y git

# Install EF Core Tools globally if needed
RUN dotnet tool install --global dotnet-ef

# Set PATH for global tools
ENV PATH="$PATH:/root/.dotnet/tools"

# Set the working directory
WORKDIR /workspace

# Restore dependencies on container start
CMD ["dotnet", "restore"]
