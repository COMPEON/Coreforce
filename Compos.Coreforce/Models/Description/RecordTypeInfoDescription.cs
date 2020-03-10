namespace Compos.Coreforce.Models.Description
{
    public class RecordTypeInfoDescription
    {
        public bool available { get; set; }
        public bool defaultRecordTypeMapping { get; set; }
        public bool master { get; set; }
        public string name { get; set; }
        public string recordTypeId { get; set; }
        public RecordTypeInfoUrlsDescription urls { get; set; }
    }
}
