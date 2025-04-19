FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# ⚠️ Instala ffmpeg e ffprobe
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    apt-get clean

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Postech.Fiap.Hackathon.VideoProcessing.Worker/Postech.Fiap.Hackathon.VideoProcessing.Worker.csproj", "src/Postech.Fiap.Hackathon.VideoProcessing.Worker/"]
RUN dotnet restore "src/Postech.Fiap.Hackathon.VideoProcessing.Worker/Postech.Fiap.Hackathon.VideoProcessing.Worker.csproj"
COPY . .
WORKDIR "/src/src/Postech.Fiap.Hackathon.VideoProcessing.Worker"
RUN dotnet build "Postech.Fiap.Hackathon.VideoProcessing.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Postech.Fiap.Hackathon.VideoProcessing.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Postech.Fiap.Hackathon.VideoProcessing.Worker.dll"]