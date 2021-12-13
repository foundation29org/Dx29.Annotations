#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build
WORKDIR /src
COPY ["Dx29.Annotations.Worker/Dx29.Annotations.Worker.csproj", "Dx29.Annotations.Worker/"]
COPY ["Dx29.Jobs/Dx29.Jobs.csproj", "Dx29.Jobs/"]
COPY ["Dx29.Azure/Dx29.Azure.csproj", "Dx29.Azure/"]
COPY ["Dx29/Dx29.csproj", "Dx29/"]
COPY ["Dx29.Annotations/Dx29.Annotations.csproj", "Dx29.Annotations/"]
RUN dotnet restore "Dx29.Annotations.Worker/Dx29.Annotations.Worker.csproj"
COPY . .
WORKDIR "/src/Dx29.Annotations.Worker"
RUN dotnet build "Dx29.Annotations.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dx29.Annotations.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dx29.Annotations.Worker.dll"]
