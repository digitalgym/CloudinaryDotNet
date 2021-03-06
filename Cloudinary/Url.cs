﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CloudinaryDotNet
{
    public class Url : ICloneable
    {
        ISignProvider m_signProvider;

        string m_cloudName;
        string m_cloudinaryAddr = Api.ADDR_RES;
        string m_apiVersion;

        bool m_shorten;
        bool m_secure;
        bool m_usePrivateCdn;
        bool m_signed;
        string m_privateCdn;
        string m_version;
        string m_cName;
        bool m_cSubDomain;

        List<string> m_customParts = new List<string>();

        string m_action = String.Empty;
        string m_resourceType = String.Empty;

        Transformation m_transformation = null;

        public Url(string cloudName)
        {
            m_cloudName = cloudName;
        }

        public Url(string cloudName, ISignProvider signProvider)
            : this(cloudName)
        {
            m_signProvider = signProvider;
        }

        public string FormatValue { get; set; }

        public Url Shorten(bool shorten)
        {
            m_shorten = shorten;
            return this;
        }

        public Url CloudinaryAddr(string cloudinaryAddr)
        {
            m_cloudinaryAddr = cloudinaryAddr;
            return this;
        }

        public Url CloudName(string cloudName)
        {
            m_cloudName = cloudName;
            return this;
        }

        public Url Add(string part)
        {
            if (!String.IsNullOrEmpty(part))
                m_customParts.Add(part);

            return this;
        }

        public Url Action(string action)
        {
            m_action = action;
            return this;
        }

        public Url ApiVersion(string apiVersion)
        {
            m_apiVersion = apiVersion;
            return this;
        }

        public Url Version(string version)
        {
            m_version = version;
            return this;
        }

        public Url Signed(bool signed)
        {
            m_signed = signed;
            return this;
        }

        public Url ResourceType(string resourceType)
        {
            m_resourceType = resourceType;
            return this;
        }

        public Url Format(string format)
        {
            FormatValue = format;
            return this;
        }

        public Url SecureDistribution(string privateCdn)
        {
            m_privateCdn = privateCdn;
            return this;
        }

        public Url CName(string cName)
        {
            m_cName = cName;
            return this;
        }

        public Url Transform(Transformation transformation)
        {
            m_transformation = transformation;
            return this;
        }

        public Url Secure(bool secure = true)
        {
            m_secure = secure;
            return this;
        }

        public Url PrivateCdn(bool privateCdn)
        {
            m_usePrivateCdn = privateCdn;
            return this;
        }

        public Url CSubDomain(bool cSubDomain)
        {
            m_cSubDomain = cSubDomain;
            return this;
        }

        public Transformation Transformation
        {
            get
            {
                if (m_transformation == null) m_transformation = new Transformation();
                return m_transformation;
            }
        }

        public string BuildSpriteCss(string source)
        {
            m_action = "sprite";
            if (!source.EndsWith(".css")) FormatValue = "css";
            return BuildUrl(source);
        }

#if NET40
        public IHtmlString BuildImageTag(string source, params string[] keyValuePairs)
#else
        public string BuildImageTag(string source, params string[] keyValuePairs)
#endif
        {
            return BuildImageTag(source, new StringDictionary(keyValuePairs));
        }

#if NET40
        public IHtmlString BuildImageTag(string source, StringDictionary dict)
#else
        public string BuildImageTag(string source, StringDictionary dict)
#endif
        {
            string url = BuildUrl(source);

            if (!String.IsNullOrEmpty(m_transformation.HtmlWidth))
                dict.Add("width", m_transformation.HtmlWidth);

            if (!String.IsNullOrEmpty(m_transformation.HtmlHeight))
                dict.Add("height", m_transformation.HtmlHeight);

            StringBuilder sb = new StringBuilder();
            sb.Append("<img src='").Append(url).Append("'");
            foreach (var item in dict)
            {
                sb.Append(" ").Append(item.Key).Append("='").Append(item.Value).Append("'");
            }
            sb.Append("/>");

#if NET40
            return new HtmlString(sb.ToString());
#else
            return sb.ToString();
#endif
        }

        public string BuildUrl()
        {
            return BuildUrl(String.Empty);
        }

        public string BuildUrl(string source)
        {
            if (String.IsNullOrEmpty(m_cloudName))
                throw new ArgumentException("cloudName must be specified!");

            if (source == null) return null;

            if (m_action == "fetch" && !String.IsNullOrEmpty(FormatValue))
            {
                Transformation.FetchFormat(FormatValue);
                FormatValue = null;
            }

            string transformationStr = Transformation.Generate();

            if (Regex.IsMatch(source.ToLower(), "^https?:/.*"))
            {
                if (m_action == "upload" || m_action == "asset")
                {
                    return source;
                }
                source = Encode(source);
            }
            else
            {
                source = Encode(Decode(source));
                if (!String.IsNullOrEmpty(FormatValue))
                {
                    source += "." + FormatValue;
                }
            }

            string prefix;
            bool sharedDomain = !m_usePrivateCdn;
            string privateCdn = m_privateCdn;
            if (m_secure)
            {
                if (String.IsNullOrEmpty(privateCdn) || Cloudinary.OLD_AKAMAI_SHARED_CDN == privateCdn)
                {
                    privateCdn = m_usePrivateCdn ? m_cloudName + "-res.cloudinary.com" : Cloudinary.SHARED_CDN;
                }
                sharedDomain |= privateCdn == Cloudinary.SHARED_CDN;
                prefix = String.Format("https://{0}", privateCdn);
            }
            else
            {
                if (Regex.IsMatch(m_cloudinaryAddr.ToLower(), "^https?:/.*"))
                {
                    prefix = m_cloudinaryAddr;
                }
                else
                {
                    uint hash = Crc32.ComputeChecksum(Encoding.UTF8.GetBytes(source));
                    string subDomain = m_cSubDomain ? "a" + ((hash % 5 + 5) % 5 + 1) + "." : String.Empty;
                    string host = m_cName != null ? m_cName : (m_usePrivateCdn ? m_cloudName + "-" : String.Empty) + m_cloudinaryAddr;

                    prefix = "http://" + subDomain + host;
                }
            }

            List<string> urlParts = new List<string>(new string[] { prefix });
            if (!String.IsNullOrEmpty(m_apiVersion))
            {
                urlParts.Add(m_apiVersion);
                urlParts.Add(m_cloudName);
            }
            else if (sharedDomain)
            {
                urlParts.Add(m_cloudName);
            }

            if (m_shorten && m_resourceType == "image" && m_action == "upload")
            {
                m_resourceType = String.Empty;
                m_action = "iu";
            }

            urlParts.Add(m_resourceType);
            urlParts.Add(m_action);
            urlParts.AddRange(m_customParts);

            if (source.Contains("/") && !Regex.IsMatch(source, "^v[0-9]+/") && !Regex.IsMatch(source, "https?:/.*") && String.IsNullOrEmpty(m_version))
            {
                m_version = "1";
            }

            var version = String.IsNullOrEmpty(m_version) ? String.Empty : String.Format("v{0}", m_version);

            if (m_signed)
            {
                if (m_signProvider == null)
                    throw new NullReferenceException("Reference to ISignProvider-compatible object must be provided in order to sign URI!");

                var signedPart = String.Join("/", new string[] { transformationStr, version, source });
                signedPart = Regex.Replace(signedPart, "^/+", String.Empty);
                signedPart = Regex.Replace(signedPart, "([^:])/{2,}", "$1/");
                signedPart = Regex.Replace(signedPart, "/$", String.Empty);

                signedPart = m_signProvider.SignUriPart(signedPart);
                urlParts.Add(signedPart);
            }

            urlParts.Add(transformationStr);
            urlParts.Add(version);
            urlParts.Add(source);

            string uriStr = String.Join("/", urlParts.ToArray());
            uriStr = Regex.Replace(uriStr, "([^:])/{2,}", "$1/");
            uriStr = Regex.Replace(uriStr, "/$", String.Empty);

            return uriStr;
        }

        private static string Decode(string input)
        {
            StringBuilder resultStr = new StringBuilder();

            int pos = 0;

            while (pos < input.Length)
            {
                int ppos = input.IndexOf('%', pos);
                if (ppos == -1)
                {
                    resultStr.Append(input.Substring(pos));
                    pos = input.Length;
                }
                else
                {
                    resultStr.Append(input.Substring(pos, ppos - pos));
                    char ch = (char)Int16.Parse(input.Substring(ppos + 1, 2), NumberStyles.HexNumber);
                    resultStr.Append(ch);
                    pos = ppos + 3;
                }
            }

            return resultStr.ToString();
        }

        private static string Encode(string input)
        {
            StringBuilder resultStr = new StringBuilder();
            foreach (char ch in input)
            {
                if (IsUnsafe(ch))
                {
                    resultStr.Append('%');
                    resultStr.Append(String.Format("{0:x2}", (short)ch));
                }
                else
                {
                    resultStr.Append(ch);
                }
            }
            return resultStr.ToString();
        }

        private static bool IsUnsafe(char ch)
        {
            if (ch > 128 || ch < 0)
                return true;

            return " $&+,;=?@<>#%".IndexOf(ch) >= 0;
        }

        #region ICloneable

        public Url Clone()
        {
            Url newUrl = (Url)this.MemberwiseClone();

            if (m_transformation != null)
                newUrl.m_transformation = this.m_transformation.Clone();
            else
                newUrl.m_transformation = null;

            newUrl.m_customParts = new List<string>();
            foreach (var part in m_customParts)
            {
                newUrl.m_customParts.Add(part);
            }

            return newUrl;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }

    public class UrlBuilder : UriBuilder
    {
        StringDictionary m_queryString = null;

        public StringDictionary QueryString
        {
            get
            {
                if (m_queryString == null)
                {
                    m_queryString = new StringDictionary();
                }

                return m_queryString;
            }
        }

        public string PageName
        {
            get
            {
                string path = base.Path;
                return path.Substring(path.LastIndexOf("/") + 1);
            }
            set
            {
                string path = base.Path;
                path = path.Substring(0, path.LastIndexOf("/"));
                base.Path = string.Concat(path, "/", value);
            }
        }

        public UrlBuilder()
            : base()
        {
        }

        public UrlBuilder(string uri)
            : base(uri)
        {
            PopulateQueryString();
        }

        public UrlBuilder(string uri, IDictionary<string, object> @params)
            : base(uri)
        {
            PopulateQueryString();
            SetParameters(@params);
        }

        public UrlBuilder(Uri uri)
            : base(uri)
        {
            PopulateQueryString();
        }

        public UrlBuilder(string schemeName, string hostName)
            : base(schemeName, hostName)
        {
        }

        public UrlBuilder(string scheme, string host, int portNumber)
            : base(scheme, host, portNumber)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string pathValue)
            : base(scheme, host, port, pathValue)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string path, string extraValue)
            : base(scheme, host, port, path, extraValue)
        {
        }

        public UrlBuilder(System.Web.UI.Page page)
            : base(page.Request.Url.AbsoluteUri)
        {
            PopulateQueryString();
        }

        public void SetParameters(IDictionary<string, object> @params)
        {
            foreach (var param in @params)
            {
                if (param.Value is IEnumerable<string>)
                {
                    foreach (var s in (IEnumerable<string>)param.Value)
                    {
                        QueryString.Add(param.Key + "[]", s);
                    }
                }
                else
                {
                    QueryString[param.Key] = param.Value.ToString();
                }
            }
        }

        public new string ToString()
        {
            BuildQueryString();

            return base.Uri.AbsoluteUri;
        }

        public void Navigate()
        {
            _Navigate(true);
        }

        public void Navigate(bool endResponse)
        {
            _Navigate(endResponse);
        }

        private void _Navigate(bool endResponse)
        {
            string uri = this.ToString();
            HttpContext.Current.Response.Redirect(uri, endResponse);
        }

        private void PopulateQueryString()
        {
            string query = base.Query;

            if (query == string.Empty || query == null)
            {
                return;
            }

            if (m_queryString == null)
            {
                m_queryString = new StringDictionary();
            }

            m_queryString.Clear();

            query = query.Substring(1); //remove the ?

            string[] pairs = query.Split(new char[] { '&' });
            foreach (string s in pairs)
            {
                string[] pair = s.Split(new char[] { '=' });

                m_queryString[pair[0]] = (pair.Length > 1) ? pair[1] : string.Empty;
            }
        }

        private void BuildQueryString()
        {
            if (m_queryString == null) return;

            int count = m_queryString.Count;

            if (count == 0)
            {
                base.Query = string.Empty;
                return;
            }

            string[] keys = new string[count];
            string[] values = new string[count];
            string[] pairs = new string[count];

            m_queryString.Keys.CopyTo(keys, 0);
            m_queryString.Values.CopyTo(values, 0);

            for (int i = 0; i < count; i++)
            {
                pairs[i] = string.Concat(keys[i], "=", values[i]);
            }

            base.Query = string.Join("&", pairs);
        }
    }

    public static class Crc32
    {
        static uint[] table;

        public static uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        public static byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        static Crc32()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }
    }
}
