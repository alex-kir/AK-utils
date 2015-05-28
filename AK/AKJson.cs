//
//  AKJson.cs
//
//  Utilities and wrappers: https://github.com/alex-kir/AK-utils
//
//  Copyright (c) 2015 - Alexander Kirienko
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;

#if !NETFX_CORE
using System.Reflection;
#else
using WindowsStore.Compatibility.Reflection;
#endif

public class AKJson
{
    public class Serializable : Attribute
    {
        public string Name;
    }

    private const int TOKEN_NONE = 0;
    private const int TOKEN_CURLY_OPEN = 1;
    private const int TOKEN_CURLY_CLOSE = 2;
    private const int TOKEN_SQUARED_OPEN = 3;
    private const int TOKEN_SQUARED_CLOSE = 4;
    private const int TOKEN_COLON = 5;
    private const int TOKEN_COMMA = 6;
    private const int TOKEN_STRING = 7;
    private const int TOKEN_NUMBER = 8;
    private const int TOKEN_TRUE = 9;
    private const int TOKEN_FALSE = 10;
    private const int TOKEN_NULL = 11;

    private const int BUILDER_CAPACITY = 2000;

    private int lastErrorIndex = -1;
    private string lastDecode = "";

    public static object Decode(string json)
    {
        return new AKJson().JsonDecode(json);
    }

    public static string Encode(object json, bool checkAttrs = false)
    {
        if (checkAttrs)
            json = PrepeareObject(json);
        return new AKJson().JsonEncode(json);
    }
        
    private object JsonDecode(string json)
    {
        // save the string for debug information
        lastDecode = json;

        if (!string.IsNullOrEmpty(json))
        {
            char[] charArray = json.ToCharArray();
			if (charArray.Length == 0)
				return null;
			
            int index = 0;
            bool success = true;
            object value = ParseValue(charArray, ref index, ref success);
            if (success)
            {
                lastErrorIndex = -1;
            }
            else
            {
                lastErrorIndex = index;
            }
            return value;
        }
        else
        {
            return null;
        }
    }
        
    private string JsonEncode(object json)
    {
        StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
        bool success = SerializeValue(json, builder);
        return (success ? builder.ToString() : null);
    }
        
    private bool LastDecodeSuccessful()
    {
        return (lastErrorIndex == -1);
    }
        
    private int GetLastErrorIndex()
    {
        return lastErrorIndex;
    }
        
    private string GetLastErrorSnippet()
    {
        if (lastErrorIndex == -1)
        {
            return "";
        }
        else
        {
            int startIndex = lastErrorIndex - 5;
            int endIndex = lastErrorIndex + 15;
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            if (endIndex >= lastDecode.Length)
            {
                endIndex = lastDecode.Length - 1;
            }

            return lastDecode.Substring(startIndex, endIndex - startIndex + 1);
        }
    }

    private Dictionary<object, object> ParseObject(char[] json, ref int index)
    {
        Dictionary<object, object> dict = new Dictionary<object, object>();
        int token;

        // {
        NextToken(json, ref index);

        bool done = false;
        while (!done)
        {
            token = LookAhead(json, index);
            if (token == AKJson.TOKEN_NONE)
            {
                return null;
            }
            else if (token == AKJson.TOKEN_COMMA)
            {
                NextToken(json, ref index);
            }
            else if (token == AKJson.TOKEN_CURLY_CLOSE)
            {
                NextToken(json, ref index);
                return dict;
            }
            else
            {

                // name
                string name = ParseString(json, ref index);
                if (name == null)
                {
                    return null;
                }

                // :
                token = NextToken(json, ref index);
                if (token != AKJson.TOKEN_COLON)
                {
                    return null;
                }

                // value
                bool success = true;
                object value = ParseValue(json, ref index, ref success);
                if (!success)
                {
                    return null;
                }

                dict[name] = value;
            }
        }

        return dict;
    }

    private List<object> ParseArray(char[] json, ref int index)
    {
        List<object> list = new List<object>();

        // [
        NextToken(json, ref index);

        bool done = false;
        while (!done)
        {
            int token = LookAhead(json, index);
            if (token == AKJson.TOKEN_NONE)
            {
                return null;
            }
            else if (token == AKJson.TOKEN_COMMA)
            {
                NextToken(json, ref index);
            }
            else if (token == AKJson.TOKEN_SQUARED_CLOSE)
            {
                NextToken(json, ref index);
                break;
            }
            else
            {
                bool success = true;
                object value = ParseValue(json, ref index, ref success);
                if (!success)
                {
                    return null;
                }

                list.Add(value);
            }
        }

        return list;
    }

    private object ParseValue(char[] json, ref int index, ref bool success)
    {
        switch (LookAhead(json, index))
        {
            case AKJson.TOKEN_STRING:
                return ParseString(json, ref index);
            case AKJson.TOKEN_NUMBER:
                return ParseNumber(json, ref index);
            case AKJson.TOKEN_CURLY_OPEN:
                return ParseObject(json, ref index);
            case AKJson.TOKEN_SQUARED_OPEN:
                return ParseArray(json, ref index);
            case AKJson.TOKEN_TRUE:
                NextToken(json, ref index);
                return Boolean.Parse("TRUE");
            case AKJson.TOKEN_FALSE:
                NextToken(json, ref index);
                return Boolean.Parse("FALSE");
            case AKJson.TOKEN_NULL:
                NextToken(json, ref index);
                return null;
            case AKJson.TOKEN_NONE:
                break;
        }

        success = false;
        return null;
    }

    private string ParseString(char[] json, ref int index)
    {
        StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
        char c;

        EatWhitespace(json, ref index);

        // "
        c = json[index++];

        bool complete = false;
        while (!complete)
        {

            if (index == json.Length)
            {
                break;
            }

            c = json[index++];
            if (c == '"')
            {
                complete = true;
                break;
            }
            else if (c == '\\')
            {

                if (index == json.Length)
                {
                    break;
                }
                c = json[index++];
                if (c == '"')
                {
                    s.Append('"');
                }
                else if (c == '\\')
                {
                    s.Append('\\');
                }
                else if (c == '/')
                {
                    s.Append('/');
                }
                else if (c == 'b')
                {
                    s.Append('\b');
                }
                else if (c == 'f')
                {
                    s.Append('\f');
                }
                else if (c == 'n')
                {
                    s.Append('\n');
                }
                else if (c == 'r')
                {
                    s.Append('\r');
                }
                else if (c == 't')
                {
                    s.Append('\t');
                }
                else if (c == 'u')
                {
                    int remainingLength = json.Length - index;
                    if (remainingLength >= 4)
                    {
                        // fetch the next 4 chars
                        char[] unicodeCharArray = new char[4];
                        Array.Copy(json, index, unicodeCharArray, 0, 4);
                        // parse the 32 bit hex into an integer codepoint
                        uint codePoint = UInt32.Parse(new string(unicodeCharArray), NumberStyles.HexNumber);
                        // convert the integer codepoint to a unicode char and add to string
                        s.Append(Char.ConvertFromUtf32((int)codePoint));
                        // skip 4 chars
                        index += 4;
                    }
                    else
                    {
                        break;
                    }
                }

            }
            else
            {
                s.Append(c);
            }

        }

        if (!complete)
        {
            return null;
        }

        return s.ToString();
    }

    private double ParseNumber(char[] json, ref int index)
    {
        EatWhitespace(json, ref index);

        int lastIndex = GetLastIndexOfNumber(json, index);
        int charLength = (lastIndex - index) + 1;
        char[] numberCharArray = new char[charLength];

        Array.Copy(json, index, numberCharArray, 0, charLength);
        index = lastIndex + 1;
        return Double.Parse(new string(numberCharArray), CultureInfo.InvariantCulture);
    }

    private int GetLastIndexOfNumber(char[] json, int index)
    {
        int lastIndex;
        for (lastIndex = index; lastIndex < json.Length; lastIndex++)
        {
            if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
            {
                break;
            }
        }
        return lastIndex - 1;
    }

    private void EatWhitespace(char[] json, ref int index)
    {
        for (; index < json.Length; index++)
        {
            if (" \t\n\r".IndexOf(json[index]) == -1)
            {
                break;
            }
        }
    }

    private int LookAhead(char[] json, int index)
    {
        int saveIndex = index;
        return NextToken(json, ref saveIndex);
    }

    private int NextToken(char[] json, ref int index)
    {
        EatWhitespace(json, ref index);

        if (index == json.Length)
        {
            return AKJson.TOKEN_NONE;
        }

        char c = json[index];
        index++;
        switch (c)
        {
            case '{':
                return AKJson.TOKEN_CURLY_OPEN;
            case '}':
                return AKJson.TOKEN_CURLY_CLOSE;
            case '[':
                return AKJson.TOKEN_SQUARED_OPEN;
            case ']':
                return AKJson.TOKEN_SQUARED_CLOSE;
            case ',':
                return AKJson.TOKEN_COMMA;
            case '"':
                return AKJson.TOKEN_STRING;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case '-':
                return AKJson.TOKEN_NUMBER;
            case ':':
                return AKJson.TOKEN_COLON;
        }
        index--;

        int remainingLength = json.Length - index;

        // false
        if (remainingLength >= 5)
        {
            if (json[index] == 'f' &&
                json[index + 1] == 'a' &&
                json[index + 2] == 'l' &&
                json[index + 3] == 's' &&
                json[index + 4] == 'e')
            {
                index += 5;
                return AKJson.TOKEN_FALSE;
            }
        }

        // true
        if (remainingLength >= 4)
        {
            if (json[index] == 't' &&
                json[index + 1] == 'r' &&
                json[index + 2] == 'u' &&
                json[index + 3] == 'e')
            {
                index += 4;
                return AKJson.TOKEN_TRUE;
            }
        }

        // null
        if (remainingLength >= 4)
        {
            if (json[index] == 'n' &&
                json[index + 1] == 'u' &&
                json[index + 2] == 'l' &&
                json[index + 3] == 'l')
            {
                index += 4;
                return AKJson.TOKEN_NULL;
            }
        }

        return AKJson.TOKEN_NONE;
    }

    private bool SerializeObjectOrArray(object objectOrArray, StringBuilder builder)
    {
        if (objectOrArray is Dictionary<object, object>)
        {
            return SerializeObject((Dictionary<object, object>)objectOrArray, builder);
        }
        else if (objectOrArray is List<object>)
        {
            return SerializeArray((List<object>)objectOrArray, builder);
        }
        else
        {
            return false;
        }
    }

    private bool SerializeObject(IDictionary anObject, StringBuilder builder)
    {
        builder.Append("{");

        IDictionaryEnumerator e = anObject.GetEnumerator();
        bool first = true;
        while (e.MoveNext())
        {
            string key = e.Key.ToString();
            object value = e.Value;

            if (!first)
            {
                builder.Append(", ");
            }

            SerializeString(key, builder);
            builder.Append(":");
            if (!SerializeValue(value, builder))
            {
                return false;
            }

            first = false;
        }

        builder.Append("}");
        return true;
    }

    private bool SerializeArray(ICollection anArray, StringBuilder builder)
    {
        builder.Append("[");

        bool first = true;
        //		int i = 0;
        foreach (object value in anArray)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            if (!SerializeValue(value, builder))
            {
                return false;
            }

            first = false;
        }

        builder.Append("]");
        return true;
    }

    private bool SerializeValue(object value, StringBuilder builder)
    {
        if (value is string)
        {
            SerializeString((string)value, builder);
        }
        else if (value is IDictionary)
        {
            SerializeObject((IDictionary)value, builder);
        }
        else if (value is ICollection)
        {
            SerializeArray((ICollection)value, builder);
        }
        else if (IsNumeric(value))
        {
            SerializeNumber(Convert.ToDouble(value), builder);
        }
        else if ((value is Boolean) && ((Boolean)value == true))
        {
            builder.Append("true");
        }
        else if ((value is Boolean) && ((Boolean)value == false))
        {
            builder.Append("false");
        }
        else if (value == null)
        {
            builder.Append("null");
        }
        else
        {
            return false;
        }
        return true;
    }

    private void SerializeString(string aString, StringBuilder builder)
    {
        builder.Append("\"");

        char[] charArray = aString.ToCharArray();
        for (int i = 0; i < charArray.Length; i++)
        {
            char c = charArray[i];
            if (c == '"')
            {
                builder.Append("\\\"");
            }
            else if (c == '\\')
            {
                builder.Append("\\\\");
            }
            else if (c == '\b')
            {
                builder.Append("\\b");
            }
            else if (c == '\f')
            {
                builder.Append("\\f");
            }
            else if (c == '\n')
            {
                builder.Append("\\n");
            }
            else if (c == '\r')
            {
                builder.Append("\\r");
            }
            else if (c == '\t')
            {
                builder.Append("\\t");
            }
            else
            {
                int codepoint = Convert.ToInt32(c);
                if ((codepoint >= 32) && (codepoint <= 126))
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                }
            }
        }

        builder.Append("\"");
    }

    private void SerializeNumber(double number, StringBuilder builder)
    {
        builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
    }
        
    private bool IsNumeric(object o)
    {
        try
        {
            Double.Parse(o.ToString());
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    #region Decode<T>

	public static T Decode<T>(string json) where T : class
	{
		object obj = new AKJson().JsonDecode(json);
		return (T)TransformObject(obj, typeof(T));
	}

    public static T DecodeOrNull<T>(string json) where T : class
    {
        try
        {
            object obj = new AKJson().JsonDecode(json);
            if (obj == null)
                return null;
			return (T)TransformObject(obj, typeof(T));
        }
        catch
        {
            return null;
        }
    }

	public static T TransformObject<T>(object source)
	{
		return (T)TransformObject(source, typeof(T));
	}

	private static object TransformObject(object source, Type type)
    {
        if (source == null)
            return source;

#if !NETFX_CORE
		bool isGeneric = type.IsGenericType;
#else
		bool isGeneric = type.IsGenericType();
#endif
        {
            var dictionary = (source as IDictionary);
            if (dictionary != null && isGeneric && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];
                var ret = (IDictionary)Activator.CreateInstance(type);
                foreach (var key in dictionary.Keys)
                {
					ret.Add(TransformObject(key, keyType), TransformObject(dictionary[key], valueType));
                }
                return ret;
            }

            object[] typeAttributes = type.GetCustomAttributes(typeof(AKJson.Serializable), true);
            if (dictionary != null && typeAttributes.Length > 0)
            {
                var ret = Activator.CreateInstance(type);

                // --- Scan Properties ---
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty))
                {
                    var propertyAttributes = property.GetCustomAttributes(typeof(AKJson.Serializable), true);
                    if (propertyAttributes.Length > 0 && dictionary.Contains(property.Name))
                    {
                        var val = TransformObject(dictionary[property.Name], property.PropertyType);
                        property.SetValue(ret, val, null);
                    }
                }

                // --- Scan Fields ---
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var fieldAttributes = field.GetCustomAttributes(typeof(AKJson.Serializable), true);
                    if (fieldAttributes.Length > 0 && dictionary.Contains(field.Name))
                    {
                        var val = TransformObject(dictionary[field.Name], field.FieldType);
                        field.SetValue(ret, val);
                    }
                }

                return ret;
            }

        }
        //-----------------------------------------
        {
            var list = source as IEnumerable;
            if (list != null && isGeneric && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type itemType = type.GetGenericArguments()[0];
                var ret = (IList)Activator.CreateInstance(type);
                foreach (object item in list)
                {
					ret.Add(TransformObject(item, itemType));
                }
                return ret;
            }
            if (list is IList && type.IsArray)
            {
                int count = ((IList)list).Count;
                var ret = (Array)Activator.CreateInstance(type, new object[]{ count });
                var valueType = type.GetElementType();
                int index = 0;
                foreach (var item in list)
                {
                    ret.SetValue(TransformObject(item, valueType), index);
                    index++;
                }
                return ret;
            }
        }
        //-----------------------------------------
        if (type == typeof(string))
        {
            return source as string;
        }
        //-----------------------------------------
        if (type == typeof(bool))
        {
            return Convert.ToBoolean(source);
        }
        //-----------------------------------------
		if (type == typeof(int))
        {
			return Convert.ToInt32(source);
        }
		//-----------------------------------------
		if (type == typeof(float))
		{
			return Convert.ToSingle(source);
		}
		//-----------------------------------------
		if (type == typeof(double))
		{
			return Convert.ToDouble(source);
		}
        //-----------------------------------------
        if (type == typeof(object))
        {
            return source;
        }
        
        throw new ArgumentException("AKJson.TransfomObject() source '" + source.GetType().Name + "' type is not convertable to '" + (type == null ? "null" : type.Name), "' type");
    }

    private static object PrepeareObject(object source)
    {
        if (source == null)
            return null;

        //-----------------------------------------
        object[] typeAttributes = source.GetType().GetCustomAttributes(typeof(AKJson.Serializable), true);
        if (typeAttributes.Length > 0)
        {
            var ret = new Hashtable();

            // --- Scan Properties ---
            foreach (var property in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                var propertyAttributes = property.GetCustomAttributes(typeof(AKJson.Serializable), true);
                if (propertyAttributes.Length > 0)
                {
                    ret[property.Name] = PrepeareObject(property.GetValue(source, null));
                }
            }

            // --- Scan Fields ---
            foreach (var field in source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldAttributes = field.GetCustomAttributes(typeof(AKJson.Serializable), true);
                if (fieldAttributes.Length > 0)
                {
                    ret[field.Name] = PrepeareObject(field.GetValue(source));
                }
            }

            return ret;
        }
        //-----------------------------------------
        {
            var dictionary = (source as IDictionary);
            if (dictionary != null)
            {
                var ret = new Hashtable();
                foreach (var key in dictionary.Keys)
                    ret.Add(PrepeareObject(key), PrepeareObject(dictionary[key]));
                return ret;
            }
        }
        //-----------------------------------------
        if (source is string)
        {
            return source as string;
        }
        //-----------------------------------------
        {
            var list = source as IEnumerable;
            if (list != null)
            {
                var ret = new ArrayList();
                foreach (object item in list)
                    ret.Add(PrepeareObject(item));
                return ret;
            }
        }
        return source;
    }

    #endregion

}

