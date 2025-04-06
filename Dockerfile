# ---------------------
# Базовый образ для сборки (с SDK и инструментами)
# ---------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Создаем рабочую директорию
WORKDIR /src

# Копируем файлы проекта
COPY . .

# Восстанавливаем пакеты и собираем проект
RUN dotnet restore "OrderAutomation.csproj"
RUN dotnet build "OrderAutomation.csproj" --configuration Release --no-restore

# Публикуем проект (важно!)
RUN dotnet publish "OrderAutomation.csproj" --configuration Release --output out --no-build

# ---------------------
# Финальный образ (только runtime)
# ---------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Копируем все выходные файлы из папки 'out'
COPY --from=build /src/out/ ./

# Устанавливаем рабочую директорию
WORKDIR /

# Запуск приложения
ENTRYPOINT ["dotnet", "OrderAutomation.dll"]
