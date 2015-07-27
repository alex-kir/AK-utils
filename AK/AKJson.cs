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
#define PCL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

#if !NETFX_CORE
using System.Reflection;
#else
using WindowsStore.Compatibility.Reflection;
#endif

public class AKJson
{
    public class Serializable : Attribute
    {
    }
    char[] unicodeCharArray = new char[4];

    #region Encoding

    public static string Encode(object json)
    {
        json = PrepeareForEncoding(json);
        return new AKJson().EncodeJson(json);
    }

    private string EncodeJson(object json)
    {
        StringBuilder builder = new StringBuilder();
        bool success = EncodeValue(json, builder);
        return (success ? builder.ToString() : null);
    }

    private static object PrepeareForEncoding(object source)
    {
        if (source == null)
            return null;

        //-----------------------------------------
        if (source.GetType().HasCustomAttribute(typeof(AKJson.Serializable)))
        {
            var ret = new Dictionary<string, object>();

            // --- Scan Properties ---
            foreach (var property in source.GetType().GetRuntimeProperties())
            {
                var propertyAttributes = property.GetCustomAttributes(typeof(AKJson.Serializable), true);
                if (propertyAttributes.FirstOrDefault() != null)
                {
                    ret[property.Name] = PrepeareForEncoding(property.GetValue(source, null));
                }
            }

            // --- Scan Fields ---
            foreach (var field in source.GetType().GetRuntimeFields())
            {
                var fieldAttributes = field.GetCustomAttributes(typeof(AKJson.Serializable), true);
                if (fieldAttributes.FirstOrDefault() != null)
                {
                    ret[field.Name] = PrepeareForEncoding(field.GetValue(source));
                }
            }

            return ret;
        }
        //-----------------------------------------
        {
            var dictionary = (source as IDictionary);
            if (dictionary != null)
            {
                var ret = new Dictionary<string, object>();
                foreach (var key in dictionary.Keys)
                    ret.Add(Convert.ToString(key), PrepeareForEncoding(dictionary[key]));
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
                var ret = new List<object>();
                foreach (object item in list)
                    ret.Add(PrepeareForEncoding(item));
                return ret;
            }
        }
        return source;
    }

    private bool EncodeValue(object value, StringBuilder builder)
    {
        if (value is string)
        {
            EncodeString((string)value, builder);
        }
        else if (value is IDictionary)
        {
            EncodeObject((IDictionary)value, builder);
        }
        else if (value is ICollection)
        {
            EncodeArray((ICollection)value, builder);
        }
        else if (IsNumeric(value))
        {
            EncodeNumber(Convert.ToDouble(value), builder);
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

    private bool EncodeObject(IDictionary anObject, StringBuilder builder)
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

            EncodeString(key, builder);
            builder.Append(":");
            if (!EncodeValue(value, builder))
            {
                return false;
            }

            first = false;
        }

        builder.Append("}");
        return true;
    }

    private bool EncodeArray(ICollection anArray, StringBuilder builder)
    {
        builder.Append("[");

        bool first = true;
        //      int i = 0;
        foreach (object value in anArray)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            if (!EncodeValue(value, builder))
            {
                return false;
            }

            first = false;
        }

        builder.Append("]");
        return true;
    }

    private void EncodeString(string aString, StringBuilder builder)
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

    private void EncodeNumber(double number, StringBuilder builder)
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

    #endregion

    #region Decoding


    public static T TransformObject<T>(object source)
    {
        return (T)TransformObject(source, typeof(T));
    }

    private static object TransformObject(object source, Type type)
    {
        if (type == null)
            throw new ArgumentNullException("type", "Transform object failed");
        if (source == null)
            return source;

        bool isGeneric = type.IsGenericType();

        //-----------------------------------------
        if (isGeneric && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var dictionary = (source as IDictionary);
            Type keyType = type.GetGenericTypeArgumentAt(0);
            Type valueType = type.GetGenericTypeArgumentAt(1);
            var ret = (IDictionary)Activator.CreateInstance(type);
            foreach (var key in dictionary.Keys)
            {
                ret.Add(TransformObject(key, keyType), TransformObject(dictionary[key], valueType));
            }
            return ret;
        }

        if (type.HasCustomAttribute(typeof(AKJson.Serializable)))
        {
            var dictionary = (source as IDictionary);
            var ret = Activator.CreateInstance(type);

            // --- Scan Properties ---
            foreach (var property in type.GetRuntimeProperties())
            {
                var propertyAttributes = property.GetCustomAttributes(typeof(AKJson.Serializable), true);
                if (propertyAttributes.FirstOrDefault() != null && dictionary.Contains(property.Name))
                {
                    var val = TransformObject(dictionary[property.Name], property.PropertyType);
                    property.SetValue(ret, val, null);
                }
            }

            // --- Scan Fields ---
            foreach (var field in type.GetRuntimeFields())
            {
                var fieldAttributes = field.GetCustomAttributes(typeof(AKJson.Serializable), true);
                if (fieldAttributes.FirstOrDefault() != null && dictionary.Contains(field.Name))
                {
                    var val = TransformObject(dictionary[field.Name], field.FieldType);
                    field.SetValue(ret, val);
                }
            }

            return ret;
        }
        //-----------------------------------------
        if (isGeneric && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var list = source as IEnumerable;
            Type itemType = type.GetGenericTypeArgumentAt(0);
            var ret = (IList)Activator.CreateInstance(type);
            foreach (object item in list)
            {
                ret.Add(TransformObject(item, itemType));
            }
            return ret;
        }
        if (type.IsArray)
        {
            var list = source as IList;
            var ret = (Array)Activator.CreateInstance(type, new object[]{ list.Count });
            var valueType = type.GetElementType();
            int index = 0;
            foreach (var item in list)
            {
                ret.SetValue(TransformObject(item, valueType), index);
                index++;
            }
            return ret;
        }
        //-----------------------------------------
        if (type == typeof(string))
        {
            return Convert.ToString(source);
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
        throw new ArgumentException(string.Format("No ways found to transfom object from '{0}' type  to '{1}' type.", source.GetType().Name, type.Name));
    }

    public static object Decode(string json)
    {
        return new AKJson().DecodeJson(json);
    }

    public static T Decode<T>(string json) where T : class
    {
        object obj = new AKJson().DecodeJson(json);
        return (T)TransformObject(obj, typeof(T));
    }

    public static T DecodeOrNull<T>(string json) where T : class
    {
        try
        {
            object obj = new AKJson().DecodeJson(json);
            if (obj == null)
                return null;
            return (T)TransformObject(obj, typeof(T));
        }
        catch
        {
            return null;
        }
    }

    public static object DecodeOrNull(string json)
    {
        try
        {
            return new AKJson().DecodeJson(json);
        }
        catch
        {
            return null;
        }
    }


    private object DecodeJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentNullException("json");

        char[] charArray = json.ToCharArray();
        if (charArray.Length == 0)
            return null;

        int index = 0;
        return DecodeValue(charArray, ref index);
    }

    private object DecodeValue(char[] json, ref int index)
    {
        EatWhitespace(json, ref index);

        var ch = json[index];
        if (ch == '"')
            return DecodeString(json, ref index);
        if (ch == '{')
            return DecodeObject(json, ref index);
        if (ch == '[')
            return DecodeArray(json, ref index);
        if (ch == 't') {
            Eat(json, ref index, 't', 'r', 'u', 'e');
            return true;
        }
        if (ch == 'f') {
            Eat(json, ref index, 'f', 'a', 'l', 's', 'e');
            return false;
        }
        if (ch == 'n') {
            Eat(json, ref index, 'n', 'u', 'l', 'l');
            return null;
        }
        if ("0123456789".IndexOf(ch) != -1)
            return DecodeNumber(json, ref index);

        throw new Exception();
    }

    private Dictionary<object, object> DecodeObject(char[] json, ref int index)
    {
        Dictionary<object, object> dict = new Dictionary<object, object>();

        // {
        Eat(json, ref index, '{');

        while (true)
        {
            EatWhitespace(json, ref index);
            char ch = json[index];
            if (ch == ',')
            {
                Eat(json, ref index, ',');
            }
            else if (ch == '}')
            {
                Eat(json, ref index, '}');
                break;
            }
            else
            {
                // key
                string name = DecodeString(json, ref index);

                // :
                Eat(json, ref index, ':');

                // value
                dict[name] = DecodeValue(json, ref index);
            }
        }

        return dict;
    }

    private List<object> DecodeArray(char[] json, ref int index)
    {
        List<object> list = new List<object>();

        // [
        Eat(json, ref index, '[');

        while (true)
        {
            EatWhitespace(json, ref index);
            int ch = json[index];
            if (ch == ',')
            {
                Eat(json, ref index, ',');
            }
            else if (ch == ']')
            {
                Eat(json, ref index, ']');
                break;
            }
            else
            {
                list.Add(DecodeValue(json, ref index));
            }
        }

        return list;
    }

    private string DecodeString(char[] json, ref int index)
    {
        StringBuilder s = new StringBuilder();

        // "
        Eat(json, ref index, '"');

        while (true)
        {
            char c = json[index++];
            if (c == '"')
            {
                break;
            }
            else if (c == '\\')
            {
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
                    Array.Copy(json, index, unicodeCharArray, 0, 4);
                    uint codePoint = UInt32.Parse(new string(unicodeCharArray), NumberStyles.HexNumber);
                    s.Append(Char.ConvertFromUtf32((int)codePoint));
                    // skip 4 chars
                    index += 4;
                }
            }
            else
            {
                s.Append(c);
            }
        }

        return s.ToString();
    }

    private double DecodeNumber(char[] json, ref int index)
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

    private void Eat(char[] json, ref int index, params char[]chars)
    {
        for (int i = 0; i < chars.Length; i++,index++)
            if (json[index] != chars[i])
                throw new Exception();
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

    #endregion

}

public static class TypeExtentions
{
    #if !NETFX_CORE
    public static bool IsGenericType(this Type self)
    {
        #if PCL
        return self.GetTypeInfo().IsGenericType;
        #else
        return self.IsGenericType;
        #endif
    }
    #endif

    public static Type GetGenericTypeArgumentAt(this Type self, int index)
    {
        #if PCL
        return self.GenericTypeArguments[index];
        #else
        return self.GetGenericArguments()[index];
        #endif
    }

    public static bool HasCustomAttribute(this Type self, Type attributeType)
    {
        #if PCL
        return self.GetTypeInfo().CustomAttributes.FirstOrDefault(it => it.AttributeType == attributeType) != null;
        #else
        return self.GetCustomAttributes(attributeType, true);
        #endif
    }

    #if !PCL
    public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type self)
    {
        return self.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
    }
    public static IEnumerable<FieldInfo> GetRuntimeFields(this Type self)
    {
        return self.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }
    #endif

}