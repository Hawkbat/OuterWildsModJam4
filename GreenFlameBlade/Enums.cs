using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenFlameBlade
{
    public static class Enums
    {
        public static ItemType ItemType_Compass = EnumUtils.Create<ItemType>("Compass");
        public static ItemType ItemType_NomaiCrystal = EnumUtils.Create<ItemType>("NomaiCrystal");
        public static RepairReceiver.Type RepairReceiverType_Generic = EnumUtils.Create<RepairReceiver.Type>("Generic");
    }
}
