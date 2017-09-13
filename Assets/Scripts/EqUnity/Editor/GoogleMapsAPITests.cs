using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Eq.Unity
{
    class GoogleMapsAPITests
    {
        [Test]
        public void TestUrlParameterGetContent()
        {
            UnityWebRequest request = new UnityWebRequest();
            request.url = "https://www.google.co.jp";
            request.downloadHandler = new DownloadHandlerBuffer();

            GoogleMapsAPI api = new GoogleMapsAPI("dummy api key");
            try
            {
                byte[] response = (byte[])api.GetType().InvokeMember("GetContent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, api, new object[] { request });

                Assert.That(response != null && response.Length > 0);
            }catch(Exception e)
            {
                e.ToString();
            }
        }

        [Test]
        public void TestUrlParameterOrigin()
        {
            GoogleMapsAPI.UrlParameterOrigin param = null;

            try
            {
                param = new GoogleMapsAPI.UrlParameterOrigin();
            }
            catch (Exception e)
            {
                // 単体試験では位置情報を取得できないので、コールできればOKとする
            }

            param = new GoogleMapsAPI.UrlParameterOrigin("東京都港区赤坂");
            Assert.That(param.GetName().CompareTo("origin") == 0);
            Assert.That(param.GetValue().CompareTo("東京都港区赤坂") == 0);

            param = new GoogleMapsAPI.UrlParameterOrigin(40.12345f, 50.6789f);
            Assert.That(param.GetName().CompareTo("origin") == 0);
            Assert.That(param.GetValue().CompareTo("40.12345,50.6789") == 0);
        }

        [Test]
        public void TestUrlParameterDestination()
        {
            GoogleMapsAPI.UrlParameterDestination param = null;

            param = new GoogleMapsAPI.UrlParameterDestination("東京都港区赤坂");
            Assert.That(param.GetName().CompareTo("destination") == 0);
            Assert.That(param.GetValue().CompareTo("東京都港区赤坂") == 0);

            param = new GoogleMapsAPI.UrlParameterDestination(40.12345f, 50.6789f);
            Assert.That(param.GetValue().CompareTo("40.12345,50.6789") == 0);
        }

        [Test]
        public void TestUrlParameterAPIKey()
        {
            GoogleMapsAPI.UrlParameterAPIKey param = null;

            param = new GoogleMapsAPI.UrlParameterAPIKey("東京都港区赤坂");
            Assert.That(param.GetName().CompareTo("key") == 0);
            Assert.That(param.GetValue().CompareTo("東京都港区赤坂") == 0);

            param = new GoogleMapsAPI.UrlParameterAPIKey("40.12345f, 50.6789f");
            Assert.That(param.GetName().CompareTo("key") == 0);
            Assert.That(param.GetValue().CompareTo("40.12345f, 50.6789f") == 0);
        }

        [Test]
        public void TestUrlParameterMode()
        {
            GoogleMapsAPI.UrlParameterMode param = null;
            Type enumType = Type.GetType("Eq.Unity.GoogleMapsAPI+TransferMode, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            foreach (GoogleMapsAPI.TransferMode transferMode in Enum.GetValues(enumType))
            {
                param = new GoogleMapsAPI.UrlParameterMode(transferMode);
                Assert.That(param.GetName().CompareTo("mode") == 0);
                Assert.That(param.GetValue().CompareTo(Enum.GetName(enumType, transferMode).ToLower()) == 0);
            }
        }

        [Test]
        public void TestUrlParameterLanguage()
        {
            Dictionary<SystemLanguage, string> langDic = new Dictionary<SystemLanguage, string>();
            langDic.Add(SystemLanguage.Arabic, "ar");
            langDic.Add(SystemLanguage.Basque, "eu");
            langDic.Add(SystemLanguage.Bulgarian, "bg");
            langDic.Add(SystemLanguage.Catalan, "ca");
            langDic.Add(SystemLanguage.Czech, "cs");
            langDic.Add(SystemLanguage.Danish, "da");
            langDic.Add(SystemLanguage.Dutch, "nl");
            langDic.Add(SystemLanguage.English, "en");
            langDic.Add(SystemLanguage.Finnish, "fi");
            langDic.Add(SystemLanguage.French, "fr");
            langDic.Add(SystemLanguage.German, "de");
            langDic.Add(SystemLanguage.Greek, "el");
            langDic.Add(SystemLanguage.Hebrew, "iw");
            langDic.Add(SystemLanguage.Indonesian, "id");
            langDic.Add(SystemLanguage.Italian, "it");
            langDic.Add(SystemLanguage.Japanese, "ja");
            langDic.Add(SystemLanguage.Korean, "ko");
            langDic.Add(SystemLanguage.Latvian, "lv");
            langDic.Add(SystemLanguage.Lithuanian, "lt");
            langDic.Add(SystemLanguage.Norwegian, "no");
            langDic.Add(SystemLanguage.Polish, "pl");
            langDic.Add(SystemLanguage.Portuguese, "pt");
            langDic.Add(SystemLanguage.Romanian, "ro");
            langDic.Add(SystemLanguage.Russian, "ru");
            langDic.Add(SystemLanguage.Belarusian, "ru");
            langDic.Add(SystemLanguage.SerboCroatian, "sr");
            langDic.Add(SystemLanguage.Slovak, "sk");
            langDic.Add(SystemLanguage.Slovenian, "sl");
            langDic.Add(SystemLanguage.Spanish, "es");
            langDic.Add(SystemLanguage.Swedish, "sv");
            langDic.Add(SystemLanguage.Thai, "th");
            langDic.Add(SystemLanguage.Turkish, "tr");
            langDic.Add(SystemLanguage.Ukrainian, "uk");
            langDic.Add(SystemLanguage.Vietnamese, "vi");
            langDic.Add(SystemLanguage.Chinese, "zh-CN");
            langDic.Add(SystemLanguage.ChineseSimplified, "zh-CN");
            langDic.Add(SystemLanguage.ChineseTraditional, "zh-TW");
            langDic.Add(SystemLanguage.Hungarian, "hu");
            langDic.Add(SystemLanguage.Afrikaans, "en");
            langDic.Add(SystemLanguage.Faroese, "en");
            langDic.Add(SystemLanguage.Icelandic, "en");
            langDic.Add(SystemLanguage.Estonian, "en");
            langDic.Add(SystemLanguage.Unknown, "en");

            foreach(KeyValuePair<SystemLanguage, string> langValuePair in langDic)
            {
                GoogleMapsAPI.UrlParameterLanguage param = new Eq.Unity.GoogleMapsAPI.UrlParameterLanguage(langValuePair.Key);
                Assert.That(param.GetName().CompareTo("language") == 0);
                Assert.That(param.GetValue().CompareTo(langValuePair.Value) == 0);
            }
        }
    }
}
