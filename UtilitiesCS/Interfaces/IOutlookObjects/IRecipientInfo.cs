using System;

namespace UtilitiesCS
{
    public interface IRecipientInfo: IEquatable<IRecipientInfo>
    {
        string Address { get; set; }
        string Html { get; set; }
        string Name { get; set; }
    }
}