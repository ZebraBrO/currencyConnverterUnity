using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

// Класс для хранения данных о курсах валют
[Serializable]
public class CurrencyData
{
    public string disclaimer; // Описание
    public string date; // Дата
    public long timestamp; // Временная метка
    public string baseCurrency; // Базовая валюта
    public Dictionary<string, float> rates; // Курсы валют
}

// Класс для хранения курсов валют
[Serializable]
public class Rates
{
    public float BGN; // Болгарский лев
    public float GBP; // Фунт стерлингов
}

public class CurrencyConverter : MonoBehaviour
{
    public Dropdown currencyDropdown1; // Выпадающий список валюты 1
    public Dropdown currencyDropdown2; // Выпадающий список валюты 2
    public InputField inputField1; // Поле ввода суммы 1
    public InputField inputField2; // Поле ввода суммы 2
    public Text dateText; // Текст с датой

    private Dictionary<string, float> currencyRates = new Dictionary<string, float>(); // Словарь для хранения курсов валют

    // Метод вызывается перед первым кадром обновления
    void Start()
    {
        // Установка жёстко заданных курсов валют
        currencyRates.Add("RUB_BGN", 0.020f);
        currencyRates.Add("RUB_GBP", 0.0086f);
        currencyRates.Add("BGN_RUB", 51.27f);
        currencyRates.Add("BGN_GBP", 0.44f);
        currencyRates.Add("GBP_RUB", 117.00f);
        currencyRates.Add("GBP_BGN", 2.28f);

        // Обновление выпадающих списков с жёстко заданными парами валют
        UpdateCurrencyDropdowns();

        // Добавление слушателя для изменений валюты или суммы
        currencyDropdown1.onValueChanged.AddListener(delegate { ConvertCurrency(); });
        currencyDropdown2.onValueChanged.AddListener(delegate { ConvertCurrency(); });
        inputField1.onEndEdit.AddListener(delegate { ConvertCurrency(); });

        // Загрузка сохранённого состояния
        LoadAppState();
    }

    // Корутина для получения данных о курсах валют
    IEnumerator GetCurrencyDataCoroutine(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Запрос и ожидание ответа
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Ошибка при получении данных о курсах валют: " + webRequest.error);
            }
            else
            {
                ProcessCurrencyData(webRequest.downloadHandler.text);
            }
        }
    }

    // Метод для обработки данных о курсах валют
    void ProcessCurrencyData(string jsonData)
    {
        CurrencyData currencyData = JsonUtility.FromJson<CurrencyData>(jsonData);

        // Предполагая, что currencyData не равен null и содержит действительные данные
        currencyRates = currencyData.rates;

        // Обновление выпадающих списков после получения новых курсов
        UpdateCurrencyDropdowns();
    }

    // Метод для обновления выпадающих списков валют
    void UpdateCurrencyDropdowns()
    {
        // Очистка старых вариантов
        currencyDropdown1.ClearOptions();
        currencyDropdown2.ClearOptions();

        // Так как у нас есть конкретные пары валют, давайте использовать их как варианты
        List<string> options = new List<string> { "RUB", "BGN", "GBP" };

        // Логирование для проверки добавленных вариантов
        Debug.Log("Добавление вариантов в выпадающие списки: " + string.Join(", ", options));

        currencyDropdown1.AddOptions(options);
        currencyDropdown2.AddOptions(options);
    }

    // Метод для конвертации валюты
    void ConvertCurrency()
    {
        // Получение выбранных валют
        string currencyFrom = currencyDropdown1.options[currencyDropdown1.value].text;
        string currencyTo = currencyDropdown2.options[currencyDropdown2.value].text;
        string currencyPair = currencyFrom + "_" + currencyTo;

        float amountToConvert;
        // Попытка разбора введённой пользователем суммы
        if (!float.TryParse(inputField1.text, out amountToConvert))
        {
            Debug.LogError("Введена неверная сумма.");
            return;
        }

        // Проверка, есть ли у нас курс для выбранной пары валют
        if (currencyRates.TryGetValue(currencyPair, out float rate))
        {
            // Выполнение конвертации
            float convertedAmount = amountToConvert * rate;
            inputField2.text = convertedAmount.ToString("N2");
        }
        else
        {
            Debug.LogError("Пара валют не найдена в словаре курсов.");
            inputField2.text = "Ошибка";
        }
    }

    // Метод для сохранения состояния приложения
    void SaveAppState()
    {
        // Сохранение текущей даты и введённой суммы
        PlayerPrefs.SetString("Дата", dateText.text);
        PlayerPrefs.SetString("Сумма", inputField1.text);
        PlayerPrefs.Save();
    }

    // Метод для загрузки сохранённого состояния
    void LoadAppState()
    {
        // Загрузка сохранённой даты и суммы
        dateText.text = PlayerPrefs.GetString("Дата", DateTime.Today.ToString("yyyy/MM/dd"));
        inputField1.text = PlayerPrefs.GetString("Сумма", "1");
    }
}
