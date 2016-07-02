namespace GF.UCenter.Common.Portable.Contracts
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract]
    public class UCenterError
    {
        [DataMember]
        [JsonProperty("code")]
        public UCenterErrorCode ErrorCode { get; set; }

        [DataMember]
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}