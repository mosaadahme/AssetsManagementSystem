using System.Runtime.Serialization;

namespace AssetsManagementSystem.Models.Enums
{
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
