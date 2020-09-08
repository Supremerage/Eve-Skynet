FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302-alpine3.12 AS build-env
RUN apk add --no-cache tzdata

WORKDIR /app
COPY ./*/*.csproj .
RUN dotnet restore 
COPY ./Eve-Skynet .
RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.6-alpine3.12
RUN apk add --no-cache tzdata
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT [ "dotnet", "Eve_Skynet.Bot.dll" ]

#Mount Appsettings.json to /app
