using System;

namespace WinTail
{
    class Messages
    {
        #region Neutral/system messages

        public class ContinueProcessing { }

        #endregion

        #region Success messages

        /// <summary>
        /// Base class for signalling that user input was valid.
        /// </summary>
        public class InputSuccess
        {
            public InputSuccess(String reason)
            {
                Reason = reason;
            }

            public String Reason { get; private set; }
        }

        #endregion

        #region Error messages

        /// <summary>
        /// Base class for signalling that user input was invalid.
        /// </summary>
        public class InputError
        {
            public InputError(String reason)
            {
                Reason = reason;
            }

            public String Reason { get; private set; }
        }

        /// <summary>
        /// User provided blank input.
        /// </summary>
        public class NullInputError : InputError
        {
            public NullInputError(String reason) : base(reason) { }
        }

        /// <summary>
        /// User provided invalid input (currently, input w/ odd # chars)
        /// </summary>
        public class ValidationError : InputError
        {
            public ValidationError(String reason) : base(reason) { }
        }

        #endregion
    }
}