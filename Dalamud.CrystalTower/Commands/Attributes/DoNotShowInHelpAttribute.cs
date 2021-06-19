using System;

namespace Dalamud.CrystalTower.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotShowInHelpAttribute : Attribute
    {
    }
}