namespace GF.UCenter.Common.Portable.Contracts
{
    public enum UCenterErrorCode : short
    {
        /// <summary>
        /// 通用，成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 未处理的通用错误
        /// </summary>
        Failed = 1000,
        DatabaseError = 1001,
        HttpClientError = 1002,

        /// <summary>
        /// 帐号不存在
        /// </summary>
        AccountNotExist = 2000,

        /// <summary>
        /// 注册，用户名重复
        /// </summary>
        AccountRegisterFailedAlreadyExist,

        /// <summary>
        /// 登陆，密码错误
        /// </summary>
        AccountLoginFailedPasswordNotMatch,

        /// <summary>
        /// 登陆，Token错误
        /// </summary>
        AccountLoginFailedTokenNotMatch,

        AppNotExit = 3000,

        /// <summary>
        /// App登陆失败，secret错误
        /// </summary>
        AppAuthFailedSecretNotMatch,

        /// <summary>
        /// App读取AccountData失败
        /// </summary>
        AppReadAccountDataFailed,

        /// <summary>
        /// App写入AccountData失败
        /// </summary>
        AppWriteAccountDataFailed,

        /// <summary>
        /// 创建Charge失败
        /// </summary>
        PaymentCreateChargeFailed = 4000,
        OrderNotFound = 4001
    }
}