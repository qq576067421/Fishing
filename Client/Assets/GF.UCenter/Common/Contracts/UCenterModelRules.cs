namespace GF.UCenter.Common.Portable.Contracts
{
    public static class UCenterModelRules
    {
        /// <summary>
        /// The passsword should be follow the following rules:
        /// At least one upper case letter
        /// At least one lower case letter
        /// At least one number, that is 0-9
        /// At least one special symbol, that is: !@#$%*()_+^&}{:;?.
        /// </summary>
        public const string PasswordRule =
            @"^(?=^.{6,25}$)(?=(?:.*?\d){1})(?=.*[a-z])(?=(?:.*?[A-Z]){1})(?=(?:.*?[!@#$%*()_+^&}{:;?.]){1})(?!.*\s)[0-9a-zA-Z!@#$%*()_+^&]*$";

        /// <summary>
        /// The account name rule regex
        /// it must be start with letter or _, and follow by letter, _ or number, end with latter or number.
        /// and the length between 6 and 16
        /// </summary>
        public const string AccountNameRule = @"^[a-zA-Z_]\w{4,14}[a-zA-Z0-9]$";
    }
}