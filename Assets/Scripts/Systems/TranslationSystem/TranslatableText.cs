using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TranslatableText
{
    public string _defaultText;

    public class TextLanguageOption
    {
        public Language language { get; set; }
        public string value { get; set; }
    }

    public List<TextLanguageOption> _textLanguageOptions;

    public TranslatableText(string defaultText)
    {
        _defaultText = defaultText;
        _textLanguageOptions = null;
    }

    public TranslatableText(List<TextLanguageOption> textLanguageOptions)
    {
        _defaultText = null;
        _textLanguageOptions = textLanguageOptions;
    }
}
