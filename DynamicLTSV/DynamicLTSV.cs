﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicLTSV
{
  public class DynamicLTSV : DynamicObject
  {
    private NameValueCollection source;

    public DynamicLTSV()
    {
      this.source = new NameValueCollection();
    }

    public DynamicLTSV(string line)
      : this()
    {
      var items = line.Split('\t');
      foreach (var item in items)
      {
        var pair = CreatePair(item);
        this.source.Add(pair.Key, pair.Value);
      }
    }

    private static KeyValuePair<string, string> CreatePair(string item)
    {
      var collonIndex = item.IndexOf(':');
      string key = item.Substring(0, collonIndex);
      string value = item.Substring(collonIndex + 1);
      return new KeyValuePair<String, String>(key, value);
    }

    public static dynamic ParseLine(string line)
    {
      return new DynamicLTSV(line);
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      var value = source[binder.Name];
      result = new StringMember(value);
      return true;
    }

    public override bool TryConvert(ConvertBinder binder, out object result)
    {
      if (binder.Type != typeof(string))
      {
        result = null;
        return false;
      }
      else
      {
        result = this.ToString();
        return true;
      }
    }
  }

  class StringMember : DynamicObject
  {
    readonly string value;

    public StringMember(string value)
    {
      this.value = value;
    }

    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
      var defaultValue = args.First();

      try
      {
        result = (value == null)
            ? defaultValue
            : Convert.ChangeType(value, defaultValue.GetType());
      }
      catch (FormatException)
      {
        result = defaultValue;
      }

      return true;
    }

    public override bool TryConvert(ConvertBinder binder, out object result)
    {
      try
      {
        var type = (binder.Type.IsGenericType && binder.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            ? binder.Type.GetGenericArguments().First()
            : binder.Type;

        result = (value == null)
            ? null
            : Convert.ChangeType(value, binder.Type);
      }
      catch (FormatException)
      {
        result = null;
      }

      return true;
    }

    public override string ToString()
    {
      return value ?? "";
    }
  }

}
