using System;
using System.Linq;
using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Utility;

public static class EnumUtility
{
    public static string GetMemberValue<T>(T value) where T : struct,Enum
    {
        var memberName = Enum.GetName(value);
        var valueMember = typeof(T).GetMember(memberName).Single();
        var attribute = valueMember.GetCustomAttributes(typeof(EnumMemberAttribute), false).SingleOrDefault() as EnumMemberAttribute;
        return attribute?.Value ?? memberName;
    }
}