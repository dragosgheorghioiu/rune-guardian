using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TranslatableTextConverter : JsonConverter
{
    public class TextLanguageOption
    {
        public string language { get; set; }
        public string value { get; set; }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TranslatableText);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);

        if (token.Type == JTokenType.String) {
            return TranslationSystem.CreateTranslatableText(token.ToString());
        }

        if (token.Type == JTokenType.Array) {
            var textLanguageOptions = token.ToObject<List<TextLanguageOption>>();

            var ttTextLanguageOptions = new List<TranslatableText.TextLanguageOption>();
            foreach (var textLanguageOption in textLanguageOptions) {
                ttTextLanguageOptions.Add(new TranslatableText.TextLanguageOption() {
                    language = textLanguageOption.language == "en" ? Language.English : Language.Romanian,
                    value = textLanguageOption.value
                });
            }

            return TranslationSystem.CreateTranslatableText(ttTextLanguageOptions);
        }

        throw new JsonSerializationException("Invalid format for TranslatableText field.");

        //return null;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        //var translatableText = (TranslatableText)value;

        //if (translatableText.Texts.Count == 1 && translatableText.Texts[0].language == "default")
        //{
        //    writer.WriteValue(translatableText.Texts[0].value);
        //}
        //else
        //{
        //    serializer.Serialize(writer, translatableText.Texts);
        //}
    }
}
