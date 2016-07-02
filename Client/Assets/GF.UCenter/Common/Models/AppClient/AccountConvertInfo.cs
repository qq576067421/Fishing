namespace GF.UCenter.Common.Portable.Models.AppClient
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AccountConvertInfo
    {
        [DataMember]
        public string AccountId { get; set; }

        [DataMember]
        public string AccountName { get; set; }

        [DataMember]
        public string OldPassword { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string SuperPassword { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string PhoneNum { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string IdentityNum { get; set; }

        [DataMember]
        public Sex Sex { get; set; }
    }
}