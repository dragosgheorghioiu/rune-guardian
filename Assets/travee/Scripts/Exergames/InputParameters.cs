using Newtonsoft.Json;
using System.Collections.Generic;

public class InputParameters
{
    public List<InputParameter> inputParameters { get; set; }
    public List<InputParameterCategory> inputParameterCategories { get; set; }
    public List<InputButton> buttons { get; set; }

    //public InputParameters(string inputParametersS)
    //{

    //}
}

public class InputButton
{
    public string name;
    public string text;
}

public class InputParameter
{
    public string name;
    [JsonConverter(typeof(TranslatableTextConverter))]
    public TranslatableText text;
    [JsonConverter(typeof(TranslatableTextConverter))]
    public TranslatableText finalText;
    public string type;
    public string defaultValue;
    public List<ListOption> value;

    public string minName;
    public string maxName;
    public string minDefaultValue;
    public string maxDefaultValue;

    public string minValue;
    public string maxValue;

    public bool isImplicit;
    public bool? runtimeUpdate;
    public string categoryName;

    public bool isHomeUseOnly;

    //public bool? allowHomeUseAdjustment;
    //public string homeUseText;
}

public class ListOption
{
    [JsonConverter(typeof(TranslatableTextConverter))]
    public TranslatableText text;
    public string value;
}

public class InputParameterCategory
{
    public string name { get; set; }
    [JsonConverter(typeof(TranslatableTextConverter))]
    public TranslatableText text;
}

public class WorkInputParameter
{
    public string name;
    public TranslatableText text;
    public TranslatableText finalText;
    public string type;
    public int defaultValue;
    public List<ListOption> value;

    public string minName;
    public string maxName;
    public int minDefaultValue;
    public int maxDefaultValue;

    public int minValue;
    public int maxValue;

    public bool isImplicit;
    public bool? runtimeUpdate;
    public string categoryName;

    public bool isHomeUseOnly;

    //public bool? allowHomeUseAdjustment;
    //public string homeUseText;

    //public WorkInputParameter() { }

    public WorkInputParameter(InputParameter inputParameter)
    {
        name = inputParameter.name;
        text = inputParameter.text;
        finalText = inputParameter.finalText;
        type = inputParameter.type;
        value = inputParameter.value;

        int.TryParse(inputParameter.minValue, out minValue);
        int.TryParse(inputParameter.maxValue, out maxValue);
        int.TryParse(inputParameter.defaultValue, out defaultValue);

        isImplicit = inputParameter.isImplicit;
        runtimeUpdate = inputParameter.runtimeUpdate;
        categoryName = inputParameter.categoryName;

        isHomeUseOnly = inputParameter.isHomeUseOnly;

        //allowHomeUseAdjustment = inputParameter.allowHomeUseAdjustment;
        //homeUseText = inputParameter.homeUseText;
    }
}

public class WorkListOption
{
    public string text;
    public int value;
}
