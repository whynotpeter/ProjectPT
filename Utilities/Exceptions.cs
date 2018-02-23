using System;

namespace Utilities
{
    public class APDUException: Exception
    {
        public ByteArray APDU;
        public ByteArray Response;

        public byte SW1;
        public byte SW2;
        public ByteArray SW
        {
            get
            {
                ByteArray sw = new ByteArray(2);
                sw[0] = SW1;
                sw[1] = SW2;
                return sw;
            }
        }

        public APDUException(string message, ByteArray apdu, ByteArray response, byte sw1, byte sw2): base(message)
        {
            APDU = apdu;
            Response = response;
            SW1 = sw1;
            SW2 = sw2;
        }

        public new string ToString()
        {
            return string.Format("Błąd wykonywania polecenia apdu.\r\nWiadomość: {0}\r\nPolecenie: {1}\r\nKod błędu: {2}", Message, APDU, SW);
        }
    }

    class JavaCardAuthenticationException: Exception
    {
        public JavaCardAuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public JavaCardAuthenticationException(string message)
            : base(message)
        {

        }
    }

    public class ATRException : Exception
    {
        public ATRException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public ATRException(string message)
            : base(message)
        {

        }
    }
}
