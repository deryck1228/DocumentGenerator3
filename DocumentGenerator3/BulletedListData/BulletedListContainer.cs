namespace DocumentGenerator3.BulletedListData
{
    public class BulletedListContainer
    {
        public string ListId { get; set; }
        public BulletedListConfiguration BulletedList { get; set; } = new BulletedListConfiguration();
    }
}
