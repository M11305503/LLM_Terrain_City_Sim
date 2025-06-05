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
    [TextArea] public string userPrompt = "�г]�p�@�Ӱ��Ӥ�Ұ��B��v�C���{�N�����ϰѼ�";

    public void RequestLLMParams()
    {
        StartCoroutine(RequestLLMParamsCoroutine());
    }

    IEnumerator RequestLLMParamsCoroutine()
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string apiKey = "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"; // �аȥ��O�@���_�I

        string systemPromptRaw =
        "�A�O�@�ӫ��������ѼƳW���v�C�ЮھڥΤ�y�z�A��²�u�����A�����z�A�A�����^�Ǥ@�ե��t�αM�Ϊ������ӭM�t�ưѼ�json�C" +
        "�榡���G���z�G�]���z�����^�ѼơG�]json����^�C" +
        "�Ѽƪ��󥲶��u�]�t baseHighriseProb�BbaseIndustrialProb�BhighriseClusterBoost�BindustrialClusterBoost�BparkSpreadProb �o����key�A�Ҧp�G�ѼơG{\"baseHighriseProb\":0.12,\"baseIndustrialProb\":0.22,\"highriseClusterBoost\":0.1,\"industrialClusterBoost\":0.09,\"parkSpreadProb\":0.03}�C" +
        "���n��markdown�A���n������h�lkey�A���n�����榡�A�u�^���z�M�Ѽ�json�C";

        string systemPrompt = systemPromptRaw.Replace("\"", "\\\"");
        string reqJson = "{" +
            "\"model\":\"gpt-4o\"," +
            "\"messages\":[" +
                "{\"role\":\"system\",\"content\":\"" + systemPrompt + "\"}," +
                "{\"role\":\"user\",\"content\":\"" + userPrompt.Replace("\"", "\\\"") + "\"}" +
            "]" +
        "}";

        // ������ ��X������z�L�{��log
        UnityEngine.Debug.Log("<color=#80bfff>�iLLM Debug�juserPrompt: </color>" + userPrompt);
        UnityEngine.Debug.Log("<color=#80bfff>�iLLM Debug�jsystemPrompt: </color>" + systemPrompt);
        UnityEngine.Debug.Log("<color=#0080ff>�iLLM Debug�jREQ JSON: </color>" + reqJson);

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
            UnityEngine.Debug.Log("<color=#008080>�iLLM Debug�jAPI ��l�^��: </color>" + response);

            // �s�W�o��
            string content = ExtractContentFromLLMResponse(response);

            // ����w�q�� (string reasoning, string json) ExtractReasoningAndJson(string)
            var (reasoning, paramJson) = ExtractReasoningAndJson(content);

            UnityEngine.Debug.Log("<color=#e67e22>�iLLM ���z�z�ѡj</color>" + reasoning);
            UnityEngine.Debug.Log("<color=#2ecc71>�iLLM Debug�j�ѪR�᪺json�Ѽ�: </color>" + paramJson);

            cityGrid.ApplyLLMParameters(paramJson);
        }
        else
        {
            UnityEngine.Debug.LogError("API�ШD����: " + req.error);
        }
    }

    private (string reasoning, string json) ExtractReasoningAndJson(string content)
    {
        // �䴩�h�ر��Ҫ��ѪR
        if (string.IsNullOrWhiteSpace(content))
            return ("", "{}");

        // 1. ���է� "�ѼơG" �᪺ json
        var match = Regex.Match(content, @"�Ѽ�[:�G]\s*({[\s\S]*?})");
        if (match.Success)
        {
            string json = match.Groups[1].Value.Trim();
            // ���z����
            var reasoningMatch = Regex.Match(content, @"���z[:�G]\s*([^\n\r]*)");
            string reasoning = reasoningMatch.Success ? reasoningMatch.Groups[1].Value.Trim() : "";
            return (reasoning, json);
        }
        // 2. fallback: �u�n�� json ������
        var match2 = Regex.Match(content, @"({[\s\S]*?})");
        if (match2.Success)
            return ("", match2.Groups[1].Value.Trim());

        // ���S���
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
        UnityEngine.Debug.Log("<color=orange>�iLLM �^��content���e�j</color>\n" + content);
        return content;
    }
}
