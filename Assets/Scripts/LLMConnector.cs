using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class LLMConnector : MonoBehaviour
{
    public CACityGrid cityGrid;
    [TextArea] public string userPrompt = "請設計一個高樓比例高、住宅低的現代都市區參數";

    public void RequestLLMParams()
    {
        StartCoroutine(RequestLLMParamsCoroutine());
    }

    IEnumerator RequestLLMParamsCoroutine()
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string apiKey = "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";

        string systemPrompt = "你是一個城市模擬參數規劃師。請根據用戶描述，直接回傳一包對應城市細胞演化參數的 json。例如 {\\\"baseHighriseProb\\\":0.15, \\\"baseIndustrialProb\\\":0.07, \\\"highriseClusterBoost\\\":0.12, \\\"industrialClusterBoost\\\":0.09, \\\"parkSpreadProb\\\":0.08}。不要多餘解釋。";

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

        Debug.Log("REQ JSON: " + reqJson);  // 方便 debug

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string response = req.downloadHandler.text;
            // 解析 LLM 回應內容，找出 json 參數段
            string paramJson = ExtractJsonFromResponse(response);
            cityGrid.ApplyLLMParameters(paramJson);
        }
        else
        {
            Debug.LogError("API請求失敗: " + req.error);
        }
    }

    private string ExtractJsonFromResponse(string response)
    {
        // 嘗試抓取 "content":"{...}" 這段 json
        // 這裡直接尋找 content 裡的 {...}
        var match = Regex.Match(response, "\"content\":\\s*\"(\\{.*?\\})\"");
        if (match.Success)
        {
            string json = match.Groups[1].Value;
            // 由於 API 會回傳帶跳脫的 \"，要先還原
            json = json.Replace("\\\"", "\"");
            return json;
        }
        else
        {
            Debug.LogWarning("找不到 content 欄位的 JSON！");
            return "{}";
        }
    }
}
