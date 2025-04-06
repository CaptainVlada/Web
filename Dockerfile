# ---------------------
# Базовый образ для сборки (с SDK и инструментами)
# ---------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Установка зависимостей (если нужны пакеты Linux)
# RUN apt-get update && apt-get install -y <ваши-пакеты>

# Создаем рабочую директорию
WORKDIR /src

# Копируем файлы проекта
COPY . .

# Восстанавливаем пакеты и собираем проект
RUN dotnet restore "Web.csproj"
RUN dotnet build "Web.csproj" --configuration Release --no-restore

# ---------------------
# Финальный образ (только runtime)
# ---------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Копируем настройки безопасности и языка
COPY --from=build /src/appsettings.* ./
COPY --from=build /src/Web.deps.json ./
COPY --from=build /src/Web.dll ./
COPY --from=build /src/Web.pdb ./
COPY --from=build /src/wwwroot ./wwwroot

# Устанавливаем рабочую директорию
WORKDIR /

# Запуск приложения
ENTRYPOINT ["dotnet", "Web.dll"]

# Опционально: разрешение HTTPS сертификата (если нужно)
# RUN dotnet dev-certs https -ep /https.crt && dotnet dev-certs https --trust
