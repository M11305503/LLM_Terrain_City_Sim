using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;
using MiniJSON;


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
        string apiKey = "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"; // 請務必保護金鑰！

        string caRuleExplanation =
        "細胞演化規則如下：每個格子的型態根據以下機率決定。" +
        "若不可建築，永遠為公園。" +
        "若周圍有三個以上公園且隨機數小於 parkSpreadProb，則變公園。" +
        "其餘情況下，根據下列公式計算：" +
        "highriseProb = baseHighriseProb + highriseClusterBoost × 高樓鄰居數" +
        "industrialProb = baseIndustrialProb + industrialClusterBoost × 工業鄰居數" +
        "residentialProb = 1 - highriseProb - industrialProb" +
        "隨機值小於 highriseProb 則變高樓；介於 highriseProb 與 highriseProb+industrialProb 則變工業；否則變住宅。" +
        "你給的所有參數（baseHighriseProb, baseIndustrialProb, highriseClusterBoost, industrialClusterBoost, parkSpreadProb）都會用在這個公式裡。";

        string systemPromptRaw =
        caRuleExplanation +
        "你是一個城市模擬參數規劃師。請根據用戶描述，先簡短說明你的推理，再直接回傳一組本系統專用的城市細胞演化參數json。" +
        "格式為：推理：（推理說明）參數：（json物件）。" +
        "參數物件必須只包含 baseHighriseProb、baseIndustrialProb、highriseClusterBoost、industrialClusterBoost、parkSpreadProb 這五個key，例如：參數：{\"baseHighriseProb\":0.1,\"baseIndustrialProb\":0.1,\"highriseClusterBoost\":0.05,\"industrialClusterBoost\":0.05,\"parkSpreadProb\":0.02}。" +
        "不要用markdown，以參數：{\"baseHighriseProb\":0.1,\"baseIndustrialProb\":0.1,\"highriseClusterBoost\":0.05,\"industrialClusterBoost\":0.05,\"parkSpreadProb\":0.02}作為參數基準(這是基礎城市)，只回完整推理和參數json。";

        string systemPrompt = systemPromptRaw.Replace("\"", "\\\"");
        string reqJson = "{" +
            "\"model\":\"gpt-4o\"," +
            "\"messages\":[" +
                "{\"role\":\"system\",\"content\":\"" + systemPrompt + "\"}," +
                "{\"role\":\"user\",\"content\":\"" + userPrompt.Replace("\"", "\\\"") + "\"}" +
            "]" +
        "}";

        // ★★★ 輸出完整推理過程的log
        UnityEngine.Debug.Log("<color=#80bfff>【LLM Debug】userPrompt: </color>" + userPrompt);
        UnityEngine.Debug.Log("<color=#80bfff>【LLM Debug】systemPrompt: </color>" + systemPrompt);
        UnityEngine.Debug.Log("<color=#0080ff>【LLM Debug】REQ JSON: </color>" + reqJson);

        UnityWebRequest req = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(reqJson);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string response = req.downloadHandler.text;
            UnityEngine.Debug.Log("<color=#008080>【LLM Debug】API 原始回應: </color>" + response);

            // 新增這行
            string content = ExtractContentFromLLMResponse(response);

            // 把剛剛定義的 (string reasoning, string json) ExtractReasoningAndJson(string)
            var (reasoning, paramJson) = ExtractReasoningAndJson(content);

            UnityEngine.Debug.Log("<color=#e67e22>【LLM 推理理由】</color>" + reasoning);
            UnityEngine.Debug.Log("<color=#2ecc71>【LLM Debug】解析後的json參數: </color>" + paramJson);

            cityGrid.ApplyLLMParameters(paramJson);
        }
        else
        {
            UnityEngine.Debug.LogError("API請求失敗: " + req.error);
        }
    }

    private (string reasoning, string json) ExtractReasoningAndJson(string content)
    {
        // 支援多種情境的解析
        if (string.IsNullOrWhiteSpace(content))
            return ("", "{}");

        // 1. 嘗試抓 "參數：" 後的 json
        var match = Regex.Match(content, @"參數[:：]\s*({[\s\S]*?})");
        if (match.Success)
        {
            string json = match.Groups[1].Value.Trim();
            // 推理部分
            var reasoningMatch = Regex.Match(content, @"推理[:：]\s*([^\n\r]*)");
            string reasoning = reasoningMatch.Success ? reasoningMatch.Groups[1].Value.Trim() : "";
            return (reasoning, json);
        }
        // 2. fallback: 只要有 json 直接取
        var match2 = Regex.Match(content, @"({[\s\S]*?})");
        if (match2.Success)
            return ("", match2.Groups[1].Value.Trim());

        // 都沒抓到
        return ("", "{}");
    }

    private string ExtractContentFromLLMResponse(string response)
    {
        var dict = MiniJSON.Json.Deserialize(response) as System.Collections.Generic.Dictionary<string, object>;
        if (dict == null || !dict.ContainsKey("choices")) return "";

        var choices = dict["choices"] as System.Collections.IList;
        if (choices == null || choices.Count == 0) return "";

        var choice = choices[0] as System.Collections.Generic.Dictionary<string, object>;
        if (choice == null || !choice.ContainsKey("message")) return "";

        var message = choice["message"] as System.Collections.Generic.Dictionary<string, object>;
        if (message == null || !message.ContainsKey("content")) return "";

        string content = message["content"].ToString();
        UnityEngine.Debug.Log("<color=orange>【LLM 回傳content內容】</color>\n" + content);
        return content;
    }
}
