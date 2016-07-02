namespace GF.UCenter.Common.Portable.Models.AppClient
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AppConfigurationResponse
    {
        [DataMember]
        public string AppId { get; set; }

        [DataMember]
        public string Configuration { get; set; }
    }
}