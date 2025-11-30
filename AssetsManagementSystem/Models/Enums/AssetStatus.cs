using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AssetsManagementSystem.Models.Enums
{
    [System.Text.Json.Serialization.JsonConverter ( typeof ( JsonStringEnumConverter ) )]  
    public enum AssetStatus
    {
        //[EnumMember(Value = "Active")]
        //Active = 0,

        //[EnumMember(Value = "In Repair")]
        //InRepair = 1,

        
        [EnumMember ( Value = nameof ( Available ) )]

        Available = 0,

        
        [EnumMember ( Value = nameof ( InUse ) )]

        InUse = 1,
        
        
        
        [EnumMember ( Value = nameof ( UnderMaintenance ) )]

        UnderMaintenance = 2,


        [EnumMember ( Value = nameof ( Retired ) )]
        Retired = 3,

         

        [EnumMember ( Value = nameof ( Lost ) )]

        Lost = 4


    }
}
