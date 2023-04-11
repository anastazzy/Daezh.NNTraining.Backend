using System.Runtime.Serialization;

namespace NNTraining.Common.Enums;

public enum Types
{
    [EnumMember(Value = "System.Single")]
    Single,
    
    [EnumMember(Value = "System.String")]
    String,
}