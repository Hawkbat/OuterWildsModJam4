using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenFlameBlade
{
    public static class OWUtils
    {
        public static bool ShipLogFactKnown(string factID)
        {
            return PlayerData.GetShipLogFactSave(factID)?.revealOrder >= 0;
        }
    }
}
