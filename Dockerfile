FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TransportTrack.slnx ./
COPY TransportTrack.Api/TransportTrack.Api.csproj TransportTrack.Api/
COPY TransportTrack.Application/TransportTrack.Application.csproj TransportTrack.Application/
COPY TransportTrack.Domain/TransportTrack.Domain.csproj TransportTrack.Domain/
COPY TransportTrack.Infrastructure/TransportTrack.Infrastructure.csproj TransportTrack.Infrastructure/
RUN dotnet restore TransportTrack.Api/TransportTrack.Api.csproj

COPY TransportTrack.Api/ TransportTrack.Api/
COPY TransportTrack.Application/ TransportTrack.Application/
COPY TransportTrack.Domain/ TransportTrack.Domain/
COPY TransportTrack.Infrastructure/ TransportTrack.Infrastructure/
RUN dotnet publish TransportTrack.Api/TransportTrack.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "TransportTrack.Api.dll"]