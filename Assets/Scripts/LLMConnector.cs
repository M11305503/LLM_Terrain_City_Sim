using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class LLMConnector : MonoBehaviour
{
    public CACityGrid cityGrid;
    [TextArea] public string userPrompt = "�г]�p�@�Ӱ��Ӥ�Ұ��B��v�C���{�N�����ϰѼ�";

    public void RequestLLMParams()
    {
        StartCoroutine(RequestLLMParamsCoroutine());
    }

    IEnumerator RequestLLMParamsCoroutine()
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string apiKey = "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";

        string systemPrompt = "�A�O�@�ӫ��������ѼƳW���v�C�ЮھڥΤ�y�z�A�����^�Ǥ@�]���������ӭM�t�ưѼƪ� json�C�Ҧp {\\\"baseHighriseProb\\\":0.15, \\\"baseIndustrialProb\\\":0.07, \\\"highriseClusterBoost\\\":0.12, \\\"industrialClusterBoost\\\":0.09, \\\"parkSpreadProb\\\":0.08}�C���n�h�l�����C";

        string reqJson = @"{
            ""model"": ""gpt-4o"",
            ""messages"": [
                {""role"": ""system"", ""content"": """ + systemPrompt + @"""},
                {""role"": ""user"", ""content"": """ + userPrompt + @"""}
            ]
        }";


        UnityWebRequest req = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(reqJson);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);

        Debug.Log("REQ JSON: " + reqJson);  // ��K debug

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string response = req.downloadHandler.text;
            // �ѪR LLM �^�����e�A��X json �ѼƬq
            string paramJson = ExtractJsonFromResponse(response);
            cityGrid.ApplyLLMParameters(paramJson);
        }
        else
        {
            Debug.LogError("API�ШD����: " + req.error);
        }
    }

    private string ExtractJsonFromResponse(string response)
    {
        // ���է�� "content":"{...}" �o�q json
        // �o�̪����M�� content �̪� {...}
        var match = Regex.Match(response, "\"content\":\\s*\"(\\{.*?\\})\"");
        if (match.Success)
        {
            string json = match.Groups[1].Value;
            // �ѩ� API �|�^�Ǳa���檺 \"�A�n���٭�
            json = json.Replace("\\\"", "\"");
            return json;
        }
        else
        {
            Debug.LogWarning("�䤣�� content ��쪺 JSON�I");
            return "{}";
        }
    }
}
