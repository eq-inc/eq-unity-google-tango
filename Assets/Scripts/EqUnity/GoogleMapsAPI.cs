using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Eq.Unity
{
    public class GoogleMapsAPI : BaseAndroidBehaviour
    {
        public enum TransferMode
        {
            Driving, Walking, Bicycling, Transit
        }

        private const string UrlBaseDirections = "https://maps.googleapis.com/maps/api/directions/json?";

        private string mAPIKey;

        public GoogleMapsAPI(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException("api == null or api length is 0");
            }

            mAPIKey = apiKey;
        }

        public ResponseDirections GetDirectionsFromCurrentPosition(TransferMode transferMode, string address)
        {
            return GetDirections(transferMode, Input.location.lastData.latitude, Input.location.lastData.longitude, new UrlParameterDestination(address));
        }

        public ResponseDirections GetDirectionsFromCurrentPosition(TransferMode transferMode, float latitude, float longitude)
        {
            return GetDirections(transferMode, Input.location.lastData.latitude, Input.location.lastData.longitude, new UrlParameterDestination(latitude, longitude));
        }

        public ResponseDirections GetDirections(TransferMode transferMode, float srcLatitude, float srcLongitude, UrlParameterDestination destParameter)
        {
            StringBuilder urlBuilder = new StringBuilder(UrlBaseDirections);
            UnityWebRequest request = new UnityWebRequest();

            // modeパラメータ
            UrlParameterMode modeParameter = new UrlParameterMode(transferMode);
            urlBuilder.Append(modeParameter.GetName()).Append("=").Append(modeParameter.GetValue());

            // originパラメータ
            UrlParameterOrigin originParameter = new UrlParameterOrigin(srcLatitude, srcLongitude);
            urlBuilder.Append(originParameter.GetName()).Append("=").Append(originParameter.GetValue());

            // languageパラメータ
            UrlParameterLanguage languageParameter = new UrlParameterLanguage();
            urlBuilder.Append(languageParameter.GetName()).Append("=").Append(languageParameter.GetValue());

            // API key
            UrlParameterAPIKey apiKeyParameter = new UrlParameterAPIKey(mAPIKey);
            urlBuilder.Append(apiKeyParameter.GetName()).Append("=").Append(apiKeyParameter.GetValue());

            request.url = urlBuilder.ToString();
            byte[] content = GetContent(request);
            string contentTypeHD = request.GetResponseHeader("Content-type");
            string[] headerParams = contentTypeHD.Split(new char[';']);
            Encoding useEncoding = Encoding.UTF8;

            if (headerParams.Length > 0)
            {
                foreach (string headerParam in headerParams)
                {
                    if (headerParam.StartsWith("charset="))
                    {
                        string[] paramKeyValue = headerParam.Split(new char[] { '=' });
                        if (paramKeyValue.Length > 1)
                        {
                            useEncoding = Encoding.GetEncoding(paramKeyValue[1]);
                        }
                        break;
                    }
                }
            }

            return JsonUtility.FromJson<ResponseDirections>(useEncoding.GetString(content));
        }

        private byte[] GetContent(UnityWebRequest request)
        {
            byte[] ret = null;

            request.downloadHandler = new DownloadHandlerBuffer();
            StartCoroutine(RunWebRequest(request));
            if (request.responseCode == 200)
            {
                ret = request.downloadHandler.data;
            }

            return ret;
        }

        private IEnumerator RunWebRequest(UnityWebRequest request)
        {
            yield return request.Send();

            while (!request.isDone)
            {
                yield return null;
            }
        }

        abstract public class UrlParameter
        {
            internal string mName;
            internal string mValue;

            public UrlParameter(string name)
            {
                mName = name;
            }

            public string GetName()
            {
                return mName;
            }

            public string GetValue()
            {
                return mValue;
            }
        }

        public class UrlParameterOrigin : UrlParameter
        {
            public UrlParameterOrigin() : base("origin")
            {
                if (Input.location.lastData.timestamp == 0)
                {
                    throw new Exception("cannot get location");
                }

                mValue = Input.location.lastData.latitude + "," + Input.location.lastData.longitude;
            }

            public UrlParameterOrigin(float srcLatitude, float srcLongitude) : base("origin")
            {
                mValue = srcLatitude.ToString() + "," + srcLongitude.ToString();
            }

            public UrlParameterOrigin(string address) : base("origin")
            {
                mValue = address;
            }
        }

        public class UrlParameterDestination : UrlParameter
        {
            public UrlParameterDestination(float destLatitude, float destLongitude) : base("destination")
            {
                mValue = destLatitude.ToString() + "," + destLongitude.ToString();
            }

            public UrlParameterDestination(string address) : base("destination")
            {
                mValue = address;
            }
        }

        public class UrlParameterAPIKey : UrlParameter
        {
            public UrlParameterAPIKey(string apiKey) : base("key")
            {
                mValue = apiKey;
            }
        }

        public class UrlParameterLanguage : UrlParameter
        {
            public UrlParameterLanguage() : this(Application.systemLanguage)
            {
            }

            public UrlParameterLanguage(SystemLanguage systemLanguage) : base("language")
            {
                switch (systemLanguage)
                {
                    case SystemLanguage.Arabic: // アラビア語
                        mValue = "ar";
                        break;
                    case SystemLanguage.Basque: // バスク語
                        mValue = "eu";
                        break;
                    case SystemLanguage.Bulgarian:  // ブルガリア語
                        mValue = "bg";
                        break;
                    case SystemLanguage.Catalan:    // カタロニア語
                        mValue = "ca";
                        break;
                    case SystemLanguage.Czech:  // チェコ語
                        mValue = "cs";
                        break;
                    case SystemLanguage.Danish: // デンマーク語
                        mValue = "da";
                        break;
                    case SystemLanguage.Dutch:  // オランダ語
                        mValue = "nl";
                        break;
                    case SystemLanguage.English:    // 英語
                        mValue = "en";
                        break;
                    case SystemLanguage.Finnish:    // フィンランド語
                        mValue = "fi";
                        break;
                    case SystemLanguage.French: // フランス語
                        mValue = "fr";
                        break;
                    case SystemLanguage.German: // ドイツ語
                        mValue = "de";
                        break;
                    case SystemLanguage.Greek:  // ギリシャ語
                        mValue = "el";
                        break;
                    case SystemLanguage.Hebrew: // ヘブライ語
                        mValue = "iw";
                        break;
                    case SystemLanguage.Indonesian: // インドネシア語
                        mValue = "id";
                        break;
                    case SystemLanguage.Italian:    // イタリア語
                        mValue = "it";
                        break;
                    case SystemLanguage.Japanese:   // 日本語
                        mValue = "ja";
                        break;
                    case SystemLanguage.Korean: // 韓国語
                        mValue = "ko";
                        break;
                    case SystemLanguage.Latvian:    // ラトビア語
                        mValue = "lv";
                        break;
                    case SystemLanguage.Lithuanian: // リトアニア語
                        mValue = "lt";
                        break;
                    case SystemLanguage.Norwegian:  // ノルウェー語
                        mValue = "no";
                        break;
                    case SystemLanguage.Polish: // ポーランド語
                        mValue = "pl";
                        break;
                    case SystemLanguage.Portuguese: // ポルトガル語
                        mValue = "pt";
                        break;
                    case SystemLanguage.Romanian:   // ルーマニア語
                        mValue = "ro";
                        break;
                    case SystemLanguage.Russian:    // ロシア語
                    case SystemLanguage.Belarusian: // ベラルーシ語
                        mValue = "ru";
                        break;
                    case SystemLanguage.SerboCroatian:  // セルビアクロアチア語
                        mValue = "sr";
                        break;
                    case SystemLanguage.Slovak: // スロバキア語
                        mValue = "sk";
                        break;
                    case SystemLanguage.Slovenian:  // スロベニア語
                        mValue = "sl";
                        break;
                    case SystemLanguage.Spanish:    // スペイン語
                        mValue = "es";
                        break;
                    case SystemLanguage.Swedish:    // スウェーデン語
                        mValue = "sv";
                        break;
                    case SystemLanguage.Thai:   // タイ語
                        mValue = "th";
                        break;
                    case SystemLanguage.Turkish:    // トルコ語
                        mValue = "tr";
                        break;
                    case SystemLanguage.Ukrainian:  // ウクライナ語
                        mValue = "uk";
                        break;
                    case SystemLanguage.Vietnamese: // ベトナム語
                        mValue = "vi";
                        break;
                    case SystemLanguage.Chinese:    // 中国語
                    case SystemLanguage.ChineseSimplified:  // 中国語簡体字(simplified)
                        mValue = "zh-CN";
                        break;
                    case SystemLanguage.ChineseTraditional: // 中国語繁体字(traditional)
                        mValue = "zh-TW";
                        break;
                    case SystemLanguage.Hungarian:  // ハンガリー語
                        mValue = "hu";
                        break;

                    case SystemLanguage.Afrikaans:  // アフリカ語(非サポート)
                    case SystemLanguage.Faroese:    // フェロー語(非サポート)
                    case SystemLanguage.Icelandic:  // アイスランド語(非サポート)
                    case SystemLanguage.Estonian:   // エストニア語(非サポート)
                    case SystemLanguage.Unknown:    // 不明
                    default:
                        // 非サポートの言語は全て英語として表示
                        mValue = "en";
                        break;
                }
            }
        }

        public class UrlParameterMode : UrlParameter
        {
            public UrlParameterMode(TransferMode transferMode) : base("mode")
            {
                mValue = transferMode.ToString().ToLower();
            }
        }
    }

    public class ResponseDirections
    {
        public string status;
        public GeocodedWaypoint[] geocoded_waypoints;
        public Route[] routes;
    }

    public class GeocodedWaypoint
    {
        public string geocoder_status;
        public string place_id;
        public string[] types;
    }

    public class Route
    {
        public string summary;
        public Leg[] legs;
        public string copyrights;
        public Points overview_polyline;
        public int[] waypoint_order;
        public Bounds bounds;
    }

    public class Leg
    {
        public Step[] steps;
        public IntValue duration;
        public IntValue distance;
        public LatLng start_location;
        public LatLng end_location;
        public string start_address;
        public string end_address;
    }

    public class Step
    {
        public string travel_mode;
        public LatLng start_location;
        public LatLng end_location;
        public Points polyline;
        public IntValue duration;
        public string html_instructions;
        public IntValue distance;
    }

    public class LatLng
    {
        public float lat;
        public float lng;
    }

    public class IntValue
    {
        public int value;
        public string text;
    }

    public class Points
    {
        public string points;
    }

    public class Bounds
    {
        public LatLng southwest;
        public LatLng northeast;
    }
}
