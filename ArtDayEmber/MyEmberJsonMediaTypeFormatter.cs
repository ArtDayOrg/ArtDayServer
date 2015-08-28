using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace ArtDayEmber
{
    public class MyEmberJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        //private int _maxDepth = 256;
        private JsonSerializerSettings _jsonSerializerSettings;
        public MyEmberJsonMediaTypeFormatter()
        {
            _jsonSerializerSettings = CreateDefaultSerializerSettings();
        }
        //public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        //{
        //    var root = GetRootFieldName(type, value);
        //    var obj = new ExpandoObject() as IDictionary<string, Object>;
        //    obj[root] = value;
        //    return base.WriteToStreamAsync(type, obj as object, writeStream, content, transportContext);
        //}

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.FromResult(ReadFromStream(type, readStream, content, formatterLogger)); ;
        }

        private object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var root = GetRootFieldName(type);
            var contentHeaders = content == null ? null : content.Headers;

            // If content length is 0 then return default value for this type
            if (contentHeaders != null && contentHeaders.ContentLength == 0)
                return GetDefaultValueForType(type);

            // Get the character encoding for the content
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);

            try
            {
                using (var reader = (new StreamReader(readStream, effectiveEncoding)))
                {
                    var json = reader.ReadToEnd();
                    var jo = JObject.Parse(json);
                    return jo.SelectToken(root.ToLower(), false).ToObject(type);
                }
            }
            catch (Exception e)
            {
                if (formatterLogger == null)
                {
                    throw;
                }
                formatterLogger.LogError(String.Empty, e);
                return GetDefaultValueForType(type);
            }
        }

        private string GetRootFieldName(Type type, dynamic value = null)
        {
            //get element type if array
            if (value != null && (value is IEnumerable || type.IsArray))
                type = value[0].GetType();

            var attrs = type.CustomAttributes.Where(x => x.AttributeType == typeof(Newtonsoft.Json.JsonObjectAttribute)).ToList();

            if (attrs.Any())
            {
                var titles = attrs.First().NamedArguments.Where(arg => arg.MemberName == "Title")
                    .Select(arg => arg.TypedValue.Value.ToString()).ToList();
                if (titles.Any()) return titles.First();
            }

            return type.Name;
        }
    }
}