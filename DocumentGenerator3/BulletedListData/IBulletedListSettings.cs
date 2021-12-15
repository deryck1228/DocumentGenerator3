using System.Collections.Generic;

namespace DocumentGenerator3.BulletedListData
{
    public interface IBulletedListSettings
    {
        string service { get; set; }

        KeyValuePair<string, BulletedListConfiguration> GetBulletedListConfiguration();
    }
}