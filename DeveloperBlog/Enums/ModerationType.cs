using System.ComponentModel;

namespace BlogProject.Enums
{
    public enum ModerationType
    {
        [Description("Political propaganda")]
        Political,
        [Description("Offensive language")]
        Language,
        [Description("Inappropriate content")]
        Inappropriate,
        [Description("Threatening speech")]
        Threatening,
        [Description("Sexual content")]
        Sexual
    }
}
