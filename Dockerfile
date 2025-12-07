FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ContractApprovalSystem.Web/ContractApprovalSystem.Web.csproj", "ContractApprovalSystem.Web/"]
COPY ["ContractApprovalSystem.Services/ContractApprovalSystem.Services.csproj", "ContractApprovalSystem.Services/"]
COPY ["ContractApprovalSystem.Infrastructure/ContractApprovalSystem.Infrastructure.csproj", "ContractApprovalSystem.Infrastructure/"]
COPY ["ContractApprovalSystem.Core/ContractApprovalSystem.Core.csproj", "ContractApprovalSystem.Core/"]

# Restore dependencies
RUN dotnet restore "ContractApprovalSystem.Web/ContractApprovalSystem.Web.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/ContractApprovalSystem.Web"
RUN dotnet build "ContractApprovalSystem.Web.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ContractApprovalSystem.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ContractApprovalSystem.Web.dll"]
