using Vctoon.Identities;

namespace Vctoon.Mediums.Base;

public interface IMediumHasReadingProcess
{
    public ICollection<IdentityUserReadingProcess> Processes { get; set; }
}