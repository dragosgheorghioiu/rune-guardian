using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum Language
{
    English = 0,
    Romanian = 1
}

public class TranslationSystem
{
    static List<string> _languages = new List<string>() { "en", "ro" };

    static public TranslatableText CreateTranslatableText(string text)
    {
        return new TranslatableText(text);
    }

    static public TranslatableText CreateTranslatableText(List<TranslatableText.TextLanguageOption> textLanguageOptions)
    {
        return new TranslatableText(textLanguageOptions);
    }

    static public string Translate(TranslatableText translatableText, Language language)
    {
        if (translatableText == null) {
            return null;
        }

        if (translatableText._defaultText != null) {
            return translatableText._defaultText;
        }

        if (translatableText._defaultText == null) {
            if (translatableText._textLanguageOptions.Find(x => x.language == language) == null) {
                return "Error!! No explicit translation!";
            }

            return translatableText._textLanguageOptions.Find(x => x.language == language).value;
        }

        // TODO: Error
        return null;
    }
}
