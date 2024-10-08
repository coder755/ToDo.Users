FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG dbn
ENV TODO_USER_DB_ID=$dbn
ARG dbpw
ENV TODO_USER_DB_PW=$dbpw

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["todo.users/todo.users.csproj", "todo.users/"]
RUN dotnet restore "todo.users/todo.users.csproj"
COPY . .
WORKDIR "/src/todo.users"
RUN dotnet build "todo.users.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "todo.users.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "todo.users.dll"]

ENV ASPNETCORE_HTTP_PORTS=80