// Copyright (c) 2013-2015 Cemalettin Dervis, MIT License.
// https://github.com/cemdervis/SharpConfig

namespace SharpConfig
{
    /// <summary>
    /// Represents a comment in a configuration.
    /// </summary>
    public struct Comment
    {
        /// <summary>
        /// The string value of the comment.
        /// </summary>
        public string Value;

        /// <summary>
        /// The delimiting symbol of the comment.
        /// </summary>
        public char Symbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class.
        /// </summary>
        /// <param name="value"> The string value of the comment.</param>
        /// <param name="symbol">The delimiting symbol of the comment.</param>
        public Comment(string value, char symbol)
        {
            Value = value;
            Symbol = symbol;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        ///
        /// <returns>
        /// A <see cref="T:System.String" /> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", Symbol, Value ?? string.Empty);
        }
    }
}