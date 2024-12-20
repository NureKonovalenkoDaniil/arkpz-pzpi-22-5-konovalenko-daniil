#include <iostream>
#include <string>
#include <random>
#include <thread>
#include <chrono>
#include <cpr/cpr.h>
#include <json/json.h>

// Налаштування сервера
const std::string serverUrl = "https://localhost:7069/api/iotdevice"; // Ендпоінт для отримання інформації про пристрій
const std::string dataSendUrl = "https://localhost:7069/api/storagecondition"; // Ендпоінт для відправки даних

// Параметри сенсора (значення за замовчуванням)
float minTemperature = 2.0;
float maxTemperature = 8.0;
float minHumidity = 30.0;
float maxHumidity = 70.0;
int deviceID = 2; // ID пристрою

// Генерація випадкових значень
float generateRandomValue(float min, float max) {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_real_distribution<> dis(min, max);
    return dis(gen);
}

// Отримання інформації про пристрій з сервера
void fetchDeviceInfo(const std::string& jwtToken) {
    std::string url = serverUrl + "/" + std::to_string(deviceID);
    cpr::Response response = cpr::Get(
        cpr::Url{ url },
        cpr::Header{ {"Authorization", "Bearer " + jwtToken} }
    );

    if (response.status_code == 200) {
        try {
            Json::CharReaderBuilder reader;
            Json::Value deviceInfo;
            std::string errs;
            std::istringstream responseStream(response.text);

            if (Json::parseFromStream(reader, responseStream, &deviceInfo, &errs)) {
                minTemperature = deviceInfo["minTemperature"].asFloat();
                maxTemperature = deviceInfo["maxTemperature"].asFloat();
                minHumidity = deviceInfo["minHumidity"].asFloat();
                maxHumidity = deviceInfo["maxHumidity"].asFloat();

                std::cout << "Device configuration updated from server:\n"
                    << "Min Temperature: " << minTemperature << "\n"
                    << "Max Temperature: " << maxTemperature << "\n"
                    << "Min Humidity: " << minHumidity << "\n"
                    << "Max Humidity: " << maxHumidity << "\n";
            }
            else {
                std::cerr << "Failed to parse device info JSON: " << errs << std::endl;
            }
        }
        catch (const std::exception& e) {
            std::cerr << "Error while processing server response: " << e.what() << std::endl;
        }
    }
    else {
        std::cerr << "Failed to fetch device info. Status code: " << response.status_code << std::endl;
    }
}

// Відправка даних на сервер
void sendDataToServer(float temperature, float humidity) {
    cpr::Response response = cpr::Post(
        cpr::Url{ dataSendUrl },
        cpr::Header{ {"Content-Type", "application/json"} },
        cpr::Body{ "{\"temperature\": " + std::to_string(temperature) +
                  ", \"humidity\": " + std::to_string(humidity) +
                  ", \"deviceID\": " + std::to_string(deviceID) + "}" }
    );

    if (response.status_code == 200) {
        std::cout << "Data sent successfully: " << response.text << std::endl;
    }
    else {
        std::cerr << "Failed to send data. Status code: " << response.status_code << std::endl;
    }
}

int main() {
    SetConsoleOutputCP(CP_UTF8); // Установлення кодування UTF-8 для виводу
    SetConsoleCP(CP_UTF8);       // Установлення кодування UTF-8 для вводу

    std::cout << "Starting IoT Device Emulator for device ID: " << deviceID << std::endl;

    std::string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIyNjE1NjRmZS04OWFiLTQ2YmYtOGQxNC00ZGNhMTExZGIxZDEiLCJ1bmlxdWVfbmFtZSI6InRlc3RAZ21haWwuY29tIiwiZW1haWwiOiJ0ZXN0QGdtYWlsLmNvbSIsInJvbGUiOiJBZG1pbmlzdHJhdG9yIiwibmJmIjoxNzM0NjQ0OTU5LCJleHAiOjE3NjYxODA5NTksImlhdCI6MTczNDY0NDk1OSwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo3MDY5IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo3MDY5In0.NVlrW1DPgrflkbPe10vAr4KH7SgzwWzmXbNEkhBDMpE";

    // Початкове завантаження інформації про пристрій
    fetchDeviceInfo(jwtToken);

    while (true) {
        // Генерація випадкових даних
        float temperature = generateRandomValue(minTemperature - 5, maxTemperature + 5);
        float humidity = generateRandomValue(minHumidity - 10, maxHumidity + 10);

        // Вивід даних у консоль
        std::cout << "Generated Data -> Temperature: " << temperature
            << "°C, Humidity: " << humidity << "%" << std::endl;

        // Відправка даних на сервер
        sendDataToServer(temperature, humidity);

        // Оновлення інформації про пристрій кожні 60 секунд
        static int counter = 0;
        if (counter % 12 == 0) { // Раз на 12 циклів (60 секунд, якщо цикл 5 секунд)
            fetchDeviceInfo(jwtToken);
        }

        counter++;
        // Затримка перед наступною ітерацією (5 секунд)
        std::this_thread::sleep_for(std::chrono::seconds(5));
    }

    return 0;
}