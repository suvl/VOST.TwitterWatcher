FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine as BUILDER

RUN apk add --update nodejs npm

COPY . /src

WORKDIR /src/VOST.TwitterWatcher

RUN dotnet publish -c Release -r linux-x64 

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS RUNNER

COPY --from=BUILDER /src/VOST.TwitterWatcher/publish /app

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

CMD [""]