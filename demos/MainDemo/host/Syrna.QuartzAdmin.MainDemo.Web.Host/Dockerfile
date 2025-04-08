#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["host/Syrna.QuartzAdmin.Web.Unified/Syrna.QuartzAdmin.Web.Unified.csproj", "host/Syrna.QuartzAdmin.Web.Unified/"]
COPY ["src/Syrna.QuartzAdmin.Web/Syrna.QuartzAdmin.Web.csproj", "src/Syrna.QuartzAdmin.Web/"]
COPY ["src/Syrna.QuartzAdmin.HttpApi/Syrna.QuartzAdmin.HttpApi.csproj", "src/Syrna.QuartzAdmin.HttpApi/"]
COPY ["src/Syrna.QuartzAdmin.Application.Contracts/Syrna.QuartzAdmin.Application.Contracts.csproj", "src/Syrna.QuartzAdmin.Application.Contracts/"]
COPY ["src/Syrna.QuartzAdmin.Domain.Shared/Syrna.QuartzAdmin.Domain.Shared.csproj", "src/Syrna.QuartzAdmin.Domain.Shared/"]
COPY ["src/Syrna.QuartzAdmin.Application/Syrna.QuartzAdmin.Application.csproj", "src/Syrna.QuartzAdmin.Application/"]
COPY ["src/Syrna.QuartzAdmin.Domain/Syrna.QuartzAdmin.Domain.csproj", "src/Syrna.QuartzAdmin.Domain/"]
COPY ["src/Syrna.QuartzAdmin.EntityFrameworkCore/Syrna.QuartzAdmin.EntityFrameworkCore.csproj", "src/Syrna.QuartzAdmin.EntityFrameworkCore/"]
COPY ["host/Syrna.QuartzAdmin.Host.Shared/Syrna.QuartzAdmin.Host.Shared.csproj", "host/Syrna.QuartzAdmin.Host.Shared/"]
COPY Directory.Build.props .
RUN dotnet restore "host/Syrna.QuartzAdmin.Web.Unified/Syrna.QuartzAdmin.Web.Unified.csproj"
COPY . .
WORKDIR "/src/host/Syrna.QuartzAdmin.Web.Unified"
RUN dotnet build "Syrna.QuartzAdmin.Web.Unified.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Syrna.QuartzAdmin.Web.Unified.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Syrna.QuartzAdmin.Web.Unified.dll"]