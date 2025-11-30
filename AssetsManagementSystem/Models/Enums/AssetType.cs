using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AssetsManagementSystem.Models.Enums
{
    [System.Text.Json.Serialization.JsonConverter ( typeof ( JsonStringEnumConverter ) )]

    public enum AssetType
    {
        [EnumMember(Value = "IT")]
        IT = 0,

        [EnumMember(Value = "Non-IT")]
        NonIT = 1,

        [EnumMember(Value = "Consumable")]
        Consumable = 2
    }
}
