# ---------------------
# Базовый образ для сборки (с SDK и инструментами)
# ---------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Создаем рабочую директорию
WORKDIR /src

# Копируем файлы проекта
COPY . .

# Установка EF Core Tools (если не установлены через проект)
RUN dotnet tool install --global dotnet-ef && \
    export PATH="$PATH:/root/.dotnet/tools"

# Восстанавливаем пакеты и собираем проект
RUN dotnet restore "OrderAutomation.csproj"
RUN dotnet build "OrderAutomation.csproj" --configuration Release --no-restore

# Выполняем миграции
RUN dotnet ef database update --project OrderAutomation.csproj --startup-project OrderAutomation.csproj

# Публикуем проект
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
