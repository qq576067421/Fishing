namespace GF.UCenter.Common.Portable.Models.Ip
{
    using System.Runtime.Serialization;

    [DataContract]
    public class IPInfoResponse
    {
        [DataMember(Name = "code")]
        public IPInfoResponseCode Code { get; set; }

        [DataMember(Name = "data")]
        public IPInfoResponseContent Content { get; set; }
    }
}
