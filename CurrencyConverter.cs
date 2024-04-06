using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

// ����� ��� �������� ������ � ������ �����
[Serializable]
public class CurrencyData
{
    public string disclaimer; // ��������
    public string date; // ����
    public long timestamp; // ��������� �����
    public string baseCurrency; // ������� ������
    public Dictionary<string, float> rates; // ����� �����
}

// ����� ��� �������� ������ �����
[Serializable]
public class Rates
{
    public float BGN; // ���������� ���
    public float GBP; // ���� ����������
}

public class CurrencyConverter : MonoBehaviour
{
    public Dropdown currencyDropdown1; // ���������� ������ ������ 1
    public Dropdown currencyDropdown2; // ���������� ������ ������ 2
    public InputField inputField1; // ���� ����� ����� 1
    public InputField inputField2; // ���� ����� ����� 2
    public Text dateText; // ����� � �����

    private Dictionary<string, float> currencyRates = new Dictionary<string, float>(); // ������� ��� �������� ������ �����

    // ����� ���������� ����� ������ ������ ����������
    void Start()
    {
        // ��������� ����� �������� ������ �����
        currencyRates.Add("RUB_BGN", 0.020f);
        currencyRates.Add("RUB_GBP", 0.0086f);
        currencyRates.Add("BGN_RUB", 51.27f);
        currencyRates.Add("BGN_GBP", 0.44f);
        currencyRates.Add("GBP_RUB", 117.00f);
        currencyRates.Add("GBP_BGN", 2.28f);

        // ���������� ���������� ������� � ����� ��������� ������ �����
        UpdateCurrencyDropdowns();

        // ���������� ��������� ��� ��������� ������ ��� �����
        currencyDropdown1.onValueChanged.AddListener(delegate { ConvertCurrency(); });
        currencyDropdown2.onValueChanged.AddListener(delegate { ConvertCurrency(); });
        inputField1.onEndEdit.AddListener(delegate { ConvertCurrency(); });

        // �������� ����������� ���������
        LoadAppState();
    }

    // �������� ��� ��������� ������ � ������ �����
    IEnumerator GetCurrencyDataCoroutine(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // ������ � �������� ������
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("������ ��� ��������� ������ � ������ �����: " + webRequest.error);
            }
            else
            {
                ProcessCurrencyData(webRequest.downloadHandler.text);
            }
        }
    }

    // ����� ��� ��������� ������ � ������ �����
    void ProcessCurrencyData(string jsonData)
    {
        CurrencyData currencyData = JsonUtility.FromJson<CurrencyData>(jsonData);

        // �����������, ��� currencyData �� ����� null � �������� �������������� ������
        currencyRates = currencyData.rates;

        // ���������� ���������� ������� ����� ��������� ����� ������
        UpdateCurrencyDropdowns();
    }

    // ����� ��� ���������� ���������� ������� �����
    void UpdateCurrencyDropdowns()
    {
        // ������� ������ ���������
        currencyDropdown1.ClearOptions();
        currencyDropdown2.ClearOptions();

        // ��� ��� � ��� ���� ���������� ���� �����, ������� ������������ �� ��� ��������
        List<string> options = new List<string> { "RUB", "BGN", "GBP" };

        // ����������� ��� �������� ����������� ���������
        Debug.Log("���������� ��������� � ���������� ������: " + string.Join(", ", options));

        currencyDropdown1.AddOptions(options);
        currencyDropdown2.AddOptions(options);
    }

    // ����� ��� ����������� ������
    void ConvertCurrency()
    {
        // ��������� ��������� �����
        string currencyFrom = currencyDropdown1.options[currencyDropdown1.value].text;
        string currencyTo = currencyDropdown2.options[currencyDropdown2.value].text;
        string currencyPair = currencyFrom + "_" + currencyTo;

        float amountToConvert;
        // ������� ������� �������� ������������� �����
        if (!float.TryParse(inputField1.text, out amountToConvert))
        {
            Debug.LogError("������� �������� �����.");
            return;
        }

        // ��������, ���� �� � ��� ���� ��� ��������� ���� �����
        if (currencyRates.TryGetValue(currencyPair, out float rate))
        {
            // ���������� �����������
            float convertedAmount = amountToConvert * rate;
            inputField2.text = convertedAmount.ToString("N2");
        }
        else
        {
            Debug.LogError("���� ����� �� ������� � ������� ������.");
            inputField2.text = "������";
        }
    }

    // ����� ��� ���������� ��������� ����������
    void SaveAppState()
    {
        // ���������� ������� ���� � �������� �����
        PlayerPrefs.SetString("����", dateText.text);
        PlayerPrefs.SetString("�����", inputField1.text);
        PlayerPrefs.Save();
    }

    // ����� ��� �������� ����������� ���������
    void LoadAppState()
    {
        // �������� ���������� ���� � �����
        dateText.text = PlayerPrefs.GetString("����", DateTime.Today.ToString("yyyy/MM/dd"));
        inputField1.text = PlayerPrefs.GetString("�����", "1");
    }
}
