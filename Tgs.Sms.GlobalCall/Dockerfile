#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
RUN dotnet nuget remove source 'Microsoft Visual Studio Offline Packages' || true
WORKDIR /src
COPY ["Tgs.Sms.GlobalCall/Tgs.Sms.GlobalCall.csproj", "Tgs.Sms.GlobalCall/"]
COPY ["Tgs.Sms.GlobalCall.Core/Tgs.Sms.GlobalCall.Core.csproj", "Tgs.Sms.GlobalCall.Core/"]
COPY ["Tgs.Sms.GlobalCall.Repositories/Tgs.Sms.GlobalCall.Repositories.csproj", "Tgs.Sms.GlobalCall.Repositories/"]
RUN dotnet restore "Tgs.Sms.GlobalCall/Tgs.Sms.GlobalCall.csproj"
COPY . .
WORKDIR "/src/Tgs.Sms.GlobalCall"
RUN dotnet build "Tgs.Sms.GlobalCall.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Sms.GlobalCall.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Sms.GlobalCall.dll"]