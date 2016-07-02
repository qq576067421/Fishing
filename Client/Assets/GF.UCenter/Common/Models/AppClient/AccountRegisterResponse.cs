namespace GF.UCenter.Common.Portable.Models.AppClient
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AccountRegisterResponse : AccountRequestResponse
    {
        public override void ApplyEntity(AccountResponse account)
        {
            base.ApplyEntity(account);
        }
    }
}